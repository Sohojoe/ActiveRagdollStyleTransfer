using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;

public class StyleTransfer001Master : MonoBehaviour {
	public float FixedDeltaTime = 0.005f;

	public bool visualizeAnimator = true;

	public List<Muscle001> Muscles;
	public float PositionDistance;
	public float EndEffectorDistance; // feet and hands
	public float RotationDistance;
	public float VelocityDistance;
	public Vector3 CenterOfMass;
	public bool IgnorRewardUntilObservation;
	public float ErrorCutoff;
	public bool DebugShowWithOffset;
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

	private StyleTransfer001Animator _muscleAnimator;
	private Brain _brain;
	bool _phaseIsRunning;
	Random _random = new Random();
	Vector3 _lastCenterOfMass;

	static MuscleGroup001 FromName(string name)
    {
        name = name.ToLower();
        if (name.Contains("joint_char"))
            return MuscleGroup001.Hips;
        if (name.Contains("joint_hip"))
            return MuscleGroup001.LegUpper;
        if (name.Contains("joint_knee"))
            return MuscleGroup001.LegLower;
        if (name.Contains("foot"))
            return MuscleGroup001.Foot;
        if (name.Contains("torso"))
            return MuscleGroup001.Spine;
        if (name.Contains("head"))
            return MuscleGroup001.Head;
        if (name.Contains("shoulder"))
            return MuscleGroup001.Shoulder;
		if (name.Contains("elbow"))
            return MuscleGroup001.Elbow;
		if (name.Contains("hand"))
            return MuscleGroup001.Hand;

		throw new System.NotImplementedException();
    }


	// Use this for initialization
	void Start () {
		Time.fixedDeltaTime = FixedDeltaTime;
		Muscles = new List<Muscle001> ();
		var musicles = GetComponentsInChildren<ConfigurableJoint>();
		foreach (var m in musicles)
		{
			var muscle = new Muscle001{
				Rigidbody = m.GetComponent<Rigidbody>(),
				Transform = m.GetComponent<Transform>(),
				ConfigurableJoint = m,
				Name = m.name,
				Group = FromName(m.name),
			};
			muscle.Init();

			Muscles.Add(muscle);			
		}
		_muscleAnimator = FindObjectOfType<StyleTransfer001Animator>();
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
		var debugStepIdx = AnimationIndex;
		StyleTransfer001Animator.AnimationStep animStep = null;
		StyleTransfer001Animator.AnimationStep debugAnimStep = null;
		if (_phaseIsRunning) {
			if (DebugShowWithOffset){
				debugStepIdx += DebugAnimOffset;
				debugStepIdx = Mathf.Clamp(debugStepIdx, 0, _muscleAnimator.AnimationSteps.Count);
				debugAnimStep = _muscleAnimator.AnimationSteps[debugStepIdx];
			}
			animStep = _muscleAnimator.AnimationSteps[AnimationIndex];
		}
		PositionDistance = 0f;
		EndEffectorDistance = 0f;
		RotationDistance = 0f;
		VelocityDistance = 0f;
		foreach (var muscle in Muscles)
		{
			var i = Muscles.IndexOf(muscle);
			// if(muscle.Parent == null)
			// 	continue;
			if (_phaseIsRunning){

				if (DebugShowWithOffset) {
					muscle.MoveToAnim(
						transform.parent.position + debugAnimStep.Positions[i], 
						transform.parent.rotation * debugAnimStep.Rotaions[i],
						debugAnimStep.RotaionVelocities[i].eulerAngles / Time.fixedDeltaTime,
						debugAnimStep.Velocities[i] / Time.fixedDeltaTime);
				}
				muscle.SetAnimationPosition(
					transform.parent.position + animStep.Positions[i], 
					transform.parent.rotation * animStep.Rotaions[i]);
			}
			muscle.UpdateObservations();
			if (!DebugShowWithOffset && !DebugDisableMotor)
				muscle.UpdateMotor();
			if (_phaseIsRunning){
				PositionDistance += muscle.ObsDeltaFromAnimationPosition.magnitude;
				if (muscle.Group == MuscleGroup001.Hand || muscle.Group == MuscleGroup001.Foot)
					EndEffectorDistance += muscle.ObsDeltaFromAnimationPosition.magnitude;
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
			VelocityDistance = velocityDistance.magnitude;
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
		{
			var i = Muscles.IndexOf(muscle);
			muscle.Init();
			muscle.MoveToAnim(
				transform.parent.position + animStep.Positions[i], 
				transform.parent.rotation * animStep.Rotaions[i],
				animStep.RotaionVelocities[i].eulerAngles / Time.fixedDeltaTime,
				animStep.Velocities[i] / Time.fixedDeltaTime);
			muscle.SetAnimationPosition(
				transform.parent.position + animStep.Positions[i], 
				transform.parent.rotation * animStep.Rotaions[i]);
		}
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

		// foreach (Muscle001 m in Muscles) {
		// 	if (m.ConfigurableJoint.connectedBody != null && m.connectedBodyTarget != null) {
		// 		Debug.DrawLine(m.target.position, m.connectedBodyTarget.position, Color.cyan);
				
		// 		bool isEndMuscle = true;
		// 		foreach (Muscle001 m2 in Muscles) {
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
