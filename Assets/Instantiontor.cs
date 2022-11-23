using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiontor : MonoBehaviour
{
    public GameObject myPrefab;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100000; i++)
        {
            
            Instantiate(myPrefab, new Vector3(Random.Range(0f, 5000.0f), 1, Random.Range(0.0f, 5000.0f)), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += new Vector3(1, 0, 0);
    }
}
