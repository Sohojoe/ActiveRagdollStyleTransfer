using UnityEngine;

public static class JointHelper001
{
    public static Quaternion FromToRotation(Quaternion from, Quaternion to) {
        if (to == from) return Quaternion.identity;

        return to * Quaternion.Inverse(from);
    }    
}