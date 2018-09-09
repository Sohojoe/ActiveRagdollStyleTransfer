using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEngine;



// [CustomEditor(typeof(CopyRagDoll))]
// public class CopyRagDollEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
        
//         CopyRagDoll myScript = (CopyRagDoll)target;
//         if(GUILayout.Button("CopyRagDoll"))
//         {
//             myScript.ConvertPuppetToMojo();
//         }
//     }
// }
// public class CopyRagDollEditor : MonoBehaviour
// {
//     void Start () {
//         CopyRagDoll myScript = (CopyRagDoll)target;
//     }
// }



public class CopyRagDoll : MonoBehaviour {

    [System.Serializable]
    public class CopyItem 
    {
        public Transform Source;
        public Transform Destination;
    }
    public List<CopyItem> ConversionTable;
    public List<CopyItem> CopyTable;

    void Start () {
        // //ConvertPuppetToMojo();
        // doneFirst = true;
        // Aaa();
        // GameObject ragDollSource = GameObject.Find("humanoid");
        // Object.Destroy(ragDollSource);
        // var styleTransfer002Animator = GetComponent<StyleTransfer002Animator>();
        // styleTransfer002Animator.StopAnimation();
        // styleTransfer002Animator.BecomeRagDoll();
    }
    bool doneFirst;
    void Update () {
        if (!doneFirst)
        {
            doneFirst = true;
            Aaa();
            GameObject ragDollSource = GameObject.Find("humanoid");
            Object.Destroy(ragDollSource);
            var styleTransfer002Animator = GetComponent<StyleTransfer002Animator>();
            styleTransfer002Animator.StopAnimation();
            styleTransfer002Animator.BecomeRagDoll();
        }
    }
    List<Transform> _sources;
    List<Transform> _targets;
    public void Aaa()
    {
        GameObject ragDollSource = GameObject.Find("humanoid");
        if (ragDollSource == null)
            ragDollSource = GameObject.Find("DeepMindHumanoid");
        ragDollSource.GetComponent<MarathonAgent>().AgentReset();
        _sources = ragDollSource.GetComponentsInChildren<Transform>().ToList();
        _targets = GetComponentsInChildren<Transform>().ToList();
        //CopyBoneAsChild(new CopyItem{Source=sources.First(x=>x.name == "head"), Destination=targets.First(x=>x.name == "mixamorig:Neck")});
        // CopyBoneAsChild("butt",             "mixamorig:Hips",           new Vector3(.01f, -.057f, .004f),       Quaternion.Euler(0, 88.2f, 88.8f));
        CopyBoneAsChild("butt",             "mixamorig:Hips",           new Vector3(.01f, -.057f, .004f),       Quaternion.Euler(90, 88.2f, 88.8f));
        CopyBoneAsChild("lower_waist",      "mixamorig:Spine",          new Vector3(0, .012f, 0),               Quaternion.Euler(90, 90, 90));
        CopyBoneAsChild("upper_waist",      "mixamorig:Spine1",         new Vector3(0, .048f, 0),               Quaternion.Euler(90, 90, 90));
        CopyBoneAsChild("torso",            "mixamorig:Spine2",         new Vector3(0, .046f, 0),               Quaternion.Euler(90, 90, 90));
        CopyBoneAsChild("head",             "mixamorig:Head",           new Vector3(0,.05f,.002f),              Quaternion.Euler(0, 0, 0));

        CopyBoneAsChild("left_upper_arm",   "mixamorig:LeftArm",        new Vector3(.005f, .198f, -.011f),      Quaternion.Euler(180-2, 0, 0));
        CopyBoneAsChild("left_larm",        "mixamorig:LeftForeArm",    new Vector3(-.008f, .162f, -.022f),     Quaternion.Euler(180-2.71f, 0, 3));
        CopyBoneAsChild("left_hand",        "mixamorig:LeftHand",       new Vector3(-.013f, .04f, -.03f),       Quaternion.Euler(180, 0, 0));
        
        CopyBoneAsChild("right_upper_arm",  "mixamorig:RightArm",       new Vector3(.005f, .198f, .011f),       Quaternion.Euler(182, 0, 0));
        CopyBoneAsChild("right_larm",       "mixamorig:RightForeArm",   new Vector3(.005f, .163f, .035f),       Quaternion.Euler(185, 0, 3));
        CopyBoneAsChild("right_hand",       "mixamorig:RightHand",      new Vector3(.001f, .04f, .0492f),       Quaternion.Euler(180, 0, 0));

        CopyBoneAsChild("left_thigh",       "mixamorig:LeftUpLeg",      new Vector3(0, .209f, .006f),           Quaternion.Euler(0, 0, 0));
        CopyBoneAsChild("left_shin",        "mixamorig:LeftLeg",        new Vector3(.0f, .185f, .004f),         Quaternion.Euler(0, 0, -3));
        CopyBoneAsChild("left_left_foot",   "mixamorig:LeftToeBase",    new Vector3(-.024f, -.0622f, .0326f),   Quaternion.Euler(-8, 0, 3));
        CopyBoneAsChild("right_left_foot",  "mixamorig:LeftToeBase",    new Vector3(.019f, -.061f, .03f),       Quaternion.Euler(-8, 0, -8));

        CopyBoneAsChild("right_thigh",      "mixamorig:RightUpLeg",     new Vector3(0, .209f, .006f),           Quaternion.Euler(0, 0, 0));
        CopyBoneAsChild("right_shin",       "mixamorig:RightLeg",       new Vector3(0, .185f, .004f),           Quaternion.Euler(0, 0, -3));
        CopyBoneAsChild("right_right_foot", "mixamorig:RightToeBase",   new Vector3(-.024f, -.0622f, .0326f),   Quaternion.Euler(-8, 0, 3));
        CopyBoneAsChild("left_right_foot",  "mixamorig:RightToeBase",   new Vector3(.0144f, -.061f, .03f),      Quaternion.Euler(-8, 0, -8));

        // fix connected bodies
        var rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        var withConnection = GetComponentsInChildren<Joint>()
            .Where(x=>x.connectedBody != null)
            .ToList();
        foreach (var joint in withConnection)
        {
            var newConnectedBody = rigidbodies.First(x=>x.name.Contains(joint.connectedBody.name));
            joint.connectedBody = newConnectedBody;
        }

        // handle collision overlaps
        IgnoreCollision("torso", new []{"left_upper_arm", "right_upper_arm", "upper_waist"});
        IgnoreCollision("upper_waist", new []{"left_upper_arm", "right_upper_arm"});
        IgnoreCollision("butt", new []{"left_thigh", "right_thigh"});

        IgnoreCollision("left_larm", new []{"left_upper_arm", "left_hand"});
        IgnoreCollision("right_larm", new []{"right_upper_arm", "right_hand"});
        IgnoreCollision("left_shin", new []{"left_thigh"});
        IgnoreCollision("right_shin", new []{"right_thigh"});

        // IgnoreCollision("", new []{""});

        // FlipAnchor("right_upper_arm");
        // FlipAnchor("right_shoulder2");
        // FlipAnchor("right_larm");

        // FlipAnchor("left_upper_arm");
        // FlipAnchor("left_shoulder2");
        // FlipAnchor("left_larm");

        FlipAnchor("right_thigh");
        FlipAnchor("right_hip_z");
        FlipAnchor("right_hip_y");
        FlipAnchor("right_shin");
        FlipAnchor("left_thigh");
        FlipAnchor("left_hip_z");
        FlipAnchor("left_hip_y");
        FlipAnchor("left_shin");
        // FlipAnchor("");


    }
    void IgnoreCollision(string first, string[] seconds)
    {
        foreach (var second in seconds)
        {
            IgnoreCollision(first, second);
        }
    }
    void IgnoreCollision(string first, string second)
    {
        var rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        var rbOne = rigidbodies.First(x=>x.name.Contains(first)).GetComponent<Collider>();
        var rbTwo = rigidbodies.First(x=>x.name.Contains(second)).GetComponent<Collider>();
        Physics.IgnoreCollision(rbOne, rbTwo);
    }
    void FlipAnchor(string name)
    {
        var joints = GetComponentsInChildren<ConfigurableJoint>().ToList();
        var j = joints.First(x=>x.name.Contains(name));
        j.anchor = new Vector3(j.anchor.x, -j.anchor.y, j.anchor.z);
    }

