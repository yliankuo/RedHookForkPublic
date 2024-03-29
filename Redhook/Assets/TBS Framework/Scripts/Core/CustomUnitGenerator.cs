﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomUnitGenerator : MonoBehaviour, IUnitGenerator
{
    public Transform UnitsParent;
    public Transform CellsParent;

    public Unit SpawnNewUnit(Cell cell, Unit unit)
    {
        Unit new_unit = Instantiate(unit, cell.transform.position, new Quaternion());
        if (new_unit.IsCellMovableTo(cell))
        {
            cell.IsTaken = true;
            new_unit.Cell = cell;
            new_unit.Initialize();
            new_unit.transform.SetParent(UnitsParent.transform);
            cell.Occupier.Add(new_unit.gameObject);
            return new_unit;
        }//Unit gets snapped to the nearest cell
        else
        {
            Destroy(new_unit.gameObject);
            return null;
        }
    }

    /// <summary>
    /// Returns units that are already children of UnitsParent object.
    /// </summary>
    public List<Unit> SpawnUnits(List<Cell> cells)
    {
        List<Unit> ret = new List<Unit>();
        for (int i = 0; i < UnitsParent.childCount; i++)
        {
            var unit = UnitsParent.GetChild(i).GetComponent<Unit>();
            if(unit !=null)
            {
                var cell = cells.OrderBy(h => Math.Abs((h.transform.position - unit.transform.position).magnitude)).First();
                if (!cell.IsTaken)
                {
                    cell.IsTaken = true;
                    unit.Cell = cell;
                    unit.transform.position = cell.transform.position;
                    unit.Initialize();
                    cell.Occupier.Add(unit.gameObject);
                    ret.Add(unit);
                }//Unit gets snapped to the nearest cell
                else
                {
                    Destroy(unit.gameObject);
                }//If the nearest cell is taken, the unit gets destroyed.
            }
            else
            {
                Debug.LogError("Invalid object in Units Parent game object");
            }
            
        }
        return ret;
    }

    /// <summary>
    /// Snaps unit objects to the nearest cell.
    /// </summary>
    public void SnapToGrid()
    {
        List<Transform> cells = new List<Transform>();

        foreach(Transform cell in CellsParent)
        {
            cells.Add(cell);
        }

        foreach(Transform unit in UnitsParent)
        {
            var closestCell = cells.OrderBy(h => Math.Abs((h.transform.position - unit.transform.position).magnitude)).First();
            if (!closestCell.GetComponent<Cell>().IsTaken && !closestCell.GetComponent<Cell>().IsBlocked)
            {
                Vector3 offset = new Vector3(0, closestCell.GetComponent<Cell>().GetCellDimensions().y, 0);
                unit.localPosition = closestCell.transform.localPosition + offset;
            }//Unit gets snapped to the nearest cell
        }
    }
}

