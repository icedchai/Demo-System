namespace DemoSystem.Extensions
{
    using Exiled.API.Structs;
    using InventorySystem.Items.Firearms.Attachments;
    using PlayerRoles;
    using UnityEngine;

    /// <summary>
    /// Helps encode more advanced datatypes.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        public static Vector3 ReadVector3(this BinaryReader binaryReader)
        {
            float x = binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = binaryReader.ReadSingle();
            return new Vector3(x, y, z);
        }
        public static Vector2 ReadVector2(this BinaryReader binaryReader)
        {
            float x = binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            return new Vector2(x, y);
        }
        public static Quaternion ReadQuaternion(this BinaryReader binaryReader)
        {
            float w = binaryReader.ReadSingle();
            float x = binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = binaryReader.ReadSingle();
            return new Quaternion(x, y, z, w);
        }

        public static RoleTypeId ReadRoleTypeId(this BinaryReader binaryReader)
        {
            byte number = binaryReader.ReadByte();
            return (RoleTypeId)number;
        }

        public static ItemType ReadItemType(this BinaryReader binaryReader)
        {
            int number = binaryReader.ReadInt32();
            return (ItemType)number;
        }
    }
}
