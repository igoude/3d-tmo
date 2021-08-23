using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveCamera : MonoBehaviour
{
    public bool translation;
    public bool rotation;
    public bool half;
    public bool fly;

    public float speedTranslate = 0.1f;
    public float speedRotate = 2.0f;
    public float offsetRotate = 0.0f;
    
    Vector2 previousMousePosition = new Vector2(0.5f, 0.5f);
    float pan = 0.0f;
    float tilt = 0.0f;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Input.mousePosition.Set(0.5f, 0.5f, 0.0f);
        Cursor.visible = false;

        previousMousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        // Offset rotation
        pan = offsetRotate;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (translation) {
            if (Input.GetMouseButton(0)) {
                float up = fly ? this.transform.forward.y : 0.0f;
                this.transform.position += new Vector3(this.transform.forward.x, up, this.transform.forward.z) * speedTranslate;
            }

            //if (Input.GetKey(KeyCode.Z)) {
            //    this.transform.Translate(Vector3.forward * speedTranslate);
            //}

            //if (Input.GetKey(KeyCode.S)) {
            //    this.transform.Translate(Vector3.back * speedTranslate);
            //}

            //if (Input.GetKey(KeyCode.D)) {
            //    this.transform.Translate(Vector3.right * speedTranslate);
            //}

            //if (Input.GetKey(KeyCode.Q)) {
            //    this.transform.Translate(Vector3.left * speedTranslate);
            //}
        }
        

        if (rotation) {
            pan += Input.GetAxis("Mouse X") * speedRotate;
            if (half) {
                if (pan < -90.0f + offsetRotate) pan = -90.0f + offsetRotate;
                if (pan > 90.0f + offsetRotate) pan = 90.0f + offsetRotate;
            } else {
                if (pan < 0.0f) pan += 360.0f;
                if (pan > 360.0f) pan -= 360.0f;
            }
            
            tilt -= Input.GetAxis("Mouse Y") * speedRotate;
            if (tilt < -90.0f) tilt = -90.0f;
            if (tilt > 90.0f) tilt = 90.0f;

            this.transform.localRotation = Quaternion.Euler(tilt, pan, 0.0f);
        }
    }
}
