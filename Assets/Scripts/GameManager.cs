using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public List<GameObject> hivePop;
    public List<GameObject> genPop;
    public List<GameObject> buildings;
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
        GetBuildings();
        GetPopulation();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void GetBuildings()
    {
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
        GameObject[] people = GameObject.FindGameObjectsWithTag("Person");
        if(people[0] != null)
        {
            foreach(GameObject i in people)
            { 
                if(i.GetComponent<HiveMind>().mindState == HiveMind.MindState.singleMind)
                {
                    genPop.Add(i);
                }
            }
        }
    }
}
