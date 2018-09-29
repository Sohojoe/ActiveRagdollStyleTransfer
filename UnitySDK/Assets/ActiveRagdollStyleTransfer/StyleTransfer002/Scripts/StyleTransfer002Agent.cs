using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class StyleTransfer002Agent : Agent, IOnSensorCollision, IOnTerrainCollision {

	public List<float> SensorIsInTouch;
	StyleTransfer002Master _master;
	StyleTransfer002Animator _styleAnimator;

	List<GameObject> _sensors;

	public bool ShowMonitor = false;

	static int _startCount;

	// Use this for initialization
	void Start () {
		_master = GetComponent<StyleTransfer002Master>();
		_styleAnimator = FindObjectOfType<StyleTransfer002Animator>();
		_startCount++;
	}
	
	// Update is called once per frame
	void Update () {
		if (agentParameters.onDemandDecision && _styleAnimator.AnimationStepsReady){
			agentParameters.onDemandDecision = false;
			_master.ResetPhase();
		}
	}

	override public void InitializeAgent()
	{

	}

	override public void CollectObservations()
	{
		AddVectorObs(_master.ObsPhase);

		// if (false){
		// 	// temp hack to support old models
		// 	if (SensorIsInTouch?.Count>0){
		// 		AddVectorObs(SensorIsInTouch[0]);
		// 		AddVectorObs(0f);
		// 		AddVectorObs(SensorIsInTouch[1]);
		// 		AddVectorObs(0f);
		// 	}
		// } else {
		// 	AddVectorObs(_master.ObsCenterOfMass);
		// 	AddVectorObs(_master.ObsVelocity);
		// 	AddVectorObs(SensorIsInTouch);	
		// }

		foreach (var bodyPart in _master.BodyParts)
		{
			AddVectorObs(bodyPart.ObsLocalPosition);
			AddVectorObs(bodyPart.ObsRotation);
			AddVectorObs(bodyPart.ObsRotationVelocity);
			AddVectorObs(bodyPart.ObsVelocity);
		}
		foreach (var muscle in _master.Muscles)
		{
			if (muscle.ConfigurableJoint.angularXMotion != ConfigurableJointMotion.Locked)
				AddVectorObs(muscle.TargetNormalizedRotationX);
			if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
				AddVectorObs(muscle.TargetNormalizedRotationY);
			if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
				AddVectorObs(muscle.TargetNormalizedRotationZ);
		}
		if (false){
			// temp hack to support old models
			if (SensorIsInTouch?.Count>0){
				AddVectorObs(SensorIsInTouch[0]);
				AddVectorObs(0f);
				AddVectorObs(SensorIsInTouch[1]);
				AddVectorObs(0f);
			}
		} else {
			AddVectorObs(_master.ObsCenterOfMass);
			AddVectorObs(_master.ObsVelocity);
			AddVectorObs(SensorIsInTouch);	
		}
	}

	public override void AgentAction(float[] vectorAction, string textAction)
	{
		int i = 0;
		foreach (var muscle in _master.Muscles)
		{
			// if(muscle.Parent == null)
			// 	continue;
			if (muscle.ConfigurableJoint.angularXMotion != ConfigurableJointMotion.Locked)
				muscle.TargetNormalizedRotationX = vectorAction[i++];
			if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
				muscle.TargetNormalizedRotationY = vectorAction[i++];
			if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
				muscle.TargetNormalizedRotationZ = vectorAction[i++];
		}
        var jointsAtLimitPenality = GetJointsAtLimitPenality() * 4;
        float effort = GetEffort();
        var effortPenality = 0.05f * (float)effort;
		
		var poseReward = 1f - _master.RotationDistance;
		var velocityReward = 1f - Mathf.Abs(_master.VelocityDistance);
		var endEffectorReward = 1f - _master.EndEffectorDistance;
		var feetPoseReward = 1f - _master.FeetRotationDistance;
		var centerMassReward = 1f - _master.CenterOfMassDistance; // TODO

		float poseRewardScale = .65f;
		float velocityRewardScale = .1f;
		float endEffectorRewardScale = .15f;
		float feetRewardScale = .15f;
		float centerMassRewardScale = .1f;

		poseReward = Mathf.Clamp(poseReward, -1f, 1f);
		velocityReward = Mathf.Clamp(velocityReward, -1f, 1f);
		endEffectorReward = Mathf.Clamp(endEffectorReward, -1f, 1f);
		centerMassReward = Mathf.Clamp(centerMassReward, -1f, 1f);
		feetRewardScale = Mathf.Clamp(feetRewardScale, -1f, 1f);

		float distanceReward = 
			(poseReward * poseRewardScale) +
			(velocityReward * velocityRewardScale) +
			(endEffectorReward * endEffectorRewardScale) +
			(feetPoseReward * feetRewardScale) +
			(centerMassReward * centerMassRewardScale);
		float reward = 
			distanceReward
			// - effortPenality +
			- jointsAtLimitPenality;

		// HACK _startCount used as Monitor does not like reset
        if (ShowMonitor && _startCount < 2) {
            var hist = new []{
                reward,
				distanceReward,
                - jointsAtLimitPenality, 
                // - effortPenality, 
				(poseReward * poseRewardScale),
				(velocityReward * velocityRewardScale),
				(endEffectorReward * endEffectorRewardScale),
				(feetPoseReward * feetRewardScale),
				(centerMassReward * centerMassRewardScale),
				}.ToList();
            Monitor.Log("rewardHist", hist.ToArray());
        }

		if (!_master.IgnorRewardUntilObservation)
			AddReward(reward);
		if (!IsDone()){
			// // if (distanceReward < _master.ErrorCutoff && !_master.DebugShowWithOffset) {
			// if (shouldTerminate && !_master.DebugShowWithOffset) {
			// 	AddReward(-10f);
			// 	Done();
			// 	// _master.StartAnimationIndex = _muscleAnimator.AnimationSteps.Count-1;
			// 	if (_master.StartAnimationIndex < _styleAnimator.AnimationSteps.Count-1)
			// 		_master.StartAnimationIndex++;
			// }
			if (_master.IsDone()){
				// AddReward(1f*(float)this.GetStepCount());
				// AddReward(10f);
				Done();
				// if (_master.StartAnimationIndex > 0 && distanceReward >= _master.ErrorCutoff)
				// if (_master.StartAnimationIndex > 0 && !shouldTerminate)
				if (_master.StartAnimationIndex > 0)
				 	_master.StartAnimationIndex--;
			}
		}
	}
	float GetEffort(string[] ignorJoints = null)
	{
		double effort = 0;
		foreach (var muscle in _master.Muscles)
		{
			if(muscle.Parent == null)
				continue;
			var name = muscle.Name;
			if (ignorJoints != null && ignorJoints.Contains(name))
				continue;
			var jointEffort = Mathf.Pow(Mathf.Abs(muscle.TargetNormalizedRotationX),2);
			effort += jointEffort;
			jointEffort = Mathf.Pow(Mathf.Abs(muscle.TargetNormalizedRotationY),2);
			effort += jointEffort;
			jointEffort = Mathf.Pow(Mathf.Abs(muscle.TargetNormalizedRotationZ),2);
			effort += jointEffort;
		}
		return (float)effort;
	}	
	float GetJointsAtLimitPenality(string[] ignorJoints = null)
	{
		int atLimitCount = 0;
		foreach (var muscle in _master.Muscles)
		{
			if(muscle.Parent == null)
				continue;

			var name = muscle.Name;
			if (ignorJoints != null && ignorJoints.Contains(name))
				continue;
			if (Mathf.Abs(muscle.TargetNormalizedRotationX) >= 1f)
				atLimitCount++;
			if (Mathf.Abs(muscle.TargetNormalizedRotationY) >= 1f)
				atLimitCount++;
			if (Mathf.Abs(muscle.TargetNormalizedRotationZ) >= 1f)
				atLimitCount++;
            }
            float penality = atLimitCount * 0.2f;
            return (float)penality;
        }

	// public override void AgentOnDone()
	// {

	// }

	public override void AgentReset()
	{
		_sensors = GetComponentsInChildren<SensorBehavior>()
			.Select(x=>x.gameObject)
			.ToList();
		SensorIsInTouch = Enumerable.Range(0,_sensors.Count).Select(x=>0f).ToList();
		if (!agentParameters.onDemandDecision)
			_master.ResetPhase();
	}
	public virtual void OnTerrainCollision(GameObject other, GameObject terrain)
	{
		if (string.Compare(terrain.name, "Terrain", true) != 0)
			return;
		if (!_styleAnimator.AnimationStepsReady)
			return;
		var bodyPart = _master.BodyParts.FirstOrDefault(x=>x.Transform.gameObject == other);
		if (bodyPart == null)
			return;
		switch (bodyPart.Group)
		{
			case BodyHelper002.BodyPartGroup.None:
			case BodyHelper002.BodyPartGroup.Foot:
			case BodyHelper002.BodyPartGroup.LegLower:
				break;
			default:
				AddReward(-100f);
				Done();
				break;
			// case BodyHelper002.BodyPartGroup.Hand:
			// 	// AddReward(-.5f);
			// 	Done();
			// 	break;
			// case BodyHelper002.BodyPartGroup.Head:
			// 	// AddReward(-2f);
			// 	Done();
			// 	break;
		}
	}


	public void OnSensorCollisionEnter(Collider sensorCollider, GameObject other) {
			if (string.Compare(other.name, "Terrain", true) !=0)
                return;
            var sensor = _sensors
                .FirstOrDefault(x=>x == sensorCollider.gameObject);
            if (sensor != null) {
                var idx = _sensors.IndexOf(sensor);
                SensorIsInTouch[idx] = 1f;
            }
		}
        public void OnSensorCollisionExit(Collider sensorCollider, GameObject other)
        {
            if (string.Compare(other.gameObject.name, "Terrain", true) !=0)
                return;
            var sensor = _sensors
                .FirstOrDefault(x=>x == sensorCollider.gameObject);
            if (sensor != null) {
                var idx = _sensors.IndexOf(sensor);
                SensorIsInTouch[idx] = 0f;
            }
        }  

}
