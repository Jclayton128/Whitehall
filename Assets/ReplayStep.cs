using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplayStep
{
    public enum StepTypes { Undefined, Move, Search, Arrest, TurnCountIncrement}


    //state
    ActorHandler _actor;
    public ActorHandler Actor => _actor;
    StepTypes _stepType = StepTypes.Undefined;
    public StepTypes StepType => _stepType;
    TileHandler _targetLocation = null;
    public TileHandler TargetLocation => _targetLocation;
    TileHandler _startingTile = null;
    public TileHandler StartingTile => _startingTile;
    int _payloadInt;
    public int PayloadInt => _payloadInt;

    public ReplayStep (ActorHandler actor, StepTypes stepType, TileHandler targetLocation, int payloadInt)
    {
        _actor = actor;
        _stepType = stepType;
        _targetLocation = targetLocation;
        _payloadInt = payloadInt;
    }
}
