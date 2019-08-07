using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class supply_script : MonoBehaviour
{
    BaseTile tile;
    SpriteRenderer sprite;
    public GameObject obj;

    [SerializeField] Sprite old_tec;
    [SerializeField] Sprite new_tec;
    // Start is called before the first frame update
    void Start()
    {
        
        tile = GetComponentInParent<BaseTile>();
        sprite = GetComponent<SpriteRenderer>();
        obj = GameObject.FindObjectOfType<Dimension>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        bool CurrentlyVisible = obj.GetComponent<Dimension>().currentDimension == tile.HasHealingCargo || tile.HasHealingCargo == 0 || obj.GetComponent<Dimension>().currentDimension == 0;
        if (tile.HasHealingCargo == -1)
        {
            
            sprite.enabled = false;
        }
        else if (tile.HasHealingCargo == 0)
        {
            int dimension = obj.GetComponent<Dimension>().currentDimension;
            if (dimension == 1)
            {
                sprite.sprite = old_tec;
                sprite.enabled = CurrentlyVisible;
            }
            else if (dimension == 2)
            {
                sprite.sprite = new_tec;
                sprite.enabled = CurrentlyVisible;
            }

        }
        else if (tile.HasHealingCargo == 1)
        {
            sprite.sprite = old_tec;
            sprite.enabled = CurrentlyVisible;
        }
        else if (tile.HasHealingCargo == 2)
        {
            sprite.sprite = new_tec;
            sprite.enabled = CurrentlyVisible;
        }
    }
}
