using UnityEngine;

class StunBuff : Buff
{


    public StunBuff(int duration)
    {

        Duration = duration;
    }

    public int Duration { get; set; }
    public void TickApply(Unit unit)    
    {
        StunUnit(unit);
    }
    public void TickUndo(Unit unit)
    {

    }
    public void Apply(Unit unit)
    {

    }
    public void Undo(Unit unit)
    {
        //Note that stun buff has empty Undo method implementation.
    }

    public Buff Clone()
    {
        return new StunBuff(Duration);
    }

    private void StunUnit(Unit unit)
    {
        unit.MovementPoints = 0;
        unit.ActionPoints = 0;
    }
}
