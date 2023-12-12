using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Tower;
public  class SetTowerPath : MonoBehaviour
{
    [SerializeField] private List<SearchPath> towers;
    [SerializeField] private List<Tower> startPoints;
    [SerializeField] private SearchPath previewTower;
    [SerializeField] private Transform endPoint;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject Panel;
    [SerializeField] private GameObject losePanel;

    private int levelCount = 0;
    [SerializeField] private GameObject[] levels;

    [SerializeField] private ColorTeam currentTeam;


    private List<Tower> towersList;

    private bool isHoldingMouse = false;
    public UnityEvent refresh;

    private void Start()
    {
        Load();
        SpawnLevel();
        towersList = FindObjectsByType<Tower>(sortMode: FindObjectsSortMode.None).ToList();
    }

    private void SpawnLevel()
    {
        Instantiate(levels[levelCount], transform.position, Quaternion.identity);
    }
    public ColorTeam GetTeam()
    {
        return currentTeam;
    }
    private void SetTowersStartPoints(List<Tower> startPoints, Tower currentTower)
    {
        currentTower.SetStartPoints(startPoints);
        currentTower.SetEndTower(endPoint.GetComponent<Tower>());
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            isHoldingMouse = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isHoldingMouse = false;

            if (CheckPath() && endPoint != null)
            {
                StartSpawning();
            }
            else
            {
                Refresh();
            }
        }

    }

    public void AddTower(SearchPath tower)
    {
        if(towers.Count > 0)
        {
            SetReadyTowers();
            foreach (Tower curTower in towersList)
            {
                SetTowersStartPoints(startPoints, curTower);
            }
            if (towers[towers.Count - 1].GetComponent<Tower>().GetTeamType() == currentTeam)
            {
               
                startPoints.Add(tower.GetComponent<Tower>());
                previewTower.SetIndex(towers.Count - 1);
                towers.Add(tower);
                previewTower = tower;
            }
            else
            {

                tower.SetIndex(towers.Count - 1);

                towers[towers.Count - 1] = tower;
                startPoints[startPoints.Count - 1] = tower.GetComponent<Tower>();

                previewTower.GetComponent<Tower>().ChangeChoose(false);
                previewTower.SetParameters(false);
                previewTower = tower;

               
            }

        }
        
        else if(towers.Count == 0 && tower.GetComponent<Tower>().GetTeamType() == currentTeam)
        {
            towers.Add(tower);
            startPoints.Add(tower.GetComponent<Tower>());
            previewTower = tower;
        }
        else
        {
            Refresh();
        }

        foreach (Tower curTower in towersList)
        {
            curTower.Search();
        }

    }

    public void SetEndPoint(Transform endPoint)
    {
        this.endPoint = endPoint;

        

        //SetReadyTowers();
    }

    public bool GetHolding()
    {
        return isHoldingMouse;
    }

    private bool CheckPath()
    {
        if (endPoint == null) return false;

        SetReadyTowers();

        foreach (SearchPath tower in towers)
        {
            if(tower.GetPathExist() == false && tower.transform != endPoint)
            {
                Debug.Log("Path is not founded from " + tower.gameObject.name);
                return false;
            }
        }

        return true;
    }

    public void SetReadyTowers()
    {
        foreach (SearchPath tower in towers)
        {
            if (endPoint != null)
            {
                SetParametersTower(isHoldingMouse, tower.GetComponent<Tower>());
            }
            
        }
    }

    private void StartSpawning()
    {/*
        startPoints.Remove(startPoints[startPoints.FindIndex(s => s = endPoint.GetComponent<Tower>())]);*/
        foreach (Tower tower in startPoints) 
        {
            if (tower != endPoint.GetComponent<Tower>())
            {
                Debug.Log(tower.gameObject.name);
                tower.GetComponent<SearchPath>().StartSpawningWarriors();
            }
        }
        Refresh();
    }

    private void Refresh()
    {
        towers = new List<SearchPath>(0);
        startPoints = new List<Tower>(0);
        endPoint = null;
        refresh?.Invoke();
    }

    private void SetParametersTower(bool hold ,Tower tower)
    {
        tower.GetComponent<SearchPath>().SetParameters(hold);
    }

    private void CheckWin()
    {
        int countOwnTowers = 0;
        int countNeutralTowers = 0;
        foreach(Tower tower in towersList)
        {
            if(tower.GetTeamType() == currentTeam)
            {
                countOwnTowers++;
            }
            else if (tower.GetTeamType() == ColorTeam.Neutral)
            {
                countNeutralTowers++;
            }
        }


        if (countOwnTowers == 0)
        {
            Panel.SetActive(true);
            losePanel.SetActive(true);
        }
        else if(countOwnTowers + countNeutralTowers == towersList.Count)
        {
            Panel.SetActive(true);
            winPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Continue");
        }
    }

    private void OnEnable()
    {
        TowerWarriors.checkWin += CheckWin;
    }

    public void NextLevel()
    {
        levelCount++;
        if(levelCount >= levels.Length)
        {
            levelCount = 0;
        }
        Save();
        int scene = SceneManager.loadedSceneCount - 1;
        SceneManager.LoadScene(scene);
    }

    public void RestartLevel()
    {
        Save();
        int scene = SceneManager.loadedSceneCount - 1;
        SceneManager.LoadScene(scene);
    }

    private void Save()
    {
        PlayerPrefs.SetInt("level", levelCount);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        levelCount = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 0;
    }
}
