using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sim2Converter : MonoBehaviour
{
    public Shader m_sim2ConverterShader;
    Material m_sim2ConverterMaterial;
    
    // Start is called before the first frame update
    void Start() {
        m_sim2ConverterMaterial = new Material(m_sim2ConverterShader);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, m_sim2ConverterMaterial);    
    }
}
