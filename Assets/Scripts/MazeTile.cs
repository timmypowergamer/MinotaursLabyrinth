using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MazeTile : MonoBehaviour
{
    private Maze.MazeCell _cell;
    private int _maxDepth;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform[] wallSlots = new Transform[4];
    [SerializeField] private Gradient depthGradient;

    private bool _showDepth = false;
    public bool ShowDepth
    {
        get { return _showDepth; }
        set
        {
            _showDepth = value;
            OnShowDepthChanged();
        }
    }

    public void SetCell(Maze.MazeCell cell, int maxDepth)
    {
        _cell = cell;
        _maxDepth = maxDepth;
        for(int i = 0; i < 4; i++)
        {
            if(cell.Walls[i] == true)
            {
                Instantiate(wallPrefab, wallSlots[i], false);
            }
        }
    }

    private void OnShowDepthChanged()
    {
        if (_cell == null) return;
        if(_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if(ShowDepth)
        {
            _spriteRenderer.color = depthGradient.Evaluate((float)_cell.Depth / _maxDepth);
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }
    }
}
