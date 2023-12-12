using System.Collections.Generic;
using UnityEngine;

public class SearchPath : MonoBehaviour
{
    [SerializeField] private bool isHolding;
    [SerializeField] private bool pathExist;

    [SerializeField] private List<Transform> towers;
    [SerializeField] private List<Road> roadsPath;

    [SerializeField] private int index;
    private int countOfTowers;

    public void AddTowerToPath(Transform tower)
    {
        if (towers.Contains(tower)) return;

        towers.Insert(0, tower);
        countOfTowers = towers.Count;
    }

   

    public void SetIndex(int index)
    {
        this.index = index;
    }
    
    public int GetIndex()
    {
        return index;
    }

    public void SetParameters(bool hold)
    { 
        isHolding = hold;
        if (isHolding)
        {
            Refresh();
        }

    }


    public void StartSpawningWarriors()
    {
        if (GetComponent<Tower>().GetEndTower() == null) return;
        GetComponent<Tower>().GetEndTower().GoToStartPoint(index, this);
        GetComponent<TowerWarriors>().Refresh();
        GetComponent<TowerWarriors>().SetTowers(towers);
        GetWayRoads();
        GetComponent<TowerWarriors>().FillListRoads(roadsPath, GetComponent<Tower>().GetEndTower().gameObject);
        GetComponent<TowerWarriors>().StartSpawn();
        Refresh();
    }

   
    private void Update()
    {
        if (GetComponent<Tower>().GetEndTower() == null) return;

        if (isHolding)
        {
            pathExist = GetComponent<Tower>().GetEndTower().GetCost(index) < float.PositiveInfinity ? true : false;
        }

       
    }

    private void GetWayRoads()
    {
        roadsPath = new List<Road>();
        for (int i = 0; i < countOfTowers - 1; i++)
        {
            foreach (Road road in towers[i].GetComponent<Tower>().GetRoads())
            {
                if (road.GetTowers().Contains(towers[i].GetComponent<Tower>()) 
                    && road.GetTowers().Contains(towers[i + 1].GetComponent<Tower>()))
                {
                    //roadsPath.Insert(0, road);
                    roadsPath.Add(road);

                }
            }
        }

       
    }

    public void Refresh()
    {
        if (index < 0) return;

        foreach (Transform tower in towers)
        {
            tower.GetComponent<Tower>().ChangeCost(float.PositiveInfinity, index);
        }
        GetComponent<Tower>().ChangeCost(0, index);
        GetComponent<Tower>().SetSearchPath(this);
        towers = new List<Transform>();
        roadsPath = new List<Road>();
        GetComponent<Tower>().UpdatePath(index);
    }

    public bool GetPathExist()
    {
        return pathExist;
    }

    private void OnEnable()
    {
        FindAnyObjectByType<SetTowerPath>().refresh.AddListener( ()=> {
            index = -1;
            towers = new List<Transform>();
            roadsPath = new List<Road>();
            isHolding = false;
            pathExist = false;
        });
    }

}
