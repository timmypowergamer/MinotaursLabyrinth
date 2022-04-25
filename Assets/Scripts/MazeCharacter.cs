using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class MazeCharacter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [Tooltip("The move rate in tiles/second")]
    [SerializeField] private float moveRate;
    private Maze maze;
    private Vector2Int position = Vector2Int.zero;
    public bool IsSolving { get; private set; } = false;
    private string currentAnimation;
    private int animFrame;

    public bool IsControllable = true;
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
        if (animator == null) animator = GetComponentInChildren<Animator>();
        animator.enabled = false;
	}

	public void SetMaze(Maze maze)
	{
        this.maze = maze;
	}

    public async void SolveMaze()
    {
        try
        {
            if (maze == null)
            {
                Debug.LogError("No maze has been set on character yet!");
                return;
            }
            IsSolving = true;
            while (currentCell != null && currentCell != maze.Exit)
            {
                await Task.Delay(Mathf.RoundToInt(1 / moveRate * 1000));
                if (gameObject == null || !IsSolving) return;
                Maze.MazeCell nextCell = maze.GetBestExit(currentCell);
                if (nextCell != null)
                {
                    MoveToCell(nextCell);
                }

            }
        }
        catch (MissingReferenceException e)
		{
            //Admittedly, this is not a great practice.
            // It's to catch a specific case where the character is destroyed while the Delay task is still going.
            // But I'm not spending any more time right now on this...
            Debug.Log($"Exception of type '{e.GetType()}' was caught because Character was destroyed while solving maze");
        }
        IsSolving = false;
    }

    public void MoveToCell(Maze.MazeCell targetCell)
	{
        string animName = "";
        if (targetCell.Position.x > position.x) animName = "WalkRight";
        if (targetCell.Position.x < position.x) animName = "WalkLeft";
        if (targetCell.Position.y > position.y) animName = "WalkDown";
        if (targetCell.Position.y < position.y) animName = "WalkUp";

        Vector2 newWorldPos = new Vector2(targetCell.Position.x, -targetCell.Position.y);
        transform.position = newWorldPos;
        position = targetCell.Position;
        if(!string.IsNullOrEmpty(animName))
		{
            PlayAnimFrame(animName);
        }
        if (currentCell == maze.Exit) MazeController.Instance.MazeSolved();
    }

	private void Update()
	{
        if (maze != null && IsControllable)
        {
            //handle manual movement
            if (Input.GetButtonDown("Up"))
            {
                MoveDirection(0);
            }
            if (Input.GetButtonDown("Down"))
            {
                MoveDirection(2);
            }
            if (Input.GetButtonDown("Left"))
            {
                MoveDirection(3);
            }
            if (Input.GetButtonDown("Right"))
            {
                MoveDirection(1);
            }
            if(Input.GetButtonDown("ToggleAutoSolve"))
			{
                ToggleAutoSolve();
			}
        }
    }

    public void MoveDirection(int directionIndex)
	{
        if (!currentCell.Walls[directionIndex])
        {
            Maze.MazeCell cell = maze.GetCell(Maze.Cardinals[directionIndex] + position);
            if (cell != null) MoveToCell(cell);
        }
    }

    public void ToggleAutoSolve()
	{
        if (!IsSolving) SolveMaze();
        else IsSolving = false;
    }

    /// <summary>
    /// Play a single frame of a specified animation
    /// </summary>
    /// <param name="animationName"></param>
    public void PlayAnimFrame(string animationName)
	{
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if(currentAnimation != animationName)
		{
            currentAnimation = animationName;
            animFrame = 0;
		}
        animFrame++;
        animator.Play(animationName, -1, animFrame / 4f);
        animator.Update(0f);
        if (animFrame >= 4) animFrame = 0;
    }
}
