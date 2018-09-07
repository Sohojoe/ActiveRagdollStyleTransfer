using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(CopyRagDoll))]
public class CopyRagDollEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CopyRagDoll myScript = (CopyRagDoll)target;
        if(GUILayout.Button("CopyRagDoll"))
        {
            myScript.ConvertPuppetToMojo();
        }
    }
}


public class CopyRagDoll : MonoBehaviour {
	public void ConvertPuppetToMojo()
	{
		//throw new System.NotImplementedException();
        GameObject ragDollSource = GameObject.Find("Agent002");
        var master = ragDollSource.GetComponentInChildren<StyleTransfer002Master>();
        var sources = master.GetComponentsInChildren<Transform>();
        var sourceNames = sources.Select(x=>x.name).ToList();
        var targets = GetComponentsInChildren<Transform>()
            .Where(x=> sourceNames.Contains(x.name));
        foreach (var target in targets)
        {
            var source = sources.First(x=>x.name == target.name);
            // MeshFilter  meshFilter = source.GetComponent<MeshFilter>();
            // MeshRenderer  meshRenderer = source.GetComponent<MeshRenderer>();
            CapsuleCollider  capsuleCollider = source.GetComponent<CapsuleCollider>();
            SphereCollider  sphereCollider = source.GetComponent<SphereCollider>();
            Rigidbody  rigidbody = source.GetComponent<Rigidbody>();
            FixedJoint  fixedJoint = source.GetComponent<FixedJoint>();
            ConfigurableJoint  configurableJoint = source.GetComponent<ConfigurableJoint>();
            // if(meshFilter != null) CopyComponent(meshFilter, target.gameObject);
            // if(meshRenderer != null) CopyComponent(meshRenderer, target.gameObject);
            if(capsuleCollider != null) CopyComponent(capsuleCollider, target.gameObject);
            if(sphereCollider != null) CopyComponent(sphereCollider, target.gameObject);
            if(rigidbody != null) CopyComponent(rigidbody, target.gameObject);
            if(fixedJoint != null) CopyComponent(fixedJoint, target.gameObject);
            if(configurableJoint != null) CopyComponent(configurableJoint, target.gameObject);
        }
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
