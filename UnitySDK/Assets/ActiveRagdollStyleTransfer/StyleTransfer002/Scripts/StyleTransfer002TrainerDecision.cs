using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;

public class StyleTransfer002TrainerDecision : MonoBehaviour, Decision
{
    Brain _brain;

    public int Action;
    public int ActionCount;
    // public float[] ActionsOneHot;

    [Tooltip("Apply a random number to each action each framestep")]
    /**< \brief Apply a random number to each action each framestep*/
    public bool ApplyRandomActions;
    public float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        if (ApplyRandomActions)
        {
            Action = UnityEngine.Random.Range(0, ActionCount);
        }
        // SetOneHot();
        // return ActionsOneHot;
        return new float[]{(float)Action};
    }
    // void SetOneHot()
    // {
    //     ActionsOneHot = Enumerable.Repeat(0f, ActionCount).ToArray();
    //     ActionsOneHot[Action] = 1f;
    // }

    void Start()
    {
        _brain = GetComponent<Brain>();
        ActionCount = _brain.brainParameters.vectorActionSize[0];
        // SetOneHot();
    }

    public List<float> MakeMemory(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        return new List<float>();
    }
}