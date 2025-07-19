using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots.Interfaces;

namespace DemoSystem.Snapshots
{
    /// <summary>
    /// The base abstract class representing a snapshot. MUST HAVE A PARAMATERLESS CONSTRUCTOR OR A SILENT FAIL WILL OCCUR.
    /// </summary>
    public abstract class Snapshot
    {
        private ushort _id = 0;

        private bool _idIsNull = false;

        public ushort Id
        {
            get
            {
                if (_idIsNull)
                {
                    return 0;
                }

                if (_id == 0)
                {
                    if (SnapshotEncoder.SnapshotTypeToId.TryGetValue(GetType(), out _id))
                    {
                        return _id;
                    }
                    else
                    {
                        _idIsNull = true;
                        return 0;
                    }
                }
                return _id;
            }
        }

        public virtual void DeserializeSpecial(BinaryReader reader)
        {

        }

        public virtual void SerializeSpecial(BinaryWriter writer)
        {

        }

        public virtual void ReadSnapshot()
        {

        }
    }
}
