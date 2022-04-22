using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Maze
{
    private MazeCell[,] CellGrid;

    public int SizeX { get; private set; }
    public int SizeY { get; private set; }
    public int MaxDepth { get; private set; }

    /// <summary>
    /// If the Maze was able to be solved
    /// </summary>
    public bool Solved { get; private set; }

    //These correspond to N, E, S, W, in that order.
    // North is 0,-1 because our coordinate system starts at the top
    private Vector2Int[] cardinals = new Vector2Int[] {
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.up,
        Vector2Int.left
    };

    public MazeCell Entrance { get { return CellGrid[0, 0]; } }
    public MazeCell Exit { get { return CellGrid[SizeX-1, SizeY-1]; } }

    #region Maze parsing and solving

    public class MazeCell
    {
        public bool[] Walls = new bool[4];
        public int Depth = int.MaxValue;
        public Vector2Int Position = Vector2Int.one * -1;

        public int NumExits
        {
            get
            {
                return Walls.Count(w => w == false);
            }
        }
    }

    public Maze(string[] asciiRows)
    {
        //Some basic sanity checks here.
        // The specs say that the maze should always be in a specific format, but doesn't hurt to verify some things.
        // still going to make a lot of assumptions about the internals of it...
        if((asciiRows[0].Length - 1) % 3 != 0 ||
            (asciiRows.Length - 1) % 2 != 0)
        {
            Debug.LogError("Maze is in invalid format and can not be parsed!");
            return;
        }

        //check if all the rows are the same length
        if(asciiRows.GroupBy(e => e.Length).ToList().Count > 1)
        {
            Debug.LogError("Not all lines in the maze are the same length. Maze can not be parsed!");
            return;
        }

        SizeX = (asciiRows[0].Length - 1) / 3;
        SizeY = (asciiRows.Length - 1) / 2;

        CellGrid = new MazeCell[SizeX, SizeY];

        //odd numbered rows are the actual "halls" of the maze. Even rows are always walls.
        // Can skip even looking at even rows. We'll figure out the walls by looking around each cell
        for (int rowIdx = 1; rowIdx < asciiRows.Length; rowIdx += 2)
        {
            //each cell is 2 spaces followed by either another space (open hallway) or a | (wall) can skip 3 characters at a time to jump between cells
            for(int columnIdx = 1; columnIdx < asciiRows[rowIdx].Length; columnIdx += 3)
            {
                //Generate a cell
                MazeCell cell = new MazeCell();
                cell.Position.x = (columnIdx - 1) / 3;
                cell.Position.y = (rowIdx - 1) / 2;

                //Check each cardinal direction for blank spaces. If there is anything but a ' ', we consider it a wall
                cell.Walls[0] = asciiRows[rowIdx - 1][columnIdx] != ' ';    //N
                cell.Walls[1] = asciiRows[rowIdx][columnIdx + 2] != ' ';    //E
                cell.Walls[2] = asciiRows[rowIdx + 1][columnIdx] != ' ';    //S
                cell.Walls[3] = asciiRows[rowIdx][columnIdx - 1] != ' ';    //W
                //add it to the grid
                CellGrid[cell.Position.x, cell.Position.y] = cell;
            }
        }

        //Start at the bottom-right and try to walk the grid to the top-left
        MazeCell startCell = Exit;
        startCell.Depth = 0;
        MaxDepth = 0;
        if (walkBranch(startCell))
        {
            Solved = true;
        }
        else
        {
            Debug.LogError("Maze could not be solved!");
        }
    }

    /// <summary>
    /// Tries to walk a branch of the maze backwards until it finds the start, or only dead ends.
    ///  Recurses on new branches and increases the depth value of each step on the way.
    ///  When finished, the depth value of a cell can be considered the minimum number
    ///  of steps to get back to the exit
    /// </summary>
    /// <param name="startCell">The cell to start in</param>
    /// <returns>True if the end was found</returns>
    private bool walkBranch(MazeCell startCell)
    {
        int depth = startCell.Depth;
        MazeCell currentCell = startCell;
        MazeCell nextCell = null;
        bool foundEntrance = false;
        while (currentCell != null)
        {
            if (currentCell == Entrance) foundEntrance = true;
            if (currentCell.NumExits <= 1) //dead end
            {
                break;
            }
            else if(currentCell.NumExits == 2 && currentCell != startCell) //hallway
            {
                nextCell = walkExits(currentCell);
                if(nextCell != null)
                {
                    depth++;
                    if (depth > MaxDepth) MaxDepth = depth;
                    nextCell.Depth = depth;
                    currentCell = nextCell;                    
                    continue;
                }
                break;
            }
            else //Junction
            {
                nextCell = walkExits(currentCell);
                while(nextCell != null)
                {
                    nextCell.Depth = depth + 1;
                    if (depth > MaxDepth) MaxDepth = depth;
                    if (walkBranch(nextCell))
                    {
                        foundEntrance = true;
                    }
                    nextCell = walkExits(currentCell);
                }
                currentCell = null;
            }
        }

        return foundEntrance;
    }

    /// <summary>
    /// Get the next exit to walk for the purposes of depth-mapping the maze
    /// </summary>
    /// <param name="cell">the current cell we are in</param>
    /// <returns>A cell with a depth > the current cell + 1</returns>
    private MazeCell walkExits(MazeCell cell)
    {
        Vector2Int nextPos = Vector2Int.zero;;
        for(int i = 0; i < cell.Walls.Length; i++)
        {
            if (!cell.Walls[i])
            {
                nextPos = cell.Position + cardinals[i];
                if(!isInGrid(nextPos.x, nextPos.y))
                {
                    //outside the bounds of the maze
                    //Either this is a entry/exit cell, or someone forgot to add a wall on the edge
                    continue;
                }
                if (CellGrid[nextPos.x, nextPos.y].Depth > cell.Depth + 1)  //only return exits with a worse depth than we would assign
                {
                    return CellGrid[nextPos.x, nextPos.y];
                }
            }
        }
        return null;
    }

    #endregion

    public static Maze LoadMaze(string asciiMaze)
    {
        string[] mazeRows = asciiMaze.Split(new char[] {'\n','\r'}, System.StringSplitOptions.RemoveEmptyEntries);

        Maze m = new Maze(mazeRows);
        return m;
    }

    public MazeCell GetCell(Vector2Int position)
    {
        return GetCell(position.x, position.y);
    }

    public MazeCell GetCell(int x, int y)
    {
        if(isInGrid(x, y))
            return CellGrid[x, y];
        return null;
    }

    private bool isInGrid(int x, int y)
    {
        return x >= 0 && x < SizeX &&
                y >= 0 && y < SizeY;
    }

    /// <summary>
    /// Gets the best path to take to reach the exit in as few steps as possible
    /// </summary>
    /// <param name="currentCell">The cell you are currently in</param>
    /// <returns>The next cell to travel to</returns>
    public MazeCell GetBestExit(MazeCell currentCell)
    {
        Vector2Int nextPos = Vector2Int.zero; ;
        for (int i = 0; i < currentCell.Walls.Length; i++)
        {
            if (!currentCell.Walls[i])
            {
                nextPos = currentCell.Position + cardinals[i];
                if (!isInGrid(nextPos.x, nextPos.y))
                {
                    //outside the bounds of the maze
                    //Either this is a entry/exit cell, or someone made a bad maze :P
                    continue;
                }
                if (CellGrid[nextPos.x, nextPos.y].Depth < currentCell.Depth)
                {
                    return CellGrid[nextPos.x, nextPos.y];
                }
            }
        }
        return null;
    }
}
