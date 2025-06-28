using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Room Level Layout", menuName = "Custom/Procedural Generation/RoomLevelLayoutConfigurations")]
public class RoomLevelLayoutConfigurations : ScriptableObject
{
    [SerializeField] int width = 64;
    [SerializeField] int length = 64;

    [SerializeField] RoomTemplate[] roomTemplates;
    [SerializeField] int doorDistanceFromEdge = 1;
    [SerializeField] int hallwayLengthMin = 3;
    [SerializeField] int hallwayLengthMax = 7;
    [SerializeField] int maxRoomCount = 10;
    [SerializeField] int minRoomDistance = 1;

    public int Width => width;
    public int Length => length;

    public RoomTemplate[] RoomTemplates => roomTemplates;
    public int DoorDistanceFromEdge => doorDistanceFromEdge;
    public int HallwayLengthMin => hallwayLengthMin;
    public int HallwayLengthMax => hallwayLengthMax;
    public int MaxRoomCount => maxRoomCount;
    public int MinRoomDistance => minRoomDistance;

    public Dictionary<RoomTemplate, int> GetAvailableRooms()
    {
        Dictionary<RoomTemplate, int> availableRooms = new Dictionary<RoomTemplate,int>();
        for (int i = 0; i < roomTemplates.Length; i++) {
            availableRooms.Add(roomTemplates[i], roomTemplates[i].NumberOfRooms);
        }
        availableRooms = availableRooms.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return availableRooms;
    }
}
