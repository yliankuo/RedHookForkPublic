using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BaseTile : Square
{
    public Color NormalColor;
    public Color HighlightedColor;
    public Color ReachableColor;
    public Color AttackableColor;

    public SpriteRenderer uiIndicator;
    public SpriteRenderer sprite;
    public Sprite movepathSprite;
    public Sprite enemySpawnSprite;

    public bool IsAttackable;

    public void Start()
    {
        sprite.color = NormalColor;
    }

    public override Vector3 GetCellDimensions()
    {
        return GetComponent<Renderer>().bounds.size;
    }
    public override void MarkAsHighlighted()
    {
        sprite.color = HighlightedColor;
    }
    public override void MarkAsPath()
    {
        uiIndicator.enabled = true;
        uiIndicator.sprite = movepathSprite;
    }
    public override void MarkAsReachable()
    {
        uiIndicator.enabled = false;
        sprite.color = ReachableColor;
    }
    public override void MarkAsAttackable()
    {
        IsAttackable = true;
        sprite.color = AttackableColor;
        uiIndicator.enabled = false;
    }
    public override void UnMark()
    {
        IsAttackable = false;
        sprite.color = NormalColor;
        uiIndicator.enabled = false;
    }
}
