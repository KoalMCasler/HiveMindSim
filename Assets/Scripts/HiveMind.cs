using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class HiveMind : MonoBehaviour
{
    [Header("Pathfinding")]
    public GameManager gameManager;
    public NavMeshAgent agent;
    public enum MindState { singleMind, hiveMind, clearMind}
    [Header("States")]
    public MindState mindState;
    public enum ActionState { wandering, attacking, searching, fleeing}
    public ActionState actionState;
    [Header("Variables")]
    public GameObject target;
    public GameObject newTarget;
    public GameObject head;
    public Material hiveMindColor;
    public Material singleMindColor;
    public Material clearMindColor;
    public Material fleeingColor;
    public float maxDetectionRange = 50;
    public float minDetectionRange = 5;
    public float attackRange = 1;
    public float turnTimer;
    public float maxTurnTimer;
    public float fleeTime;
    public float maxFleeTime;
    private float distance;
    public float targetDistance;
    public float minChangeDistance;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        fleeTime = maxFleeTime;
        target = null;
        newTarget = null;
        if(gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void Awake()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(scale, 1f).SetEase(Ease.OutElastic);
        turnTimer = 0;
        CheckSuroundings();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch(actionState)
        {
            case ActionState.wandering: Wander(); break;
            case ActionState.attacking: Attack(); break;
            case ActionState.searching: Search(); break;
            case ActionState.fleeing: Flee(); break;
        }
        if(mindState == MindState.singleMind)
        {
            head.GetComponent<Renderer>().material = singleMindColor;
        }
        else if(mindState == MindState.hiveMind)
        {
            if(actionState != ActionState.fleeing)
            {
                head.GetComponent<Renderer>().material = hiveMindColor;
            }
            else
            {
                head.GetComponent<Renderer>().material = fleeingColor;
            }
        }
        else if(mindState == MindState.clearMind)
        {
            head.GetComponent<Renderer>().material = clearMindColor;
            if(gameManager.hivePop.Count <= 0)
            {
                Wander();
            }
        }
        if(target != null)
        {
            targetDistance = Vector3.Distance(this.transform.position, target.transform.position);
        }
        CheckSuroundings();
        CheckForNewTarget();
    }

    void Wander()
    {
        turnTimer -= Time.deltaTime;
        if(turnTimer <= 0)
        {
            PickRandomBuilding();
        }
        else if(targetDistance < minDetectionRange)
        {
            PickRandomBuilding();
        }
    }

    void Flee()
    {
        fleeTime -= Time.deltaTime;
        if(fleeTime < 0)
        {
            fleeTime = maxFleeTime;
            actionState = ActionState.searching;
        }
        //FindHiveMember();
    }

    void Attack()
    {
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
            else if(targetDistance < attackRange && target.GetComponent<HiveMind>().mindState == MindState.hiveMind)
            {
                actionState = ActionState.searching;
                target.GetComponent<HiveMind>().mindState = MindState.singleMind;
                target.GetComponent<HiveMind>().actionState = ActionState.wandering;
                gameManager.hivePop.Remove(target);
                gameManager.genPop.Add(target);
            }
        }
    }

    void Search()
    {
        if(target != null)
        {
            CheckTarget();
        }
    }

    void CheckSuroundings()
    {
        List<float> distances = new List<float>();
        if(mindState == MindState.hiveMind)
        {
            for(int i = 0; i < gameManager.genPop.Count; i++)
            {
                distance = Vector3.Distance(this.transform.position, gameManager.genPop[i].transform.position);
                distances.Add(distance);
            }
            if(distances[distances.IndexOf(distances.Min())] < maxDetectionRange)
            {   
                newTarget = gameManager.genPop[distances.IndexOf(distances.Min())];
            }
            if(target != null && target.GetComponent<HiveMind>().mindState == MindState.clearMind && actionState != ActionState.fleeing)
            {
                actionState = ActionState.fleeing;
            }
            else if(actionState == ActionState.fleeing)
            {
                PickRandomBuilding();
            }
            else
            {
                actionState = ActionState.searching;
            }
        }
        if(mindState == MindState.clearMind)
        {
            for(int i = 0; i < gameManager.hivePop.Count; i++)
            {
                distance = Vector3.Distance(this.transform.position, gameManager.hivePop[i].transform.position);
                distances.Add(distance);
            }
            if(distances.Count > 0)
            {
                if(distances[distances.IndexOf(distances.Min())] < maxDetectionRange)
                {   
                    newTarget = gameManager.hivePop[distances.IndexOf(distances.Min())];
                }
                else
                {
                    actionState = ActionState.searching;
                }
            }
        }
    }

    void CheckForNewTarget()
    {
        if(target != newTarget && newTarget != null)
        {
            target = newTarget;
            agent.SetDestination(target.transform.position);
        }
    }

    void CheckTarget()
    {
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
            if(mindState == MindState.clearMind)
            {
                actionState = ActionState.attacking;
            }
        }
    }

    void PickRandomBuilding()
    {
        List<GameObject> tempList = new List<GameObject>();
        foreach(GameObject i in gameManager.buildings)
        {
            float distance = Vector3.Distance(transform.position, i.transform.position);
            if(distance > minChangeDistance)
            {
                tempList.Add(i);
            }
        }
        turnTimer = maxTurnTimer;
        if(tempList.Count >= 0)
        {
            newTarget = tempList[UnityEngine.Random.Range(0, tempList.Count)];
        }
        else
        {
            newTarget = gameManager.buildings[UnityEngine.Random.Range(0, gameManager.buildings.Count)];
        }
    }
}
