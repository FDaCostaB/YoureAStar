using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRandomly : MonoBehaviour
{
    private float lastUpdate = 0;
    // Start is called before the first frame update
    void Start()
    {
        lastUpdate = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - lastUpdate > 0.1)
        {
            lastUpdate = Time.time;
            transform.position += new Vector3(1, 0, 1);

        }
    }
}
