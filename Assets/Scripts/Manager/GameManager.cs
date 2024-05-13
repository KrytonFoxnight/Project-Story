using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

// Declare the status of the current game as enum variable.
// 현재 State를 열거형으로 선언해서 관리함.
public enum GameState {
    Error,
    Idle,
    SelectSynopsis,
    SelectStage,
    SelectMap,
    DialogueNormal,
    DialogueHead,
    DialogueTail,
    InitUnits,
    GlobalEffects,
    SpawnPlayer,
    SpawnEnemy,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Lose,
    Draw
}

public class GameManager : MonoBehaviour
{   
    // Current GameState
    public GameState currentState;
    public bool isPlaying;

    // For Debug UI
    public TextMeshProUGUI currentTurnText;
    public TextMeshProUGUI currentStateText;
    private bool DEBUG_MODE = true;
    
    // Player Units List
    public List<GameObject> playerUnits;

    // For Temp Idea
    public GameObject[] playerSpawnPoint;

    // Enemy Units List
    public List<GameObject> enemyUnits;
    
    // For Temp Idea
    public GameObject[] enemySpawnPoint;

    // For Enemy AI.
    public Priority_Queue<Intelligence> intelligenceArchive = new Priority_Queue<Intelligence>();
    public List<Player> visiblePlayerUnits;

    // For Current Data
    public string currentMap;
    private Synopsis_MetaData currentMetadata;

    // For Global Effect
    public delegate void GlobalEffect();
    private GlobalEffect globalEffect;
    private bool test_flag;

    // Initialize Current GameState
    // 처음 시작할 때의 State를 Idle로 설정
    void Start()
    {
        ChangeState(GameState.Idle);
        test_flag = true;
        globalEffect = Test_Global;

        currentMap = "";
        isPlaying = false;
    }

    // Change GameState with some functions.
    // 인자로 받은 State로 변경시키고 State에 알맞게 Wrapping된 함수를 실행시킴.
    // State간의 Race 문제로 인한 오류가 발생했었음. 추후 async 적용을 고려.
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        if(currentStateText != null) currentStateText.text = newState.ToString();
        switch(newState)
        {
            case GameState.Error:
                Debug.Log("Error State");
                break;

            case GameState.Idle:
                break;

            case GameState.SelectSynopsis:
                break;

            case GameState.SelectStage:
                break;

            case GameState.DialogueNormal:
                break;

            case GameState.DialogueHead:
                break;

            case GameState.DialogueTail:
                break;

            case GameState.SelectMap:
                isPlaying = true;
                SelectMap();
                break;

            case GameState.InitUnits:
                playerUnits.Clear();
                enemyUnits.Clear();
                UnitSetting();
                ChangeState(GameState.GlobalEffects);
                break;

            case GameState.GlobalEffects:
                globalEffect();
                ChangeState(GameState.PlayerTurn);
                break;

            case GameState.SpawnPlayer:
                break;

            case GameState.SpawnEnemy:
                break;

            case GameState.PlayerTurn:
                StartPlayerTurn();
                break;

            case GameState.EnemyTurn:
                InfoUpdate();
                StartEnemyTurn();
                break;

            case GameState.Victory:
                isPlaying = false;
                break;

            case GameState.Lose:
                isPlaying = false;
                break;

            case GameState.Draw:
                isPlaying = false;
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    /* Wrapping Functions for GameManager */
    void SelectMap()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.SelectMapForPlay();
    }

    void UnitSetting()
    {
        UnitManager.Instance.UnitSettingForPlay();
        foreach(GameObject unit in playerUnits)
        {
            unit.tag = "Player";
        }

        foreach(GameObject unit in enemyUnits)
        {
            unit.tag = "Enemy";
        }
    }

    // StartPlayerTurn, StartEnemyTurn 수정
    void StartPlayerTurn()
    {
        CalculateUnitsVision();
        //currentTurnText.text = "Player Turn";
        foreach(GameObject playerUnit in playerUnits)
        {
            playerUnit.GetComponent<Unit>().NewTurn();
        }
    }

