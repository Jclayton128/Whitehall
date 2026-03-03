using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public static DebugController Instance {  get; private set; }

    //state
    bool _isInDebugMode = false;
    public bool IsInDebugMode => _isInDebugMode;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.X))
        {
            _isInDebugMode = !_isInDebugMode;

            if (_isInDebugMode)
            {
                TileController.Instance.ShowAllTileValues();
            }
            else
            {
                TileController.Instance.HideAllTileValues();
                ActorController.Instance.Enemy.HideSprite();
            }
        }
        if (Input.GetKeyUp(KeyCode.Z) && (_isInDebugMode))
        {
            ActorController.Instance.Enemy.ShowSprite();
        }
        if (Input.GetKeyUp(KeyCode.R) && (_isInDebugMode))
        {
            TileController.Instance.HideAllTileValues();
            ActorController.Instance.Enemy.ShowSprite();
            ReplayController.Instance.BeginPlayback();
        }

        //if (Input.GetKeyUp(KeyCode.Z))
        //{
        //    Debug.Log("Beginning pathfinding from active actor to Fox");
        //    TileController.Instance.GetShortestPathToDestination(
        //        ActorController.Instance.PriorityActor.CurrentTile,
        //        ActorController.Instance.Enemy.CurrentTile, fal);
        //}
    }
}
