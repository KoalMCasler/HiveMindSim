using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class HiveMind : MonoBehaviour
{
    [Header("Pathfinding")]
    public GameManager gameManager;
    public NavMeshAgent agent;
    public enum MindState { singleMind, hiveMind}
    [Header("States")]
    public MindState mindState;
    public enum ActionState { wandering, attacking, following, searching}
    public ActionState actionState;
    [Header("Variables")]
    public GameObject target;
    public GameObject head;
    public Material hiveMindColor;
    public Material singleMindColor;
    public float maxDetectionRange = 50;
    public float minDetectionRange = 5;
    public float attackRange = 1;
    public float turnTimer;
    public float maxTurnTimer;
    private float distance;
    public float targetDistance;
    public float baseSpeed;
    public float HiveSpeedBuff;
    void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
        gameManager = GameManager.gameManager;
    }

    void Start()
    {
        if(mindState == MindState.hiveMind)
        {
            gameManager.hivePop.Add(this.gameObject);
        }
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
            agent.speed = baseSpeed + HiveSpeedBuff;
        }
        if(target != null)
        {
            targetDistance = Vector3.Distance(this.transform.position, target.transform.position);
        }
        if(CheckForPopulationInfection())
        {
            if(this.gameObject == gameManager.hivePop[0])
            {
                actionState = ActionState.wandering;
            }
            else
            {
                actionState = ActionState.following;
            }
        }
    }

    void Wander()
    {
        if(mindState == MindState.singleMind || gameManager.genPop.Count == 0)
        {
            CheckSuroundings();
            turnTimer -= Time.deltaTime;
            if(turnTimer <= 0)
            {
                PickRandomBuilding();
                agent.SetDestination(target.transform.position);
                turnTimer = maxTurnTimer - Random.Range(-2f,2f);
            }
            else if(targetDistance < minDetectionRange)
            {
                PickRandomBuilding();
            }
            CheckTarget();
        }
        else
        {
            CheckSuroundings();
        }
    }

    void Attack()
    {
        CheckSuroundings();
        if(target != null)
        {
            if(targetDistance < attackRange && target.GetComponent<HiveMind>().mindState == MindState.singleMind)
            {
                actionState = ActionState.searching;
                target.GetComponent<HiveMind>().mindState = MindState.hiveMind;
                target.GetComponent<HiveMind>().actionState = ActionState.searching;
                gameManager.genPop.Remove(target);
                gameManager.hivePop.Add(target);
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
        target = gameManager.hivePop[0];
        agent.SetDestination(target.transform.position);
    }

    void CheckSuroundings()
    {
        if(mindState == MindState.hiveMind)
        {
            List<float> distances = new List<float>();
            for(int i = 0; i < gameManager.genPop.Count; i++)
            {
                distance = Vector3.Distance(this.transform.position, gameManager.genPop[i].transform.position);
                distances.Add(distance);
                if(distances[distances.IndexOf(distances.Min())] < maxDetectionRange && gameManager.genPop[distances.IndexOf(distances.Min())].GetComponent<HiveMind>().mindState != MindState.hiveMind && gameManager.genPop[distances.IndexOf(distances.Min())] != this.gameObject)
                {   
                    target = gameManager.genPop[distances.IndexOf(distances.Min())];
                }
                else if(gameManager.genPop[distances.IndexOf(distances.Min())].GetComponent<HiveMind>().mindState == MindState.hiveMind && CheckForPopulationInfection())
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
                    target = gameManager.genPop[distances.IndexOf(distances.Max())];
                }
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
                actionState = ActionState.searching;
            }
        }
    }

    void PickRandomBuilding()
    {
        target = gameManager.buildings[Random.Range(0,gameManager.buildings.Count)];
    }

    void FindClosestPerson()
    {
        List<float> distances = new List<float>();
        for(int i = 0; i < gameManager.hivePop.Count; i++)
        {
            distance = Vector3.Distance(this.transform.position, gameManager.hivePop[i].transform.position);
            distances.Add(distance);
            if(distances[i] < maxDetectionRange)
            {   
                target = gameManager.hivePop[distances.IndexOf(distances.Min())];
            }
        }
    }

    bool CheckForPopulationInfection()
    {
        bool isPopulationInfected;
        isPopulationInfected = false;
        if(gameManager.genPop.Count == 0)
        {
            isPopulationInfected = true;
        }
        return isPopulationInfected;
    }
}