	public void ConvertPuppetToMojo()
	{
        var items = ConversionTable;
        // var items = CopyTable.Where(x=>x.Destination != null && x.Source != null).ToList();
        foreach (var item in items)
        {
            //CopyBone(item);
            CopyBoneAsChild(item, Vector3.zero, Quaternion.Euler(0, 0, 0));
        }
	}
    void CopyBoneAsChild(string sourceName, string targetName, Vector3 offset, Quaternion rotation)
    {
        var item = new CopyItem{
            Source=_sources.First(x=>x.name == sourceName), 
            Destination=_targets.First(x=>x.name == targetName)
            };
        CopyBoneAsChild(item, offset, rotation);
    }
    void CopyBoneAsChild(CopyItem item, Vector3 offset, Quaternion rotation)
    {
        var source = item.Source;
        var target = item.Destination;
        var copy = Instantiate(source, item.Destination);
        Rigidbody  rigidbody = copy.GetComponent<Rigidbody>();
        if(rigidbody != null)
            rigidbody.isKinematic = true;
        CapsuleCollider capsuleCollider = copy.GetComponent<CapsuleCollider>();
        copy.transform.localPosition = offset;
        copy.transform.localRotation = rotation;
        return;
    }
    void CopyBone(CopyItem item)
    {
        var source = item.Source;
        var target = item.Destination;

        MeshFilter  meshFilter = source.GetComponent<MeshFilter>();
        MeshRenderer  meshRenderer = source.GetComponent<MeshRenderer>();
        CapsuleCollider  capsuleCollider = source.GetComponent<CapsuleCollider>();
        SphereCollider  sphereCollider = source.GetComponent<SphereCollider>();
        Rigidbody  rigidbody = source.GetComponent<Rigidbody>();
        FixedJoint  fixedJoint = source.GetComponent<FixedJoint>();
        ConfigurableJoint  configurableJoint = source.GetComponent<ConfigurableJoint>();
        SensorBehavior sensorBehavior = source.GetComponent<SensorBehavior>();
        HandleOverlap handleOverlap = source.GetComponent<HandleOverlap>();
        if(meshFilter != null) CopyComponent(meshFilter, target.gameObject);
        if(meshRenderer != null) CopyComponent(meshRenderer, target.gameObject);
        if(capsuleCollider != null) CopyComponent(capsuleCollider, target.gameObject);
        if(sphereCollider != null) CopyComponent(sphereCollider, target.gameObject);
        if(rigidbody != null) CopyComponent(rigidbody, target.gameObject);
        if(fixedJoint != null) CopyComponent(fixedJoint, target.gameObject);
        if(configurableJoint != null) CopyComponent(configurableJoint, target.gameObject);
        if(sensorBehavior != null) CopyComponent(sensorBehavior, target.gameObject);
        if(handleOverlap != null) CopyComponent(handleOverlap, target.gameObject);
        if(sphereCollider != null) target.localScale = source.localScale;
        target.GetComponent<Rigidbody>().isKinematic = true;

    }
    T CopyComponent<T>(T original, GameObject destination) where T : Component
     {
         System.Type type = original.GetType();
         var dst = destination.GetComponent(type) as T;
         if (!dst) dst = destination.AddComponent(type) as T;
         var fields = type.GetFields();
         foreach (var field in fields)
         {
             if (field.IsStatic) continue;
             field.SetValue(dst, field.GetValue(original));
         }
         var props = type.GetProperties();
         foreach (var prop in props)
         {
             if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
             prop.SetValue(dst, prop.GetValue(original, null), null);
         }
         return dst as T;
     }
}
