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


    public void BeginPlayback(float delayTime)
    {
        Invoke(nameof(BeginPlayback), delayTime);
    }

    public void BeginPlayback()
    {
        TileController.Instance.DeHighlightAllTiles();
        TileController.Instance.ClearClueStateFromAllTiles();

        TileController.Instance.EnemyStartTile.SetClue(TileHandler.ClueTypes.Origin);
        TileController.Instance.EnemyDestinationTile.SetClue(TileHandler.ClueTypes.Origin);
        ActorController.Instance.Enemy.ShowSprite();

        GameController.Instance.SetTurnIndication(1);

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

            if (currentStep.Actor == ActorController.Instance.Enemy)
            {
                //When the fox is back up, clear out all the Just Searched clues from the 3 agents' turns
                TileController.Instance.ClearActionTakensFromAllTiles();
            }

            ActorController.Instance.EnlargeActorPortait(currentStep.Actor);

            switch (currentStep.StepType)
            {
                case ReplayStep.StepTypes.Undefined:
                    Debug.Log("Step type undefined");
                    break;

                case ReplayStep.StepTypes.Move:
                    //slide to target location
                    currentStep.Actor.SlideToNewTile_Replay(currentStep.TargetLocation, _stepTweenTime);
                    break;


                case ReplayStep.StepTypes.Search:
                    TileController.Instance.SearchForClue(currentStep.TargetLocation);
                    currentStep.TargetLocation.SetActionTaken(TileHandler.ActionTypes.Searched);
                    break;

                case ReplayStep.StepTypes.Arrest:
                    currentStep.TargetLocation.SetActionTaken(TileHandler.ActionTypes.Arrested);
                    break;

                case ReplayStep.StepTypes.TurnCountIncrement:
                    GameController.Instance.SetTurnIndication(currentStep.PayloadInt);
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
