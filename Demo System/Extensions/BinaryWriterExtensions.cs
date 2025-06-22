namespace DemoSystem.Extensions
{
    using PlayerRoles;
    using UnityEngine;

    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter binaryWriter, Vector3 vector3)
        {
            binaryWriter.Write(vector3.x);
            binaryWriter.Write(vector3.y);
            binaryWriter.Write(vector3.z);
        }
        public static void Write(this BinaryWriter binaryWriter, Quaternion vector4)
        {
            binaryWriter.Write(vector4.w);
            binaryWriter.Write(vector4.x);
            binaryWriter.Write(vector4.y);
            binaryWriter.Write(vector4.z);
        }

        public static void Write(this BinaryWriter binaryWriter, RoleTypeId roleTypeId)
        {
            binaryWriter.Write((byte)roleTypeId);
        }

        public static void Write(this BinaryWriter binaryWriter, ItemType itemType)
        {
            binaryWriter.Write((int)itemType);
        }
    }
}
