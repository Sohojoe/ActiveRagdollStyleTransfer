using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StyleTransfer001Animator : MonoBehaviour {

	internal Animator anim;

	public List<AnimationStep> AnimationSteps;
	public bool AnimationStepsReady;
	public bool IsLoopingAnimation;

	[Range(0f,1f)]
	public float NormalizedTime;
	public float Lenght;
	// public float CurTime;

	StyleTransfer001Master _master;
	private List<Vector3> _lastPosition;
	private List<Quaternion> _lastRotation;
	// public string animName = "0008_Skipping001";

	Quaternion _initialBaseRotation;
	List<Quaternion> _initialRotations;

	List<Transform> _animBones;

    private Vector3 _lastVelocityPosition;

    [System.Serializable]
	public class AnimationStep
	{
		public float TimeStep;
		public float NormalizedTime;
		public List<Vector3> RootPositions;
		public List<Vector3> Velocities;
		public Vector3 Velocity;
		public List<Quaternion> RotaionVelocities;
		public List<Vector3> AngularVelocities;
		// public List<Vector3> NormalizedAngularVelocities;
		public List<Quaternion> RootRotations;
		public List<Vector3> RootAngles;

		public List<Vector3> Positions;
		public List<Quaternion> Rotaions;

	}

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		anim.Play("Record",0, NormalizedTime);
		anim.Update(0f);
		_master = FindObjectOfType<StyleTransfer001Master>();
		AnimationSteps = new List<AnimationStep>();
		_initialBaseRotation = transform.rotation;
	}
	void Reset()
	{
		_lastPosition = Enumerable.Repeat(Vector3.zero, _master.Muscles.Count).ToList();
		_lastRotation = Enumerable.Repeat(Quaternion.identity, _master.Muscles.Count).ToList();
		_lastVelocityPosition = transform.position;
		var anims = GetComponentsInChildren<Transform>();
		_animBones = _master.Muscles.Select(x=> anims.First(y=>y.name == x.Name).transform).ToList();
		_initialRotations = _animBones
			.Select(x=> x.rotation)
			.ToList();

	}
	
	void FixedUpdate () {
		if (_lastPosition == null)
			Reset();
		AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
		AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
		Lenght = stateInfo.length;
		NormalizedTime = stateInfo.normalizedTime;
		IsLoopingAnimation = stateInfo.loop;
		var timeStep = stateInfo.length * stateInfo.normalizedTime;
		var endTime = 1f;
		if (IsLoopingAnimation)
			endTime = 3f;
		if (NormalizedTime <= endTime) {
			UpdateAnimationStep(timeStep);
		}
		else {
			AnimationStepsReady = true;
			anim.enabled=false;
		}
	}
	void UpdateAnimationStep(float timeStep)
    {
		// HACK deal with two of first frame
		if (NormalizedTime == 0f && AnimationSteps.FirstOrDefault(x=>x.NormalizedTime == 0f) != null)
			return;

		var c = _master.Muscles.Count;
		var animStep = new AnimationStep();
		animStep.TimeStep = timeStep;
		animStep.NormalizedTime = NormalizedTime;
		animStep.RootPositions = Enumerable.Repeat(Vector3.zero, c).ToList();
		animStep.Velocities = Enumerable.Repeat(Vector3.zero, c).ToList();
		animStep.RotaionVelocities = Enumerable.Repeat(Quaternion.identity, c).ToList();
		animStep.AngularVelocities = Enumerable.Repeat(Vector3.zero, c).ToList();
		// animStep.NormalizedAngularVelocities = Enumerable.Repeat(Vector3.zero, c).ToList();
		animStep.RootRotations = Enumerable.Repeat(Quaternion.identity, c).ToList();
		animStep.RootAngles = Enumerable.Repeat(Vector3.zero, c).ToList();
		animStep.Positions = Enumerable.Repeat(Vector3.zero, c).ToList();
		animStep.Rotaions = Enumerable.Repeat(Quaternion.identity, c).ToList();
		animStep.Velocity = transform.position - _lastVelocityPosition;
		_lastVelocityPosition = transform.position;

		var rootBone = _animBones[0];
		// Quaternion rootRotation = rootBone.rotation;
		var toRootSpace = Quaternion.Inverse(_master.Muscles[0].Rigidbody.rotation) * rootBone.rotation;

		foreach (var m in _master.Muscles)
		{
			var i = _master.Muscles.IndexOf(m);
			var animBone = _animBones[i];
			Quaternion rootRotation = Quaternion.Inverse(rootBone.rotation * toRootSpace) * animBone.rotation;

			animStep.RootPositions[i] = animBone.position - rootBone.position;
			animStep.RootRotations[i] = rootRotation;
			animStep.RootAngles[i] = rootRotation.eulerAngles;
			animStep.Positions[i] = animBone.position - transform.parent.position;
			animStep.Rotaions[i] = animBone.rotation * Quaternion.Inverse(transform.parent.rotation);
			if (NormalizedTime != 0f) {
				animStep.Velocities[i] = animStep.Positions[i] - _lastPosition[i];
				animStep.RotaionVelocities[i] = JointHelper001.FromToRotation(_lastRotation[i], animStep.Rotaions[i]);
				animStep.AngularVelocities[i] = animStep.RotaionVelocities[i].eulerAngles;
			}
			_lastPosition[i] = animStep.Positions[i];
			_lastRotation[i] = animStep.Rotaions[i];

		}
		AnimationSteps.Add(animStep);
    }
}
