using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;

public class StyleTransfer002Master : MonoBehaviour {

	public bool visualizeAnimator = true;

	public List<Muscle002> Muscles;
	public float PositionDistance;
	public float EndEffectorDistance; // feet and hands
	public float RotationDistance;
	public float VelocityDistance;
	public Vector3 CenterOfMass;
	public bool IgnorRewardUntilObservation;
	public float ErrorCutoff;
	public bool DebugShowWithOffset;
	public bool DebugMode;
	public bool DebugDisableMotor;
    [Range(-100,100)]
	public int DebugAnimOffset;

	public float Phase;

	public float TimeStep;
	public int AnimationIndex;
	public int EpisodeAnimationIndex;
	public int StartAnimationIndex;
	public bool UseRandomIndex;
	public bool CameraFollowMe;
	public Transform CameraTarget;

	private bool _isDone;
	bool _resetCenterOfMassOnLastUpdate;
	bool _fakeVelocity;


	// public List<float> vector;

	private StyleTransfer002Animator _muscleAnimator;
	private Brain _brain;
	bool _phaseIsRunning;
	Random _random = new Random();
	Vector3 _lastCenterOfMass;

	static MuscleGroup002 FromName(string name)
    {
        name = name.ToLower();
        if (name.Contains("butt"))
            return MuscleGroup002.Hips;
        if (name.Contains("thigh") || name.Contains("hip"))
            return MuscleGroup002.LegUpper;
        if (name.Contains("shin"))
            return MuscleGroup002.LegLower;
        if (name.Contains("foot") || name.Contains("ankle"))
            return MuscleGroup002.Foot;
        if (name.Contains("torso"))
            return MuscleGroup002.Torso;
        if (name.Contains("waist") || name.Contains("abdomen"))
            return MuscleGroup002.Spine;
        if (name.Contains("head"))
            return MuscleGroup002.Head;
        if (name.Contains("upper_arm") || name.Contains("shoulder"))
            return MuscleGroup002.Shoulder;
		if (name.Contains("larm"))
            return MuscleGroup002.Elbow;
		if (name.Contains("hand"))
            return MuscleGroup002.Hand;

		throw new System.NotImplementedException();
    }


	// Use this for initialization
	void Start () {
		Muscles = new List<Muscle002> ();
		var musicles = GetComponentsInChildren<ConfigurableJoint>();
		ConfigurableJoint rootConfigurableJoint = null;
		var ragDoll = GetComponent<RagDoll002>();
		foreach (var m in musicles)
		{
			var muscle = new Muscle002{
				Rigidbody = m.GetComponent<Rigidbody>(),
				Transform = m.GetComponent<Transform>(),
				ConfigurableJoint = m,
				Name = m.name,
				Group = FromName(m.name),
				MaximumForce = new Vector3(ragDoll.MusclePowers.First(x=>x.Muscle == m.name).Power,0,0)
			};
			if (muscle.Group == MuscleGroup002.Hips)
				rootConfigurableJoint = muscle.ConfigurableJoint;
			muscle.RootConfigurableJoint = rootConfigurableJoint;
			muscle.Init();

			Muscles.Add(muscle);			
		}
		_muscleAnimator = FindObjectOfType<StyleTransfer002Animator>();
		_brain = FindObjectOfType<Brain>();
	}
	
	// Update is called once per frame
	void Update () {

	}
	static float SumAbs(Vector3 vector)
	{
		var sum = Mathf.Abs(vector.x);
		sum += Mathf.Abs(vector.y);
		sum += Mathf.Abs(vector.z);
		return sum;
	}
	static float SumAbs(Quaternion q)
	{
		var sum = Mathf.Abs(q.w);
		sum += Mathf.Abs(q.x);
		sum += Mathf.Abs(q.y);
		sum += Mathf.Abs(q.z);
		return sum;
	}

