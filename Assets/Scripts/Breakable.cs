using UnityEngine;
using Exploder;

class Breakable : MonoBehaviour
{
    Transform root;
    ExploderObject exploder;

    float m_Health = 100.0f;

    void Start()
    {
        root = GetRoot();
        exploder = root.GetComponent<ExploderObject>();
    }

    Transform GetRoot()
    {
        Transform root = transform;
        while(root.parent != null && root.GetComponent<Root>() == null)
        {
            root = root.parent;
        }

        return root;
    }

    void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform;
        while (root.parent != null && root.GetComponent<Root>() == null)
        {
            root = root.parent;
        }

        bool colliderNotHeld = root.parent == null;
        if (colliderNotHeld)
        {
            other.transform.parent = transform;
        }

        m_Health -= 20f;
        if (m_Health < 0)
        {
            exploder.ExplodeObject(exploder.gameObject);
        }
    }
}
