using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
class CellGridStateUnitSelected : CellGridState
{
    private Unit _unit;
    private HashSet<Cell> _pathsInRange;
    private List<Cell> _attacksInRange;
    private List<Unit> _unitsInRange;

    private Cell _unitCell;

    private List<Cell> _currentPath;

    public CellGridStateUnitSelected(CellGrid cellGrid, Unit unit) : base(cellGrid)
    {
        _unit = unit;
        _pathsInRange = new HashSet<Cell>();
        _currentPath = new List<Cell>();
        _unitsInRange = new List<Unit>();
    }

    private bool DimensionCheck(Cell cell)
    {
        foreach (GameObject g in cell.Occupier)
        {
            if (g.GetComponent<BaseUnit>() != null && (BaseUnit)(_unit) != null && (g.GetComponent<BaseUnit>().Dimension == ((BaseUnit)(_unit)).Dimension || g.GetComponent<BaseUnit>().Dimension == 0))
            {
                return false;
            }
        }
        return true;
    }
    public override void OnCellClicked(Cell cell)
    {
        if (_unit.isMoving)
            return;
        if((cell.IsTaken && !DimensionCheck(cell))|| !_pathsInRange.Contains(cell))
        {
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
            return;
        }
            
        var path = _unit.FindPath(_cellGrid.Cells, cell);
        _unit.Move(cell,path);
        _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
    }

    public override void OnUnitClicked(Unit unit)
    {
        if (_unit.isMoving)
            return;

        if (_unitsInRange.Contains(unit) && _unit.ActionPoints > 0)
        {
            Cell origin = _unit.Cell;
            _unit.MoveAndDealDamageHandler(unit);   
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
            OnStateExit();

        }

        if(unit.Equals(_unit))
        {
            OnStateExit();
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
            return;
        }

        if (unit.PlayerNumber.Equals(_unit.PlayerNumber))
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
        }
            
    }
    public override void OnCellDeselected(Cell cell)
    {
        base.OnCellDeselected(cell);
        foreach(var _cell in _currentPath)
        {
            _pathsInRange = _unit.GetAvailableDestinations(_cellGrid.Cells);
            if (_pathsInRange.Contains(_cell))
            {
                _cell.MarkAsReachable();
            }
            else
                _cell.UnMark();
        }
    }
    public override void OnCellSelected(Cell cell)
    {
        base.OnCellSelected(cell);
        if (!_pathsInRange.Contains(cell)) return;

        _currentPath = _unit.FindPath(_cellGrid.Cells, cell);
        foreach (var _cell in _currentPath)
        {
            _cell.MarkAsPath();
        }
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        _unit.OnUnitSelected();
        _unitCell = _unit.Cell;

        if ((BaseUnit)_unit != null)
        {
            _pathsInRange = ((BaseUnit)_unit).GetAvailableDestinations(_cellGrid.Cells);
        }
        else
        {
            _pathsInRange = _unit.GetAvailableDestinations(_cellGrid.Cells);
        }
        var cellsNotInRange = _cellGrid.Cells.Except(_pathsInRange);

        foreach (var cell in cellsNotInRange)
        {
            cell.UnMark();
        }
        foreach (var cell in _pathsInRange)
        {
            cell.MarkAsReachable();
        }
        if (_unit.MovementPoints != 0) {
            _attacksInRange = _unit.GetAttackable(_unit.MinAttackRange,_unit.MaxAttackRange, _pathsInRange);
        }
        else
        {
            _attacksInRange = _unit.GetAttackable(_unit.MinAttackRange, _unit.MaxAttackRange, _unitCell);
        }
        foreach (var cell in _attacksInRange)
        {
            cell.MarkAsAttackable();
        }

        if (_unit.ActionPoints <= 0)
        {
            OnStateExit();
            return;
        }

        foreach (var currentUnit in _cellGrid.Units)
        {
            if (currentUnit.PlayerNumber != 2) continue;
            if (((BaseUnit)currentUnit).Dimension != ((BaseUnit)_unit).Dimension
                && ((BaseUnit)currentUnit).Dimension != 0) continue;
            if (_unit.isUnitInMovableAttackRange(currentUnit,_unit.Cell))
            {
                currentUnit.SetState(Unit.UnitStateEnum.REACHABLE_ENEMY);
                _unitsInRange.Add(currentUnit);
            }
        }

        if (_unitCell.GetNeighbours(_cellGrid.Cells).FindAll(c => c.MovementCost <= _unit.MovementPoints).Count == 0
            && _unitsInRange.Count == 0 && (_unit.ActionPoints == 0))
        {
            _unit.SetState(Unit.UnitStateEnum.FINISHED);
            OnStateExit();
        }
    }
    public override void OnStateExit()
    {
        _unit.deselecting = true;
        _unit.OnUnitDeselected();
        foreach (var unit in _unitsInRange)
        {
            if (unit == null) continue;
            unit.SetState(Unit.UnitStateEnum.NORMAL);
        }
        foreach (var cell in _cellGrid.Cells)
        {
            cell.UnMark();
        }
        _unit.deselecting = false;
    }
}

