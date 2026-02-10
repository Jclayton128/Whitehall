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
    }
}