	void FixedUpdate()
	{
		if (DebugMode)
			AnimationIndex = 0;
		var debugStepIdx = AnimationIndex;
		StyleTransfer002Animator.AnimationStep animStep = null;
		StyleTransfer002Animator.AnimationStep debugAnimStep = null;
		if (_phaseIsRunning) {
				debugStepIdx += DebugAnimOffset;
			if (DebugShowWithOffset){
				debugStepIdx = Mathf.Clamp(debugStepIdx, 0, _muscleAnimator.AnimationSteps.Count);
				debugAnimStep = _muscleAnimator.AnimationSteps[debugStepIdx];
			}
			animStep = _muscleAnimator.AnimationSteps[AnimationIndex];
		}
		PositionDistance = 0f;
		EndEffectorDistance = 0f;
		RotationDistance = 0f;
		VelocityDistance = 0f;
		if (_phaseIsRunning && DebugShowWithOffset)
			MimicAnimationFrame(debugAnimStep);
		else if (_phaseIsRunning)
			CompareAnimationFrame(animStep);
		foreach (var muscle in Muscles)
		{
			var i = Muscles.IndexOf(muscle);
			muscle.UpdateObservations();
			if (!DebugShowWithOffset && !DebugDisableMotor)
				muscle.UpdateMotor();
			if (!muscle.Rigidbody.useGravity)
				continue; // skip sub joints
			if (_phaseIsRunning){
				PositionDistance += muscle.ObsDeltaFromAnimationPosition.sqrMagnitude;
				if (muscle.Group == MuscleGroup002.Elbow // Hand is not a muscle
					|| muscle.Group == MuscleGroup002.Torso
					|| muscle.Group == MuscleGroup002.Foot)
					EndEffectorDistance += muscle.ObsDeltaFromAnimationPosition.sqrMagnitude;
				RotationDistance += Mathf.Abs(muscle.ObsAngleDeltaFromAnimationRotation)/360f;
			}
		}

		CenterOfMass = GetCenterOfMass();
		var velocity = CenterOfMass-_lastCenterOfMass;
		if (_fakeVelocity)
			velocity = animStep.Velocity;
		_lastCenterOfMass = CenterOfMass;
		if (!_resetCenterOfMassOnLastUpdate)
			_fakeVelocity = false;

		if (_phaseIsRunning){
			var animVelocity = animStep.Velocity / Time.fixedDeltaTime;
			velocity /= Time.fixedDeltaTime;
			var velocityDistance = velocity-animVelocity;
			VelocityDistance = velocityDistance.sqrMagnitude;
		}

		if (IgnorRewardUntilObservation)
			IgnorRewardUntilObservation = false;

		if (_phaseIsRunning){
			if (!DebugShowWithOffset)
				AnimationIndex++;
			if (AnimationIndex>=_muscleAnimator.AnimationSteps.Count) {
				//ResetPhase();
				Done();
				AnimationIndex--;
			}
			Phase = _muscleAnimator.AnimationSteps[AnimationIndex].NormalizedTime % 1f;
		}
	}
	void CompareAnimationFrame(StyleTransfer002Animator.AnimationStep animStep)
	{
		var animBones = GetComponentsInChildren<Rigidbody>()
			//.Where(x=>x.GetComponent<ConfigurableJoint>() != null)
			//.Select(x=>x.transform)
			.ToList();
		foreach (var bone in animBones)
		{
			if (bone.name.Contains("lower_waist"))
				animStep = animStep;
			var i = animStep.Names.IndexOf(bone.name);
			var muscle = Muscles.FirstOrDefault(x=>x.Name==bone.name);
			Vector3 animPosition = transform.parent.position + animStep.Positions[i];
			Quaternion animRotation = transform.parent.rotation * animStep.Rotaions[i];
			// Vector3 angularVelocity = animStep.RotaionVelocities[i].eulerAngles / Time.fixedDeltaTime;
			Vector3 angularVelocity = animStep.AngularVelocities[i] / Time.fixedDeltaTime;
			Vector3 velocity = animStep.Velocities[i] / Time.fixedDeltaTime;
			if (!bone.useGravity)
				velocity = angularVelocity = Vector3.zero;
			if (muscle != null) {
				muscle.SetAnimationPosition(
					transform.parent.position + animStep.Positions[i], 
					transform.parent.rotation * animStep.Rotaions[i]);
			}
		}
	}

	void MimicAnimationFrame(StyleTransfer002Animator.AnimationStep animStep)
	{
		var animBones = GetComponentsInChildren<Rigidbody>()
			//.Where(x=>x.GetComponent<ConfigurableJoint>() != null)
			//.Select(x=>x.transform)
			.ToList();
		foreach (var bone in animBones)
		{
			if (bone.name.Contains("lower_waist"))
				animStep = animStep;
			var i = animStep.Names.IndexOf(bone.name);
			var muscle = Muscles.FirstOrDefault(x=>x.Name==bone.name);
			Vector3 animPosition = transform.parent.position + animStep.Positions[i];
			Quaternion animRotation = transform.parent.rotation * animStep.Rotaions[i];
			// Vector3 angularVelocity = animStep.RotaionVelocities[i].eulerAngles / Time.fixedDeltaTime;
			Vector3 angularVelocity = animStep.AngularVelocities[i] / Time.fixedDeltaTime;
			Vector3 velocity = animStep.Velocities[i] / Time.fixedDeltaTime;
			if (!bone.useGravity)
				velocity = angularVelocity = Vector3.zero;
			// angularVelocity = Vector3.zero;
			// velocity = Vector3.zero;
			if (muscle == null) {
		        bone.position = animPosition;
        		bone.rotation = animRotation;
        		bone.GetComponent<Rigidbody>().angularVelocity = angularVelocity;
        		bone.GetComponent<Rigidbody>().velocity = velocity;
			} else {
				muscle.MoveToAnim(animPosition, animRotation, angularVelocity, velocity);
				muscle.SetAnimationPosition(
					transform.parent.position + animStep.Positions[i], 
					transform.parent.rotation * animStep.Rotaions[i]);
			}
		}
	}

	protected virtual void LateUpdate() {
		if (_resetCenterOfMassOnLastUpdate){
			CenterOfMass = GetCenterOfMass();
			_lastCenterOfMass = CenterOfMass;
			_resetCenterOfMassOnLastUpdate = false;
		}
		#if UNITY_EDITOR
			VisualizeTargetPose();
		#endif
	}

