using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{

    public int columns = 25;
    public int rows = 25;
    [Range(0, 100)]
    public int enemySpawnChance;
    [Range(101, 200)]
    public int trapSpawnChance;
    [Range(201, 300)]
    public int treasureSpawnChance;
    public Dungeon[,] grid;
    public List<GameObject> roomList;
    public List<GameObject> enemyList;
    public List<GameObject> trapList;
    public List<GameObject> treasureList;


    public enum Dungeon //Voor de numberbased generation. Sinds we calculations doen met deze getallen heb ik niet de Path 3 gemaakt sinds dat voor overlap zou zorgen
    {
        empty = 0,
        floor = 1,
        wall = 2,
        path = 100,
        door = 200,
    }



    public class chamber
    {
        public chamber(int width, int height) //Hoe ziet een kamer eruit?
        {
            this.plan = new Dungeon[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                    {
                        plan[i, j] = Dungeon.wall;
                    }
                    else
                    {
                        plan[i, j] = Dungeon.floor;
                    }
                }
            }

        }
        public chamber(int r) //Als je een circulaire room wilt (Ik Wilde de Bossroom rond maken, alleen dat  zorgde voor conflicten met de door Placement)
        {
            int c = r * r;
            this.plan = new Dungeon[2*r, 2*r];
            for (int i = 0; i < 2*r; i++)
            {
                for (int j = 0; j < 2*r; j++)
                {
                    if (Mathf.Abs((Mathf.Pow(i-r,2) + Mathf.Pow(j-r,2)) - c) < 0.6)
                    {
                        plan[i, j] = Dungeon.wall;
                    }

                }
            }
            for (int i = 0; i < 2 * r; i++)
            {
                for (int j = 0; j < 2 * r; j++)
                {
                    if (Mathf.Pow(i - r, 2) + Mathf.Pow(j - r, 2) < c && plan[i,j] != Dungeon.wall)
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
        grid = new Dungeon[columns, rows]; //Verschillende types kamers die hij over een vaste grid plaatst (Kamer size en Grid Size kunnen veranderen)
        ChamberInsert(new chamber(8, 8));
        ChamberInsert(new chamber(4, 5));
        ChamberInsert(new chamber(3, 3));
        ChamberInsert(new chamber(7, 5));
        ChamberInsert(new chamber(3, 6));
        ChamberInsert(new chamber(4, 7));
        ChamberInsert(new chamber(6, 9));
        ChamberInsert(new chamber(5, 4));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(4, 6));
        ChamberInsert(new chamber(3, 4));
        ChamberInsert(new chamber(5, 7));
        ChamberInsert(new chamber(7, 4));
        ChamberInsert(new chamber(8, 5));
        ChamberInsert(new chamber(4, 4));
        ChamberInsert(new chamber(4, 4));
        ChamberInsert(new chamber(4, 4));
        ChamberInsert(new chamber(4, 4));
        ChamberInsert(new chamber(4, 4));



        List<int[]> startingPoints = FindStartingPoints(); // Kies een allowed starting Point, Maak vanuit daar Paths die gaan sliertend stoppend bij Chambers
        while (startingPoints.Count > 0)
        {
            int[] startingPoint = startingPoints[Random.Range(0, startingPoints.Count)];
            CreatePath(startingPoint[0], startingPoint[1]);
            startingPoints = FindStartingPoints();
        }
        List<List<int[]>> doorGroups = DoorGroupFinder(); //Kies uit een lijst van deuren een Valid Door. Nu plaatst ie er 2 (BUG)
        foreach (List<int[]> group in doorGroups)
        {
            int[] chosenDoor = group[Random.Range(0, group.Count)];
            grid[chosenDoor[0], chosenDoor[1]] = Dungeon.door;
        }
        RemovingDeadEndsAndCleaningUp();
        SpawnPoint();
        Print2DArray(grid);
        Print3DArray(grid);
    }

    bool ChamberCheck(chamber c, int x, int y) // Kies Valid Chambers
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

    bool ChamberInsert(chamber c) // Plaats de Valid Doors
    {
        for (int i = 0; i < 500; i++)
        {
            int x = Random.Range(0, columns);
            int y = Random.Range(0, rows);
            if (ChamberCheck(c, x, y))
            {
                ChamberPlace(c, x, y);
                return true;
            }
        }
        return false;
    }

    List<int[]> FindStartingPoints() // Starting Points voor Paths
    {
        List<int[]> startingPoints = new List<int[]>();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (grid[i, j] == 0 && ValidStartingPoint(i, j))
                {
                    startingPoints.Add(new int[2] { i, j });
                }
            }
        }
        return startingPoints;
    }

    bool ValidStartingPoint(int x, int y) //Een Starting Point mag natuurlijk niet in een ander ding bevinden
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        if (!xP || !xN || !yP || !yN)
        {
            return false;
        }
        if (grid[x + 1, y] == Dungeon.path || grid[x - 1, y] == Dungeon.path || grid[x, y + 1] == Dungeon.path || grid[x, y - 1] == Dungeon.path)
        {
            return false;
        }
        return true;
    }

    void CreatePath(int x, int y) //Path gaat basically drunkards Walk doen totdat ie vastloopt, dan gaat ie terug naar de vorige valid Path totdat alle paths geplaatst zijn
    {
        grid[x, y] = Dungeon.path;
        int currentX = x;
        int currentY = y;
        List<int> beatenPath = new List<int>();
        for (int i = 0; i < 400; i++)
        {
            List<int> directions = GetRandomDirection(currentX, currentY);
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
                grid[currentX, currentY] = Dungeon.path;

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

    List<int> GetRandomDirection(int x, int y) //We willen natuurlijk niet dat de paden in elkaar gaan loopen, en door Muren heen gaan
    {
        List<int> directions = new List<int>();
        if (x < columns - 1 && grid[x + 1, y] == Dungeon.empty && !PathAdjacent(0, x, y))
        {
            directions.Add(0);
        }
        if (x > 0 && grid[x - 1, y] == Dungeon.empty && !PathAdjacent(1, x, y))
        {
            directions.Add(1);
        }
        if (y < rows - 1 && grid[x, y + 1] == Dungeon.empty && !PathAdjacent(2, x, y))
        {
            directions.Add(2);
        }
        if (y > 0 && grid[x, y - 1] == Dungeon.empty && !PathAdjacent(3, x, y))
        {
            directions.Add(3);
        }
        return directions;
    }

    bool PathAdjacent(int direction, int x, int y) // Zijn er Paden naast?
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
                if (yP && grid[x + 1, y + 1] == Dungeon.path)
                {
                    return true;
                }
                if (yN && grid[x + 1, y - 1] == Dungeon.path)
                {
                    return true;
                }
                if (xPP && grid[x + 2, y] == Dungeon.path)
                {
                    return true;
                }
                break;
            case 1:
                if (yP && grid[x - 1, y + 1] == Dungeon.path)
                {
                    return true;
                }
                if (yN && grid[x - 1, y - 1] == Dungeon.path)
                {
                    return true;
                }
                if (xNN && grid[x - 2, y] == Dungeon.path)
                {
                    return true;
                }
                break;
            case 2:
                if (xP && grid[x + 1, y + 1] == Dungeon.path)
                {
                    return true;
                }
                if (xN && grid[x - 1, y + 1] == Dungeon.path)
                {
                    return true;
                }
                if (yPP && grid[x, y + 2] == Dungeon.path)
                {
                    return true;
                }
                break;
            case 3:
                if (xP && grid[x + 1, y - 1] == Dungeon.path)
                {
                    return true;
                }
                if (xN && grid[x - 1, y - 1] == Dungeon.path)
                {
                    return true;
                }
                if (yNN && grid[x, y - 2] == Dungeon.path)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    List<List<int[]>> DoorGroupFinder() //We kijken naar alle potentiele deuren
    {
        List<int[]> potentialDoors = new List<int[]>();
        List<List<int[]>> potentialDoorsGroup = new List<List<int[]>>();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (ValidDoor(i, j))
                {
                    potentialDoors.Add(new int[2] { i, j });
                }
            }
        }
        List<int[]> visited = new List<int[]>();

        foreach (int[] potentialDoor in potentialDoors)
        {
            if (!visited.Contains(potentialDoor, new DuoIntArrComparer()))
            {
                List<int[]> newDoorGroup = new List<int[]>();
                newDoorGroup.Add(potentialDoor);
                List<int[]> newNeighdoors = GetNeighdoors(potentialDoor, potentialDoors, new List<int[]>());
                foreach (int[] gate in newNeighdoors)
                {
                    visited.Add(gate);
                }
                potentialDoorsGroup.Add(newDoorGroup);
            }
        }
        return potentialDoorsGroup;
    }

    List<int[]> HasNeighdoor(int[] gate, List<int[]> potentialDoors) //We willen niet dat deuren naast elkaar spawnen. (DOET IE NU WEL MAAR MAX 2 NAAST ELKAAR (BUG))
    {
        int x = gate[0];
        int y = gate[1];
        List<int[]> newDoorGroup = new List<int[]>();
        foreach (int[] door in potentialDoors)
        {
            if (y == door[1])
            {
                if (x == door[0] + 1)
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

    List<int[]> GetNeighdoors(int[] gate, List<int[]> potentialDoors, List<int[]> visited)
    {
        List<int[]> groupDoors = new List<int[]>();
        visited.Add(gate);
        groupDoors.Add(gate);
        List<int[]> neighDoors = HasNeighdoor(gate, potentialDoors);
        foreach (int[] neighDoor in neighDoors)
        {
            if (!visited.Contains(neighDoor, new DuoIntArrComparer()))
            {
                groupDoors.AddRange(GetNeighdoors(neighDoor, potentialDoors, visited));
            }
        }
        return groupDoors.Distinct(new DuoIntArrComparer()).ToList();
    }


    bool ValidDoor(int x, int y)
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        if (grid[x, y] != Dungeon.wall)
        {
            return false;
        }
        if (!xP || !xN || !yP || !yN)
        {
            return false;
        }
        if (grid[x + 1, y] == Dungeon.path && grid[x - 1, y] == Dungeon.floor || grid[x, y + 1] == Dungeon.path && grid[x, y - 1] == Dungeon.floor)
        {
            return true;
        }
        if (grid[x + 1, y] == Dungeon.floor && grid[x - 1, y] == Dungeon.path || grid[x, y + 1] == Dungeon.floor && grid[x, y - 1] == Dungeon.path)
        {
            return true;
        }
        if (grid[x + 1, y] == Dungeon.empty || grid[x - 1, y] == Dungeon.empty || grid[x, y + 1] == Dungeon.empty || grid[x, y - 1] == Dungeon.empty)
        {
            return false;
        }
        if (grid[x + 1, y] == Dungeon.floor && grid[x - 1, y] == Dungeon.floor || grid[x, y + 1] == Dungeon.floor && grid[x, y - 1] == Dungeon.floor)
        {
            return true;
        }
        return false;
    }

    public void RemovingDeadEndsAndCleaningUp() //Al Die paden die nergens heen lopen moeten verwijderd worden, zo hebben we een cleaner dungeon
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                for (int k = 0; k < rows; k++)
                {
                    if (IsDeadEnd(j, k))
                    {
                        grid[j, k] = Dungeon.empty;
                    }
                }
            }
        }
    }

    public bool IsDeadEnd(int x, int y)
    {
        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        int pathCount = 0;
        if (grid[x, y] == Dungeon.floor || grid[x, y] == Dungeon.wall)
        {
            return false;
        }
        if (xP && (grid[x+1,y] == Dungeon.path || grid[x + 1, y] == Dungeon.door || grid[x + 1, y] == Dungeon.floor))
        {
            pathCount++;
        }
        if (xN && (grid[x - 1, y] == Dungeon.path || grid[x - 1, y] == Dungeon.door || grid[x - 1, y] == Dungeon.floor))
        {
            pathCount++;
        }
        if (yP && (grid[x, y + 1] == Dungeon.path || grid[x, y + 1] == Dungeon.door || grid[x, y + 1] == Dungeon.floor))
        {
            pathCount++;
        }
        if (yN && (grid[x, y - 1] == Dungeon.path || grid[x, y - 1] == Dungeon.door || grid[x, y - 1] == Dungeon.floor))
        {
            pathCount++;
        }
        return pathCount <= 1;
    }

    void SpawnPoint() //Spawn Point is zo ver mogelijk van het midden af
    {
        int[] furthestPoint = new int[2];
        int xMiddle = columns / 2;
        int yMiddle = rows / 2;
        int maxDistance = 0;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if(grid[i,j] == Dungeon.floor)
                {
                    if((Mathf.Abs(i - xMiddle) + Mathf.Abs (j - yMiddle)) > maxDistance)
                    {
                        maxDistance = Mathf.Abs(i - xMiddle) + Mathf.Abs(j - yMiddle);
                        furthestPoint[0] = i;
                        furthestPoint[1] = j;                        
                    }
                }
            }
        }
        Instantiate(enemyList[0], new Vector3(furthestPoint[1] * 4, 0, furthestPoint[0] * 4), Quaternion.identity);
        BossSpawnPoint(furthestPoint);
    }

    void BossSpawnPoint(int[] spawnPoint) //Boss is zo ver mogelijk van de Spawn Point Af
    {
        int[] furthestPoint = new int[2];
        int x = spawnPoint[0];
        int y = spawnPoint[1];
        int maxDistance = 0;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (grid[i, j] == Dungeon.floor)
                {
                    if ((Mathf.Abs(i - x) + Mathf.Abs(j - y)) > maxDistance)
                    {
                        maxDistance = Mathf.Abs(i - x) + Mathf.Abs(j - y);
                        furthestPoint[0] = i;
                        furthestPoint[1] = j;
                    }
                }
            }
        }
        Instantiate(enemyList[1], new Vector3(furthestPoint[1] * 4, 0, furthestPoint[0] * 4), Quaternion.identity);
    }

    public void Print2DArray(Dungeon[,] rawNodes) // Print de 2D Grid
    {
        int rowLength = rawNodes.GetLength(0);
        int colLength = rawNodes.GetLength(1);
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                string character = " ";
                if (rawNodes[i, j] == Dungeon.floor)
                {
                    character = "#";
                }
                if (rawNodes[i, j] == Dungeon.wall)
                {
                    character = " ";
                }
                if (rawNodes[i, j] == Dungeon.path)
                {
                    character = "=";
                }
                if (rawNodes[i, j] == Dungeon.door)
                {
                    character = "0";
                }
                arrayString += character += " ";
            }
            arrayString += System.Environment.NewLine + System.Environment.NewLine;
        }

        Debug.Log(arrayString);
    }
    public void Print3DArray(Dungeon[,] rawNodes) // Plaats de 3d Prefabs
    {
        int rowLength = rawNodes.GetLength(0);
        int colLength = rawNodes.GetLength(1);
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                Vector3 location = new Vector3(j * 4, 0, i * 4); // Keer 4 omdat de Assets die Grootte hebben

                if (rawNodes[i, j] == Dungeon.floor)
                {
                    DungeonFloorPlacer(i, j, location);
                }
                if (rawNodes[i, j] == Dungeon.wall)
                {
                    DungeonWallPlacer(i, j, location);
                }
                if (rawNodes[i, j] == Dungeon.path)
                {
                    DungeonPathPlacer(i, j, location);
                }
                if (rawNodes[i, j] == Dungeon.door)
                {
                    DungeonDoorPlacer(i, j, location);
                    DungeonFloorPlacer(i, j, location);

                }
            }
        }
    }
    public void DungeonFloorPlacer(int x, int y, Vector3 location) // Sinds Alles spawnt in deze ruimtes, heb ik het nu zo gedaan
    {
        Instantiate(roomList[0], location, Quaternion.identity);

        int randomObjectPlace = Random.Range(0, 301);
        if (randomObjectPlace <= enemySpawnChance)
        {
            Instantiate(enemyList[Random.Range(2, 4)], location, Quaternion.identity); 
        }
        if (101 <= randomObjectPlace && randomObjectPlace <= trapSpawnChance) //Zo zorg ik ervoor dat niks op elkaar spawnt
        {
            Instantiate(trapList[Random.Range(0, trapList.Count)], location, Quaternion.identity);
        }
        if (201 <= randomObjectPlace && randomObjectPlace <= treasureSpawnChance) // Beetje Dirty, maar het werkt haha
        {
            Instantiate(treasureList[Random.Range(0, treasureList.Count)], location, Quaternion.identity);
        }

    }
    public void DungeonPathPlacer(int x, int y, Vector3 location) //Sinds ik niet wilde dealen met Corner Assets of directions meten heb ik dit iets anders gedaan
    {
        Instantiate(roomList[1], location, Quaternion.identity);

        bool xP = x < columns - 1;
        bool xN = x > 0;
        bool yP = y < rows - 1;
        bool yN = y > 0;
        if (xP && (grid[x + 1, y] == Dungeon.path || grid[x + 1, y] == Dungeon.door))
        {
            Instantiate(roomList[1], location + new Vector3(0, 0, 4/3f), Quaternion.identity); //Ik deel de grid in 3en en ga dan mn pathfinding Algoritme opnieuw kleiner toepassen
        }
        if (xN && (grid[x - 1, y] == Dungeon.path || grid[x - 1, y] == Dungeon.door))
        {
            Instantiate(roomList[1], location + new Vector3(0, 0, - 4 / 3f), Quaternion.identity);

        }
        if (yP && (grid[x, y + 1] == Dungeon.path || grid[x, y + 1] == Dungeon.door)) //Kinda like a quad trees
        {
            Instantiate(roomList[1], location + new Vector3(4/3f, 0, 0), Quaternion.identity);

        }
        if (yN && (grid[x, y - 1] == Dungeon.path || grid[x, y - 1] == Dungeon.door))
        {
            Instantiate(roomList[1], location + new Vector3(-4 / 3f, 0, 0), Quaternion.identity);
        }
    }
    public void DungeonWallPlacer(int x, int y, Vector3 location)
    {
        Instantiate(roomList[2], location, Quaternion.identity); // EZ
    }
    public void DungeonDoorPlacer(int x, int y, Vector3 location) // De Deuren moeten natuurlijk de goede richting hebben, gelukkig hoef ik niet zoals de Paths ze te onderverdelen
    {
        if (grid[x + 1, y] == Dungeon.floor && grid[x - 1, y] == Dungeon.floor)
        {
            Instantiate(roomList[3], location, Quaternion.identity); //ze kunnen namelijk maar 4 richtingen op staan
        }
        if (grid[x, y + 1] == Dungeon.floor && grid[x, y - 1] == Dungeon.floor)
        {
            Instantiate(roomList[3], location, Quaternion.Euler(0,90,0));
        }
        if (grid[x + 1, y] == Dungeon.floor && grid[x - 1, y] == Dungeon.path)
        {
            Instantiate(roomList[3], location, Quaternion.identity);
        }
        if (grid[x, y + 1] == Dungeon.floor && grid[x, y - 1] == Dungeon.path)
        {
            Instantiate(roomList[3], location, Quaternion.Euler(0, 90, 0));
        }
        if (grid[x + 1, y] == Dungeon.path && grid[x - 1, y] == Dungeon.floor)
        {
            Instantiate(roomList[3], location, Quaternion.Euler(0, 180, 0));
        }
        if (grid[x, y + 1] == Dungeon.path && grid[x, y - 1] == Dungeon.floor)
        {
            Instantiate(roomList[3], location, Quaternion.Euler(0, 270, 0));
        }
    }
}



class DuoIntArrComparer : EqualityComparer<int[]>
{
    public override bool Equals(int[] x, int[] y)
    {
        if (x[0] == y[0] && x[1] == y[1])
            return true;
        else
            return false;
    }

    public override int GetHashCode(int[] obj)
    {
        return obj [0] * obj [1];
    }
}