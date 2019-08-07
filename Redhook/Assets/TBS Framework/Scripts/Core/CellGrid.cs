using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

[Serializable]
public struct EnemySpawn
{
    public Vector2 spawn_point;
    public BaseUnit spawn_unit;
    public int dimension;
};

[Serializable]
public struct PickupSpawn
{
    public Vector2 spawn_point;
    public int dimension;
};

[Serializable]
public struct Wave
{
    public EnemySpawn[] spawns;
    public PickupSpawn[] supplySpawns;
    public DialogLine dialogToPlay;
};

/// <summary>
/// CellGrid class keeps track of the game, stores cells, units and players objects. It starts the game and makes turn transitions. 
/// It reacts to user interacting with units or cells, and raises events related to game progress. 
/// </summary>
public class CellGrid : MonoBehaviour
{
    /// <summary>
    /// LevelLoading event is invoked before Initialize method is run.
    /// </summary>
    public event EventHandler LevelLoading;
    /// <summary>
    /// LevelLoadingDone event is invoked after Initialize method has finished running.
    /// </summary>
    public event EventHandler LevelLoadingDone;
    /// <summary>
    /// GameStarted event is invoked at the beggining of StartGame method.
    /// </summary>
    public event EventHandler GameStarted;
    /// <summary>
    /// GameEnded event is invoked when there is a single player left in the game.
    /// </summary>
    public event EventHandler GameEnded;
    /// <summary>
    /// Turn ended event is invoked at the end of each turn.
    /// </summary>
    public event EventHandler TurnEnded;

    /// <summary>
    /// UnitAdded event is invoked each time AddUnit method is called.
    /// </summary>
    public event EventHandler<UnitCreatedEventArgs> UnitAdded;

    
    
    private CellGridState _cellGridState; //The grid delegates some of its behaviours to cellGridState object.
    public CellGridState CellGridState
    {
        private get
        {
            return _cellGridState;
        }
        set
        {
            if(_cellGridState != null)
                _cellGridState.OnStateExit();
            _cellGridState = value;
            _cellGridState.OnStateEnter();
        }
    }

    public int NumberOfPlayers { get; private set; }
    public RectangularSquareGridGenerator gridGenerator;
    private bool endingTurn = false;

