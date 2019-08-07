using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitGUIController : MonoBehaviour
{
    public CellGrid CellGrid;

    public GameObject GUIRenderer;
    public Image Panel;
    public Sprite OldArmyPanel;
    public Sprite NewArmyPanel;
    public Image UnitImage;
    public TextMeshPro UnitName;
    public TextMeshPro StatsText;
    public TMP_FontAsset BossNameFont;
    private TMP_FontAsset NormalNameFont;

    private bool isSelected = false;

    void Awake()
    {
        NormalNameFont = UnitName.font;
        GUIRenderer.SetActive(false);

        CellGrid.UnitAdded += OnUnitAdded;
        CellGrid.TurnEnded += OnTurnEnded;
    }

    private void OnUnitHighlighted(object sender, EventArgs e)
    {
        GUIRenderer.SetActive(true);
        var unit = sender as BaseUnit;

        UnitName.text = unit.UnitName;
        UnitImage.sprite = unit.uiSprite;
        string hp = "HP: " + unit.HitPoints + "/" + unit.TotalHitPoints;
        if (unit.tag == "Boss")
        {
            UnitName.fontSize = UnitName.fontSize + 100;
            UnitName.font = BossNameFont;
            StatsText.text = hp;
            return;
        }
        string range = "                   RANGE: ";
        int minRange = Mathf.Clamp(unit.MinAttackRange, 1, int.MaxValue);
        if(minRange == unit.MaxAttackRange) {
            range += minRange;
        }
        else
        {
            range += minRange + " - " + unit.MaxAttackRange;
        }

        StatsText.text = unit.Description + "\n\n"
                         + hp + "                     POWER: "
                         + unit.AttackFactor + "\nMOVE: " + unit.getMovementPoints() + range;

        bool unitSelected = false;
        Unit selected = null;

        foreach(Unit u in CellGrid.Units)
        {
            if(u.UnitState == Unit.UnitStateEnum.SELECTED)
            {
                unitSelected = true;
                selected = u;
            }
        }
        if (!unitSelected)
        {
            int temp = 0;
            if (unit.PlayerNumber == 2)
            {
                temp = unit.MovementPoints;
                unit.MovementPoints = unit.getMovementPoints();
            }
            HashSet<Cell> moveRange = unit.GetAvailableDestinations(CellGrid.Cells);
            if (unit.PlayerNumber == 2) unit.MovementPoints = temp;

            List<Cell> attackRange = unit.GetAttackable(unit.MinAttackRange,unit.MaxAttackRange, moveRange);
            foreach (Cell c in moveRange)
            {
                if (unit.PlayerNumber != 2)
                {
                    c.MarkAsReachable();
                }
                else
                {
                    c.MarkAsAttackable();
                }
            }
            foreach (Cell c in attackRange)
            {
                c.MarkAsAttackable();
            }
        }

        if(unit.UnitState == Unit.UnitStateEnum.REACHABLE_ENEMY && selected != null)
        {
            if (((BaseUnit)selected).attacksTP)
            {
                unit.teleportable.enabled = true;
            }
            else
            {
                unit.crosshairs.enabled = true;
            }
            Vector2 c = unit.Cell.OffsetCoord;
            bool selectedOverlapping = (CellGrid.findCell(c.x, c.y - 1).IsTaken) || (CellGrid.findCell(c.x, c.y - 2).IsTaken);
            unit.GetComponent<HealthBar>().HealthForecast(selected.AttackFactor, selectedOverlapping);
        }
    }
    private void OnUnitSelected(object sender, EventArgs e)
    {
        var unit = sender as BaseUnit;
        if (unit.UnitState != Unit.UnitStateEnum.SELECTED) return;
        isSelected = true;
        GUIRenderer.SetActive(true);

        UnitName.text = unit.UnitName;
        string hp = "HP: " + unit.HitPoints + "/" + unit.TotalHitPoints;
        StatsText.text = unit.Description + "\n\n" + hp + "                     POWER: " + unit.AttackFactor
                         + "\nMOVE: " + unit.getMovementPoints() + "                   RANGE: " + unit.MaxAttackRange;
        UnitImage.sprite = unit.uiSprite;
    }
    private void OnUnitDehighlighted(object sender, EventArgs e)
    {
        var unit = sender as BaseUnit;
        if (!isSelected)
        {
            GUIRenderer.SetActive(false);
            foreach (Cell c in CellGrid.Cells)
            {
                c.UnMark();
            }
        }
        unit.GetComponent<HealthBar>().CancelHealthForecast();
        unit.crosshairs.enabled = false;
        unit.teleportable.enabled = false;

        UnitName.font = NormalNameFont;
       if(unit.tag == "Boss") UnitName.fontSize = UnitName.fontSize - 100;
    }
    private void OnUnitDeselected(object sender, EventArgs e)
    {
        isSelected = false;
        GUIRenderer.SetActive(false);
    }
    private void OnTurnEnded(object sender, EventArgs e)
    {
        int nextPlayer = ((sender as CellGrid).CurrentPlayerNumber + 1);
        if (nextPlayer == 1)
        {
            Panel.sprite = OldArmyPanel;
        }
        else
        {
            Panel.sprite = NewArmyPanel;
        }
    }

    private void OnUnitAdded(object sender, UnitCreatedEventArgs e)
    {
        RegisterUnit(e.unit);
    }
    private void RegisterUnit(Transform unit)
    {
        unit.GetComponent<Unit>().UnitHighlighted += OnUnitHighlighted;
        unit.GetComponent<Unit>().UnitDehighlighted += OnUnitDehighlighted;
        unit.GetComponent<Unit>().UnitSelected += OnUnitSelected;
        unit.GetComponent<Unit>().UnitDeselected += OnUnitDeselected;
        unit.GetComponent<Unit>().UnitFinished += OnUnitDeselected;
    }
}
