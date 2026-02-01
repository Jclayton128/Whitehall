using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            TileController.Instance.HandleTileClick_LMB();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TileController.Instance.HandleTileClick_RMB();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            GameController.Instance.StartRun();
        }
    }
}
