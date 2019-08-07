using System.Collections.Generic;

public interface IUnitGenerator
{ 
     Unit SpawnNewUnit(Cell location, Unit unit);
     List<Unit> SpawnUnits(List<Cell> cells);
}

