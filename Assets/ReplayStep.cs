using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplayStep
{
    public enum StepTypes { Undefined, Move, Search, Arrest}


    //state
    ActorHandler _actor;
    public ActorHandler Actor => _actor;
    StepTypes _stepType = StepTypes.Undefined;
    public StepTypes StepType => _stepType;
    TileHandler _targetLocation = null;
    public TileHandler TargetLocation => _targetLocation;
    TileHandler _startingTile = null;
    public TileHandler StartingTile => _startingTile;

    public ReplayStep (ActorHandler actor, StepTypes stepType, TileHandler startingTile, TileHandler targetLocation)
    {
        _actor = actor;
        _stepType = stepType;
        _startingTile = startingTile;
        _targetLocation = targetLocation;
    }
}
