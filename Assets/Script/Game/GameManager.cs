using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    private float interactDistance = 1f;

    public bool isPlaying = false;
    private string filename;

    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    float totalEnemyCount;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        totalEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filename = Application.dataPath + "/Playthrough_" + timestamp + ".csv";

        UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private void OnDestroy()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
    }

    private void PlayModeStateChanged(UnityEditor.PlayModeStateChange stateChange)
    {
        if (stateChange == UnityEditor.PlayModeStateChange.EnteredPlayMode)
        {
            isPlaying = true;
        }
        else if (stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            isPlaying = false;
        }
    }

    float levelOneCount;
    float levelTwoCount;
    float levelThreeCount;

    private void CheckInteractionWithLevelEndObjects()
    {
        string levelName = SceneManager.GetActiveScene().name;

        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("LevelOneEntry") != null)
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("LevelOneEntry").transform.position) <= interactDistance)
            {
                SceneManager.LoadScene("GetToEndTest");
                levelOneCount++;
            }
        } 

        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("LevelTwoEntry") != null)
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("LevelTwoEntry").transform.position) <= interactDistance)
            {
                SceneManager.LoadScene("LevelKillTest");
                levelTwoCount++;
            }
        } 

        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("LevelThreeEntry") != null)
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("LevelThreeEntry").transform.position) <= interactDistance)
            {
                SceneManager.LoadScene("LevelHeistTest");
                levelThreeCount++;
            }
        } 

        //GET TO END LEVEL OBJECTIVE
        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("EndObject") && SceneManager.GetActiveScene().name == "GetToEndTest")
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("EndObject").transform.position) <= interactDistance)
            {
                LevelComplete();
                SceneManager.LoadScene("MainWorldTest");
                Debug.Log("You have returned to the main world and completed " + levelName);
                PrintLevelCompletionStatistics();
                if (isPlaying)
                {
                    WriteCSV();
                }
            }
        }

        //KILL ALL ENEMIES LEVEL OBJECTIVE
        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("EndObject") && totalEnemiesRemaining == 0 && SceneManager.GetActiveScene().name == "LevelKillTest")
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("EndObject").transform.position) <= interactDistance)
            {
                LevelComplete();
                SceneManager.LoadScene("MainWorldTest");
                Debug.Log("You have returned to the main world and completed " + levelName);
                PrintLevelCompletionStatistics();
                if (isPlaying)
                {
                    WriteCSV();
                }
            }
        }

        GameObject weaponPosObject = GameObject.FindWithTag("WeaponPos");  

        //HEIST LEVEL OBJECTIVE
        if (GameObject.FindWithTag("Player") != null && GameObject.FindWithTag("EndObject") && weaponPosObject != null && weaponPosObject.transform.childCount > 0 && weaponPosObject.transform.GetChild(0).CompareTag("HeistItem") && SceneManager.GetActiveScene().name == "LevelHeistTest")
        {
            if (Vector2.Distance(GameObject.FindWithTag("Player").transform.position, GameObject.FindWithTag("EndObject").transform.position) <= interactDistance)
            {
                LevelComplete();
                SceneManager.LoadScene("MainWorldTest");
                Debug.Log("You have returned to the main world and completed " + levelName);
                PrintLevelCompletionStatistics();
                if (isPlaying)
                {
                    WriteCSV();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteractionWithLevelEndObjects();
        }

        totalEnemiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    private Dictionary<string, float> levelCompletionTimes = new Dictionary<string, float>();
    private Dictionary<string, float> levelTotalEnemiesRemaining = new Dictionary<string, float>();
    float totalEnemiesRemaining;
    float totalShieldsUsed;
    float totalBottlesUsed;
    float totalGunsUsed;
    

    public void LevelComplete()
    {
        string levelName = SceneManager.GetActiveScene().name;

        if (!levelCompletionTimes.ContainsKey(levelName))
        {
            levelCompletionTimes.Add(levelName, Time.timeSinceLevelLoad);
            levelTotalEnemiesRemaining.Add(levelName, totalEnemiesRemaining);
        }
        else
        {
            float previousCompletionTime = levelCompletionTimes[levelName];
            float newCompletionTime = Time.timeSinceLevelLoad;

            if (newCompletionTime < previousCompletionTime)
            {
                levelCompletionTimes[levelName] = newCompletionTime;
            }

            float previousTotalEnemiesRemaining = levelTotalEnemiesRemaining[levelName];
            float newTotalEnemiesRemaining = totalEnemiesRemaining;

            if (newTotalEnemiesRemaining < previousTotalEnemiesRemaining)
            {
                levelTotalEnemiesRemaining[levelName] = newTotalEnemiesRemaining;
            }
        }

    }

    public void PrintLevelCompletionStatistics()
    {
        foreach (KeyValuePair<string, float> entry in levelCompletionTimes)
        {
            if (entry.Key == SceneManager.GetActiveScene().name)
            {
                Debug.Log("You completed the level " + entry.Key + " in " + Time.timeSinceLevelLoad + " seconds" + ", Compared to your highscore " + entry.Value + " seconds.");
                Debug.Log("You have completed" + entry.Key + levelOneCount + " times.");
            }
        }
        foreach (KeyValuePair<string, float> entry in levelTotalEnemiesRemaining)
        {
            if (entry.Key == SceneManager.GetActiveScene().name)
            {
                Debug.Log("You completed the level " + entry.Key + " with a total enemies remaining = " + entry.Value + " out of " + totalEnemyCount);
            }
        }

        totalGunsUsed = GameObject.FindWithTag("Whip").GetComponent<WhipPullClick>().gunCount;
        totalShieldsUsed = GameObject.FindWithTag("Whip").GetComponent<WhipPullClick>().shieldCount;
        totalBottlesUsed = GameObject.FindWithTag("Whip").GetComponent<WhipPullClick>().bottleCount;
        Debug.Log("You used the bottle " + totalBottlesUsed + " times." +
                  " The gun a total of " + totalGunsUsed + " times." +
                  " The shield a total of " +totalShieldsUsed + " times.");

        Debug.Log("You have completed Level One " + levelOneCount + " times." +
                  " Level Two " + levelTwoCount + " times." +
                  " Level Three " + levelThreeCount + " times.");

    }

    public void WriteCSV()
    {
        bool fileExists = File.Exists(filename);

        using (StreamWriter tw = new StreamWriter(filename, true))
        {
            if (!fileExists)
            {
                string header = "Level Name,Level Completion Time,Total Enemy Count, Total Enemies Remaining,Total Bottles Used,Total Guns Used,Total Shields Used,Level One Completions,Level Two Completions, Level Three Completions";
                tw.WriteLine(header);
            }

            string data = SceneManager.GetActiveScene().name + "," + Time.timeSinceLevelLoad + "," + totalEnemyCount + "," + totalEnemiesRemaining + "," + totalBottlesUsed + "," + totalGunsUsed + "," + totalShieldsUsed + "," + levelOneCount + "," + levelTwoCount + "," + levelThreeCount;
            tw.WriteLine(data);
        }
    }
}
