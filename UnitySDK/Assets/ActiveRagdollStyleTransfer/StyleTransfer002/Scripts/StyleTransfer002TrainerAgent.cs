using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class StyleTransfer002TrainerAgent : Agent
{
    int _startIdx;
    int _totalAnimFrames;
	StyleTransfer002Master _master;
	private Brain _brain;

    void Start () {
		_master = GetComponent<StyleTransfer002Master>();
		_brain = FindObjectsOfType<Brain>().First(x=>x.name=="TrainerBrain");
	}

    void Update () {
    }

    public void RequestDecision(float averageReward)
    {
        var reward = 1f - averageReward;
        AddReward(reward);
        RequestDecision();
    }

    public void SetBrainParams(int totalAnimFrames)
    {
        _totalAnimFrames = totalAnimFrames;
        _brain.brainParameters.vectorObservationSize = totalAnimFrames;
    }

    override public void InitializeAgent()
	{

	}

    override public void CollectObservations()
	{
        AddVectorObs(_startIdx, _totalAnimFrames);
    }
    
    public override void AgentAction(float[] vectorAction, string textAction)
	{
        int action = (int)vectorAction[0];
        _startIdx = action;
        _master.SetStartIndex(_startIdx);
    }

}