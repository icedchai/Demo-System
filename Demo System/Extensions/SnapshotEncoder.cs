using DemoSystem.Snapshots.Interfaces;
using DemoSystem.Snapshots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using DemoSystem.Snapshots.PlayerSnapshots;

namespace DemoSystem.Extensions
{
    public static class SnapshotEncoder
    {
        private static Dictionary<ushort, Type> idToType = new Dictionary<ushort, Type>()
        {
            { 1, typeof(PlayerTransformSnapshot) },
            { 2, typeof(EndOfFrameSnapshot) },
            { 3, typeof(PlayerVerifiedSnapshot) },
            { 4, typeof(PlayerSpawnedSnapshot) },
            { 5, typeof(PlayerVoiceChatSnapshot) },
            { 6, typeof(PlayerNoclipToggledSnapshot) },
        };

        private static Dictionary<Type, ushort> typeToId = null;

        public static Dictionary<ushort, Type> IdToSnapshotType => idToType;

        public static Dictionary<Type, ushort> SnapshotTypeToId
        {
            get
            {
                if (typeToId is null)
                {
                    Dictionary<Type, ushort> temporaryDict = new Dictionary<Type, ushort>();

                    foreach (var kvp in idToType)
                    {
                        temporaryDict.Add(kvp.Value, kvp.Key);
                    }

                    typeToId = temporaryDict;
                }
                return typeToId;
            }
        }

        public static void Serialize(this Snapshot snapshot, BinaryWriter writer)
        {
            writer.Write(snapshot.Id);

            if (snapshot is IPlayerSnapshot playerSnapshot)
            {
                writer.Write(playerSnapshot.Player);
            }

            if (snapshot is IRoleSnapshot roleSnapshot)
            {
                writer.Write(roleSnapshot.Role);
            }

            if (snapshot is IPositionSnapshot positionSnapshot)
            {
                writer.Write(positionSnapshot.Position);
            }

            if (snapshot is IRotationSnapshot rotationSnapshot)
            {
                writer.Write(rotationSnapshot.Rotation);
            }

            snapshot.SerializeSpecial(writer);
        }

        public static bool Deserialize(BinaryReader reader, out Snapshot snapshot)
        {
            snapshot = null;

            ushort type = reader.ReadUInt16();

            if (!IdToSnapshotType.TryGetValue(type, out Type snapshotType))
            {
                return false;
            }

            if (snapshotType == null || snapshotType.BaseType != typeof(Snapshot))
            {
                return false;
            }

            snapshot = (Snapshot)Activator.CreateInstance(snapshotType);

            if (snapshot is IPlayerSnapshot playerSnapshot)
            {
                playerSnapshot.Player = reader.ReadInt32();
            }

            if (snapshot is IRoleSnapshot roleSnapshot)
            {
                roleSnapshot.Role = reader.ReadRoleTypeId();
            }

            if (snapshot is IPositionSnapshot positionSnapshot)
            {
                positionSnapshot.Position = reader.ReadVector3();
            }

            if (snapshot is IRotationSnapshot rotationSnapshot)
            {
                rotationSnapshot.Rotation = reader.ReadQuaternion();
            }

            snapshot.DeserializeSpecial(reader);
            return true;
        }
    }
}
