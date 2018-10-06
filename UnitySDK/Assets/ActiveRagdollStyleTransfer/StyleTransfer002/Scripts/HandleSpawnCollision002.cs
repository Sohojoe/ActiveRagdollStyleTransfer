using UnityEngine;
public class HandleSpawnCollision002 : MonoBehaviour
{
    Collider _collider;
    StyleTransfer002Master _master;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _master = GetComponentInParent<StyleTransfer002Master>();
    }
    void OnCollisionEnter(Collision other)
    {
        if (string.Compare(other.gameObject.name, "Terrain", true) ==0) {
            // if (_master.IsPreFirstFixedUpdateAfterReset) {
            //     foreach (var rb in _master.GetComponentsInChildren<Rigidbody>())
            //     {
            //         rb.angularVelocity = Vector3.zero;
            //         rb.velocity = Vector3.zero;
            //         rb.drag = 0f;
            //     }
            // }
            return;
        }
        print($"OnCollisionEnter: {_collider.name} & {other.gameObject.name}");
    }

    void OnCollisionExit(Collision other)
    {
        if (string.Compare(other.gameObject.name, "Terrain", true) ==0)
            return;
        print($"OnCollisionExit: {_collider.name} & {other.gameObject.name}");
    }
    // void OnTriggerEnter(Collider other)
    // {
    //     print($"OnTriggerEnter: {_collider.name} & {other.gameObject.name}");
    // }
    // void OnTriggerExit(Collider other)
    // {
    //     print($"OnTriggerExit: {_collider.name} & {other.gameObject.name}");
    // }
    // void OnTriggerStay(Collider other)
    // {
    //     print("OnTriggerStay");
    // }
}