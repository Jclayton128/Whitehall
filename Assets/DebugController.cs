using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    //state
    bool _isInDebugMode = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.X))
        {
            _isInDebugMode = !_isInDebugMode;

            if (_isInDebugMode)
            {
                ActorController.Instance.Enemy.ShowSprite();
            }
            else
            {
                ActorController.Instance.Enemy.HideSprite();
            }
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            Debug.Log("Beginning pathfinding from active actor to Fox");
            TileController.Instance.GetShortestPathToDestination(
                ActorController.Instance.PriorityActor.CurrentTile,
                ActorController.Instance.Enemy.CurrentTile);
        }
    }
}