	public bool IsDone()
	{
		return _isDone;
	}
	void Done()
	{
		_isDone = true;
	}

	public void ResetPhase()
	{
		// _animationIndex =  UnityEngine.Random.Range(0, _muscleAnimator.AnimationSteps.Count);
		if (!_phaseIsRunning){
			StartAnimationIndex = _muscleAnimator.AnimationSteps.Count-1;
			// StartAnimationIndex = 0;
			// ErrorCutoff = .25f;
			// ErrorCutoff = -10f;
			EpisodeAnimationIndex = _muscleAnimator.AnimationSteps.Count-1;
			AnimationIndex = EpisodeAnimationIndex;
			if (CameraFollowMe){
				var camera = FindObjectOfType<Camera>();
				var follow = camera.GetComponent<SmoothFollow>();
				follow.target = CameraTarget;
			}
		}
		// ErrorCutoff = UnityEngine.Random.Range(-15f, 2f);
		// ErrorCutoff = UnityEngine.Random.Range(-10f, 1f);
		// ErrorCutoff = UnityEngine.Random.Range(-5f, 1f);
		ErrorCutoff = UnityEngine.Random.Range(-3f, .5f);
		if (_brain.brainType == BrainType.Internal)
			ErrorCutoff = UnityEngine.Random.Range(-3f, -3f);
		var lastLenght = AnimationIndex - EpisodeAnimationIndex;
		if (lastLenght >=  _muscleAnimator.AnimationSteps.Count-2){
			StartAnimationIndex = _muscleAnimator.AnimationSteps.Count-1;
			// StartAnimationIndex = 0;
			// ErrorCutoff += 0.25f;
			EpisodeAnimationIndex = _muscleAnimator.AnimationSteps.Count-1;
			AnimationIndex = EpisodeAnimationIndex;
		}

		if (UseRandomIndex) {
			int idx = UnityEngine.Random.Range(0, _muscleAnimator.AnimationSteps.Count);
			AnimationIndex = idx;
		} else {
			var minIdx = StartAnimationIndex;
			if (_muscleAnimator.IsLoopingAnimation)
				minIdx = minIdx == 0 ? 1 : minIdx;
			var maxIdx = _muscleAnimator.AnimationSteps.Count-1;
			var range = 30f;//maxIdx-minIdx;
			var rnd = (NextGaussian() /3f) * (float) range;
			var idx = Mathf.Clamp((float)minIdx + rnd, minIdx, (float)maxIdx);
			AnimationIndex = (int)idx;
		}
		// AnimationIndex = StartAnimationIndex;
		_phaseIsRunning = true;
		_isDone = false;
		var animStep = _muscleAnimator.AnimationSteps[AnimationIndex];
		TimeStep = animStep.TimeStep;
		PositionDistance = 0f;
		EndEffectorDistance = 0f;
		RotationDistance = 0f;
		VelocityDistance = 0f;
		IgnorRewardUntilObservation = true;
		_resetCenterOfMassOnLastUpdate = true;
		_fakeVelocity = true;
		foreach (var muscle in Muscles)
			muscle.Init();
		MimicAnimationFrame(animStep);
		EpisodeAnimationIndex = AnimationIndex;
	}

	Vector3 GetCenterOfMass()
	{
		var centerOfMass = Vector3.zero;
		float totalMass = 0f;
		foreach (Rigidbody rb in Muscles.Select(x=>x.Rigidbody))
		{
			centerOfMass += rb.worldCenterOfMass * rb.mass;
			totalMass += rb.mass;
		}
		centerOfMass /= totalMass;
		centerOfMass -= transform.position;
		return centerOfMass;
	}

	float NextGaussian(float mu = 0, float sigma = 1)
	{
		var u1 = UnityEngine.Random.value;
		var u2 = UnityEngine.Random.value;

		var rand_std_normal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
							Mathf.Sin(2.0f * Mathf.PI * u2);

		var rand_normal = mu + sigma * rand_std_normal;

		return rand_normal;
	}

	private void VisualizeTargetPose() {
		if (!visualizeAnimator) return;
		if (!Application.isEditor) return;

		// foreach (Muscle002 m in Muscles) {
		// 	if (m.ConfigurableJoint.connectedBody != null && m.connectedBodyTarget != null) {
		// 		Debug.DrawLine(m.target.position, m.connectedBodyTarget.position, Color.cyan);
				
		// 		bool isEndMuscle = true;
		// 		foreach (Muscle002 m2 in Muscles) {
		// 			if (m != m2 && m2.ConfigurableJoint.connectedBody == m.rigidbody) {
		// 				isEndMuscle = false;
		// 				break;
		// 			}
		// 		}
				
		// 		if (isEndMuscle) VisualizeHierarchy(m.target, Color.cyan);
		// 	}
		// }
	}
	
	// Recursively visualizes a bone hierarchy
	private void VisualizeHierarchy(Transform t, Color color) {
		for (int i = 0; i < t.childCount; i++) {
			Debug.DrawLine(t.position, t.GetChild(i).position, color);
			VisualizeHierarchy(t.GetChild(i), color);
		}
	}


}
