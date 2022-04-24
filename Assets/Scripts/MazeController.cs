using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    [SerializeField] private TextAsset mazeFile;

    [SerializeField] private MazeTile mazeTilePrefab;
    [SerializeField] private MazeTile exitTilePrefab;
    [SerializeField] private MazeCharacter characterPrefab;
    [SerializeField] private Camera cam;

    public bool ShowDepthOverlay;

    [SerializeField] private bool _loadMazeFromFile = false;

    private Maze maze;
    private string mazeText;
    private List<MazeTile> tiles = new List<MazeTile>();
    public MazeCharacter Character { get; private set; }

    public static MazeController Instance { get; private set; }

	private void Awake()
	{
        Instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
        if(cam == null)
        {
            cam = Camera.main;
        }
        //LoadMaze(mazeFile.text);
    }

    public async void LoadMaze(string asciiText = "")
    {
        //re-load current maze if nothing passed in
        if (!string.IsNullOrEmpty(asciiText)) mazeText = asciiText;
        if(maze != null)
        {
            ClearMaze();
        }
        maze = await Maze.LoadMaze(mazeText);
        if (maze.Solved)
        {
            UIManager.Instance.ShowMenu(false);
            cam.transform.position = new Vector3((maze.SizeX - 1) / 2f, -(maze.SizeY - 1) / 2f, -10);
            //This is a rough way of sizing the cam to keep margins around the maze so it won't overlap UI elements. It's dirty, but works well enough :D
            cam.orthographicSize = Mathf.Max(maze.SizeY / 2f + Mathf.CeilToInt(maze.SizeY/ 10f), maze.SizeX / 2f + Mathf.CeilToInt(maze.SizeX / 10f));

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
                //we yeild after each row to keep it responsive when instantiating a lot of prefabs at once.
                // Makes a sort of natural loading indicator as you see the maze build
                await System.Threading.Tasks.Task.Yield();
            }
            MazeTile exitTile = Instantiate(exitTilePrefab, new Vector3(maze.SizeX, -maze.SizeY + 1, 0), Quaternion.identity, transform);
            tiles.Add(exitTile);

            Character = Instantiate(characterPrefab);
            Character.SetMaze(maze);
            Character.MoveToCell(maze.Entrance);
            UIManager.Instance.ShowHUD(true);
        }
        else
		{
            UIManager.Instance.ShowMenu(true);
            UIManager.Instance.ShowErrorBox(maze.Error);
            maze = null;
		}
        Debug.Log("Maze finished loading");
    }

    public void ClearMaze()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i].gameObject);
        }
        tiles.Clear();
        if (Character != null)
        {
            if(Character.IsSolving) Character.ToggleAutoSolve();
            Destroy(Character.gameObject);
        }
        maze = null;
    }

    private void Update()
    {
        if(_loadMazeFromFile)
        {
            LoadMaze(mazeFile.text);
            _loadMazeFromFile = false;
        }
        if (maze != null && tiles.Count > 0)
        {
            if (tiles[tiles.Count - 1].ShowDepth != ShowDepthOverlay)
            {
                foreach (MazeTile tile in tiles)
                {
                    tile.ShowDepth = ShowDepthOverlay;
                }
            }
        }
    }

    public async void MazeSolved()
	{
        if(maze != null && Character != null)
		{
            UIManager.Instance.ShowCongratsMessage();
            Character.IsControllable = false;
            await System.Threading.Tasks.Task.Delay(1000);
            Character.PlayAnimFrame("Win");
            Character.transform.position = tiles[tiles.Count - 1].transform.position + new Vector3(0.5f, 0.5f);
		}
	}
}
