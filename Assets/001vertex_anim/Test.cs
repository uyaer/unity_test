using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Transform prefab;
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                Transform cube = Instantiate(prefab);
                cube.position = new Vector3(i, 0, j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
