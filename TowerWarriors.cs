using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Tower;

public class TowerWarriors : MonoBehaviour
{
    [SerializeField] private int maxWarriors;
    [SerializeField] private int countWarriors = 0;
    [SerializeField] private TextMeshPro textCount;
    [SerializeField] private float timeRegenerateWarriors;
    [SerializeField] private float spawnInterval;
    [SerializeField] private Warrior warriorPref;
    [SerializeField] private Warrior[] warriors;
    private GameObject endTower;
    private float timerRegenerate;
    [SerializeField] private int amountSpawningWarriors;
    private ColorTeam teamTypeTower;

    public static event Action checkWin;

    [SerializeField] private int spawningCount = 0;

    private List<Road> roads;
    [SerializeField] private List<Transform> towers;
    [SerializeField] private List<Transform> wayPoints;

    private void Start()
    {
        FillWarriors();
        timerRegenerate = timeRegenerateWarriors;
        teamTypeTower = GetComponent<Tower>().GetTeamType();
        textCount.text = countWarriors.ToString();
    }
    private void FillWarriors()
    {
        countWarriors = maxWarriors;
        amountSpawningWarriors = 0;
    }

    private void Update()
    {
        if(timerRegenerate <= 0)
        {
            timerRegenerate = timeRegenerateWarriors;
           
            if (countWarriors > maxWarriors)
            {
                countWarriors--;
                textCount.text = countWarriors.ToString();
            }
            else if(countWarriors < maxWarriors)
            {
                countWarriors++;
                textCount.text = countWarriors.ToString();
            }
        }

        if(teamTypeTower != ColorTeam.Neutral)
        {
            timerRegenerate -= Time.deltaTime;
        }
        
        

    }

    public void Refresh()
    {
        wayPoints = new List<Transform>();
        towers = new List<Transform>();
        roads = new List<Road>();
        textCount.text = countWarriors.ToString();
    }

    public void SetTowers(List<Transform> towers)
    {
        this.towers = towers;
    }

    public int GetMaxWarriors()
    {
        return maxWarriors;
    }

    public int GetCountWarriors()
    {
        return countWarriors;
    }
    public void CheckWays()
    {
        GetWayPoints();
    }

    public void FillListRoads(List<Road> roads, GameObject endPoint)
    {
        this.roads = roads;
        endTower = endPoint;
        
        GetWayPoints();
    }

    private void GetWayPoints()
    {
        Vector3 startPosition = transform.position;
       
        for (int i = 0; i < roads.Count; i++)
        {
            List<Transform> points = roads[i].GetComponentsInChildren<Transform>().ToList();
            
            
            points.Remove(points[0]);

            if (Vector3.Distance(startPosition, points[0].position) > Vector3.Distance(startPosition, points[points.Count - 1].position))
            {
                points.Reverse();
            }

            points.Insert(0, towers[i]);

            for (int j = 0; j < points.Count; j++)
            {
                wayPoints.Add(points[j]);
            }

            startPosition = points[points.Count - 1].position;
        }

        wayPoints.Add(towers[roads.Count]);
    }

    public void StartSpawn()
    {
        ChangeCountSpawnedWarriors();
        spawningCount++;
        StartCoroutine(SpawnWarriors(amountSpawningWarriors));
    }

    private void ChangeCountSpawnedWarriors()
    {
        int amountSpawningWarriors = (int)Math.Ceiling(Convert.ToDouble(countWarriors / 2));
        ChangeCountWarriors(-amountSpawningWarriors, teamTypeTower);
        this.amountSpawningWarriors = amountSpawningWarriors;
    }

    public void ChangeCountWarriors(int value, ColorTeam team)
    {
        if (countWarriors == 0)
        {
            teamTypeTower = team;
            GetComponent<Tower>().SetColor(team);
            checkWin?.Invoke();
        }

        if (team == teamTypeTower)
        {
            countWarriors += value;
        }
        else
        {
            countWarriors -= value;
        }

        textCount.text = countWarriors.ToString();



    }

    private IEnumerator SpawnWarriors(int amount)
    {
        List<Transform> points = new List<Transform>(wayPoints.Count);
        GameObject endPoint = endTower;

        foreach (Transform t in wayPoints)
        {
            points.Add(t);
        }


        wayPoints = new List<Transform>();
        while (amount > 0)
        {
            yield return new WaitForSeconds(spawnInterval);
            switch (teamTypeTower)
            {
                case ColorTeam.Red:
                    warriorPref = warriors[0];
                    break;
                case ColorTeam.Purple:
                    warriorPref = warriors[1];
                    break;
                case ColorTeam.Green:
                    warriorPref = warriors[2];
                    break;
            }
            Vector3 spawnPos = new Vector3(transform.position.x, 0, transform.position.z);
            Warrior warrior = Instantiate(warriorPref, spawnPos, Quaternion.identity);
            warrior.SetWayPoints(points, endPoint);
            amount--;
            warrior.SetColorTeam(teamTypeTower);
        }
    }
}

