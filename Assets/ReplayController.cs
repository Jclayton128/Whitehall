using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    public static ReplayController Instance { get; private set; }

    //settings

    [SerializeField] float _stepTweenTime = 0.25f;

    //state
    Queue<ReplayStep> _workingStepQueue = new Queue<ReplayStep>();
    List<ReplayStep> _masterStepList = new List<ReplayStep>();


    private void Awake()
    {
        Instance = this;

    }

    public void ResetReplayQueue()
    {
        _masterStepList.Clear();
    }

    public void AddStep(ReplayStep newStep)
    {
        _masterStepList.Add(newStep);
        //Debug.Log($"{newStep.Actor} added a step", newStep.Actor);
        //Debug.Log($"queue length: {_stepQueue.Count}. Step Type {newStep.StepType}");
    }

    public void BeginPlayback()
    {
        TileController.Instance.DeHighlightAllTiles();
        TileController.Instance.FoxDestinationTile.SetClue(TileHandler.ClueTypes.Origin);

        if (_masterStepList.Count > 0)
        {
            _workingStepQueue.Clear();
            _workingStepQueue = new Queue<ReplayStep>(_masterStepList);

            for (int i = 0; i < ActorController.Instance.ActorList.Count; i++)
            {
                ActorController.Instance.ActorList[i].transform.parent = ActorController.Instance.ActorList[i].StartingTile.VisualsTransform;
                ActorController.Instance.ActorList[i].transform.localPosition = Vector2.zero;
            }


            PlaybackNextReplayStep();
        }
    }

    private void PlaybackNextReplayStep()
    {
        if (_workingStepQueue.Count > 0)
        {
            ReplayStep currentStep = _workingStepQueue.Dequeue();

            ActorController.Instance.EnlargeActorPortait(currentStep.Actor);

            switch (currentStep.StepType)
            {
                case ReplayStep.StepTypes.Undefined:
                    Debug.Log("Step type undefined");
                    break;

                //case ReplayStep.StepTypes.Start:
                //    //teleport to target location
                //    Debug.Log("Step type start");
                //    currentStep.Actor.transform.parent = currentStep.TargetLocation.VisualsTransform;
                //    currentStep.Actor.transform.localPosition = Vector2.zero;

                //    break;


                case ReplayStep.StepTypes.Move:
                    //slide to target location
                    Debug.Log($"{currentStep.Actor.name} moving", currentStep.Actor);
                    //currentStep.Actor.transform.parent = null;
                    //currentStep.Actor.transform.position = currentStep.StartingTile.transform.position;
                    currentStep.Actor.SlideToNewTile(currentStep.TargetLocation, _stepTweenTime);
                    break;


                case ReplayStep.StepTypes.Search:
                    Debug.Log("Step type undefined");
                    break;

                case ReplayStep.StepTypes.Arrest:
                    Debug.Log("Step type undefined");
                    break;

            }

            Invoke(nameof(IncrementPlayback), _stepTweenTime);
        }
        else
        {
            GameController.Instance.EndRun_Neither();
            Debug.Log("Playback complete");
        }
    }

    private void IncrementPlayback()
    {
        PlaybackNextReplayStep();
    }
}
