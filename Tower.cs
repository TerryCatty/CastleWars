using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private ColorTeam teamTypeTower;
    public enum ColorTeam
    {
        Red,
        Green,
        Purple,
        Neutral
    }

    [SerializeField] private double[] cost;
    [SerializeField] private List<Tower> startTowers;
    [SerializeField] private List<Tower> towerList;
    [SerializeField] private List<Road> roads;
    [SerializeField] private List<Road> allRoads;
    [SerializeField] GameObject[] towerPrefabs;
    [SerializeField] GameObject myTower;

    [SerializeField] private Tower endTower;
    private SearchPath searchPath;

    private SetTowerPath towerPath;
    private bool isChosen;


    public static event Action refreshTowersInfo;




    public List<Tower> GetConnectedTower()
    {
        return towerList;
    }
    public void SetStartPoints(List<Tower> startPoints)
    {
        startTowers = new List<Tower>(startPoints.Count);
        cost = new double[startPoints.Count];

        for (int i = 0; i < startPoints.Count; i++)
        {
            startTowers.Add(startPoints[i]);
            SetCosts(float.PositiveInfinity, i);
        }
    }

    public void Search()
    {
        if (this != endTower) GetComponent<SearchPath>().Refresh();

    }

    public void SetCosts(double value, int index)
    {
        cost[index] = value;
    }

    private void Start()
    {
        towerPath = FindFirstObjectByType<SetTowerPath>();
        ChangeColor();
        GetRoadList();
        GetTowerList();

        FindAnyObjectByType<SetTowerPath>().refresh.AddListener( () =>
        {
            isChosen = false;
            endTower = null;
            startTowers = new List<Tower>();
            cost = new double[0];
            ChangeChoose(false);
        });
    }

    public List<Road> GetRoads()
    {
        return roads;
    }

    public void SetColor(ColorTeam teamTypeTower)
    {
        this.teamTypeTower = teamTypeTower;
        ChangeColor();
    }
    private void ChangeColor()
    {
        refreshTowersInfo?.Invoke();
        switch (teamTypeTower)
        {
            case ColorTeam.Red:
                SpawnNewTower(0);
                break;
            case ColorTeam.Purple:
                SpawnNewTower(1);
                break;
            case ColorTeam.Green:
                SpawnNewTower(2);
                break;
            case ColorTeam.Neutral:
                SpawnNewTower(3);
                break;
        }
    }

    private void SpawnNewTower(int index)
    {
        GameObject newTower = Instantiate(towerPrefabs[index], myTower.transform.position, myTower.transform.rotation);
        newTower.transform.SetParent(transform);
        Destroy(myTower.gameObject);
        myTower = newTower;
    }



    public void Refresh()
    {
        cost = new double[0];
        ChangeColor();
        GetTowerList();
        //searchPath.Refresh();
    }

    private void GetTowerList()
    {
        towerList = new List<Tower>();
        for (int i = 0; i < roads.Count; i++)
        {
            roads[i].GetTowerList();
            Tower addTower = roads[i].GetTower(this);
            if (addTower != null)
            {
                towerList.Add(addTower);
            }
        }
    }

    private void GetRoadList()
    {
        roads = new List<Road>();
        allRoads = FindObjectsOfType<Road>().ToList();

        foreach (Road road in allRoads)
        {
            List<Transform> points = road.GetComponentsInChildren<Transform>().ToList();
            points.Remove(points[0]);

            foreach (Transform point in points)
            {
                Vector3 posPoint = new Vector3(point.transform.position.x, 0, point.transform.position.z);
                Vector3 posTower = new Vector3(transform.position.x, 0, transform.position.z);

                float distance = Vector3.Distance(posPoint, posTower);

                if (distance <= 1)
                {
                    roads.Add(road);
                    break;
                }
            }
        }
    }
    public void ChangeChoose(bool chosen)
    {
        isChosen = chosen;

        transform.localScale = chosen ?
            new Vector3(1.2f, 1.2f, 1.2f) :
            Vector3.one;
    }

    public ColorTeam GetTeamType()
    {
        return teamTypeTower;
    }
    public double GetCost(int index)
    {
        return Math.Round(cost[index], 2);
    }

    public void ChangeCost(double newCost, int index)
    {
        cost[index] = newCost;
        cost[index] = Math.Round(cost[index], 2);
    }

    public void SetEndTower(Tower tower)
    {
        endTower = tower;
    }

    public void SetSearchPath(SearchPath search)
    {
        searchPath = search;
    }

    public Tower GetEndTower()
    {
        return endTower;
    }
    public void UpdatePath(int index)
    {

        foreach (Tower tower in towerList)
        {
            tower.SetSearchPath(searchPath);


            if (tower.GetTeamType() != towerPath.GetTeam() && tower != endTower)
            {
                DontMovePoint(tower, index);
            }
            else
            {
                MovePoint(tower, index);
            }
        }
    }

    private void MovePoint(Tower tower, int index)
    {
        Vector3 posTower = new Vector3(tower.transform.position.x, 0, tower.transform.position.z);
        Vector3 posThisTower = new Vector3(transform.position.x, 0, transform.position.z);

        double distance = Math.Round(Vector3.Distance(posTower, posThisTower), 2);

        double newCost = cost[index] + distance;

        if (newCost <= tower.GetCost(index))
        {
            tower.ChangeCost(newCost, index);
            tower.UpdatePath(index);
        }
    }

    private void DontMovePoint(Tower tower, int index)
    {
        tower.ChangeCost(float.PositiveInfinity, index);
    }

    public void GoToStartPoint(int index, SearchPath search)
    {
        search.GetComponent<SearchPath>().AddTowerToPath(transform);

        foreach (Tower tower in towerList)
        {
            Vector3 posTower = new Vector3(tower.transform.position.x, 0, tower.transform.position.z);
            Vector3 posThisTower = new Vector3(transform.position.x, 0, transform.position.z);

            /* if (tower == startTower)
             {
                 tower.GoToStartPoint();
                 break;
             }*/

            double distance = Math.Round(Vector3.Distance(posTower, posThisTower), 2);

            if (Math.Round(cost[index] - distance, 2) == tower.GetCost(index))
            {
                tower.GoToStartPoint(index, search);
                break;
            }
        }
    }

    private void OnMouseOver()
    {
        if (towerPath.GetHolding())
        {
            SetPathParameters();
        }
    }

    private void OnMouseEnter()
    {
        if (towerPath.GetHolding())
        {
            SetPathParameters();
        }
    }

    private void OnMouseExit()
    {
        if (towerPath.GetHolding())
        {
            towerPath.SetEndPoint(null);
        }
    }

    private void SetPathParameters()
    {
        towerPath.SetEndPoint(transform);

        if (isChosen == false)
        {
            towerPath.AddTower(GetComponent<SearchPath>());
        }
        //towerPath.SetReadyTowers();
        ChangeChoose(true);
    }

    
}