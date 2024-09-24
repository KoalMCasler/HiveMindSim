using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public List<GameObject> hivePop;
    public List<GameObject> genPop;
    public List<GameObject> buildings;
    public float clearMindSpawnCount;
    public float infectedSpawnCount;

    void Awake()
    {
        if(gameManager != null)
        {
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
            gameManager = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Initialize();  
    }

    public void Initialize()
    {
        GetBuildings();
        GetPopulation();
        for(int i = 0; i < infectedSpawnCount; i++)
        {
            PickInfected();
        }
        for(int i = 0; i < clearMindSpawnCount; i++)
        {
            PickClearMind();      
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void GetBuildings()
    {
        buildings.Clear();
        GameObject[] buildingArray = GameObject.FindGameObjectsWithTag("Building");
        if(buildingArray[0] != null)
        {
            foreach(GameObject i in buildingArray)
            {
                if(i != this.gameObject)
                {
                    buildings.Add(i);
                }
            }
        }
    }

    void GetPopulation()
    {
        hivePop.Clear();
        genPop.Clear();
        GameObject[] people = GameObject.FindGameObjectsWithTag("Person");
        if(people[0] != null)
        {
            foreach(GameObject i in people)
            { 
                if(i.GetComponent<HiveMind>().mindState == HiveMind.MindState.singleMind || i.GetComponent<HiveMind>().mindState == HiveMind.MindState.clearMind)
                {
                    genPop.Add(i);
                }
            }
        }
    }

    void PickClearMind()
    {
        GameObject target = genPop[Random.Range(0,genPop.Count)];
        if(target.GetComponent<HiveMind>().mindState != HiveMind.MindState.clearMind)
        {
            target.GetComponent<HiveMind>().mindState = HiveMind.MindState.clearMind;
            target.GetComponent<HiveMind>().actionState = HiveMind.ActionState.searching;
        }
        else
        {
            PickClearMind();
        }
    }

    void PickInfected()
    {
        GameObject target = genPop[Random.Range(0,genPop.Count)];
        if(target.GetComponent<HiveMind>().mindState != HiveMind.MindState.clearMind)
        {
            target.GetComponent<HiveMind>().mindState = HiveMind.MindState.hiveMind;
            genPop.Remove(target);
            hivePop.Add(target);
        }
        else
        {
            PickInfected();
        }
    }
}
