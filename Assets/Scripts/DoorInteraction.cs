using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform m_camera;
    public Transform m_door;
    public Transform m_initTransform;
    public Transform m_anchor;

    public float m_activeDistance;
    public float m_openingTime;
    public float m_openingFactor;
    float m_side;
    
    bool m_isOpen;

    float m_timer;
    bool m_isOpening;
    bool m_isClosing;

    public bool m_enable;

    public bool m_openStep;

    public LuminanceWitness m_witness;

    // Start is called before the first frame update
    void Start() {
        m_timer = 0.0f;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (m_openStep) {
            m_door.transform.RotateAround(m_anchor.position, m_door.transform.forward, 10.0f);
            m_openStep = false;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            m_enable = true;
        }
        if (!m_enable) {
            return;
        }

        if (m_isOpening) {
            m_timer += Time.deltaTime;
            m_door.transform.RotateAround(m_anchor.position, m_door.transform.forward, m_side*m_openingFactor);

            if(m_timer > m_openingTime) {
                m_isOpening = false;
                m_isOpen = true;
                m_timer = 0.0f;
            }

            m_witness.NeedUpdate();

            return;
        }

        if (m_isClosing) {
            m_timer += Time.deltaTime;
            m_door.transform.RotateAround(m_anchor.position, m_door.transform.forward, m_side*m_openingFactor);

            if (m_timer > m_openingTime) {
                m_isClosing = false;
                m_isOpen = false;
                m_timer = 0.0f;
                m_door.transform.position = m_initTransform.position;
                m_door.transform.rotation = m_initTransform.rotation;
            }

            m_witness.NeedUpdate();

            return;
        }

        float currentDistance = Vector2.Distance(new Vector2(m_initTransform.position.x, m_initTransform.position.z), new Vector2(m_camera.position.x, m_camera.position.z));

        // Should open
        if (currentDistance < m_activeDistance && !m_isOpen) {
            m_side = (Vector3.Dot(Vector3.right, (m_initTransform.position - m_camera.position).normalized) > 0.0f) ? 1.0f : -1.0f;
            m_isOpening = true;
        }

        // Should close
        if (currentDistance > m_activeDistance && m_isOpen) {
            m_side = (Vector3.Dot(Vector3.right, (m_initTransform.position - m_camera.position).normalized) > 0.0f) ? 1.0f : -1.0f;
            m_isClosing = true;
        }
    }
}
