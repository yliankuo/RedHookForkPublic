using System;
using UnityEngine;
using UnityEngine.UI;

public class TerrainGUIController : MonoBehaviour
{
    public CellGrid CellGrid;
    public GameObject GUIRenderer;

    public TextMesh StatsText;

    void Awake()
    {
        CellGrid.UnitAdded += OnUnitAdded;
        CellGrid.GameStarted += OnGameStarted;
    }

    private void OnGameStarted(object sender, EventArgs e)
    {
        foreach (Transform cell in CellGrid.transform)
        {
            cell.GetComponent<Cell>().CellHighlighted += OnCellHighlighted;
            cell.GetComponent<Cell>().CellDehighlighted += OnCellDehighlighted;
        }
    }
    private void OnCellHighlighted(object sender, EventArgs e)
    {
        GUIRenderer.SetActive(true);
        StatsText.text = "Movement Cost: " + (sender as Cell).MovementCost + "        Buff: ";
    }
    private void OnCellDehighlighted(object sender, EventArgs e)
    {
        GUIRenderer.SetActive(false);
        StatsText.text = "";
    }

    private void OnUnitHighlighted(object sender, EventArgs e)
    {
        GUIRenderer.SetActive(true);
        StatsText.text = "Movement Cost: " + (sender as Unit).Cell.MovementCost + "        Buff: ";
    }

    private void OnUnitAdded(object sender, UnitCreatedEventArgs e)
    {
        RegisterUnit(e.unit);
    }
    private void RegisterUnit(Transform unit)
    {
        unit.GetComponent<Unit>().UnitHighlighted += OnUnitHighlighted;
    }
}
