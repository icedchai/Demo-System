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
using DemoSystem.Extensions;
using System.Reflection;

namespace DemoSystem.SnapshotHandlers
{
    public static class SnapshotEncoder
    {
        private static Dictionary<ushort, Type> idToType = new Dictionary<ushort, Type>();


        private static Dictionary<Type, ushort> typeToId = null;

        public static Dictionary<ushort, Type> IdToSnapshotType => idToType;

        public static Dictionary<Type, ushort> SnapshotTypeToId
        {
            get
            {
                if (typeToId is null || typeToId.Count != idToType.Count)
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

        /// <summary>
        /// Registers all types deriving from <see cref="Snapshot"/> in the current assembly.
        /// </summary>
        public static void RegisterSnapshotTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> validTypes = assembly.GetTypes().Where(t => t != typeof(Snapshot) && typeof(Snapshot).IsAssignableFrom(t)).ToList();
            validTypes.Sort((a, b) => a.FullName.CompareTo(b.FullName));
            foreach (Type type in validTypes)
            {
                SnapshotEncoder.RegisterSnapshotType(type);
            }
        }

        public static void RegisterSnapshotType<T>() where T : Snapshot
        {
            RegisterSnapshotType(typeof(T));
        }

        public static void UnregisterSnapshotType<T>() where T : Snapshot
        {
            UnregisterSnapshotType(typeof(T));
        }

        public static bool RegisterSnapshotType(Type type)
        {
            if (type != typeof(Snapshot) && !typeof(Snapshot).IsAssignableFrom(type))
            {
                return false;
            }

            idToType.Add((ushort)(idToType.Count + 1), type);
            return true;
        }

        public static bool UnregisterSnapshotType(Type type)
        {
            if (type != typeof(Snapshot) && !typeof(Snapshot).IsAssignableFrom(type))
            {
                return false;
            }

            foreach (var kvp in idToType)
            {
                if (kvp.Value == type)
                {
                    idToType.Remove(kvp.Key);
                }
            }
            return true;
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

            if (snapshot is IScaleSnapshot scaleSnapshot)
            {
                writer.Write(scaleSnapshot.Scale);
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

            if (snapshot is IScaleSnapshot scaleSnapshot)
            {
                scaleSnapshot.Scale = reader.ReadVector3();
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