    public Player CurrentPlayer
    {
        get { return Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)); }
    }
    public int CurrentPlayerNumber { get; private set; }

    /// <summary>
    /// GameObject that holds player objects.
    /// </summary>
    /// 

    public int[] PlayerToMusicID;

    public Transform PlayersParent;
    public bool_var dialogueOn;

    public List<Player> Players { get; private set; }
    public List<Cell> Cells { get; private set; }
    public List<Unit> Units { get; private set; }

    private Transform[] spawnPreviews;
    public Transform portalPrefab;
    public bool gameOver;
    int maxWave = 0;

    [Header("Wave Settings")]
    public Wave[] waves;
    public int randomizationIndex = -1; //If more or equal to 0, waves past this point will be chosen randomly
    public int next_wave = 0;
    public List<Unit> units_in_wave { get; private set; }

    public void HideSpawnPreview()
    {
        for (int i = 0; i < maxWave; i++)
        {
            spawnPreviews[i].gameObject.SetActive(false);
        }
    }

    public void PreviewNextSpawn()
    {
        HideSpawnPreview();
        for (int i = 0; i < waves[next_wave].spawns.Length; i++)
        {
            if (waves[next_wave].spawns[i].spawn_unit.PlayerNumber == 2)
            {
                spawnPreviews[i].gameObject.SetActive(true);
                spawnPreviews[i].transform.position = findCell(waves[next_wave].spawns[i].spawn_point).transform.position + new Vector3(0, 2.5f, -0.5f);
            }
        }
    }

    public  void SpawnNextWave()
    {
        IUnitGenerator unitGenerator = GetComponent<IUnitGenerator>();
        if (unitGenerator != null && next_wave < waves.Length)
        {
            EnemySpawn[] new_spawns = waves[next_wave].spawns;
            for (int i = 0; i < new_spawns.Length; i++)
            {
                Unit new_unit = unitGenerator.SpawnNewUnit(findCell(new_spawns[i].spawn_point), new_spawns[i].spawn_unit);
                
                if(new_unit)
                {
                    if (new_spawns[i].dimension >= 0 && new_spawns[i].dimension <= 2)
                    {
                        new_unit.GetComponent<BaseUnit>().Dimension = new_spawns[i].dimension;
                    }
                    else
                    {
                        new_unit.GetComponent<BaseUnit>().Dimension = UnityEngine.Random.Range(1,3);
                    }
                    AddUnit(new_unit.transform);
                    if (new_unit.PlayerNumber == 2 && new_unit.tag != "Boss")
                    {
                        units_in_wave.Add(new_unit);
                    }
                    Units.Add(new_unit);
                    BaseUnit bu = (BaseUnit)new_unit;
                    switch (bu.Dimension)
                    {
                        case 1:
                            bu.DimensionMaterial.material.color = bu.OldDimension;
                            break;
                        case 2:
                            bu.DimensionMaterial.material.color = bu.NewDimension;
                            break;
                        case 0:
                        default:
                            bu.DimensionMaterial.material.color = bu.BothDimension;
                            break;

                    }
                }
            }
            foreach( PickupSpawn spawn in waves[next_wave].supplySpawns)
            {
                findCell(spawn.spawn_point).GetComponent<BaseTile>().HasHealingCargo = spawn.dimension;
            }

        }
        
        
    }

    public void MoveToNextWave()
    {
        if (next_wave >= randomizationIndex && randomizationIndex >= 0)
        {
            next_wave = UnityEngine.Random.Range(randomizationIndex, waves.Length);
        }
        else
        {
            next_wave += 1;
        }
    }

    public void OnNextPhaseBegin()
    {
        // Spawn on enemy turn
        if (units_in_wave.Count == 0 && CurrentPlayerNumber == 2)
        {
            SpawnNextWave();
            HideSpawnPreview();
            StartCoroutine(DialogueDelay());
        }
        else
        {
            Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
            Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
            
        }
       
        
    }

    IEnumerator DialogueDelay()
    {
        yield return new WaitForSeconds(1f);
        if (next_wave < waves.Length && !waves[next_wave].dialogToPlay.isNull)
        {
            GameObject.FindGameObjectWithTag("DialogController").GetComponent<DialogueController>().StartDialogue(waves[next_wave].dialogToPlay);
        }
        while(dialogueOn.val)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
        Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
        MoveToNextWave();
        PreviewNextSpawn();
    }

    private void Start()
    {
        if (LevelLoading != null)
            LevelLoading.Invoke(this, new EventArgs());

        Initialize();

        if (LevelLoadingDone != null)
            LevelLoadingDone.Invoke(this, new EventArgs());

        StartGame();

        PreviewNextSpawn();
    }

    private void Initialize()
    {
        for(int i = 0; i < waves.Length; i++)
        {
            maxWave = Mathf.Max(maxWave, waves[i].spawns.Length);
        }
        spawnPreviews = new Transform[maxWave];
        for(int i = 0; i < maxWave; i++)
        {
            spawnPreviews[i] = Instantiate(portalPrefab);
            spawnPreviews[i].gameObject.SetActive(false);
        }

        units_in_wave = new List<Unit>();
        Players = new List<Player>();
        for (int i = 0; i < PlayersParent.childCount; i++)
        {
            var player = PlayersParent.GetChild(i).GetComponent<Player>();
            if (player != null)
                Players.Add(player);
            else
                Debug.LogError("Invalid object in Players Parent game object");
        }
        NumberOfPlayers = Players.Count;
        CurrentPlayerNumber = Players.Min(p => p.PlayerNumber);

        Cells = new List<Cell>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var cell = transform.GetChild(i).gameObject.GetComponent<Cell>();
            if (cell != null)
                Cells.Add(cell);
            else
                Debug.LogError("Invalid object in cells parent game object");
        }

        foreach (var cell in Cells)
        {
            cell.CellClicked += OnCellClicked;
            cell.CellHighlighted += OnCellHighlighted;
            cell.CellDehighlighted += OnCellDehighlighted;
            cell.GetComponent<Cell>().GetNeighbours(Cells);
        }

        var unitGenerator = GetComponent<IUnitGenerator>();
        if (unitGenerator != null)
        {
            Units = unitGenerator.SpawnUnits(Cells);
            foreach (var unit in Units)
            {
                AddUnit(unit.GetComponent<Transform>());
            }
        }
        else
            Debug.LogError("No IUnitGenerator script attached to cell grid");
    }

    private void OnCellDehighlighted(object sender, EventArgs e)
    {
        CellGridState.OnCellDeselected(sender as Cell);
    }
    private void OnCellHighlighted(object sender, EventArgs e)
    {
        CellGridState.OnCellSelected(sender as Cell);
    } 
    private void OnCellClicked(object sender, EventArgs e)
    {
        CellGridState.OnCellClicked(sender as Cell);
    }

    private void OnUnitClicked(object sender, EventArgs e)
    {
        CellGridState.OnUnitClicked(sender as Unit);
    }

    private void OnUnitDestroyed(object sender, AttackEventArgs e)
    {
        BaseUnit killedUnit = (sender as BaseUnit);
        Units.Remove(sender as Unit);
        units_in_wave.Remove(sender as  Unit);
        var totalEnemiesAlive = Units.Where(u => (u.PlayerNumber == 2)).ToList(); //Checking if the game is over
        var totalPlayers1Alive = Units.Where(u => (u.PlayerNumber == 0)).ToList(); //Checking if the game is over
        var totalPlayers2Alive = Units.Where(u => (u.PlayerNumber == 1)).ToList(); //Checking if the game is over

        if ((killedUnit.tag == "Boss")
            || totalPlayers1Alive.Count() == 0
            || totalPlayers2Alive.Count() == 0)
        {
            if(GameEnded != null)
                GameEnded.Invoke(this, new EventArgs());
            gameOver = true;
        }
    }

    /// <summary>
    /// Adds unit to the game
    /// </summary>
    /// <param name="unit">Unit to add</param>
    public void AddUnit(Transform unit)
    {
        unit.GetComponent<Unit>().UnitClicked += OnUnitClicked;
        unit.GetComponent<Unit>().UnitDestroyed += OnUnitDestroyed;
        unit.GetComponent<Unit>().grid = this;
        if(UnitAdded != null)
            UnitAdded.Invoke(this, new UnitCreatedEventArgs(unit)); 
    }

    /// <summary>
    /// Method is called once, at the beginning of the game.
    /// </summary>
    public void StartGame()
    {
        if(GameStarted != null)
            GameStarted.Invoke(this, new EventArgs());

        SoundManager.Instance.FadeInMusic(PlayerToMusicID[CurrentPlayerNumber],0.5f,1.0f);

        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
        Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
    }
    /// <summary>
    /// Method makes turn transitions. It is called by player at the end of his turn.
    /// </summary>
    public void EndTurn()
    {
        if (endingTurn) return;
        endingTurn = true;
        if (Units.Select(u => u.PlayerNumber).Distinct().Count() == 1)
        {
            return;
        }
        CellGridState = new CellGridStateTurnChanging(this);

        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnEnd(); });

        CurrentPlayerNumber = (CurrentPlayerNumber + 1) % NumberOfPlayers;

        if (TurnEnded != null)
            TurnEnded.Invoke(this, new EventArgs());

        if (!gameOver)
        {
            if (PlayerToMusicID[CurrentPlayerNumber] == 3 && GameObject.FindWithTag("Boss") != null)
            {
                SoundManager.Instance.FadeInMusic(4, 0.5f, 1.0f);
            }
            else
            {
                SoundManager.Instance.FadeInMusic(PlayerToMusicID[CurrentPlayerNumber], 0.5f, 1.0f);
            }
        }
        endingTurn = false;
    }

    public Cell findCell(Vector2 OffsetCoor)
    {
        return Cells[(int)((OffsetCoor.y * gridGenerator.Width) + OffsetCoor.x)];
    }

    public Cell findCell(float x, float y)
    {
        int ind = (int)((y * gridGenerator.Width) + x);

        if (ind <= 0 || ind >= Cells.Count || x >= this.GetComponent<RectangularSquareGridGenerator>().Width || y >= this.GetComponent<RectangularSquareGridGenerator>().Height) return null;

        return Cells[ind];
    }
}
