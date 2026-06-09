using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class Lv1_Init : MonoBehaviour
{
    public GameObject agileMan;
    public GameObject strongMan;
    public Vector3 StrongManSpawnPoint;
    public Vector3 AgileManSpawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StrongManSpawnPoint = new Vector3(5.0f, 0.0f, 0.0f);
        AgileManSpawnPoint = new Vector3(0.0f, 0.0f, 0.0f);
        Instantiate(agileMan, AgileManSpawnPoint, Quaternion.identity);
        Instantiate(strongMan, StrongManSpawnPoint, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
