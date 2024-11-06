using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public List<GameObject> hivePop;
    public List<GameObject> genPop;
    public List<GameObject> buildings;
    public GameObject[] spawnPoints;
    public float clearMindSpawnCount;
    public float infectedSpawnCount;
    public NavMeshSurface surface;
    public GameObject person;
    public int maxPopulation;
    private int population;
    public float spawnFrequency;

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
        //Initialize();  
    }

    public void Initialize()
    {
        surface = GameObject.FindGameObjectWithTag("Surface").GetComponent<NavMeshSurface>();
        GetBuildings();
        surface.BuildNavMesh();
        StartCoroutine(MakePeople());
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
        spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
        if(spawnPoints[0] != null)
        {
            maxPopulation = spawnPoints.Count();
            genPop.Add(Instantiate(person, spawnPoints[population].transform.position, spawnPoints[population].transform.rotation));
            population ++;
            if(population < maxPopulation)
            {
                StartCoroutine(MakePeople());
            }
            if(population >= maxPopulation)
            {
                for(int i = 0; i < infectedSpawnCount; i++)
                {
                    PickInfected();
                }
                for(int i = 0; i < clearMindSpawnCount; i++)
                {
                    PickClearMind();      
                }
            }
        }
    }

    IEnumerator MakePeople()
    {
        yield return new WaitForSeconds(spawnFrequency);
        GetPopulation();
    }
    void PickClearMind()
    {
        GameObject target = genPop[UnityEngine.Random.Range(0,genPop.Count)];
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
        GameObject target = genPop[UnityEngine.Random.Range(0,genPop.Count)];
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

    public void ClearPopulation()
    {
        genPop.Clear();
        hivePop.Clear();
        spawnPoints = new GameObject[1];
        population = 0;
    }
}
