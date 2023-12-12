using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Tower;

public class EnemyTowerPath : MonoBehaviour
{
    [SerializeField] Tower[] towersList;
    //[SerializeField] GameObject levelParent;
    [SerializeField] List<Tower> towersOwnList;
    [SerializeField] Tower startTower;
    private int indexStart = -1;
    [SerializeField] Tower endTower;
    private int indexEnd = -1;
    [SerializeField] private TowerNode[] towers;

    [SerializeField] private List<Transform> towerPoints;

    [SerializeField] private ColorTeam team;

    [SerializeField] private float timeCheckAttack = 1f;

    [SerializeField] private List<Road> roadsPath;
    private float timer;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        GetTowers();
        FillNodes();
        GetOwnTowers();
    }
   

    private void UpdateInfoTowers()
    {
        GetOwnTowers();
        foreach (TowerNode tower in towers)
        {
            tower.UpdateInfo();
        }
    }

    private void GetTowers()
    {
        towersList = null;
        towersList = FindObjectsOfType<Tower>(); //levelParent.GetComponentsInChildren<Tower>(); 
        towers = new TowerNode[towersList.Length];
    }

    private void FillNodes()
    {
       for (int i = 0; i < towersList.Length; i++)
       {
            towers[i].SetTower(towersList[i]);
       }
    }
    private void GetOwnTowers()
    {
        towersOwnList = new List<Tower>();
        foreach (Tower t in towersList)
        {
            if(t.GetTeamType() == team)
            {
                towersOwnList.Add(t);
            }
        }
    }
    private void Update()
    {
        if (timer <= 0)
        {
            timer = timeCheckAttack;
            CheckAttack();
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    private void CheckAttack()
    {
        foreach(Tower tower in towersOwnList)
        {
            int warriors = tower.GetComponent<TowerWarriors>().GetCountWarriors();
            int max = tower.GetComponent<TowerWarriors>().GetMaxWarriors();
            float attackValue = max / UnityEngine.Random.Range(1, 1.5f);
            foreach (TowerNode tNode in towers)
            {
                tNode.ChangeCost(float.PositiveInfinity);
            }
            if (warriors >= attackValue)
            {
                ChoicePath(tower);
            }
        }

    }

    private void ChoicePath(Tower tower)
    {
        //List<Tower> connectedTowers = tower.GetConnectedTower();

        List<Tower> connectedTowers = new List<Tower>();

        foreach(var towerOwn in towersOwnList)
        {
            foreach(var conTower in towerOwn.GetConnectedTower())
            {
                connectedTowers.Add(conTower);
            }
        }

        foreach (TowerNode tNode in towers)
        {
            tNode.ChangeCost(float.PositiveInfinity);
            startTower = null;
            endTower = null;
        }

        foreach (Tower t in connectedTowers)
        {
            if(t.GetTeamType() == team && t.GetComponent<TowerWarriors>().GetCountWarriors() < 5)
            {
                startTower = tower;
                endTower = t;
                SearchStart();
                SearchEnd();

                break;
            }
            else if(t.GetTeamType() != team && t != endTower)
            {
                startTower = tower;
                endTower = t;
                SearchStart();
                SearchEnd();

                break;
            }
        }

        if (CheckPathExist() && endTower != null)
        {
            startTower.GetComponent<TowerWarriors>().Refresh();
           
            startTower.GetComponent<TowerWarriors>().SetTowers(towerPoints);
            GetWayRoads();
            startTower.GetComponent<TowerWarriors>().FillListRoads(roadsPath, endTower.gameObject);
            startTower.GetComponent<TowerWarriors>().StartSpawn();
        }
        else
        {
            Debug.Log("Path is not exist");
        }

        Refresh();

    }
    private void SearchStart()
    {

        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i].GetTower() == startTower)
            {
                indexStart = i;
                towers[i].ChangeCost(0);
            }
        }

        for (int i = 0; i < towers[indexStart].GetConnectedTowers().Length; i++)
        {
            SearchPath(towers[indexStart].GetConnectedTowers()[i], towers[indexStart].GetCost(), towers[indexStart].GetDistances()[i]);
        }
    }

    private void SearchPath(Tower currentTower, double cost, double distance)
    {
        if (endTower != currentTower)
        {
            if (currentTower.GetTeamType() != team)
            {
                foreach (TowerNode tower in towers)
                {
                    if (tower.GetTower() == endTower)
                    {
                        tower.SetVisit(true);
                        return;
                    }

                }
            }
        }

        int index = -1;
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i].GetTower() == currentTower)
            {
                index = i; break;
            }
        }

        

        if (towers[index].GetVisit())
        {
            return;
        }

        double newCost = cost + distance;

        if(towers[index].GetCost() > newCost)
        {
            towers[index].ChangeCost(newCost);
            towers[index].SetVisit(true);
            for (int i = 0; i < towers[index].GetConnectedTowers().Length; i++)
            {
                SearchPath(towers[index].GetConnectedTowers()[i], towers[index].GetCost(), towers[index].GetDistances()[i]);
            }
        }

       
    }

    private void SearchEnd()
    {
        foreach (TowerNode tower in towers)
        {
            if (tower.GetTower() == endTower)
            {
               if(tower.GetCost() == float.PositiveInfinity)
                {
                    return;
                }
            }

        }

        towerPoints = new List<Transform>();
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i].GetTower() == endTower)
            {
                indexEnd = i;
            }
        }
        towerPoints.Add(towers[indexEnd].GetTower().transform);

        for (int i = 0; i < towers[indexEnd].GetConnectedTowers().Length; i++)
        {
            SearchWayPoints(towers[indexEnd].GetConnectedTowers()[i], towers[indexEnd].GetCost(), towers[indexEnd].GetDistances()[i]);
        }

        towerPoints.Reverse();
    }

    private void SearchWayPoints(Tower currentTower, double cost, double distance)
    {
        int index = -1;
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i].GetTower() == currentTower)
            {
                index = i; break;
            }
        }
        if (Math.Round(cost - distance, 2) == Math.Round(towers[index].GetCost(), 2))
        {
            towerPoints.Add(towers[index].GetTower().transform);
            for (int i = 0; i < towers[index].GetConnectedTowers().Length; i++)
            {
                SearchWayPoints(towers[index].GetConnectedTowers()[i], towers[index].GetCost(), towers[index].GetDistances()[i]);
            }
        }
    }

    private void GetWayRoads()
    {
        roadsPath = new List<Road>();
        for (int i = 0; i < towerPoints.Count - 1; i++)
        {
            foreach (Road road in towerPoints[i].GetComponent<Tower>().GetRoads())
            {
                if (road.GetTowers().Contains(towerPoints[i].GetComponent<Tower>())
                    && road.GetTowers().Contains(towerPoints[i + 1].GetComponent<Tower>()))
                {
                    //roadsPath.Insert(0, road);
                    roadsPath.Add(road);

                }
            }
        }


    }

    private bool CheckPathExist()
    {
       

        foreach (TowerNode tower in towers)
        {
            if(tower.GetTower() == endTower)
            {
                if(tower.GetCost() != float.PositiveInfinity)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void Refresh()
    {
        GetTowers();
        FillNodes();
        GetOwnTowers();
    }

    private void OnEnable()
    {
        refreshTowersInfo += UpdateInfoTowers;
    }

    private void OnDisable()
    {
        refreshTowersInfo += UpdateInfoTowers;
    }
}

[Serializable]
public struct TowerNode
{
    [SerializeField] private Tower tower;
    [SerializeField] private ColorTeam team;
    [SerializeField] private double cost;
    [SerializeField] private Tower[] connectedTowers;
    [SerializeField] private double[] distance;
    private bool isVisit;

    public void UpdateInfo()
    {
        SetTeam();
    }
    public bool GetVisit()
    {
        return isVisit;
    }

    public void SetVisit(bool isVisit)
    {
        this.isVisit = isVisit;
    }
    public Tower GetTower()
    {
        return tower;
    }
    public void SetTower(Tower tower)
    {
        this.tower = tower;
        cost = float.PositiveInfinity;
        SetTeam();
        SetConnectedTower(tower.GetConnectedTower().ToArray());
    }

    private void SetConnectedTower(Tower[] connected)
    {
        connectedTowers = connected;
        SetCountDistance();
    }

    public Tower[] GetConnectedTowers()
    {
        return connectedTowers;
    }

    public double[] GetDistances()
    {
        return distance;
    }

    private void SetCountDistance()
    {
        distance = new double[connectedTowers.Length];
        SetDistance();
    }
    private void SetDistance()
    {
        for(int i = 0; i < distance.Length; i++)
        {
            Vector3 posTower = new Vector3(connectedTowers[i].transform.position.x, 0, connectedTowers[i].transform.position.z);
            Vector3 posThisTower = new Vector3(tower.transform.position.x, 0, tower.transform.position.z);
            distance[i] = Math.Round(Vector3.Distance(posTower, posThisTower), 2);
        }
    }


    private void SetTeam()
    {
        team = tower.GetTeamType();
    }

    public ColorTeam GetTeam()
    {
        return team;
    }

    public void ChangeCost(double cost)
    {
        this.cost = cost;
    }

    public double GetCost()
    {
        return cost;
    }


}
