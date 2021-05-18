using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public room [,] grid;
    public int columns = 140;
    public int rows = 140;
    public int roomAmount = 60;
    public List<GameObject> roomList;
    public (int x, int y) startLoc = (40 / 2, 40 / 2);

    public class room
    {
        public room(int useless) 
        {
            this.visited = false;
            this.top = false;
            this.right = false;
            this.bottom = false;
            this.left = false;
        }
        public bool visited;
        public bool top;
        public bool right;
        public bool bottom;
        public bool left;
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = new room[columns, rows];
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                grid[i, j] = new room(69);
            }
        }
        HallMonitor();
        roomSpawner();

    }

    void HallMonitor()
    {
        (int x, int y) currentLoc = startLoc;
        for (int i = 0; i < roomAmount; i++)
        {
            room currentRoom = grid[currentLoc.x % (rows - 1), currentLoc.y % (columns - 1)];
            room newRoom = new room(69);
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    currentLoc.x--;
                    newRoom = grid[currentLoc.x % (rows - 1), currentLoc.y % (columns - 1)];
                    currentRoom.bottom = true;
                    newRoom.top = true;                
                    break;
                case 1:
                    currentLoc.x++;
                    newRoom = grid[currentLoc.x % (rows - 1), currentLoc.y % (columns - 1)];
                    currentRoom.top = true;
                    newRoom.bottom = true;
                    break;
                case 2:
                    currentLoc.y--;
                    newRoom = grid[currentLoc.x % (rows - 1), currentLoc.y % (columns - 1)];
                    currentRoom.left = true;
                    newRoom.right = true;
                    break;
                case 3:
                    currentLoc.y++;
                    newRoom = grid[currentLoc.x % (rows - 1), currentLoc.y % (columns - 1)];
                    currentRoom.right = true;
                    newRoom.left = true;
                    break;
                //put //like //a //(funny) //ghoul //heeerre
            }
            
        }
    }
    void roomSpawner()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                room currentRoom = grid[i, j];
                if (!currentRoom.top && !currentRoom.bottom && !currentRoom.left && !currentRoom.right)
                {
                    continue;
                }

                Vector3 location = new Vector3(j * 4, 0, i * 4);
                if (currentRoom.top == true)
                {
                    Instantiate(roomList[0], location, Quaternion.identity);                 
                }
                else
                {
                    Instantiate(roomList[4], location, Quaternion.identity);
                }
                if (currentRoom.right == true)
                {
                    Instantiate(roomList[1], location, Quaternion.identity);
                }
                else
                {
                    Instantiate(roomList[5], location, Quaternion.identity);
                }
                if (currentRoom.bottom == true)
                {
                    Instantiate(roomList[2], location, Quaternion.identity);
                }
                else
                {
                    Instantiate(roomList[6], location, Quaternion.identity);
                }
                if (currentRoom.left == true)
                {
                    Instantiate(roomList[3], location, Quaternion.identity);
                }
                else
                {
                    Instantiate(roomList[7], location, Quaternion.identity);
                }

            }
        }
    }
}
