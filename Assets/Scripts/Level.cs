using System.Collections.Generic;
using UnityEngine;

public class Level
{
    int width;
    int length;

    List<Hallway> hallways;
    List<Room> rooms;

    public int Width => width;
    public int Length => length;

    public Room[] Rooms => rooms.ToArray();
    public Hallway[] Hallways => hallways.ToArray();

    public Level(int width, int length)
    {
        this.width = width;
        this.length = length;
        rooms = new List<Room>();
        hallways = new List<Hallway>();
    }

    public void AddRoom(Room newRoom) => rooms.Add(newRoom);
    public void AddHallway(Hallway hallway) => hallways.Add(hallway);
}
