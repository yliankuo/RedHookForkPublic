using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using cakeslice;

public class BaseUnit : Unit
{
    public GameObject obj;
    public bool CurrentlyVisible;
    public bool useInvisibility;
    public string UnitName;
    public string Description;
    public Sprite uiSprite;
    public SpriteRenderer crosshairs;
    public SpriteRenderer teleportable;
    public bool AttackOnlyStraightLine = false;
    public Color NormalColor;
    public Color FinishedColor;
    public Color DamagedColor;
    public Color AttackColor;
    public Color OldDimension;
    public Color NewDimension;
    public Color BothDimension;
    public GameObject DimensionIndicator;
    public Renderer DimensionMaterial;
    public bool OwnersTurn;
    private Renderer UnitRenderer;
    private Renderer[] ChildRenderers;
    public bool RespectDimensions = true;
    public int Dimension;
    private Camera cam;
    private Collider[] colls;
    public bool attacksTP = false;
    public bool attacksStun = false;
    public bool attacksParasite = false;
    private BaseUnit parasiteHost = null;
    public int turnsStunned = 0;
    public int turnsParasited = 0;
    private BaseUnit parasite = null;
    public bool requiresDimAttacks = false;
    private List<BaseUnit> attackers;
    private Vector3 originalposition;
    public float offset = 0.25f;
    public float lerp = 0.025f;
    public ParticleSystem on_hit_particles;
    public ParticleSystem on_attack_long_particles;
    public ParticleSystem on_attack_particles;
    public ParticleSystem on_teleport_new_particles;
    public ParticleSystem on_teleport_old_particles;
    public ParticleSystem on_death_particles;
    public ParticleSystem on_heal_particles;
    private GameObject placeholder;

    //Sounds
    public AudioSource audioSource;
    public AudioClip take_damage_sfx = null;
    public AudioClip move_sfx = null;
    public AudioClip attack_sfx = null;
    public AudioClip take_heal_sfx = null;
    public AudioClip give_heal_sfx = null;
    public AudioClip give_teleport_sfx = null;
    public AudioClip take_teleport_sfx = null;
    public AudioClip death_sfx = null;



    public override void Initialize()
    {
        audioSource = GetComponent<AudioSource>();
        colls = GetComponents<Collider>();
        obj = GameObject.FindObjectOfType<Dimension>().gameObject;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("No \"MainCamera\" tagged camera detected on level! Things will break!");
        }

        UnitRenderer = this.GetComponent<Renderer>();
        Debug.Log(UnitRenderer);
        ChildRenderers = GetComponentsInChildren<Renderer>();
        base.Initialize();
        transform.position += new Vector3(0, 0.5f, 0);
        UnitRenderer.material.color = NormalColor;
        attackers = new List<BaseUnit>();
        originalposition = this.GetComponentInChildren<SpriteRenderer>().transform.localPosition;
        placeholder = Resources.Load("ParasitePlaceholder") as GameObject;
        this.GetComponentInChildren<Outline>().enabled = false;

