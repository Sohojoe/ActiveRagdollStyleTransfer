using UnityEngine;

namespace MLAgents
{
    public class SensorBehavior : MonoBehaviour
    {
        IOnSensorCollision _onSensorCollision;
        Collider _collider;
        void Start ()
        {
            _onSensorCollision = GetComponentInParent<IOnSensorCollision>();
            _collider = GetComponent<Collider>();
        }
        void OnCollisionEnter(Collision other) 
        {
            if (_onSensorCollision!=null)
                _onSensorCollision.OnSensorCollisionEnter(_collider, other);
        }
        void OnCollisionExit(Collision other) 
        {
            if (_onSensorCollision!=null)
                _onSensorCollision.OnSensorCollisionExit(_collider, other);
        }

    }
}