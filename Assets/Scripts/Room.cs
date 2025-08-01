using System.Collections.Generic;
using UnityEngine;
using Flags = System.FlagsAttribute;

[Flags]
public enum RoomType
{
    Default = 1,
    Start = 1 << 1,
    Exit = 1 << 2,
    Boss = 1 << 3,
    Treasure = 1 << 4,
    Prison = 1 << 5,
    Library = 1 << 6,
    Kitchen = 1 << 7
}

public class Room
{
    List<Hallway> hallways;

    RectInt area;
    public RectInt Area => area;
    public Texture2D LayoutTexture { get; }
    public RoomType Type { get; set; } = RoomType.Default;
    public int Connectedness => hallways.Count;

    public Room(RectInt area)
    {
        this.area = area;
        hallways = new List<Hallway>();
    }
    public Room(int x, int y, Texture2D layoutTexture)
    {
        area = new RectInt(x, y, layoutTexture.width, layoutTexture.height);
        LayoutTexture = layoutTexture;
        hallways = new List<Hallway>();
    }
    public List<Hallway> CalculateAllPossibleDoorways(int width, int length, int minDistanceFromEdge)
    {
        if (LayoutTexture == null)
        {
            return CalculateAllPossibleDoorwaysForRectangularRooms(width, length, minDistanceFromEdge);
        }
        else
        {
            return CalculateAllPossibleDoorwaysPositions(LayoutTexture);
        }
    }

    private List<Hallway> CalculateAllPossibleDoorwaysPositions(Texture2D layoutTexture)
    {
        List<Hallway> possibleHallwayPositions = new List<Hallway>();
        int width = layoutTexture.width;
        int height = layoutTexture.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = layoutTexture.GetPixel(x, y);
                HallwayDirection direction = GetHallwayDirection(pixelColor);
                if (direction != HallwayDirection.Undefined)
                {
                    Hallway hallway = new Hallway(direction, new Vector2Int(x, y));
                    possibleHallwayPositions.Add(hallway);
                }
            }
        }
        return possibleHallwayPositions;
    }

    private HallwayDirection GetHallwayDirection(Color color)
    {
        Dictionary<Color, HallwayDirection> colorToDirectionMap = HallwayDirectionExtensions.GetColorToDirectionMap();
        return colorToDirectionMap.TryGetValue(color, out HallwayDirection direction) ? direction : HallwayDirection.Undefined;
    }

    public List<Hallway> CalculateAllPossibleDoorwaysForRectangularRooms(int width, int length, int minDistanceFromEdge)
    {
        List<Hallway> hallwayCandidates = new List<Hallway>();

        int top = length - 1;
        int minX = minDistanceFromEdge;
        int maxX = width - minDistanceFromEdge;

        for (int x = minX; x < maxX; x++)
        {
            hallwayCandidates.Add(new Hallway(HallwayDirection.Bottom, new Vector2Int(x, 0)));
            hallwayCandidates.Add(new Hallway(HallwayDirection.Top, new Vector2Int(x, top)));
        }

        int right = width - 1;
        int minY = minDistanceFromEdge;
        int maxY = length - minDistanceFromEdge;

        for (int y = minY; y < maxY; y++)
        {
            hallwayCandidates.Add(new Hallway(HallwayDirection.Left, new Vector2Int(0, y)));
            hallwayCandidates.Add(new Hallway(HallwayDirection.Right, new Vector2Int(right, y)));
        }

        return hallwayCandidates;
    }

    public void AddHallway(Hallway selectedHallway)
    {
        hallways.Add(selectedHallway);
    }
}
