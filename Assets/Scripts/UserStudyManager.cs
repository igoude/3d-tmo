using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyManager : MonoBehaviour {
    public float m_zMin;
    public float m_zMax;
    public float m_speed = 0.3f;

    bool m_isDriving;

    float m_cameraHeight;


    void Start() {
        m_cameraHeight = Camera.main.transform.position.y;    
    }
    
    
    // Update is called once per frame
    void FixedUpdate() {
        // Move car
        //if (Input.GetKey(KeyCode.Space)) {
        //    m_isDriving = true;
        //}

        //if (m_isDriving) {
        //    if (this.transform.position.z < m_zMax)
        //        this.transform.position += Vector3.forward * m_speed;
        //}

        //if (Input.GetKey(KeyCode.DownArrow)) {
        //    if (this.transform.position.z > m_zMin)
        //        this.transform.position -= Vector3.forward * m_speed;
        //}

        if (Input.GetMouseButton(0)) {
            if (this.transform.position.z < m_zMax)
                this.transform.position += Vector3.forward * m_speed;
        }

        if (Input.GetMouseButton(1)) {
            if (this.transform.position.z > m_zMin)
                this.transform.position -= Vector3.forward * m_speed;
        }

        if (Input.GetKey(KeyCode.R)) {
            m_isDriving = false;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, m_zMin);
        }

        // Adjust camera
        //if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
        //    Camera.main.transform.parent.Translate(Vector3.up * 0.025f);
        //}
        //if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
        //    Camera.main.transform.parent.Translate(-Vector3.up * 0.025f);
        //}
    }
}
