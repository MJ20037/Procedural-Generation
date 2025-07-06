using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LayoutGeneratorRooms : MonoBehaviour
{
    [SerializeField] int seed = Environment.TickCount;
    [SerializeField] RoomLevelLayoutConfigurations levelConfig;

    [SerializeField] GameObject levelLayoutDisplay;
    [SerializeField] List<Hallway> openDoorways;

    Random random;
    Level level;
    Dictionary<RoomTemplate, int> availableRooms;

    [ContextMenu("Generate Level Layout")]

    public Level GenerateLevel()
    {
        SharedLevelData.Instance.ResetRandom();
        random = SharedLevelData.Instance.Rand;

        availableRooms = levelConfig.GetAvailableRooms();
        openDoorways = new List<Hallway>();
        level = new Level(levelConfig.Width, levelConfig.Length);
        RoomTemplate startRoomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));

        RectInt roomRect = GetStartRoomRect(startRoomTemplate);
        Room room = CreateNewRoom(roomRect, startRoomTemplate);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        hallways.ForEach((h) => h.StartRoom = room);
        hallways.ForEach((h) => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(openDoorways.Count)];
        AddRooms();
        AddHallwaysToRooms();
        AssignRoomTypes();
        DrawLayout(selectedEntryway, roomRect);

        return level;
    }

    private void AssignRoomTypes()
    {
        List<Room> borderRooms = level.Rooms.Where(room => room.Connectedness == 1).ToList();
        if (borderRooms.Count < 2)
        {
            return;
        }
        int startRoomIndex = random.Next(0, borderRooms.Count);
        Room randomStartRoom = level.Rooms[startRoomIndex];
        level.playerStartRoom = randomStartRoom;
        randomStartRoom.Type = RoomType.Start;
        borderRooms.Remove(randomStartRoom);

        Room fathestRoom = borderRooms.OrderByDescending(room => Vector2.Distance(randomStartRoom.Area.center, room.Area.center))
        .FirstOrDefault();
        fathestRoom.Type = RoomType.Exit;
        borderRooms.Remove(fathestRoom);

        List<Room> treasureRoom = borderRooms.OrderBy(r => random.Next()).Take(3).ToList();
        borderRooms.RemoveAll(room => treasureRoom.Contains(room));
        treasureRoom.ForEach(room => room.Type = RoomType.Treasure);

        List<Room> emptyRooms = level.Rooms.Where(room => room.Type.HasFlag(RoomType.Default)).ToList();
        Room bossRoom = emptyRooms.OrderByDescending(room => Vector2.Distance(randomStartRoom.Area.center, room.Area.center))
        .OrderByDescending(room => room.Connectedness)
        .OrderByDescending(room => room.Area.width * room.Area.height)
        .FirstOrDefault();
        bossRoom.Type = RoomType.Boss;
        emptyRooms.Remove(bossRoom);

        emptyRooms = emptyRooms.OrderBy(room => random.Next()).ToList();
        RoomType[] typesToAssign = { RoomType.Prison, RoomType.Library, RoomType.Kitchen };
        List<Room> roomsToAssign = emptyRooms.Take(typesToAssign.Length).ToList();
        for (int i = 0; i < roomsToAssign.Count; i++)
        {
            roomsToAssign[i].Type = typesToAssign[i];
        }
    }

    private void AddHallwaysToRooms()
    {
        foreach (Room room in level.Rooms)
        {
            Hallway[] hallwaysStartingAtRoom = Array.FindAll(level.Hallways, hallway => hallway.StartRoom == room);
            Array.ForEach(hallwaysStartingAtRoom, hallway => room.AddHallway(hallway));
            Hallway[] hallwaysEndingAtRoom = Array.FindAll(level.Hallways, hallway => hallway.EndRoom == room);
            Array.ForEach(hallwaysEndingAtRoom, hallway => room.AddHallway(hallway)); 
        }
    }

    [ContextMenu("Generate new seed")]
    public void GenerateNewSeed()
    {
        SharedLevelData.Instance.GenerateSeed();
    }

    [ContextMenu("Generate new Seed and Level")]
    public void GenerateNewSeedAndLevel()
    {
        GenerateNewSeed();
        GenerateLevel();
    }

    private void AddRooms()
    {
        while (openDoorways.Count > 0 && level.Rooms.Length < levelConfig.MaxRoomCount && availableRooms.Count>0)
        {
            Hallway selectedEntryway = openDoorways[random.Next(0, openDoorways.Count)];
            Room newRoom = ConstructAdjacentRoom(selectedEntryway);

            if (newRoom == null)
            {
                openDoorways.Remove(selectedEntryway);
                continue;
            }

            level.AddRoom(newRoom);
            level.AddHallway(selectedEntryway);

            selectedEntryway.EndRoom = newRoom;
            List<Hallway> newOpenHallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, levelConfig.DoorDistanceFromEdge);
            newOpenHallways.ForEach(h => h.StartRoom = newRoom);

            openDoorways.Remove(selectedEntryway);
            openDoorways.AddRange(newOpenHallways);
        }
    }

    private void UseUpRoomTemplate(RoomTemplate roomTemplate)
    {
        availableRooms[roomTemplate] -= 1;
        if (availableRooms[roomTemplate] == 0)
        {
            availableRooms.Remove(roomTemplate);
        }
    }

    private Room CreateNewRoom(RectInt roomCandidateRect, RoomTemplate roomTemplate, bool useUp = true)
    {
        if (useUp)
        {
            UseUpRoomTemplate(roomTemplate);
        }
        
        if (roomTemplate.LayoutTexture == null)
        {
            return new Room(roomCandidateRect);
        }
        else
        {
            return new Room(roomCandidateRect.x, roomCandidateRect.y, roomTemplate.LayoutTexture);
        }
    }

    private RectInt GetStartRoomRect(RoomTemplate roomTemplate)
    {
        RectInt roomSize = roomTemplate.GenerateRoomCandidateRect(random);
        int roomWidth = roomSize.width;
        int availableWidthX = levelConfig.Width / 2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);

        int roomX = randomX + levelConfig.Width / 4;

        int roomLength = roomSize.height;
        int availableLengthY = levelConfig.Length / 2 - roomLength;
        int randomY = random.Next(0, availableLengthY);

        int roomY = randomY + levelConfig.Length / 4;

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    void DrawLayout(Hallway selectedEntryway = null, RectInt roomCandidateRect = new RectInt(), bool IsDebug=false)
    {
        var renderer = levelLayoutDisplay.GetComponent<Renderer>();

        var layoutTexture = (Texture2D)renderer.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(levelConfig.Width, levelConfig.Length);
        int scale = SharedLevelData.Instance.Scale;
        levelLayoutDisplay.transform.localScale = new Vector3(levelConfig.Width*scale, levelConfig.Length*scale, 1);
        float xPos = level.Width * scale / 2.0f - scale;
        float zPos = level.Length * scale / 2.0f - scale;
        levelLayoutDisplay.transform.position = new Vector3(xPos, 0.2f, zPos);
        layoutTexture.FillWithColor(Color.black);

        foreach (Room room in level.Rooms)
        {
            if (room.LayoutTexture != null)
            {
                layoutTexture.DrawTexture(room.LayoutTexture,room.Area);
            }
            else
            {
                layoutTexture.DrawRectangle(room.Area, Color.white);
            }
        }
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute, hallway.EndPositionAbsolute, Color.white));
        layoutTexture.ConvertToBlackAndWhite();
        if (IsDebug)
        {
            layoutTexture.DrawRectangle(roomCandidateRect, Color.cyan);

            openDoorways.ForEach(hallway => layoutTexture.SetPixel(hallway.StartPositionAbsolute.x, hallway.StartPositionAbsolute.y, hallway.StartDirection.GetColor()));
        }
        
        if (IsDebug && selectedEntryway != null)
            layoutTexture.SetPixel(selectedEntryway.StartPositionAbsolute.x, selectedEntryway.StartPositionAbsolute.y, Color.red);
        layoutTexture.SaveAsset();
    }

    private Hallway SelectHallwayCandidate(RectInt roomCandidate, RoomTemplate roomTemplate, Hallway entryway)
    {
        Room room = CreateNewRoom(roomCandidate, roomTemplate, false);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        HallwayDirection requiredDirection = entryway.StartDirection.GetOppositeDirection();
        List<Hallway> filteredHallwayCandidates = candidates.Where(hallwayCandidate => hallwayCandidate.StartDirection == requiredDirection).ToList();

        return filteredHallwayCandidates.Count > 0 ? filteredHallwayCandidates[random.Next(filteredHallwayCandidates.Count)] : null;
    }

    private Vector2Int CalculateRoomPosition(Hallway entryway, int roomWidth, int roomLength, int distance, Vector2Int endPosition)
    {
        Vector2Int roomPosition = entryway.StartPositionAbsolute;
        switch (entryway.StartDirection)
        {
            case HallwayDirection.Left:
                roomPosition.x -= distance + roomWidth;
                roomPosition.y -= endPosition.y;
                break;

            case HallwayDirection.Right:
                roomPosition.x += distance + 1;
                roomPosition.y -= endPosition.y;
                break;

            case HallwayDirection.Top:
                roomPosition.x -= endPosition.x;
                roomPosition.y += distance + 1;
                break;

            case HallwayDirection.Bottom:
                roomPosition.x -= endPosition.x;
                roomPosition.y -= distance + roomLength;
                break;

        }
        return roomPosition;
    }

    private Room ConstructAdjacentRoom(Hallway selectedEntryway)
    {
        RoomTemplate roomTemplate= availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
        RectInt roomCandidateRect = roomTemplate.GenerateRoomCandidateRect(random);
        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, roomTemplate, selectedEntryway);

        if (selectedExit == null && availableRooms.Count > 0)
        {
            // try to get another room if available
            for (int r = 0; r < availableRooms.Count; r++)
            {
                roomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
                roomCandidateRect = roomTemplate.GenerateRoomCandidateRect(random);
                selectedExit = SelectHallwayCandidate(roomCandidateRect, roomTemplate, selectedEntryway);

                if (selectedExit != null)
                {
                    break;
                }
            }
        }

        if (selectedExit == null) { return null; }

        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, random.Next(levelConfig.HallwayLengthMin, levelConfig.HallwayLengthMax + 1), selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect))
        {
            return null;
        }

        Room newRoom = CreateNewRoom(roomCandidateRect, roomTemplate);
        selectedEntryway.EndRoom = newRoom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;

        return newRoom;
    }

    private bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        RectInt levelRect = new RectInt(1, 1, levelConfig.Width - 2, levelConfig.Length - 2);
        return levelRect.Contains(roomCandidateRect) && !CheckRoomOverlap(roomCandidateRect,level.Rooms,level.Hallways, levelConfig.MinRoomDistance);
    }

    private bool CheckRoomOverlap(RectInt roomCandidateRect, Room[] rooms, Hallway[] hallways, int minRoomDistance)
    {
        RectInt paddedRoomRect = new RectInt
        {
            x = roomCandidateRect.x - minRoomDistance,
            y = roomCandidateRect.y - minRoomDistance,
            width = roomCandidateRect.width + 2 * minRoomDistance,
            height = roomCandidateRect.height + 2 * minRoomDistance
        };

        foreach (Room room in rooms)
        {
            if (paddedRoomRect.Overlaps(room.Area))
            {
                return true;
            }
        }

        foreach (Hallway hallway in hallways)
        {
            if (paddedRoomRect.Overlaps(hallway.Area))
            {
                return true;
            }
        }
        return false;
    }
}
