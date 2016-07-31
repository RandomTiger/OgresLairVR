using UnityEngine;
using System.Collections;

public class WeaponHand : MonoBehaviour {

    public GameObject AxePrefab;
    public GameObject AxeInstance;

    private SteamVR_TrackedObject m_TrackedObj;
    private SteamVR_TrackedController m_TrackedCtl;
    private NewtonVR.NVRHand m_Hand;

    Vector3 m_Velocity = Vector3.zero;
    Vector3 m_VelocityOverTime = Vector3.zero;
    Collider[] m_ColliderList;

    // Use this for initialization
    void Start ()
    {
        m_TrackedObj = GetComponent<SteamVR_TrackedObject>();
        m_TrackedCtl = GetComponent<SteamVR_TrackedController>();
        m_Hand       = GetComponent<NewtonVR.NVRHand>();

        m_TrackedCtl.TriggerClicked += TriggerDown;
        m_TrackedCtl.TriggerUnclicked += TriggerUp;

        m_ColliderList = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
    }

    void TriggerDown(object sender, ClickedEventArgs e)
    {
        AxeInstance = (GameObject)Instantiate(AxePrefab, transform, false);
        AxeInstance.GetComponent<Rigidbody>().isKinematic = true;

        m_VelocityOverTime = Vector3.zero;
        m_Velocity = Vector3.zero;
    }

    void TriggerUp(object sender, ClickedEventArgs e)
    {
        if (AxeInstance == null || m_Hand == null)
        {
            return;
        }

        m_Velocity = m_Hand.GetVelocityEstimation();

        Rigidbody rigidbody = AxeInstance.GetComponent<Rigidbody>();

        AxeInstance.transform.parent = null;
        rigidbody.isKinematic = false;

        Vector3 direction = Vector3.Lerp(transform.up * -1.0f, transform.forward, 0.5f);
        float magnitude = 1000.0f * m_Velocity.magnitude;

        rigidbody.AddForce(direction * magnitude);
        rigidbody.AddTorque(new Vector3(360.0f, 0, 0));
        AxeInstance = null;
    }
}