        crosshairs.enabled = false;
        teleportable.enabled = false;
        DimensionMaterial = DimensionIndicator.GetComponent<Renderer>();
        switch (this.Dimension)
        {
            case 1:
                DimensionMaterial.material.color = OldDimension;
                break;
            case 2:
                DimensionMaterial.material.color = NewDimension;
                break;
            case 0:
            default:
                DimensionMaterial.material.color = BothDimension;
                break;

        }
    }

    public void playSFX(AudioClip sfx)
    {
        if(sfx != null)
        {
            audioSource.clip = sfx;
            audioSource.Play();
        }
    }

    public void Stun(BaseUnit other)
    {
        other.turnsStunned++;
    }
  
    public void onTeleported()
    {
        //plays take Teleport sound
        
       //if(on_teleport_particles) Instantiate(on_teleport_particles, transform.position+new Vector3(0,10,-4), new Quaternion());
        audioSource.clip = take_teleport_sfx;
        audioSource.Play();
    }
   
    public void Leach(BaseUnit other)
    {
        parasiteHost = other;
        other.parasite = this;
        other.turnsParasited++;
    }
    public void resolveOccupiedCell(BaseUnit other, int dimension)
    {
        Cell c = other.Cell;
        c.Occupier.Remove(other.gameObject);
        c.IsTaken = c.Occupier.Count > 0;
        other.Dimension = dimension;

        if (c.Occupier.Count == 0)
        {
            c.IsTaken = true;
            c.Occupier.Add(other.gameObject);
            other.transform.position =  new Vector3(c.transform.position.x, other.transform.position.y, c.transform.position.z);
        }
        else
        {
            int tmp = MovementPoints;
            MovementPoints = 4;
            foreach (Cell dc in other.GetAvailableDestinations(grid.Cells))
            {
                MovementPoints = tmp;
                if (other.IsCellMovableTo(dc))
                {
                    dc.IsTaken = true;
                    dc.Occupier.Add(other.gameObject);
                    other.transform.position = new Vector3(dc.transform.position.x, other.transform.position.y, dc.transform.position.z);
                    other.Cell = dc;
                    return;
                }
            }
        }
    }

    public void Teleport(BaseUnit other)
    {
        switch (other.Dimension)
        {
            case 0:
                //Plays attack sound?
                playSFX(attack_sfx);
                break;
            case 1:
                //Plays give teleport sound
                playSFX(give_teleport_sfx);
                resolveOccupiedCell(other, 2);
                other.DimensionMaterial.material.color = NewDimension;
                if (on_teleport_new_particles) Instantiate(on_teleport_new_particles, other.transform.position + new Vector3(0, 10, -4), new Quaternion());
                other.onTeleported();
                break;
            case 2:
                //Plays give teleport sound
                playSFX(give_teleport_sfx);
                resolveOccupiedCell(other, 1);
                other.DimensionMaterial.material.color = OldDimension;
                if (on_teleport_old_particles) Instantiate(on_teleport_old_particles, other.transform.position + new Vector3(0, 10, -4), new Quaternion());
                other.onTeleported();
                break;
        }
        Stun(other);
    }

	private IEnumerator FlashWhite(float duration)
	{
		SpriteRenderer renderer = this.GetComponentInChildren<SpriteRenderer>();
		renderer.material.shader = Shader.Find("GUI/Text Shader");
		renderer.color = Color.white;
		yield return new WaitForSeconds(duration);
		renderer.material.shader = Shader.Find("Sprites/Default");
	}

    public override void DealDamage(Unit other)
    {
        if (((BaseUnit) other).requiresDimAttacks){
            if (isMoving)
                return;
            if (ActionPoints == 0)
                return;
            if (!IsUnitAttackable(other, Cell))
                return;

            MarkAsAttacking(other);
            ActionPoints--;

            if (ActionPoints == 0)
            {
                SetState(UnitStateEnum.FINISHED);
                MovementPoints = 0;
            }
            //Plays attack sound;
            playSFX(attack_sfx);

            ((BaseUnit)other).attackers.Add(this);
            other.GetComponent<HealthBar>().HoldHealth(this.AttackFactor);

            if(on_attack_long_particles) Instantiate(on_attack_long_particles, transform.position, new Quaternion());
            if(on_attack_particles) Instantiate(on_attack_particles, other.transform.position, new Quaternion());
            Transform sprite = other.GetComponentInChildren<SpriteRenderer>().transform;
            Transform self = this.GetComponentInChildren<SpriteRenderer>().transform;
            sprite.localPosition += offset * Vector3.Normalize(sprite.transform.position - self.transform.position);
        }
        else
        {
            if (attacksParasite)
            {
                if(parasiteHost == null)
                {
                    Leach((BaseUnit)other);
                    switch (this.Dimension)
                    {
                        case 0:
                            break;
                        case 1:
                            //Plays take teleport sfx?
                            playSFX(take_teleport_sfx);
                            resolveOccupiedCell(this, 2);
                            break;
                        case 2:
                            //Plays take teleport sfx?
                            playSFX(take_teleport_sfx);
                            resolveOccupiedCell(this, 1);
                            break;
                    }
                    var parasiteplaceholder = Instantiate(placeholder,this.GetComponentInChildren<SpriteRenderer>().transform.position,Quaternion.identity);
                    parasiteplaceholder.transform.parent = this.transform;
                    var hostplaceholder = Instantiate(placeholder, other.GetComponentInChildren<SpriteRenderer>().transform.position, Quaternion.identity);
                    hostplaceholder.transform.parent = other.transform;
                }
                else
                {
                    Leach(parasiteHost);
                }
            }
            else
            {
                //Plays attack sound;
                playSFX(attack_sfx);
                Transform sprite = other.GetComponentInChildren<SpriteRenderer>().transform;
                Transform self = this.GetComponentInChildren<SpriteRenderer>().transform;
                Vector3 AttackDir = Vector3.Normalize(sprite.transform.position - self.transform.position);
                float AttackAngle = Mathf.Rad2Deg * Mathf.Atan2(-AttackDir.z, AttackDir.x);
                 if(on_attack_long_particles) Instantiate(on_attack_long_particles, self.transform.position, Quaternion.Euler(0,AttackAngle + 90,0));
                 if(on_attack_particles) Instantiate(on_attack_particles, sprite.transform.position, new Quaternion());
                base.DealDamage(other);
            }
        }

        if (other.HitPoints > 0)
        {
            if (attacksStun) Stun((BaseUnit)other);
            if (attacksTP) Teleport((BaseUnit)other);
            if (((BaseUnit)other).parasiteHost != null && AttackFactor > 0)
            {
                ((BaseUnit)other).parasiteHost = null;
            }
        }
    }
    protected override void OnDestroyed()
    {
        if(this.parasite != null)
        {
            this.parasite.parasiteHost = null;
            Destroy(this.parasite.transform.Find("ParasitePlaceholder(Clone)").gameObject);
        }
        if(this.parasiteHost != null)
        {
            Destroy(this.parasiteHost.transform.Find("ParasitePlaceholder(Clone)").gameObject);
        }
        //Play Death sound
        playSFX(death_sfx);
        base.OnDestroyed();
    }
    protected override void Defend(Unit other, int damage)
    {
        Transform sprite = this.GetComponentInChildren<SpriteRenderer>().transform;
        Transform self = other.GetComponentInChildren<SpriteRenderer>().transform;
        sprite.localPosition += offset * Vector3.Normalize(sprite.transform.position - self.transform.position);
        this.GetComponentInChildren<SpriteRenderer>().transform.localPosition = new Vector3(sprite.localPosition.x, sprite.localPosition.y, sprite.localPosition.z);
		StartCoroutine(FlashWhite(0.2f));
        //plays take damage sfx
        playSFX(take_damage_sfx);
        base.Defend(other, damage);
    }
    public override void OnUnitSelected()
    {
        if (CurrentlyVisible)
        {
            base.OnUnitSelected();
        }
    }

    public override void OnUnitDeselected()
    {
        base.OnUnitDeselected();
		this.GetComponentInChildren<Outline>().enabled = false;
    }

    public bool canInteractDimension(BaseUnit interacter, BaseUnit target)
    {
        return !interacter.RespectDimensions || interacter.Dimension == 0 || target.Dimension == 0 || interacter.Dimension == target.Dimension;
    }


    public override bool Move(Cell destinationCell, List<Cell> path)
    {
        bool didMove = false;
        if (CurrentlyVisible)
        {
            didMove = didMove || base.Move(destinationCell,path);
        }

        if (didMove)
        {
            //Play moving sfx;
            playSFX(move_sfx);
        }
        return didMove;
    }


    protected override void OnMouseDown()
    {
        if (CurrentlyVisible)
        {
            base.OnMouseDown();
        }
    }
    protected override void OnMouseEnter()
    {
        if (CurrentlyVisible)
        {
            base.OnMouseEnter();
        }
    }
    protected override void OnMouseExit()
    {
        if (CurrentlyVisible)
        {
            base.OnMouseExit();
        }
    }

    public override bool IsCellMovableTo(Cell cell)
    {
        if (cell.IsBlocked)
        {
            return false;
        }
        if (!cell.IsTaken)
        {
            return true;
        }
        else
        {
            foreach(GameObject g in cell.Occupier){
                if (g != null && g.GetComponent<BaseUnit>() != null && canInteractDimension(this, g.GetComponent<BaseUnit>())){
                    return false;
                }

            }
            return false;
        }
    }

    public override HashSet<Cell> GetAvailableDestinations(List<Cell> cells)
    {
        var cachedPaths = new Dictionary<Cell, List<Cell>>();

        var paths = cachePaths(cells);
        foreach (var key in paths.Keys)
        {
            if (!IsCellMovableTo(key))
            {
                continue;
            }
            var path = paths[key];

            var pathCost = path.Sum(c => c.MovementCost);
            if (pathCost <= MovementPoints)
            {
                cachedPaths.Add(key, path);
            }
        }
        return new HashSet<Cell>(cachedPaths.Keys);
    }

    public override void OnTurnStart()
    {
        if (requiresDimAttacks)
        {
            bool d1 = false;
            bool d2 = false;
            int totdmg = 0;
            foreach(BaseUnit bu in attackers){
                totdmg += bu.AttackFactor;
                switch (bu.Dimension) {
                    case 1:
                        d1 = true;
                        break;
                    case 2:
                        d2 = true;
                        break;
                }
                
            }
            if (d1 && d2)
            {
                Defend(this, totdmg);
            }
            this.GetComponent<HealthBar>().CancelHoldHealth();
        }
        attackers = new List<BaseUnit>();
        
        if(turnsParasited > 0)
        {
            turnsParasited--;
            Defend(parasite, parasite.AttackFactor);
        }
        if(parasiteHost != null)
        {
            DealDamage(parasiteHost);
            ActionPoints = 0;
            MovementPoints = 0;
            SetState(UnitStateEnum.FINISHED);
        }
        base.OnTurnStart();
        if (turnsStunned > 0)
        {
            turnsStunned--;
            ActionPoints = 0;
            MovementPoints = 0;
            SetState(UnitStateEnum.FINISHED);
        }
    }
    public override bool IsCellTraversable(Cell cell)
    {
        if (cell.IsBlocked)
        {
            return false;
        }

        if (!cell.IsTaken)
        {
            return true;
        }
        else
        {
            foreach (GameObject g in cell.Occupier)
            {
                if (g != null && g.GetComponent<BaseUnit>() != null && canInteractDimension(this, g.GetComponent<BaseUnit>()))
                {
                    return false;
                }

            }
            return true;
        }
    }

    public override bool isUnitInMovableAttackRange(Unit other, Cell sourceCell)
    {
        if (other.Cell.GetComponent<BaseTile>().IsAttackable || sourceCell.GetDistance(other.Cell) <= MovementPoints)
        {

            return canInteractDimension(this, (BaseUnit)other);

        }

        return false;
    }

    public override bool IsUnitAttackable(Unit other, Cell sourceCell)
    {
        if (sourceCell.GetDistance(other.Cell) >= MinAttackRange && sourceCell.GetDistance(other.Cell) <= MaxAttackRange)
        {   
            if (AttackOnlyStraightLine)
            {
                if ((int)sourceCell.OffsetCoord.x == (int)other.Cell.OffsetCoord.x
                    || (int)sourceCell.OffsetCoord.y == (int)other.Cell.OffsetCoord.y)
                {
                    return canInteractDimension(this, (BaseUnit)other);
                }
            }
            else
            {
                return canInteractDimension(this, (BaseUnit)other);
            }
        }
   

        return false;
    }

    void Update()
    {
        CurrentlyVisible = obj.GetComponent<Dimension>().currentDimension == Dimension || Dimension == 0 || obj.GetComponent<Dimension>().currentDimension == 0;
        if(useInvisibility) UnitRenderer.enabled = CurrentlyVisible;
        if(CurrentlyVisible)
        {
            UnitRenderer.material.color = new Color(UnitRenderer.material.color.r,
                                                    UnitRenderer.material.color.g,
                                                    UnitRenderer.material.color.b,
                                                    1.0f);
        }
        else
        {
            UnitRenderer.material.color = new Color(UnitRenderer.material.color.r,
                                                    UnitRenderer.material.color.g,
                                                    UnitRenderer.material.color.b,
                                                    0.2f);
        }
        for (int i = 0; i < colls.Length; i++)
        {
            colls[i].enabled = CurrentlyVisible;
        }
        for (int i = 0; i < ChildRenderers.Length; i++)
        {
            if (useInvisibility) ChildRenderers[i].enabled = CurrentlyVisible;
            //ChildRenderers[i].enabled = CurrentlyVisible;
            if (CurrentlyVisible)
            {
                ChildRenderers[i].material.color = new Color(ChildRenderers[i].material.color.r,
                                                        ChildRenderers[i].material.color.g,
                                                        ChildRenderers[i].material.color.b,
                                                        1.0f);
            }
            else
            {
                ChildRenderers[i].material.color = new Color(ChildRenderers[i].material.color.r,
                                                        ChildRenderers[i].material.color.g,
                                                        ChildRenderers[i].material.color.b,
                                                        0.4f);
            }
        }
        if(this.GetComponentInChildren<SpriteRenderer>().transform.localPosition != originalposition)
        {
            this.GetComponentInChildren<SpriteRenderer>().transform.localPosition = Vector3.Lerp(GetComponentInChildren<SpriteRenderer>().transform.localPosition, originalposition, lerp);
        }
    }

    protected override void OnDestroyedFinish()
    {
        if (on_death_particles) Instantiate(on_death_particles, transform.position, new Quaternion());
        CameraShake.Instance.ShakeCamera(0.2f, 0.05f);
        base.OnDestroyedFinish();
    }

    public override void MarkAsFinished()
    {
        UnitRenderer.material.color = FinishedColor;
        this.GetComponentInChildren<SpriteRenderer>().color = new Color(FinishedColor.r, FinishedColor.g, FinishedColor.b, 255f);
		this.GetComponentInChildren<Outline>().enabled = false;
    }

    // Apply after unit is killed
    public override void MarkAsDestroyed()
    {
        UnitRenderer.material.color = DamagedColor;
    }

    // Apply when unit is attacking
    public override void MarkAsAttacking(Unit u)
    {
        UnitRenderer.material.color = AttackColor;
    }

    protected override void Heal(int HealingFactor)
    {
        if(on_heal_particles) Instantiate(on_heal_particles, transform.position+new Vector3(0,10,-5),  Quaternion.Euler(90,0,0));
        base.Heal(HealingFactor);
    }

    // Apply when unit is attacked
    public override void MarkAsDefending(Unit u)
    {
        if(on_hit_particles) Instantiate(on_hit_particles, transform.position + new Vector3(0,3,-0.2f), new Quaternion());
        UnitRenderer.material.color = DamagedColor;
        Vector2 c = this.Cell.OffsetCoord;
        if (grid.findCell(c.x, c.y - 1).IsTaken || grid.findCell(c.x, c.y - 2).IsTaken) this.GetComponent<HealthBar>().MoveHealthbarUp();
        crosshairs.enabled = false;
        teleportable.enabled = false;
    }

    // Apply at start of turn for all units of current army being moved;
    // The default state
    public override void MarkAsFriendly()
    {
        this.GetComponent<HealthBar>().MoveHealthbarDown();
        UnitRenderer.material.color = NormalColor;
    }

    // Apply when a unit IN RANGE is an attackable enemy
    public override void MarkAsReachableEnemy()
    {
        UnitRenderer.material.color = NormalColor;
        this.GetComponentInChildren<SpriteRenderer>().color = new Color(Color.red.r, Color.red.g, Color.red.b, 255f);
    }

    // Apply when unit has been clicked on to be selected to move
    public override void MarkAsSelected()
    {
        UnitRenderer.material.color = NormalColor;
		this.GetComponentInChildren<Outline>().enabled = true;
    }

    // Apply when unit is not currently movable (off turns)
    public override void UnMark()
    {
        this.GetComponent<HealthBar>().MoveHealthbarDown();
        UnitRenderer.material.color = NormalColor;
        this.GetComponentInChildren<SpriteRenderer>().color = new Color(Color.white.r, Color.white.g, Color.white.b, 255f);
        crosshairs.enabled = false;
        teleportable.enabled = false;
    }

}
