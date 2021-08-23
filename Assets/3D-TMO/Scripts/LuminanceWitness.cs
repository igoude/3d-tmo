using UnityEngine;

[RequireComponent(typeof(ReflectionProbe))]
public class LuminanceWitness : MonoBehaviour {
    // Luminance witness through a reflection probe
    [HideInInspector]
    public ReflectionProbe m_reflectionProbe;

    // Compute shader
    ComputeShader m_keyValuesComputeShader;
    int m_keyValuesKernel;
    ComputeBuffer m_keyValuesBuffer;
    uint[] m_keyValuesArray;
    uint[] m_clearValuesArray;
    public Vector3 m_keyValuesVector;

    // Log compression variables
    float m_minLogLum = -5.0f;
    float m_maxLogLum = 10.0f;

    bool m_needUpdate = true;

    public void NeedUpdate() {
        m_needUpdate = true;
    }


    // Start is called before the first frame update
    void Start() {
        // Get reflection probe
        m_reflectionProbe = this.GetComponent<ReflectionProbe>();

        // Init parameters
        m_keyValuesComputeShader = (ComputeShader)Instantiate(Resources.Load("WitnessComputeShader"));
        m_keyValuesKernel = m_keyValuesComputeShader.FindKernel("KeyValues");
        m_keyValuesBuffer = new ComputeBuffer(3, sizeof(uint));
        m_keyValuesArray = new uint[3];
        m_clearValuesArray = new uint[3];
        m_clearValuesArray[0] = uint.MaxValue;
        m_keyValuesVector = Vector3.zero;
        
        // Init compute shader
        m_keyValuesComputeShader.SetInt("_Size", m_reflectionProbe.resolution);
        m_keyValuesComputeShader.SetFloat("_MinLogLum", m_minLogLum);
        m_keyValuesComputeShader.SetFloat("_MaxLogLum", m_maxLogLum);
        m_keyValuesComputeShader.SetBuffer(m_keyValuesKernel, "_KeyValues", m_keyValuesBuffer);
    }
    
    // Update is called once per frame
    public void UpdateWitness() {
        if (!m_needUpdate) return;

        // Update luminance witness only one time at start
        if (m_reflectionProbe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake) {
            m_needUpdate = false;
        }

        // Update luminance witness only when usefull, managed by the user
        if (m_reflectionProbe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting) {
            // To refresh the probe, call NeedUpdate() method from another script when it useful (cf. DoorInteraction.cs)
            m_reflectionProbe.RenderProbe();
            m_needUpdate = false;
        }

        UpdateKeyValues();
    }

    // Compute key values
    void UpdateKeyValues() {
        // Clear key values buffer
        m_keyValuesBuffer.SetData(m_clearValuesArray);

        // Set reflection probe texture as the source
        Texture probe = m_reflectionProbe.texture;
        // BUG: At start, reflection probe texture may be null
        if (probe == null) {
            m_needUpdate = true;
            return;
        }

        int groups = Mathf.CeilToInt(probe.height / 16.0f);
        m_keyValuesComputeShader.SetTexture(m_keyValuesKernel, "_Source", probe);

        // Spherical coordinates: appreciated
        float nbProbesPixels = (float)(probe.width * probe.width);
        m_keyValuesComputeShader.Dispatch(m_keyValuesKernel, groups, groups, 1);

        // Cartesian coordinates: it is very costly
        //float nbProbesPixels = (float)(probe.width * probe.width * probe.width);
        //m_keyValuesComputeShader.Dispatch(m_keyValuesKernel, groups, groups, groups);

        // Get back key values from GPU to CPU
        m_keyValuesBuffer.GetData(m_keyValuesArray);

        // Compute current black value
        float blackGlobalLum = m_keyValuesArray[0] / Mathf.Pow(2.0f, 10.0f);
        blackGlobalLum = Mathf.Exp((blackGlobalLum * (m_maxLogLum - m_minLogLum)) + m_minLogLum);

        // Compute current white value
        float whiteGlobalLum = m_keyValuesArray[1] / Mathf.Pow(2.0f, 10.0f);
        whiteGlobalLum = Mathf.Exp((whiteGlobalLum * (m_maxLogLum - m_minLogLum)) + m_minLogLum);

        // Compute current key value
        float avgGlobalNormLogLum = m_keyValuesArray[2] / Mathf.Pow(2.0f, 10.0f);
        
        float avgGlobalLogLum = (avgGlobalNormLogLum * (m_maxLogLum - m_minLogLum)) + (nbProbesPixels * m_minLogLum);
        float globalKeyValue = Mathf.Exp(avgGlobalLogLum / nbProbesPixels);

        // Set key values vector
        m_keyValuesVector[0] = blackGlobalLum;
        m_keyValuesVector[1] = whiteGlobalLum;
        m_keyValuesVector[2] = globalKeyValue;
    }

    void OnDestroy() {
        m_keyValuesBuffer.Release();
        m_keyValuesBuffer = null;
    }
}
