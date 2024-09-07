using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class HiveMind : MonoBehaviour
{
    [Header("Pathfinding")]
    public NavMeshAgent agent;
    public Rigidbody rb;
    public enum MindState { singleMind, hiveMind}
    [Header("States")]
    public MindState mindState;
    public enum ActionState { wandering, attacking, following, searching}
    public ActionState actionState;
    [Header("Variables")]
    public GameObject head;
    public List<GameObject> population;
    public List<GameObject> buildings;
    public Material hiveMindColor;
    public Material singleMindColor;
    public float maxDetectionRange = 50;
    public float minDetectionRange = 10;
    public float attackRange = 1;
    public GameObject target;
    public float turnTimer;
    public float maxTurnTimer;
    private float distance;
    private float targetDistance;
    public float baseSpeed;
    public float HiveSpeedBuff;
    void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
        rb = this.GetComponent<Rigidbody>();
        GetPopulation();
        getBuildings();
    }

    // Update is called once per frame
    void Update()
    {
        switch(actionState)
        {
            case ActionState.wandering: Wander(); break;
            case ActionState.following: Follow(); break;
            case ActionState.attacking: Attack(); break;
            case ActionState.searching: Search(); break;
        }
        if(mindState == MindState.singleMind)
        {
            head.GetComponent<Renderer>().material = singleMindColor;
        }
        if(mindState == MindState.hiveMind)
        {
            head.GetComponent<Renderer>().material = hiveMindColor;
            maxDetectionRange = 500;
            agent.speed = baseSpeed + HiveSpeedBuff;
        }
        if(target != null)
        {
            targetDistance = Vector3.Distance(this.transform.position, target.transform.position);
        }
    }

    void Wander()
    {
        CheckSuroundings();
        turnTimer -= Time.deltaTime;
        if(turnTimer <= 0)
        {
            PickRandomBuilding();
            turnTimer = maxTurnTimer - Random.Range(-2f,2f);
            agent.SetDestination(target.transform.position);
        }
        if(targetDistance < minDetectionRange)
        {
            PickRandomBuilding();
        }
        CheckTarget();
    }

    void Attack()
    {
        CheckSuroundings();
        if(target != null)
        {
            if(targetDistance < attackRange)
            {
                actionState = ActionState.searching;
                target.GetComponent<HiveMind>().mindState = MindState.hiveMind;
            }
            agent.SetDestination(target.transform.position);
        }
    }

    void Search()
    {
        CheckSuroundings();
        if(target != null)
        {
            CheckTarget();
        }
        else
        {
            actionState = ActionState.following;
        }
    }

    void Follow()
    {
        FindClosestPerson();
        agent.SetDestination(target.transform.position);
        CheckSuroundings();
    }

    void CheckSuroundings()
    {
        List<float> distances = new List<float>();
        for(int i = 0; i < population.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, population[i].transform.position);
            distances.Add(distance);
            if(distances[i] < maxDetectionRange && population[distances.IndexOf(distances.Min())].GetComponent<HiveMind>().mindState != mindState)
            {   
                target = population[distances.IndexOf(distances.Min())];
                targetDistance = distances.Min();
            }
            else if(population[distances.IndexOf(distances.Min())].GetComponent<HiveMind>().mindState == mindState && CheckForPopulationInfection())
            {
                actionState = ActionState.following;
            }
            else
            {
                if(mindState == MindState.singleMind)
                {
                    actionState = ActionState.wandering;
                }
                if(mindState == MindState.hiveMind)
                {
                    actionState = ActionState.searching;
                }
            }
        }
        if(target != null)
        {
            if(target.GetComponent<HiveMind>() != null && target.GetComponent<HiveMind>().target == this)
            {
                target = population[distances.IndexOf(distances.Max())];
            }
        }
    }

    void CheckTarget()
    {
        if(target.GetComponent<HiveMind>() == null)
        {
            agent.SetDestination(target.transform.position);
            return;
        }
        if(target.GetComponent<HiveMind>().mindState == HiveMind.MindState.singleMind)
        {
            if(mindState == MindState.singleMind)
            {
                actionState = ActionState.wandering;
            }
            if(mindState == MindState.hiveMind)
            {
                actionState = ActionState.attacking;
            }
        }
        else if(target.GetComponent<HiveMind>().mindState == HiveMind.MindState.hiveMind)
        {
            if(mindState == MindState.hiveMind)
            {
                actionState = ActionState.wandering;
            }
        }
    }

    void PickRandomBuilding()
    {
        target = buildings[Random.Range(0,buildings.Count)];
    }

    void RotateAwayFromTarget()
    {
        Vector3 targetDirecion = target.transform.position + this.transform.position;
        Vector3 directionAwayFromTarget = targetDirecion.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(transform.up, directionAwayFromTarget);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
        rb.MoveRotation(rotation);
    }

    void GetPopulation()
    {
        GameObject[] people = GameObject.FindGameObjectsWithTag("Person");
        if(people[0] != null)
        {
            foreach(GameObject i in people)
            {
                if(i != this.gameObject)
                {
                    population.Add(i);
                }
            }
        }
    }

    void FindClosestPerson()
    {
        List<float> distances = new List<float>();
        for(int i = 0; i < population.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, population[i].transform.position);
            distances.Add(distance);
            if(distances[i] < maxDetectionRange)
            {   
                target = population[distances.IndexOf(distances.Min())];
            }
        }
    }

    bool CheckForPopulationInfection()
    {
        bool isPopulationInfected;
        isPopulationInfected = false;
        int infectionCount;
        infectionCount = 0;
        for(int i = 0; i < population.Count; i++)
        {
            if(population[i].GetComponent<HiveMind>().mindState == MindState.hiveMind)
            {
                infectionCount += 1;
            }
        }
        if(infectionCount == population.Count)
        {
            isPopulationInfected = true;
        }
        return isPopulationInfected;
    }

    void getBuildings()
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
}
