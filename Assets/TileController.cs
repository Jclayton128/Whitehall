using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public static TileController Instance { get; private set; }


    //ref
    [SerializeField] Transform _tileHolder = null;
    [SerializeField] TileHandler _tilePrefab = null;

    //settings
    [SerializeField] int _arenaSize_X = 11;
    [SerializeField] int _arenaSize_Y = 9;
    public float ClickTweenTime = 0.75f;
    [SerializeField] float _yFactor = 1.0f;

    [SerializeField] float _tileGap_x = 0.5f;
    [SerializeField] float _tileGap_y = 0.5f;

    public Sprite OriginClue = null;
    public Sprite PassageClue = null;
    public Sprite JustSearchedClue = null;

    public Color Color_noMove = Color.black;
    public Color Color_knownOccupant = Color.blue;
    public Color Color_legalMove = Color.green;
    public Color Color_highlight = Color.yellow;

    //state
    bool _isProcessingClick = false;
    [SerializeField] List<TileHandler> _tilesRaw = new List<TileHandler>();
    Dictionary<Vector2Int, TileHandler> _tilesLocation = new Dictionary<Vector2Int, TileHandler>();
    TileHandler _tileUnderCursor;

    #region Arena Setup

    private void Awake()
    {
        Instance = this;


    }

    public void BuildNewArena()
    {
        ConstructArena();
        LinkTiles();
        RecenterTiles();
    }

 

    private void ConstructArena()
    {
        RemoveAllTiles();

        Vector2 walker = Vector2.zero;

        for (int ix = 0; ix < _arenaSize_X; ix++)
        {
            for (int iy = 0; iy < _arenaSize_Y; iy++)
            {
                walker.x = ix + (_tileGap_x * ix);
                walker.y = (iy * _yFactor) + (_tileGap_y * iy);
                CreateNewTile(walker, new Vector2Int(ix, iy));
            }
        }

        
    }

    private void CreateNewTile(Vector2 location, Vector2Int indexPos)
    {
        TileHandler newTile = Instantiate(_tilePrefab, _tileHolder);
        newTile.transform.position = location;
        newTile.AssignIndexPos(indexPos);

        _tilesRaw.Add(newTile);
        _tilesLocation.Add(indexPos, newTile);
    }

    private void LinkTiles()
    {
        foreach (var tileIndex in _tilesLocation.Keys)
        {
            var neighbors = GetNeighboringTiles(tileIndex);

            foreach (var neighbor in neighbors)
            {
                _tilesLocation[tileIndex].AddLinkedTile(neighbor);
            }
        }
    }

    private List<TileHandler> GetNeighboringTiles(Vector2Int originTile)
    {
        List<TileHandler> neighboringTiles = new List<TileHandler>();

        Vector2Int walker = Vector2Int.zero;

        for (int ix = -1; ix <= 1; ix++)
        {
            for (int iy = -1; iy <= 1; iy++)
            {
                walker.x = originTile.x + ix;
                walker.y = originTile.y + iy;

                if (_tilesLocation.ContainsKey(walker))
                {
                    neighboringTiles.Add(_tilesLocation[walker]);
                }

            }
        }

        return neighboringTiles;
    }

    private void RecenterTiles()
    {
        Vector2 newPos = Vector2.zero;
        newPos.x = -(((float)_arenaSize_X / 2f) + (3f * _tileGap_x));
        newPos.y = -((((float)_arenaSize_Y + 1f) / 2f) + (3f * _tileGap_y));
        _tileHolder.transform.position = newPos;
    }

    private void RemoveAllTiles()
    {
        if (_tilesRaw.Count > 0)
        {
            for (int i = _tilesRaw.Count - 1; i >= 0; i--)
            {
                _tilesRaw[i].RemoveTile();
            }
            _tilesRaw.Clear();
        }

    }




    #endregion

    #region Helpers
    public Vector2Int GetVec2Int(TileHandler tile)
    {
        Vector2Int origin = Vector2Int.zero;
        origin.x = Mathf.RoundToInt(tile.transform.localPosition.x);
        origin.y = Mathf.RoundToInt(tile.transform.localPosition.y / _yFactor);
        return origin;

    }

    public TileHandler GetTileAtVec2Int(Vector2Int vector2Int)
    {
        if (_tilesLocation.ContainsKey(vector2Int))
        {
            return _tilesLocation[vector2Int];
        }
        else
        {
            return null;
        }
    }

    public TileHandler GetTileAtWorldPosition(Vector2 worldPosition)
    {
        Vector2Int origin = Vector2Int.zero;
        origin.x = Mathf.RoundToInt(worldPosition.x);
        origin.y = Mathf.RoundToInt(worldPosition.y / _yFactor);

        if (_tilesLocation.ContainsKey(origin))
        {
            return _tilesLocation[origin];
        }
        else
        {
            Debug.Log($"No tiles found at {origin}");
            return null;
        }
    }
    public List<TileHandler> GetOrthogonalTiles(TileHandler originTile)
    {
        List<TileHandler> orthoTiles = new List<TileHandler>();

        Vector2Int origin = GetVec2Int(originTile);

        Vector2Int walker = origin;
        walker.x -= 1;
        if (_tilesLocation.ContainsKey(walker))
        {
            orthoTiles.Add(_tilesLocation[walker]);
        }

        walker = origin;
        walker.x += 1;
        if (_tilesLocation.ContainsKey(walker))
        {
            orthoTiles.Add(_tilesLocation[walker]);
        }

        walker = origin;
        walker.y -= 1;
        if (_tilesLocation.ContainsKey(walker))
        {
            orthoTiles.Add(_tilesLocation[walker]);
        }

        walker = origin;
        walker.y += 1;
        if (_tilesLocation.ContainsKey(walker))
        {
            orthoTiles.Add(_tilesLocation[walker]);
        }
        //Debug.Log($"clicked on {origin}, which has {orthoTiles.Count} neighbors");
        return orthoTiles;
    }
    #endregion

    #region Gameplay

    public void SetTileUnderCursor(TileHandler tileUnderCursor)
    {
        _tileUnderCursor = tileUnderCursor;
    }

    public void HandleTileClick_LMB()
    {
        if (_tileUnderCursor == null) return;

        if (ActorController.Instance.PriorityActor.LegalMoves.Contains(_tileUnderCursor) ||
            ActorController.Instance.PriorityActor.CurrentTile == _tileUnderCursor)
        {
            //UnraiseAllTiles();
            ActorController.Instance.PriorityActor.ExecuteClickViaCurrentAction(_tileUnderCursor);
            //ActorController.Instance.PriorityActor.SlideToNewTile(_tileUnderCursor);
        }
    }

    public bool SearchForClue(TileHandler searchedLocation)
    {
        Debug.Log("checking for clues");
        bool foundClue = searchedLocation.CheckRevealClue();
        return foundClue;
    }

    public void HandleTileClick_RMB()
    {
        if (_tileUnderCursor == null) return;

        if (ActorController.Instance.PriorityActor.LegalMoves.Contains(_tileUnderCursor))
        {
            //arrest via RMB?

        }
    }



    #endregion

    #region All Tile Effects

    public void DimAllTiles()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.DimTile();
        }
    }

    public void HalfdimOrthogonalTiles(TileHandler hoveredTile)
    {
        List<TileHandler> orthoTiles = GetOrthogonalTiles(hoveredTile);
        foreach (var tile in orthoTiles)
        {
            tile.HalfdimTile();
        }
    }

    public void UnraiseAllTiles()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.UnraiseTile();
        }
    }

    public void HalfraiseAllOrthogonalTiles(TileHandler originTile)
    {
        List<TileHandler> orthoTiles = GetOrthogonalTiles(originTile);
        foreach (var tile in orthoTiles)
        {
            tile.HalfraiseTile();
        }
    }

    #endregion
}
