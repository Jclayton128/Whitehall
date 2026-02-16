using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileController : MonoBehaviour
{
    public static TileController Instance { get; private set; }


    //ref


    [SerializeField] Transform _tileHolder = null;
    [SerializeField] TileHandler _tilePrefab = null;

    //settings
    [SerializeField] int _arenaSize_X = 11;
    public int ArenaSize_X => _arenaSize_X;
    [SerializeField] int _arenaSize_Y = 9;
    public float ClickTweenTime = 0.75f;
    [SerializeField] float _yFactor = 1.0f;

    [SerializeField] float _tileGap_x = 0.5f;
    [SerializeField] float _tileGap_y = 0.5f;

    [SerializeField] TileLinkageHandler _tileLinkagePrefab = null;

    public Sprite OriginClue = null;
    public Sprite PassageClue = null;
    public Sprite JustSearchedClue = null;

    public Color Color_noMove = Color.black;
    public Color Color_pass = Color.blue;
    public Color Color_legalMove = Color.green;
    public Color Color_legalSearch = Color.green;
    public Color Color_highlight = Color.yellow;

    //state
    bool _isProcessingClick = false;
    [SerializeField] List<TileHandler> _tilesRaw = new List<TileHandler>();
    Dictionary<Vector2Int, TileHandler> _tilesLocation = new Dictionary<Vector2Int, TileHandler>();

    TileHandler _tileUnderCursor;
    Dictionary<int, TileLinkageHandler> _cantorIndexDictionary = new Dictionary<int, TileLinkageHandler>();
    public TileHandler FoxDestinationTile;

    #region Arena Setup

    private void Awake()
    {
        Instance = this;


    }

    public void BuildNewArena()
    {
        ConstructArena();
        LinkTilesLogically();

        TrimTilesLogically();

        LinkTilesGraphically();

        SetFoxDestinationTile();

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

        newTile.AssignIndexNumber(_tilesRaw.IndexOf(newTile));
    }

    private void LinkTilesLogically()
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

    private void TrimTilesLogically()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.RemoveRandomLinkedTiles();
        }
    }

    private void LinkTilesGraphically()
    {
        foreach (var tile in _tilesRaw)
        {
            foreach (var neighbor in tile.LinkedTiles)
            {
                int cantorIndex = ConvertIndicesIntoCantorIndex(tile.TileIndex, neighbor.TileIndex);
                //convert tile and neighbor index into a CantorIndex int;
                //Debug.Log("CantorIndex: " + cantorIndex);

                //Check cantorIndex if that CantorIndex int is already in the dictionary.
                if (_cantorIndexDictionary.ContainsKey(cantorIndex))
                {
                    //If yes, then continue
                    //This linkage must already be in the dictionary
                    
                    continue;
                }
                else
                {   
                    //if not, create a new tilelinkage with that CI int and add to dictionary

                    TileLinkageHandler newTileLinkage = Instantiate(_tileLinkagePrefab, _tileHolder);
                    newTileLinkage.SetTileLinkage(tile, neighbor, cantorIndex);
                    _cantorIndexDictionary.Add(cantorIndex, newTileLinkage);
                }
            }
        }
    }

    private int ConvertIndicesIntoCantorIndex(int index1, int index2)
    {

        if (index1 < 0 || index2 < 0)
        {
            throw new ArgumentException("Inputs must be non-negative integers.");
        }

        int x;
        int y;

        if (index1 > index2)
        {
            x = index1;
            y = index2;
        }
        else
        {
            x = index2;
            y = index1;
        }

        return ((x + y) * (x + y + 1)) / 2 + y;
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
    private void SetFoxDestinationTile()
    {
        //get an edge or next-to-edge tile

        List<TileHandler> possibleOptions = new List<TileHandler>();    

        foreach (var tile in _tilesRaw)
        {
            if (tile.IndexPos.x == 0)
            {
                possibleOptions.Add(tile);
            }
        }

        int rand = UnityEngine.Random.Range(0, possibleOptions.Count);
        FoxDestinationTile = possibleOptions[rand];
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

    #region Pathfinding

    public int GetDistanceToClosestAgent(TileHandler startingTile)
    {
        if (startingTile.Occupant && startingTile.Occupant.IsAgent)
        {
            return 0;
        }

        foreach (var tile in _tilesRaw)
        {
            tile.PreviousTile = null;
        }

        Stack<TileHandler> currentCheckPath = new Stack<TileHandler>();

        List<TileHandler> tilesChecked = new List<TileHandler>();
        Queue<TileHandler> tilesToCheck = new Queue<TileHandler>();


        tilesToCheck.Enqueue(startingTile);
        TileHandler tileBeingChecked = null;

        while (tilesToCheck.Count > 0)
        {

            tileBeingChecked = tilesToCheck.Dequeue();

            if (tileBeingChecked.Occupant && tileBeingChecked.Occupant.IsAgent)
            {
                //Debug.Log("found destination!");
                break;
            }

            tilesChecked.Add(tileBeingChecked);

            //if (isBlockedByAgents && tileBeingChecked.Occupant != null && tileBeingChecked.Occupant.IsAgent)
            if (false == true)
            {
                //Debug.Log($"blocked at {tileBeingChecked.TileIndex}");

            }
            else
            {
                foreach (var tile in tileBeingChecked.LinkedTiles)
                {
                    if (tilesChecked.Contains(tile) || tilesToCheck.Contains(tile))
                    {

                    }
                    //else if (isBlockedByAgents && tile.Occupant != null && tile.Occupant.IsAgent)
                    else if (false == true)
                    {
                        //Debug.Log($"blocked at {tile.TileIndex}");
                    }
                    else
                    {
                        tilesToCheck.Enqueue(tile);
                        //Debug.Log($"Enqueueing {tile.TileIndex} (child of {tileBeingChecked.TileIndex})");

                        if (tile.PreviousTile == null)
                        {
                            tile.PreviousTile = tileBeingChecked;
                        }

                    }

                }
            }


            //Debug.Log($"checked {tilesChecked.Count} tiles. {tilesToCheck.Count} in queue.");


            if (tilesChecked.Count > 500)
            {
                //Debug.Log("Break at 500!");
                break;
            }
        }

        currentCheckPath.Push(tileBeingChecked);
        TileHandler reverseWalker = null;

        int breaker = 40;
        while (reverseWalker != startingTile)
        {
            reverseWalker = currentCheckPath.Peek().PreviousTile;
            currentCheckPath.Push(reverseWalker);

            breaker--;
            if (breaker == 0)
            {
                //Debug.Log("path breaker");
                break;
            }
        }

        List<TileHandler> pathAsList = new List<TileHandler>(currentCheckPath);

        return pathAsList.Count - 1;
    }

    public int GetDistanceToDestination(TileHandler startingTile)
    {

        if (startingTile == FoxDestinationTile)
        {
            return 0;
        }

        foreach (var tile in _tilesRaw)
        {
            tile.PreviousTile = null;
        }

        Stack<TileHandler> currentCheckPath = new Stack<TileHandler>();

        List<TileHandler> tilesChecked = new List<TileHandler>();
        Queue<TileHandler> tilesToCheck = new Queue<TileHandler>();

        tilesToCheck.Enqueue(startingTile);

        TileHandler tileBeingChecked = null;

        while (tilesToCheck.Count > 0)
        {

            tileBeingChecked = tilesToCheck.Dequeue();

            if (tileBeingChecked == FoxDestinationTile)
            {
                break;
            }

            tilesChecked.Add(tileBeingChecked);

            //if (isBlockedByAgents && tileBeingChecked.Occupant != null && tileBeingChecked.Occupant.IsAgent)
            if (false == true)
            {
                //Debug.Log($"blocked at {tileBeingChecked.TileIndex}");

            }
            else
            {
                foreach (var tile in tileBeingChecked.LinkedTiles)
                {
                    if (tilesChecked.Contains(tile) || tilesToCheck.Contains(tile))
                    {

                    }
                    else
                    {
                        tilesToCheck.Enqueue(tile);

                        if (tile.PreviousTile == null)
                        {
                            tile.PreviousTile = tileBeingChecked;
                        }

                    }

                }
            }


            //Debug.Log($"checked {tilesChecked.Count} tiles. {tilesToCheck.Count} in queue.");


            if (tilesChecked.Count > 500)
            {
                //Debug.Log("Break at 500!");
                break;
            }
        }

        currentCheckPath.Push(tileBeingChecked);
        TileHandler reverseWalker = null;

        int breaker = 40;
        while (reverseWalker != startingTile)
        {
            reverseWalker = currentCheckPath.Peek().PreviousTile;
            currentCheckPath.Push(reverseWalker);

            breaker--;
            if (breaker == 0)
            {
                //Debug.Log("path breaker");
                break;
            }
        }

        List<TileHandler> pathAsList = new List<TileHandler>(currentCheckPath);

        return pathAsList.Count - 1;
    }

    public List<TileHandler> GetShortestPathToDestination(TileHandler startingTile, TileHandler destinationTile)
    {
        foreach (var tile in _tilesRaw)
        {
            tile.PreviousTile = null;
        }

        Stack<TileHandler> currentCheckPath = new Stack<TileHandler>();

        List<TileHandler> tilesChecked = new List<TileHandler>();
        Queue<TileHandler> tilesToCheck = new Queue<TileHandler>();

        tilesToCheck.Enqueue(startingTile);

        TileHandler tileBeingChecked;

        while (tilesToCheck.Count > 0)
        {
            
            tileBeingChecked = tilesToCheck.Dequeue();

            if (tileBeingChecked ==  destinationTile)
            {
                //Debug.Log("found destination!");
                break;
            }

            tilesChecked.Add(tileBeingChecked);

            //if (isBlockedByAgents && tileBeingChecked.Occupant != null && tileBeingChecked.Occupant.IsAgent)
            if (false == true)
            {
                //Debug.Log($"blocked at {tileBeingChecked.TileIndex}");

            }
            else
            {
                foreach (var tile in tileBeingChecked.LinkedTiles)
                {
                    if (tilesChecked.Contains(tile) || tilesToCheck.Contains(tile))
                    {

                    }
                    else
                    {
                        tilesToCheck.Enqueue(tile);

                        if (tile.PreviousTile == null)
                        {
                            tile.PreviousTile = tileBeingChecked;
                        }

                    }

                }
            }

               
            //Debug.Log($"checked {tilesChecked.Count} tiles. {tilesToCheck.Count} in queue.");
            

            if (tilesChecked.Count > 500)
            {
                //Debug.Log("Break at 500!");
                break;
            }
        }

        currentCheckPath.Push(destinationTile);
        TileHandler reverseWalker = null;

        int breaker = 40;
        while (reverseWalker != startingTile)
        {
            reverseWalker = currentCheckPath.Peek().PreviousTile;
            currentCheckPath.Push(reverseWalker);

            breaker--;
            if (breaker == 0)
            {
                //Debug.Log("path breaker");
                break;
            }
        }

        List<TileHandler> pathAsList = new List<TileHandler>(currentCheckPath);

        string pathRoute = "Winning path is through: ";
        for (int i = 0; i < pathAsList.Count; i++)
        {
            pathRoute += pathAsList[i].TileIndex + ", ";
        }
        Debug.Log(pathRoute);
        return pathAsList;
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

        ActorController.Instance.PriorityActor.ExecuteClickViaCurrentAction(_tileUnderCursor);

        //if (ActorController.Instance.PriorityActor.LegalMoves.Contains(_tileUnderCursor) ||
        //    ActorController.Instance.PriorityActor.CurrentTile == _tileUnderCursor)
        //{
        //    //UnraiseAllTiles();
        //    ActorController.Instance.PriorityActor.ExecuteClickViaCurrentAction(_tileUnderCursor);
        //    //ActorController.Instance.PriorityActor.SlideToNewTile(_tileUnderCursor);
        //}
    }

    public bool SearchForClue(TileHandler searchedLocation)
    {
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

    public void DeHighlightAllTiles()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.ColorTileToAbility(AgentData.AgentAbility.None);
        }
    }

    public void FindAllAgentDistances()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.FindAndPublishClosestAgentDistance();
        }
    }

    public void FindAllDestinationDistances()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.FindAndPublishDestinationDistance();
        }
    }

    #endregion
}
