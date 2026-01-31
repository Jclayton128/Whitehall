using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public static TileController Instance { get; private set; }
    public Action TileRemappingComplete;


    //ref
    [SerializeField] Transform _tileHolder = null;
    [SerializeField] TileHandler _tilePrefab = null;

    //settings
    [SerializeField] int _arenaSize = 7;
    public float ClickTweenTime = 0.75f;
    [SerializeField] float _yFactor = 1.0f;

    [SerializeField] float _tileGap_x = 0.5f;
    [SerializeField] float _tileGap_y = 0.5f;

    //state
    bool _isProcessingClick = false;
    [SerializeField] List<TileHandler> _tilesRaw = new List<TileHandler>();
    Dictionary<Vector2Int, TileHandler> _tilesLocation = new Dictionary<Vector2Int, TileHandler>();

    #region Arena Setup

    private void Awake()
    {
        Instance = this;
        BuildNewArena();

    }

    public void BuildNewArena()
    {
        ConstructArena();
        RemapTileLocations();
    }


    private void ConstructArena()
    {
        RemoveAllTiles();

        Vector2 walker = Vector2.zero;

        for (int ix = 0; ix < _arenaSize; ix++)
        {
            for (int iy = 0; iy < _arenaSize; iy++)
            {
                walker.x = ix + (_tileGap_x * ix);
                walker.y = (iy * _yFactor) + (_tileGap_y * iy);
                CreateNewTile(walker);
            }
        }

        
    }

    private void CreateNewTile(Vector2 location)
    {
        TileHandler newTile = Instantiate(_tilePrefab, _tileHolder);
        newTile.transform.position = location;

        _tilesRaw.Add(newTile);
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

    private void RemapTileLocations()
    {
        _tilesLocation.Clear();
        foreach (var tile in _tilesRaw)
        {
            _tilesLocation.Add(GetVec2Int(tile), tile);
        }
        TileRemappingComplete?.Invoke();

    }



    #endregion

    #region Helpers
    public Vector2Int GetVec2Int(TileHandler tile)
    {
        Vector2Int origin = Vector2Int.zero;
        origin.x = Mathf.RoundToInt(tile.transform.position.x);
        origin.y = Mathf.RoundToInt(tile.transform.position.y / _yFactor);
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
    TileHandler _clickedTile;
    List<TileHandler> _orthoTiles = new List<TileHandler>();
    List<TileHandler> _consumableOrthoTiles = new List<TileHandler>();

    public void HandleTileClick(TileHandler clickedTile)
    {
        
    }

    

    public void DeregisterTile(TileHandler tile)
    {
        _tilesRaw.Remove(tile);
    }

   

    private void HandleClickProcessingComplete()
    {
        _isProcessingClick = false;
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
