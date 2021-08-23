using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Scene_TMO : MonoBehaviour {
    // Camera component
    Camera m_camera;

    // Witnesses used to compute the scene tone mapping
    public LuminanceWitness[] m_witnesses;
    public int m_maxWitnessesConsidered;
    int[] m_witnessesIndex;
    float[] m_distances;

    // Compute shader
    public ComputeShader m_keyValuesViewportComputeShader;
    int m_keyValuesKernel;

    // Tone Mapping shader
    public Shader m_TMOShader;
    Material m_TMOMaterial;
    
    // Viewport TMO
    ComputeBuffer m_keyValuesViewportBuffer;
    uint[] m_keyValuesViewportArray;
    uint[] m_clearValuesViewportArray;
    public Vector3 m_keyValuesViewportVector;

    // Scene TMO
    public Vector3 m_keyValuesGlobalVector;

    // Log compression variables
    float m_minLogLum = -5.0f;
    float m_maxLogLum = 5.0f;

    // Rendering variables
    [Range(0.0f, 1.0f)]
    public float m_globalExposure = 0.02f;
    [Range(0.0f, 1.0f)]
    public float m_viewportExposure = 0.18f;
    [Range(0.0f, 2.0f)]
    public float m_saturation = 0.7f;
    [Range(0.0f, 2.0f)]
    public float m_gamma = 1.0f;
    [Range(0.0f, 1.0f)]
    public float m_switch = 0.5f;

    // Eye adaptation speed
    public float m_adaptedSpeedViewport = 1.0f;
    public float m_adaptedSpeedGlobal = 0.5f;

    // DEBUG
    public bool m_debug = false;
    public bool m_log = false;

    // Start is called before the first frame update
    void Start() {
        // Init camera
        m_camera = this.GetComponent<Camera>();
        m_camera.depthTextureMode = DepthTextureMode.DepthNormals;

        m_distances = new float[m_witnesses.Length];
        m_witnessesIndex = new int[m_witnesses.Length];

        // Init rendering material
        m_TMOMaterial = new Material(m_TMOShader);

        // Init viewport compute shader
        m_keyValuesKernel = m_keyValuesViewportComputeShader.FindKernel("KeyValues");
        m_keyValuesViewportBuffer = new ComputeBuffer(3, sizeof(uint));
        m_keyValuesViewportArray = new uint[3];
        m_clearValuesViewportArray = new uint[3];
        m_clearValuesViewportArray[0] = uint.MaxValue;
        m_keyValuesViewportVector = Vector3.zero;
        m_keyValuesViewportComputeShader.SetFloat("_MinLogLum", m_minLogLum);
        m_keyValuesViewportComputeShader.SetFloat("_MaxLogLum", m_maxLogLum);
        m_keyValuesViewportComputeShader.SetBuffer(m_keyValuesKernel, "_KeyValues", m_keyValuesViewportBuffer);
    }

    // Update is called once per frame
    void Update() {
        // Update distance to witnesses
        for (int i = 0; i < m_witnesses.Length; i++) {
            m_distances[i] = Vector3.Distance(this.transform.position, m_witnesses[i].transform.position);
            m_witnessesIndex[i] = i;
        }

        // Sort witnesses by distance
        for (int i = 0; i < m_witnesses.Length; i++) {
            for (int j = i; j < m_witnesses.Length; j++) {
                if (m_distances[i] > m_distances[j]) {
                    float tempD = m_distances[i];
                    m_distances[i] = m_distances[j];
                    m_distances[j] = tempD;

                    int tempI = m_witnessesIndex[i];
                    m_witnessesIndex[i] = m_witnessesIndex[j];
                    m_witnessesIndex[j] = tempI;
                }
            }
        }

        // Enable/Disable witnesses
        for (int i = 0; i < m_witnesses.Length; i++) {
            m_witnesses[m_witnessesIndex[i]].m_reflectionProbe.enabled = (i < m_maxWitnessesConsidered) ? true : false;
        }

        // Update nearest witnesses key values
        float blackGlobalLum = float.MaxValue;
        float whiteGlobalLum = float.MinValue;
        float globalKeyValue = 0.0f;
        for (int i = 0; i < m_maxWitnessesConsidered; i++) {
            m_witnesses[m_witnessesIndex[i]].UpdateWitness();
            blackGlobalLum = Mathf.Min(blackGlobalLum, m_witnesses[m_witnessesIndex[i]].m_keyValuesVector[0]);
            whiteGlobalLum = Mathf.Max(whiteGlobalLum, m_witnesses[m_witnessesIndex[i]].m_keyValuesVector[1]);
            globalKeyValue += m_witnesses[m_witnessesIndex[i]].m_keyValuesVector[2] / (float)m_maxWitnessesConsidered;
        }

        m_keyValuesGlobalVector[0] = Mathf.Lerp(m_keyValuesGlobalVector[0], blackGlobalLum, Time.deltaTime * m_adaptedSpeedGlobal);
        m_keyValuesGlobalVector[1] = Mathf.Lerp(m_keyValuesGlobalVector[1], whiteGlobalLum, Time.deltaTime * m_adaptedSpeedGlobal);
        m_keyValuesGlobalVector[2] = Mathf.Lerp(m_keyValuesGlobalVector[2], globalKeyValue, Time.deltaTime * m_adaptedSpeedGlobal);
    }

    private void OnGUI() {
        for (int i = 0; i < m_maxWitnessesConsidered; i++) {
            Debug.DrawLine(this.transform.position, m_witnesses[m_witnessesIndex[i]].transform.position, Color.green, 0.1f, false);
        }
    }

    [ImageEffectTransformsToLDR]
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        // Update Viewport Key Values
        UpdateLocalKeyValues(source);

        // Update key values
        m_TMOMaterial.SetVector("_KeyValuesGlobal", m_keyValuesGlobalVector);
        m_TMOMaterial.SetVector("_KeyValuesViewport", m_keyValuesViewportVector);

        // Update rendering material
        m_TMOMaterial.SetFloat("_ExposureG", m_globalExposure);
        m_TMOMaterial.SetFloat("_ExposureV", m_viewportExposure);
        m_TMOMaterial.SetFloat("_Saturation", m_saturation);
        m_TMOMaterial.SetFloat("_Gamma", m_gamma);
        m_TMOMaterial.SetFloat("_Switch", m_switch);
        
        m_TMOMaterial.SetInt("_Debug", m_debug ? 1 : 0);
        m_TMOMaterial.SetInt("_Log", m_log ? 1 : 0);
        
        // Apply TMO
        Graphics.Blit(source, destination, m_TMOMaterial);
    }

    // Compute key values
    void UpdateLocalKeyValues(RenderTexture source) {
        m_keyValuesViewportBuffer.SetData(m_clearValuesViewportArray);
        m_keyValuesViewportComputeShader.SetTexture(m_keyValuesKernel, "_Source", source);
        m_keyValuesViewportComputeShader.Dispatch(m_keyValuesKernel, Mathf.CeilToInt(source.width / 16.0f), Mathf.CeilToInt(source.height / 16.0f), 1);
        m_keyValuesViewportBuffer.GetData(m_keyValuesViewportArray);
        
        // Compute current black value
        float blackViewportLum = m_keyValuesViewportArray[0] / Mathf.Pow(2.0f, 10.0f);
        blackViewportLum = Mathf.Exp((blackViewportLum * (m_maxLogLum - m_minLogLum)) + m_minLogLum);

        // Compute current white value
        float whiteViewportLum = m_keyValuesViewportArray[1] / Mathf.Pow(2.0f, 10.0f);
        whiteViewportLum = Mathf.Exp((whiteViewportLum * (m_maxLogLum - m_minLogLum)) + m_minLogLum);

        // Compute current key value
        float avgViewportNormLogLum = m_keyValuesViewportArray[2] / Mathf.Pow(2.0f, 10.0f);
        float avgViewportLogLum = (avgViewportNormLogLum * (m_maxLogLum - m_minLogLum)) + ((float)(source.width * source.height) * m_minLogLum);
        float viewportKeyValue = Mathf.Exp(avgViewportLogLum / (float)(source.width * source.height));

        m_keyValuesViewportVector[0] = Mathf.Lerp(m_keyValuesViewportVector[0], blackViewportLum, Time.deltaTime * m_adaptedSpeedViewport);
        m_keyValuesViewportVector[1] = Mathf.Lerp(m_keyValuesViewportVector[1], whiteViewportLum, Time.deltaTime * m_adaptedSpeedViewport);
        m_keyValuesViewportVector[2] = Mathf.Lerp(m_keyValuesViewportVector[2], viewportKeyValue, Time.deltaTime * m_adaptedSpeedViewport);
    }

    void OnDestroy() {
        m_keyValuesViewportBuffer.Release();
        m_keyValuesViewportBuffer = null;
    }
}
