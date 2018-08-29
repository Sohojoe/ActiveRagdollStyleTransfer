// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;



// [CustomEditor(typeof(PuppetToMojo))]
// public class PuppetToMojoEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
        
//         PuppetToMojo myScript = (PuppetToMojo)target;
//         if(GUILayout.Button("Convert Puppet To Mojo"))
//         {
//             myScript.ConvertPuppetToMojo();
//         }
//     }
// }


// public class PuppetToMojo : MonoBehaviour {
// 	public void ConvertPuppetToMojo()
// 	{
// 		throw new System.NotImplementedException();
// 		var puppetMaster = GetComponent<RootMotion.Dynamics.PuppetMaster>();
// 		var master = GetComponent<StyleTransfer001Master>();

// 		var muscles = puppetMaster.muscles.Select( x=> 
// 			new Muscle001{
// 				Group = (MuscleGroup001)x.props.group,
// 				Name = x.name,
// 				ConfigurableJoint = x.joint,
// 				target = x.target

// 			}).ToList();
// 		master.Muscles = muscles;
// 	}
// }
