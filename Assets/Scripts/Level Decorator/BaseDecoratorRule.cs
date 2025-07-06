using UnityEngine;

public abstract class BaseDecoratorRule : ScriptableObject
{
    [SerializeField, EnumFlags] RoomType roomType;
    public RoomType RoomTypes=>roomType;
    internal abstract bool CanBeApplied(TileType[,] levelDecorated, Room room);
    internal abstract void Apply(TileType[,] levelDecorated, Room room, Transform parent);
}
