using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagDoll002 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Setup();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Setup () {
		// handle collision overlaps
        // IgnoreCollision("torso", new []{"left_upper_arm", "right_upper_arm", "upper_waist"});
        // IgnoreCollision("upper_waist", new []{"left_upper_arm", "right_upper_arm", "torso"});
        IgnoreCollision("torso", new []{"left_upper_arm", "right_upper_arm"});
        //IgnoreCollision("lower_waist", new []{"upper_waist", "butt"});
        IgnoreCollision("butt", new []{"left_thigh", "right_thigh"});

        IgnoreCollision("left_larm", new []{"left_upper_arm", "left_hand"});
        IgnoreCollision("right_larm", new []{"right_upper_arm", "right_hand"});
        IgnoreCollision("left_shin", new []{"left_thigh"});
        IgnoreCollision("right_shin", new []{"right_thigh"});

        // IgnoreCollision("right_right_foot", new []{"left_right_foot"});
        // IgnoreCollision("right_shin", new []{"left_right_foot", "right_right_foot"});
        // IgnoreCollision("right_left_foot", new []{"left_left_foot"});
        // IgnoreCollision("left_shin", new []{"left_left_foot", "right_left_foot"});
        IgnoreCollision("right_shin", new []{"right_right_foot"});
        IgnoreCollision("left_shin", new []{"left_left_foot"});


        //
        var joints = GetComponentsInChildren<Joint>().ToList();
        foreach (var joint in joints)
            joint.enablePreprocessing = false;
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
}
