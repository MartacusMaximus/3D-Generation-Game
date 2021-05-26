using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{

    public int columns = 25;
    public int rows = 25;
    public Dungeon[,] grid;
    public enum Dungeon 
    {
        empty = 0,
        floor = 1,
        wall = 2,
        path = 100,
        door = 200,
    }
    
    public class chamber
    {
        public chamber(int width, int height)
        {
            this.plan = new Dungeon[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i == 0 || i == width-1 || j== 0 || j == height - 1)
                    {
                        plan[i, j] =  Dungeon.wall;
                    }
                    else
                    {
                        plan[i, j] = Dungeon.floor;
                    }
                }
            }
        }
        public Dungeon[,] plan;        
    }



    // Start is called before the first frame update
    void Start()
    {
        grid = new Dungeon[columns, rows];
        ChamberInsert(new chamber(8, 8));
        ChamberInsert(new chamber(4, 5));
        ChamberInsert(new chamber(3, 3));
        ChamberInsert(new chamber(7, 5));
        ChamberInsert(new chamber(3, 6));
        ChamberInsert(new chamber(4, 7));
        ChamberInsert(new chamber(6, 9));
        ChamberInsert(new chamber(5, 4));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(3, 4));
        ChamberInsert(new chamber(5, 7));
        ChamberInsert(new chamber(7, 4));
        ChamberInsert(new chamber(8, 5));
        ChamberInsert(new chamber(4, 4));


        List<int[]> startingPoints = FindStartingPoints();
       

        while(startingPoints.Count > 0)
        {
            int[] startingPoint = startingPoints[Random.Range(0, startingPoints.Count)];
            CreatePath(startingPoint[0], startingPoint[1]);
            startingPoints = FindStartingPoints();
        }
        List < List<int[]> > doorGroups = DoorGroupFinder();
        foreach (List<int[]> group in doorGroups)
        {
            int[] chosenDoor = group[Random.Range(0, group.Count)];
            grid[chosenDoor[0], chosenDoor[1]] = Dungeon.door;
        }
        Print2DArray(grid);
    }

    bool ChamberCheck(chamber c, int x, int y)
    {
        Dungeon[,] plan = c.plan;
        for (int i = 0; i < plan.GetLength(0); i++)
        {
            for (int j = 0; j < plan.GetLength(1); j++)
            {
                if (i + x >= columns || j + y >= rows)
                {
                    return false;
                }
                Dungeon isChamber = plan[i, j];
                Dungeon isOccupied = grid[x + i, y + j];
                if (isChamber == Dungeon.floor && isOccupied == Dungeon.floor)
                {
                    return false;
                }
                if ((int)isChamber + (int)isOccupied == (int)Dungeon.floor + (int)Dungeon.wall) //Don't change Enums or this will break :^)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void ChamberPlace(chamber c, int x, int y)
    {
        Dungeon[,] plan = c.plan;
        for (int i = 0; i < plan.GetLength(0); i++)
        {
            for (int j = 0; j < plan.GetLength(1); j++)
            {
                grid[x + i, y + j] = plan[i, j];
            }
        }
    }

    bool ChamberInsert (chamber c)
    {
        for (int i = 0; i < 20; i++)
        {
            int x = Random.Range(0, columns);
            int y = Random.Range(0, rows);
            if (ChamberCheck(c,x,y))
            {
                ChamberPlace(c, x, y);
                return true;
            }
        }
        return false;
    }

    List<int[]> FindStartingPoints()
    {
        List<int[]> startingPoints = new List<int[]>();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (grid[i, j] == 0 && ValidStartingPoint(i,j))
                {
                    startingPoints.Add(new int[2] {i,j});
                }
            }
        }
        return startingPoints;
    }

    bool ValidStartingPoint(int x, int y)
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        if (!xP || !xN || !yP || !yN)
        {
            return false;
        }
        if (grid[x + 1, y] == Dungeon.path || grid[x - 1, y] == Dungeon.path || grid[x,y+1] == Dungeon.path || grid[x,y-1] == Dungeon.path)
        {
            return false;
        }
        return true;
    }

    void CreatePath(int x, int y)
    {
        grid[x, y] = Dungeon.path;
        int currentX = x;
        int currentY = y;
        List<int> beatenPath = new List<int>();
        for(int i = 0; i < 400; i++)
        {
            List <int> directions = GetRandomDirection(currentX, currentY);
            if (directions.Count != 0)
            {
                int dir = directions[Random.Range(0, directions.Count)];
                switch (dir)
                {
                    case 0:
                        beatenPath.Add(0);
                        currentX++;
                        break;
                    case 1:
                        beatenPath.Add(1);
                        currentX--;
                        break;
                    case 2:
                        beatenPath.Add(2);
                        currentY++;
                        break;
                    case 3:
                        beatenPath.Add(3);
                        currentY--;
                        break;
                }
                grid[currentX, currentY] =  Dungeon.path;

            }
            else
            {
                int bpLength = beatenPath.Count;
                if (bpLength == 0)
                {
                    break;
                }
                int dir = beatenPath[bpLength - 1];
                beatenPath.RemoveAt(bpLength - 1);
                switch (dir)
                {
                    case 0:
                        currentX--;
                        break;
                    case 1:
                        currentX++;
                        break;
                    case 2:
                        currentY--;
                        break;
                    case 3:
                        currentY++;
                        break;
                }
            }
        }       
    }

    List<int> GetRandomDirection(int x, int y)
    {
        List<int> directions = new List<int>();
        if (x < columns - 1 && grid [x + 1, y] ==  Dungeon.empty && !PathAdjacent(0,x,y))
        {
            directions.Add(0);
        }        
        if (x > 0 && grid [x - 1, y] ==  Dungeon.empty && !PathAdjacent(1, x, y))
        {
            directions.Add(1);
        }        
        if (y < rows - 1 && grid [x, y + 1] ==  Dungeon.empty && !PathAdjacent(2, x, y))
        {
            directions.Add(2);
        }        
        if (y > 0 && grid [x, y - 1] ==  Dungeon.empty && !PathAdjacent(3, x, y))
        {
            directions.Add(3);
        }
        return directions;
    }

    bool PathAdjacent (int direction, int x, int y)
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        bool xPP = x < columns - 2;
        bool xNN = x > 1;
        bool yPP = y < rows - 2;
        bool yNN = y > 1;
        switch (direction)
        {
            case 0:
                if (yP && grid[x+1, y+1] ==  Dungeon.path)
                {
                    return true;
                }
                if(yN && grid[x + 1, y -1] ==  Dungeon.path)
                {
                    return true;
                }
                if (xPP && grid[x + 2, y] ==  Dungeon.path)
                {
                    return true;
                }
                break;
            case 1:
                if (yP && grid[x - 1, y + 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (yN && grid[x - 1, y - 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (xNN && grid[x - 2, y] ==  Dungeon.path)
                {
                    return true;
                }
                break;
            case 2:
                if (xP && grid[x + 1, y + 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (xN && grid[x - 1, y + 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (yPP && grid[x, y + 2] ==  Dungeon.path)
                {
                    return true;
                }
                break;
            case 3:
                if (xP && grid[x + 1, y - 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (xN && grid[x - 1, y - 1] ==  Dungeon.path)
                {
                    return true;
                }
                if (yNN && grid[x, y - 2] ==  Dungeon.path)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    List<List<int[]>> DoorGroupFinder()
    {
        List<int[]> potentialDoors = new List<int[]>();
        List<List<int[]>> potentialDoorsGroup = new List<List<int[]>>();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (ValidDoor(i,j))
                {
                    potentialDoors.Add(new int[2] { i, j });
                }
            }
        }
        List<int[]> visited = new List<int[]>();
        
        foreach (int[] potentialDoor in potentialDoors)
        {
            if (!visited.Contains(potentialDoor))
            {
                List<int[]> newDoorGroup = new List<int[]>();
                newDoorGroup.Add(potentialDoor);
                List<int[]> newNeighdoors = GetNeighdoors(potentialDoor, potentialDoors, new List<int[]>());
                foreach (int[] gate in newNeighdoors)
                {
                    potentialDoors.Remove(gate);
                    visited.Add(gate);
                }
                potentialDoorsGroup.Add(newDoorGroup);
            }
        }
        return potentialDoorsGroup;
    }

    List<int[]> HasNeighdoor (int[] gate, List<int[]> potentialDoors)
    {
        int x = gate[0];
        int y = gate[1];
        List<int[]> newDoorGroup = new List<int[]>();
        foreach (int[] door in potentialDoors)
        {
            if (y == door[1])
            {
                if (x == door[0] + 1 )
                {
                    newDoorGroup.Add(new int[2] { x + 1, y });
                }
                if (x == door[0] - 1)
                {
                    newDoorGroup.Add(new int[2] { x - 1, y });

                }
            }
           
            if (x == door[0])
            {
                if (y == door[1] + 1)
                {
                    newDoorGroup.Add(new int[2] { x, y + 1 });
                }
                if (y == door[1] - 1)
                {
                    newDoorGroup.Add(new int[2] { x, y - 1 });

                }
            }          
        }
        return newDoorGroup;
    }

    List<int[]> GetNeighdoors (int[] gate, List<int[]> potentialDoors, List<int[]> visited)
    {
        List<int[]> groupDoors = new List<int[]>();
        visited.Add(gate);
        List<int[]> neighDoors = HasNeighdoor(gate, potentialDoors);
        foreach (int[] neighDoor in neighDoors)
        {
            if (!visited.Contains(neighDoor))
            {
                visited.Add(neighDoor);
                groupDoors.AddRange(GetNeighdoors(neighDoor, potentialDoors, visited));
            }
        }
        return new HashSet<int[]>(groupDoors).ToList() ;
    }


    bool ValidDoor(int x, int y)
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        if (grid[x,y] !=  Dungeon.wall)
        {
            return false;
        }
        if (!xP || !xN || !yP || !yN)
        {
            return false;
        }
        if (grid[x + 1, y] ==  Dungeon.path || grid[x - 1, y] ==  Dungeon.path || grid[x, y + 1] ==  Dungeon.path || grid[x, y - 1] ==  Dungeon.path)
        {
            return true;
        }
        if (grid[x + 1, y] ==  Dungeon.empty || grid[x - 1, y] ==  Dungeon.empty || grid[x, y + 1] ==   Dungeon.empty || grid[x, y - 1] ==  Dungeon.empty)
        {
            return false;
        }
        if (grid[x + 1, y] ==  Dungeon.floor && grid[x - 1, y] ==  Dungeon.floor || grid[x, y + 1] ==  Dungeon.floor && grid[x, y - 1] ==  Dungeon.floor)
        {
            return true;
        }
        return false;
    }


    public void Print2DArray(Dungeon[,] rawNodes)
    {
        int rowLength = rawNodes.GetLength(0);
        int colLength = rawNodes.GetLength(1);
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                string character = " ";
                if (rawNodes[i,j] ==  Dungeon.floor)
                {
                    character = "#";
                }  
                if (rawNodes[i,j] ==  Dungeon.wall)
                {
                    character = " ";
                }
                if (rawNodes[i, j] ==  Dungeon.path)
                {
                    character = "=";
                }
                if (rawNodes[i, j] ==  Dungeon.door)
                {
                    character = "0";
                }
                arrayString += character += " ";
            }
            arrayString += System.Environment.NewLine + System.Environment.NewLine;
        }

        Debug.Log(arrayString);
    }
}
