using System.Collections.Generic;
using UnityEngine;
using static Tower;
using DG.Tweening;

public class Warrior : MonoBehaviour
{
    private ColorTeam teamColor;
    [SerializeField] private float speed;
    [SerializeField] private int strength;
    private List<Transform> wayPoints;
    private GameObject endTower;

    private int indexPoint = 0;
    [SerializeField] private bool isRunning = false;
    private Vector3 target;

    private void Start()
    {
        MoveToTheTarget();
    }
    public void SetColorTeam(ColorTeam teamColor)
    {
        this.teamColor = teamColor;
    }

    public void SetWayPoints(List<Transform> wayPoints, GameObject endPoint)
    {
        this.wayPoints = new List<Transform>(wayPoints.Count);

        foreach (Transform t in wayPoints)
        {
            this.wayPoints.Add(t);
        }
        indexPoint = -1;
        SetEndPoint(endPoint);
    }

    public ColorTeam GetTeam()
    {
        return this.teamColor;
    }
    private void SetEndPoint(GameObject endPoint)
    {
        endTower = endPoint;
        MoveToTheTarget();
    }

    private void MoveToTheTarget()
    {
        ChangeTarget();
        transform.LookAt(target);
        isRunning = true;
    }

    private void ChangeTarget()
    {
        indexPoint++;
        target = wayPoints[indexPoint].position;
        target = new Vector3(target.x, transform.position.y, target.z);
        transform.DOLookAt(target, 1f);
    }

    private void Update()
    {
        if (!isRunning) return;


        //transform.LookAt(target);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        if (Vector3.Distance(transform.position, target) < 0.5f && indexPoint < wayPoints.Count - 1)
        {
            ChangeTarget();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject == endTower || collision.gameObject.GetComponent<Tower>().GetTeamType() != teamColor)
        {
            collision.GetComponent<TowerWarriors>().ChangeCountWarriors(strength, teamColor);
            Destroy(gameObject);
        }

       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Warrior")
        {
            if(collision.gameObject.GetComponent<Warrior>().GetTeam() == teamColor)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            }
            if(collision.gameObject.GetComponent<Warrior>().GetTeam() != teamColor)
            {
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
        }
    }

}
