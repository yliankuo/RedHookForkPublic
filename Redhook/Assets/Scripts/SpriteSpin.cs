using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSpin : MonoBehaviour
{
    public float spinSpeed;
    public bool counterClockwise;
    private float angleChange;
    // Start is called before the first frame update
    void Start()
    {
        if (counterClockwise)
        {
            angleChange = spinSpeed / 10.0f;
        }
        else
        {
            angleChange = -1.0f * spinSpeed / 10.0f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,
                                                      this.transform.localEulerAngles.y,
                                                      this.transform.localEulerAngles.z + angleChange);
    }
}
