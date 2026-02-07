using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLinkageHandler : MonoBehaviour
{
    //ref
    [SerializeField] LineRenderer _lr = null;

    //state
    TileHandler _tile0;
    TileHandler _tile1;

    [SerializeField] int _cantorIndex = 0;

    public void SetTileLinkage(TileHandler tile0, TileHandler tile1, int cantorIndex)
    {
        _tile0 = tile0;
        _tile1 = tile1;

        _lr.positionCount = 2;
        _lr.SetPosition(0, _tile0.transform.position);
        _lr.SetPosition(1, _tile1.transform.position);

        _cantorIndex = cantorIndex;
    }
}
