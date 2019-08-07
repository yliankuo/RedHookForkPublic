using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float drawBelowUnit;
    public float drawSizeX;
    public float drawSizeY;
    public float spacingSize;
    public Color backingColor;
    public Color currentHealthColor;
    public Color lostHealthColor;
    public Color forecastHealthColor;
    public Color heldHealthColor;

    private float projectionHeight = 50.0f;
    private float projectAbove = 2.3f;
    private int totalHealth;
    private int currentHealth;
    private int heldHealth = 0;
    private GameObject backing;
    private Dictionary<int, GameObject> healthUnits = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        totalHealth = this.GetComponent<Unit>().HitPoints;
        currentHealth = totalHealth;
        drawInitial();
    }

    // Draws the initial health bar
    private void drawInitial()
    {
        // Set up backing
        backing = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backing.transform.SetParent(this.transform);
        backing.GetComponent<Renderer>().material.color = backingColor;
        backing.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        backing.transform.localPosition = new Vector3(0.0f, projectionHeight + 190.0f, -1.0f * drawBelowUnit);
        backing.transform.localScale = new Vector3(drawSizeX, drawSizeY, 1.0f);
        backing.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

        // Set up discrete health units
        // Enumerate each health unit left to right, indexed 1 to totalHealth
        float healthSizeX = ((drawSizeX - spacingSize) / totalHealth) - spacingSize;
        float healthSizeY = drawSizeY - (2.0f * spacingSize);
        float xPos = (-1 * drawSizeX / 2.0f) + spacingSize + (healthSizeX / 2.0f);
        for (int i = 1; i <= totalHealth; i++)
        {

            GameObject healthUnit = GameObject.CreatePrimitive(PrimitiveType.Quad);
            healthUnit.transform.SetParent(this.transform);
            healthUnit.GetComponent<Renderer>().material.color = currentHealthColor;
            healthUnit.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            healthUnit.transform.localPosition = new Vector3(xPos, projectionHeight + 190.0f + 0.02f, -1.0f * drawBelowUnit + 0.002f);
            healthUnit.transform.localScale = new Vector3(healthSizeX, healthSizeY, 1.0f);
            healthUnit.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

            healthUnits.Add(i, healthUnit);
             
            xPos += healthSizeX + spacingSize;
        }
    }

    void Update()
    {
        BaseUnit unit = GetComponent<BaseUnit>();
        if (unit != null)
        {
            backing.GetComponent<Renderer>().enabled = unit.CurrentlyVisible;

            for (int i = 0; i < totalHealth+1; i++)
            {
                GameObject healthUnit;
                bool found = healthUnits.TryGetValue(i, out healthUnit);
                if (found)
                {
                    healthUnit.GetComponent<Renderer>().enabled = unit.CurrentlyVisible;
                }
            }
        }
    }
    // Update health to given amount
    public void UpdateHealth(int x)
    {
        if(x < currentHealth)
        {
            SubtractHealth(currentHealth - x);
        }
        if(x > currentHealth)
        {
            AddHealth(x - currentHealth);
        }
        currentHealth = x;
    }

    // Subtract amount x from health
    private void SubtractHealth(int x)
    {
        for(int i = 0; i < x; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(currentHealth - i, out healthUnit);
            if(found)
            {
                healthUnit.GetComponent<Renderer>().material.color = lostHealthColor;
            }
        }
    }

    // Add amount x to health
    private void AddHealth(int x)
    {
        for (int i = 1; i <= currentHealth + x; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = currentHealthColor;
            }
        }
    }

    public void MoveHealthbarUp()
    {
        if (backing == null) return;
        backing.transform.localPosition = new Vector3(0.0f, projectionHeight + 300.0f, projectAbove);
        for (int i = 1; i <= totalHealth; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.transform.localPosition = new Vector3(healthUnit.transform.localPosition.x,
                                                                projectionHeight + 300.0f + 0.02f,
                                                                projectAbove + 0.002f);
            }
        }
    }

    public void MoveHealthbarDown()
    {
        if (backing == null) return;
        backing.transform.localPosition = new Vector3(0.0f, projectionHeight + 190.0f, -1.0f * drawBelowUnit);
        for (int i = 1; i <= totalHealth; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.transform.localPosition = new Vector3(healthUnit.transform.localPosition.x,
                                                                projectionHeight + 190.0f + 0.02f,
                                                                -1.0f * drawBelowUnit + 0.002f);
            }
        }
    }

    public void HealthForecast(int x, bool attackerOverlapping)
    {
        for (int i = 0; i < x; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(currentHealth - heldHealth - i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = forecastHealthColor;
            }
        }

        if (attackerOverlapping) MoveHealthbarUp();
    }

    public void CancelHealthForecast()
    {
        for (int i = 1; i <= currentHealth - heldHealth; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = currentHealthColor;
            }
        }
        for(int i = currentHealth - heldHealth + 1; i < currentHealth; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = heldHealthColor;
            }
        }
        MoveHealthbarDown();
    }

    public void HoldHealth(int x)
    {
        for (int i = 0; i < x; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(currentHealth - heldHealth - i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = heldHealthColor;
            }
        }
        heldHealth += x;
    }

    public void CancelHoldHealth()
    {
        heldHealth = 0;
        for (int i = 1; i <= currentHealth; i++)
        {
            GameObject healthUnit;
            bool found = healthUnits.TryGetValue(i, out healthUnit);
            if (found)
            {
                healthUnit.GetComponent<Renderer>().material.color = currentHealthColor;
            }
        }
    }
}
