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
	StyleTransfer002Agent _agent;
	private Brain _brain;
    int _decisions = 0;
    void Start () {
		_agent = GetComponent<StyleTransfer002Agent>();
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
        _decisions++;
        if (_decisions > 10){
            Done();
            _decisions = 0;
        }
    }
    public override void AgentReset()
	{
        _decisions = 0;
    }

    public void SetBrainParams(int totalAnimFrames)
    {
        _totalAnimFrames = totalAnimFrames / _agent.agentParameters.numberOfActionsBetweenDecisions;
        _brain.brainParameters.vectorObservationSize = _totalAnimFrames;
    }

    override public void InitializeAgent()
	{

	}

    override public void CollectObservations()
	{
        var len = _totalAnimFrames;
        if (len == 0)
            len = _brain.brainParameters.vectorObservationSize / _agent.agentParameters.numberOfActionsBetweenDecisions;
        AddVectorObs(_startIdx, len);
    }
    
    public override void AgentAction(float[] vectorAction, string textAction)
	{
        int action = (int)vectorAction[0];
        _startIdx = action;
        _master.SetStartIndex(_startIdx);
    }

}