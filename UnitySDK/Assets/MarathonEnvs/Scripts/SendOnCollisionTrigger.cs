using UnityEngine;

namespace MLAgents
{
    public class SendOnCollisionTrigger : MonoBehaviour
    {
        void OnCollisionEnter(Collision other)
        {
            // Messenger.
            var otherGameobject = other.gameObject;
            var marathonAgent = otherGameobject.GetComponentInParent<IOnTerrainCollision>();
            if (marathonAgent != null)
                marathonAgent.OnTerrainCollision(otherGameobject, this.gameObject);
        }
    }
}