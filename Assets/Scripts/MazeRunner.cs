using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRunner : MonoBehaviour
{
    [SerializeField] private TextAsset mazeFile;

    [SerializeField] private MazeTile mazeTilePrefab;
    [SerializeField] private Camera cam;

    [SerializeField] private bool showDepthOverlay;

    [SerializeField] private bool _loadMaze = false;

    private Maze maze;
    private List<MazeTile> tiles = new List<MazeTile>();

    // Start is called before the first frame update
    void Start()
    {
        if(cam == null)
        {
            cam = Camera.main;
        }
        LoadCurrentMaze();
    }

    public void LoadCurrentMaze()
    {
        if(maze != null)
        {
            ClearMaze();
        }
        maze = Maze.LoadMaze(mazeFile.text);
        if (maze.Solved)
        {
            for (int y = 0; y < maze.SizeY; y++)
            {
                for (int x = 0; x < maze.SizeX; x++)
                {
                    Maze.MazeCell cell = maze.GetCell(x, y);
                    MazeTile tile = Instantiate(mazeTilePrefab, new Vector3(x, -y, 0), Quaternion.identity, transform);
                    tile.name = $"Tile [{x},{y}]";
                    tile.SetCell(cell, maze.MaxDepth);
                    tiles.Add(tile);
                }
            }
            cam.transform.position = new Vector3((maze.SizeX - 1) / 2f, -(maze.SizeY - 1) / 2f, -10);
            cam.orthographicSize = maze.SizeY / 2f + 1;
        }
    }

    public void ClearMaze()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i].gameObject);
        }
        tiles.Clear();
        maze = null;
    }

    private void Update()
    {
        if(_loadMaze)
        {
            LoadCurrentMaze();
            _loadMaze = false;
        }
        if (maze != null && tiles.Count > 0)
        {
            if (tiles[0].ShowDepth != showDepthOverlay)
            {
                foreach (MazeTile tile in tiles)
                {
                    tile.ShowDepth = showDepthOverlay;
                }
            }
        }
    }
}