    void StartEnemyTurn()
    {
        //currentTurnText.text = "Enemy Turn";
        foreach(GameObject enemyUnit in enemyUnits)
        {
            enemyUnit.GetComponent<Unit>().NewTurn();
        }
    }

    // Change Turn(Player to Enemy || Enemy to Player)
    // Player와 Enemy간의 턴 변화를 수행해주는 코드.
    public void ChangeTurn()
    {
        switch(currentState)
        {
            case GameState.PlayerTurn:
                ChangeState(GameState.EnemyTurn);
                break;

            case GameState.EnemyTurn:
                ChangeState(GameState.PlayerTurn);
                break;
            
            default:
                Debug.Log("Invalid Current State");
                break;
        }
    }

    public void CheckResult()
    {
        string result = "wut?";
        Debug.Log("Checking... Enemy:" + enemyUnits.Count);
        if(enemyUnits.Count == 0)
        {
            if(playerUnits.Count == 0)
            {
                result = "Draw!";
                Debug.Log("Draw!");
                ChangeState(GameState.Draw);
            }

            else
            {
                result = "Victory!";
                Debug.Log("Victory!");
                ChangeState(GameState.Victory);
            }
        }

        else if(playerUnits.Count == 0)
        {
            if(enemyUnits.Count == 0)
            {
                result = "Draw!";
                Debug.Log("Draw!");
                ChangeState(GameState.Draw);
            }

            else
            {
                result = "Lose";
                Debug.Log("Lose!");
                ChangeState(GameState.Lose);
            }
        }
    }

    void Test_Global()
    {
        if(test_flag)
        {
            foreach(GameObject unitObject in playerUnits)
            {
                Unit unit = unitObject.GetComponent<Unit>();
                unit.armor += 10;
            }
        }
    }

    void CalculateUnitsVision()
    {
        foreach(GameObject unitObject in playerUnits)
        {
            Player unit = unitObject.GetComponent<Player>();
            unit.CalculateVision();
        }

        if(DEBUG_MODE)
        {
            foreach(GameObject unitObject in enemyUnits)
            {
                Enemy unit = unitObject.GetComponent<Enemy>();
                unit.CalculateVision();
            }
        }
    }

    public void Set_CurrentMetadata(Synopsis_MetaData meta)
    {
        this.currentMetadata = meta;
    }

    public Synopsis_MetaData Get_CurrentMetadata() { return currentMetadata; }

    // For Information at BT.
    public Grid GetTarget()
    {
        /*
            2024-04-11 회의록 참고
        */
        Grid target = null;
        float min_value = 200.0f;

        // 가장 가까운 Player Unit을 target으로 선정합니다.
        if(visiblePlayerUnits.Count > 0)
        {    
            foreach(Player player in visiblePlayerUnits)
            {
                if(target == null) target = player.Current_Grid;
                else
                {
                    float dist = Vector3.Distance(target.grid_pos, player.Current_Grid.grid_pos);
                    if(min_value > dist)
                    target = player.Current_Grid;
                    min_value = dist;
                }
            }
        }

        // 만약, 시야에 Player Unit이 없는 경우, Intelligence를 통해 target을 설정합니다.
        else if(intelligenceArchive.Count() > 0)
        {
            /*  논의사항
                1. Peek : 가장 우선해야할 정보 하나만을 가지고 모든 Enemy가 행동하게됨.
                2. Dequeue : 한 정보는 단 한 번만 사용됨
            */
            Intelligence intel = intelligenceArchive.Peek();
            target = intel.target;
        }

        // 모든 정보가 존재하지 않는 경우 null로 반환합니다. 이 때, BT에서는 Patrol을 수행하도록 합니다.
        // 아니면 이 함수에서 임의의 Grid를 선출하도록 하는 방법도 존재한다. > 이게 맞을 듯
        return target;
    }

    void InfoUpdate()
    {
        foreach(GameObject enemy in enemyUnits)
        {
            Enemy script = enemy.GetComponent<Enemy>();
        }
    }
}
