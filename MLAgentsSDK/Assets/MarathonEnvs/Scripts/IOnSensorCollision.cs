using UnityEngine;

namespace MLAgents
{
    public interface IOnSensorCollision
    {
         void OnSensorCollisionEnter(Collider sensorCollider, Collision other);
         void OnSensorCollisionExit(Collider sensorCollider, Collision other);

    }
}