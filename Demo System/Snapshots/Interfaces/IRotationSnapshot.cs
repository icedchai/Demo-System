namespace DemoSystem.Snapshots.Interfaces
{
    using UnityEngine;

    /// <summary>
    /// Represents a snapshot that contains rotation information.
    /// </summary>
    public interface IRotationSnapshot
    {
        public Quaternion Rotation { get; set; }
    }
}
