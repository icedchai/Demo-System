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
using CommandSystem.Commands.RemoteAdmin;
using DemoSystem.Snapshots.Enums;
using Exiled.API.Features;

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
                RegisterSnapshotType(type);
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

            if (snapshot is IItemSnapshot itemSnapshot)
            {
                writer.Write(itemSnapshot.Item);
            }

            if (snapshot is IPlayerTransformSnapshot transformSnapshot)
            {
                writer.Write((byte)transformSnapshot.TransformDifference);

                if (transformSnapshot.TransformDifference.HasFlag(TransformDifference.Position))
                {
                    writer.Write(transformSnapshot.Position);
                }
                if (transformSnapshot.TransformDifference.HasFlag(Snapshots.Enums.TransformDifference.Rotation))
                {
                    writer.Write(transformSnapshot.Rotation);
                }
                if (transformSnapshot.TransformDifference.HasFlag(Snapshots.Enums.TransformDifference.Scale))
                {
                    writer.Write(transformSnapshot.Scale);
                }
            }

            snapshot.SerializeSpecial(writer);
        }

        public static bool Deserialize(BinaryReader reader, out Snapshot snapshot)
        {
            snapshot = null;

            ushort type = reader.ReadUInt16();

            if (!IdToSnapshotType.TryGetValue(type, out Type snapshotType))
            {
                Log.Error(type);
                return false;
            }

            if (snapshotType == null || snapshotType.BaseType != typeof(Snapshot))
            {
                Log.Error("Couldnt deserialize type");
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

            if (snapshot is IItemSnapshot itemSnapshot)
            {
                itemSnapshot.Item = reader.ReadUInt16();
            }

            if (snapshot is IPlayerTransformSnapshot transformSnapshot)
            {
                transformSnapshot.TransformDifference = (TransformDifference)reader.ReadByte();

                if (transformSnapshot.TransformDifference.HasFlag(TransformDifference.Position))
                {
                    transformSnapshot.Position = reader.ReadVector3();
                }
                if (transformSnapshot.TransformDifference.HasFlag(TransformDifference.Rotation))
                {
                    transformSnapshot.Rotation = reader.ReadVector2();
                }
                if (transformSnapshot.TransformDifference.HasFlag(TransformDifference.Scale))
                {
                    transformSnapshot.Scale = reader.ReadVector3();
                }
            }

            snapshot.DeserializeSpecial(reader);
            return true;
        }
    }
}
