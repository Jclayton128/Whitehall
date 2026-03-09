using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    public static TreeController Instance {  get; private set; }

    //settings

    [SerializeField] TreeHandler _treePrefab = null;

    //state
    System.Random _rnd;

    [SerializeField] float _yTreeFactor = 1.0f;
    [SerializeField] float _yTreeOffset = 0.2f;
    [SerializeField] float _perlinZoom = 0.2f;

    public float YTreeFactor => _yTreeFactor;
    public float YTreeOffset => _yTreeOffset;


    [SerializeField] float _minTreeSpacing = 0.25f;
    [SerializeField] float _forestEdgeBuffer = 1.0f;
    [SerializeField] float _plantingJitter = 0.125f;

    [SerializeField] List<TreeHandler> _plantedTrees = new List<TreeHandler>();

    private void Awake()
    {
        Instance = this;
        _rnd = new System.Random();
    }

    public void GenerateTrees()
    {
        if (_plantedTrees.Count > 0)
        {
            for (int i = _plantedTrees.Count - 1; i >= 0; i--)
            {
                Destroy(_plantedTrees[i].gameObject);
            }
            _plantedTrees.Clear();
        }



        float xRand = (float)_rnd.NextDouble();
        float yRand = (float)_rnd.NextDouble();

        //walk through the x-y bounds
        // at each x-y coordinate, evaluate the perlin noise there. 
        // assign a tree level based on perlin noise.

        Vector2 walkingPoint = Vector2.zero;
        for (float i = 0 - _forestEdgeBuffer; i < TileController.Instance.XBound_Arena + _forestEdgeBuffer; i += _minTreeSpacing)
        {
            for (float j = 0 - _forestEdgeBuffer;  j < TileController.Instance.YBound_Arena + _forestEdgeBuffer; j += _minTreeSpacing)
            {
                walkingPoint.x = i;
                walkingPoint.y = j;

                float xFactor = ((walkingPoint.x ) / TileController.Instance.XBound_Arena) + xRand;
                float yFactor = ((walkingPoint.y ) / TileController.Instance.YBound_Arena) + yRand;

                Vector2 jitteryWalkingPoint = walkingPoint + (UnityEngine.Random.insideUnitCircle * _plantingJitter);
                float rawTree = Mathf.PerlinNoise(xFactor * _perlinZoom, yFactor * _perlinZoom);
                int treeCount = Mathf.FloorToInt(Mathf.Lerp(0, 5, rawTree));

                var hit = Physics2D.OverlapPoint(jitteryWalkingPoint,  1 << 6, -99f, 99f);
                if (hit != null)
                {
                    SpawnTree(jitteryWalkingPoint, 1);
                }
                else
                {
                    if (treeCount == 1)
                    {
                        SpawnTree(jitteryWalkingPoint, 1);
                    }
                    else
                    {
                        SpawnTree(jitteryWalkingPoint, treeCount);
                    }

                }

            }
        }


    }

    public void SpawnTree(Vector2 location, int treeLevel)
    {
        var tree = Instantiate(_treePrefab, TileController.Instance.TileHolder);

        tree.transform.localPosition = location;
        tree.SetupTree(treeLevel);
        _plantedTrees.Add(tree);        
    }
}
