using UnityEngine;
using System.Collections.Generic;

public class Road : MonoBehaviour
{
    [SerializeField] private List<Tower> connectedTowers;
    [SerializeField] private Tower[] AllTowers;


    
    
    public void GetTowerList()
    {
        AllTowers = FindObjectsByType<Tower>(sortMode: default);
        Transform[] childs = GetComponentsInChildren<Transform>();
        foreach(Tower tower in AllTowers)
        {
            Vector3 posChild1 = new Vector3(childs[1].transform.position.x, 0, childs[1].transform.position.z);
            Vector3 posChild2 = new Vector3(childs[childs.Length - 1].transform.position.x, 0, childs[childs.Length - 1].transform.position.z);

            Vector3 posTower = new Vector3(tower.transform.position.x, 0, tower.transform.position.z);

            float distance1 = Vector3.Distance(posChild1, posTower);
            float distance2 = Vector3.Distance(posChild2, posTower);
/*
            Debug.Log(childs[1].name + " до " + tower.name + " = " + distance1);
            Debug.Log(childs[childs.Length - 1].name + " до " + tower.name + " = " + distance2);*/
            if (distance1 <= 1 || distance2 <= 1)
            {
                if(connectedTowers.Contains(tower) == false)
                {
                    connectedTowers.Add(tower);
                }
               
            }
        }

    }

   
    public List<Tower> GetTowers()
    {
        return connectedTowers;
    }
    public Tower GetTower(Tower tower)
    {
        for (int i = 0; i < connectedTowers.Count; i++)
        {
            if (connectedTowers[i] != tower)
            {
                return connectedTowers[i];
            }
        }

        return null;
    }
}
