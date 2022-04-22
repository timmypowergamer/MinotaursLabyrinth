using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MazeCharacter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [Tooltip("The move rate in tiles/second")]
    [SerializeField] private float moveRate;
    private Maze maze;
    private Vector2Int position = Vector2Int.zero;
    public Maze.MazeCell currentCell
    {
        get
        {
            if(maze != null)
            {
                return maze.GetCell(position);
            }
            return null;
        }
    }

    private void Start()
    {
        solveMaze();
    }

    private async void solveMaze()
    {
        while (currentCell != maze.Exit)
        {
            await Task.Delay(Mathf.RoundToInt(1 / moveRate * 1000));
            
        }
    }
}
