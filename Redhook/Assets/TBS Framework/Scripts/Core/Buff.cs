using UnityEngine;

/// <summary>
/// Class representing an "upgrade" to a unit.
/// </summary>
[CreateAssetMenu(menuName = "buffs/genericbuff")]
public class Buff : ScriptableObject
{
    /// <summary>
    /// Determines how long the buff should last (expressed in turns). If set to negative number, buff will be permanent.
    /// </summary>
    public virtual int Duration { get; set; }

    /// <summary>
    /// Describes how the unit should be upgraded.
    /// </summary>
    public virtual void Apply(Unit unit) { }
    /// <summary>
    /// Returns units stats to normal.
    /// </summary>
    public virtual void Undo(Unit unit) { }


    public virtual void TickApply(Unit unit) { }
    public virtual void TickUndo(Unit unit) { }


    /// <summary>
    /// Returns deep copy of the Buff object.
    /// </summary>
    public virtual Buff Clone() { return null; }
}