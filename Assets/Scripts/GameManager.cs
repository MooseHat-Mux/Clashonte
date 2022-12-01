using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Game Settings")]
    public bool gameRunning;
    public float npcSpawnRadius;
    public float npcSpawnHeight;
    public float difficultyModifier;
    public float currentDifficulty;
    public NPClines npcLineLibrary;

    [Space]

    [Header("Game References")]
    public Transform startingPlayerPos;
    public Transform[] npcSpawnZones;
    public GameObject[] currentNPCs;

    [Space]

    [Header("References")]
    public IK_Animator player;
    public GameObject npc;

    [Space]

    [Header("UI References")]
    public UImode currentUI;
    public Canvas[] canvases;


    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    IEnumerator GameRoutine()
    {
        while (gameRunning)
        {
            currentDifficulty += difficultyModifier * Time.deltaTime;
            yield return null;
        }
    }

    public void StartGame()
    {
        gameRunning = true;

        StartCoroutine(GameRoutine());
    }

    public void ResetGame()
    {
        gameRunning = false;

        int npcLength = currentNPCs.Length;
        for (int i = 0; i < npcLength; i++)
        {
            currentNPCs[i].SetActive(false);
        }

        StartGame();
    }

    public void EndGame()
    {
        gameRunning = false;
    }

    #region Character/Object Instantiation
    public void spawnPlayer()
    {

    }

    public void spawnNPC()
    {

    }
    #endregion
}
