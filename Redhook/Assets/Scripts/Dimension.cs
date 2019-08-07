using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dimension : MonoBehaviour
{
    public CellGrid grid; 
    [SerializeField] public int[] playerToDimension;
    public int currentDimension;
    public GameObject[] dimensionEnvironment;
    // Start is called before the first frame update
    void Start()
    {
        grid.TurnEnded += Grid_TurnEnded;
    }

    private void Grid_TurnEnded(object sender, System.EventArgs e)
    {
        setCurrentDimension(playerToDimension[grid.CurrentPlayerNumber]);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("0"))
        {
            setCurrentDimension(0);
        }
        if (Input.GetKeyDown("1"))
        {
            setCurrentDimension(1);
        }
        if (Input.GetKeyDown("2"))
        {
            setCurrentDimension(2);
        }
    }

    public void setCurrentDimension(int x)
    {
        currentDimension = x;

        // Update environment assets
        if(x != 0)
        {
            for (int i = 1; i < dimensionEnvironment.Length; i++)
            {
                dimensionEnvironment[i].SetActive(false);
            }
            dimensionEnvironment[x].SetActive(true);
        }
    }
}
