using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class UnitManager : MonoBehaviour
{
    // For Singleton
    public static UnitManager Instance;

    // Raycast에 필요한 변수들
    public RaycastHit2D hit;

    // 좌클릭으로 선택한 unit & 우클릭으로 선택된 Grid
    public GameObject unitToControl = null;
    public GameObject targetGrid = null;

    // 도달 가능한 Grid List & 공격 가능한 Grid List
    public List<Grid> arrivableGrid = new List<Grid>();
    public List<Grid> attackableGrid = new List<Grid>();

    // Player unit & Enemy unit
    public GameObject PFB_Player;
    public GameObject PFB_Enemy;

    // 게임이 시작될 때 유닛의 위치
    // 비일치 경우가 발생할 수 있어서 player unit 스폰 기능 구현 후 삭제할 예정(오류 발생 가능)
    public Vector3 Pos_Player;
    public Vector3 Pos_Enemy;

    // new ui test
    public GameObject PFB_Item;

    void Awake()
    {
        //싱글턴
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void SetSelectedUnit(Unit unit)
    {
        unitToControl = unit.gameObject;
    }

    // 공격 논리를 Update함수에서 굴리게 되면 오버헤드가 발생할 것으로 예상되서 논리 수정 후 위키 기재 예정
    // 공격과 움직임에 대해 처리되지 않는 경우가 있을 수 있으므로 조금 다듬어볼 예정.
    void Update()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        // 좌클릭 시 leftClick을 통해 움직일 unit을 얻어옴.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            unitToControl = leftClick();
        }

        // 우클릭 시 마우스가 있는 Grid를 얻어와서 설정해줍니다. 이후 unitOnGrid를 통해 적인지 아군인지 확인할 수 있습니다.
        if (Input.GetMouseButtonDown(1))
        {
            targetGrid = rightClick();
        }

        // Player 순서이면서 현재 조종하는 유닛과 공격할 유닛이 설정되어있는 경우
        if(unitToControl != null && targetGrid != null && (game.currentState == GameState.PlayerTurn)){

            Grid targetGridScript = targetGrid.GetComponent<Grid>();
            GameObject targetGridUnit = targetGridScript.GetUnit("Enemy");

            // 만약 선택된 Grid에 어떠한 unit도 없는 경우
            if (targetGridUnit == null)
            {
                // 도달 가능한 Grid일 경우 Player의 유닛을 움직인다.
                if (targetGridScript.arrivableHere)
                {
                    Unit unit = unitToControl.GetComponent<Unit>();
                    unit.moveTo(targetGrid);
                    targetGrid = null;
                    InitGrids();
                    return;
                }
            }

            // 만약 해당 Grid에 Enemy가 있고 공격 가능 영역에 있는 경우
            else if(targetGridUnit.tag == "Enemy" && targetGridScript.attackableHere)
            {
                Debug.Log("Attack!");
                Unit unitScript = unitToControl.GetComponent<Unit>();
                Unit targetScript = targetGridUnit.GetComponent<Unit>();

                unitScript.Attack(targetScript);

                unitToControl = null;
                targetGrid = null;
                InitGrids();
                return;
            }

            // 모두 해당되지 않는 경우
            unitToControl = null;
            targetGrid = null;
            InitGrids();
        }
    }


    // 마우스 커서 위치에서 레이캐스트 광선을 쏘고 RaycastHit 정보를 반환하기
    RaycastHit2D cursorRaycastInfo()
    {
        Camera camera = GameObject.FindWithTag("IngameCamera").GetComponent<Camera>();
        Vector2 worldPoint = camera.ScreenToWorldPoint(Input.mousePosition);
        return Physics2D.Raycast(worldPoint, Vector2.zero);
    }

    // 마우스 왼쪽클릭한 발판에서 플레이어 태그를 가지고 있는 유닛 오브젝트를 반환
    // 선택한 발판에 그런 유닛이 없으면 null 반환
    // (플레이어 유닛 선택할 때 쓰는 함수)
    public GameObject leftClick()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.InitializeUnitUI();

        // 레이캐스트 정보 받아오기
        RaycastHit2D hit = cursorRaycastInfo();

        // Raycast가 감지한 오브젝트가 없으면 null 반환
        if (hit.collider == null)
        {
            if(unitToControl != null)
            {
                unitToControl.GetComponent<Unit>().SelectSignOff();

                CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
                camera.InitFocusUnit();
                UnitManager.Instance.InitGrids();
            }
            return null;
        }
        
        // 자식 오브젝트 중에 Player 태그가 있는 오브젝트 찾기
        Transform[] children = hit.collider.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.tag == "Player")
            {
                child.GetComponent<Unit>().SelectSignOn();
                
                CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
                camera.FocusUnit(child.gameObject);
                return child.gameObject;
            }
        }

        // Player 태그가 있는 자식 오브젝트가 없으면 null 반환
        if(unitToControl != null)
        {
            unitToControl.GetComponent<Unit>().SelectSignOff();
            
            CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
            camera.InitFocusUnit();
            UnitManager.Instance.InitGrids();
        }
    
        return null;
    }

    // 오른쪽 클릭한 발판을 반환
    // 이때 unitToMove가 null이 아닌 상황에만 발판을 반환
    // (유닛의 종착지 선택할 때)
    public GameObject rightClick()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.InitializeUnitUI();

        // 레이캐스트 정보 받아오기
        hit = cursorRaycastInfo();

        // 제어할 유닛을 선택하지 않은 채로 우클릭 시 null 반환
        if (unitToControl == null)
        {
            InitGrids();
            return null;
        }

        // Raycast가 감지한 오브젝트가 없으면 null 반환
        if (hit.collider == null){
            unitToControl.GetComponent<Unit>().SelectSignOff();
            CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
            camera.InitFocusUnit();
            unitToControl = null;
            InitGrids();
            return null;
        }

        // Raycast가 감지한 오브젝트가 발판이 아니라면 null 반환
        Grid detectedGrid = hit.collider.GetComponent<Grid>();
        if(detectedGrid == null){
            unitToControl.GetComponent<Unit>().SelectSignOff();
            CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
            camera.InitFocusUnit();
            unitToControl = null;
            InitGrids();
            return null;
        }

        Debug.Log(detectedGrid.gameObject.name);

        return detectedGrid.gameObject;
    }

    // Grid에 저장된 grid_cost를 가지고 도달 가능한 모든 Grid를 List에 저장해준다.
    public void SearchArrivableGrids(Unit unit, int remainingCost, Grid currentGrid)
    {
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if (remainingCost < 0)
        {
            return;
        }

        Grid gridToCheck; // 확인할 발판
        List<Grid> gridToNextCheckList = new List<Grid>(); // 다음 재귀에서 확인할 발판의 중앙 발판 리스트들 (다음 재귀에서 이 리스트에 들어있는 발판들의 동서남북 발판을 확인하는거임/ 뭐라 설명을 못하겠네ㄷㄷ)

        int[] dx = {-1, 1, 0, 0};
        int[] dy = {0, 0, -1, 1};

        for(int i = 0; i < 4; ++i)
        {
            gridToCheck = Grid.GridValidity((int)currentGrid.grid_pos.x + dx[i], (int)currentGrid.grid_pos.y + dy[i]);

            if (gridToCheck != null && gridToCheck.movementCostAfterArrive < 9990 && !gridToCheck.IsThereUnit())
            {
                // 1. 확인할 발판에 남은 비용으로 이동이 가능한가?
                // 2. 확인할 발판에 도달할 수 있는 더 적은 비용이 있는가?
                if (gridToCheck.MovementCostReturn(unit) <= remainingCost &&
                    gridToCheck.movementCostAfterArrive <= remainingCost - gridToCheck.MovementCostReturn(unit))
                {
                    gridToNextCheckList.Add(gridToCheck);    // 다음 체크할 리스트에 추가
                    gridToCheck.arrivableHere = true;       // 발판에 이동 가능한 상태
                    gridToCheck.movementCostAfterArrive = remainingCost - gridToCheck.MovementCostReturn(unit); // 발판에 도달한 뒤 남는 코스트 재설정
                    gridToCheck.Movable_Range.SetActive(true);

                    if (!arrivableGrid.Contains(gridToCheck))
                    {
                        arrivableGrid.Add(gridToCheck); // 최종 선택 플로어 리스트에 추가 (유닛이 현재 도달 가능한 플로어 리스트)
                    }
                }
            }
        }

        for (int i = 0; i < gridToNextCheckList.Count; i++)
        {
            SearchArrivableGrids(unit, remainingCost - gridToNextCheckList[i].MovementCostReturn(unit), gridToNextCheckList[i]);
        }
    }

    // 이번엔 grid_rangecost를 활용해서 동일한 논리로 진행함.
    public void SearchAttackableGrids(Unit unit, int remainingRangeCost, Grid currentGrid)
    {
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if (remainingRangeCost < 0)
        {
            return;
        }

        Grid gridToCheck; // 확인할 발판
        List<Grid> gridToNextCheckList = new List<Grid>(); // 다음 재귀에서 확인할 발판의 중앙 발판 리스트들 (다음 재귀에서 이 리스트에 들어있는 발판들의 동서남북 발판을 확인하는거임/ 뭐라 설명을 못하겠네ㄷㄷ)

        if (unit.attackCnt <= 0)
        {
            return;
        }

        int[] dx = {-1, 1, 0, 0};
        int[] dy = {0, 0, -1, 1};

        for(int i = 0; i < 4; ++i)
        {
            gridToCheck = Grid.GridValidity((int)currentGrid.grid_pos.x + dx[i], (int)currentGrid.grid_pos.y + dy[i]);

            if (gridToCheck != null && gridToCheck.rangeCostToHere < 9990 && gridToCheck.GetUnit("Player") != unit.gameObject)
            {
                // 1. 확인할 발판을 남은 사거리로 공격이 가능한가?
                // 2. 확인할 발판에 도달할 수 있는 더 적은 비용이 있는가?
                if (gridToCheck.RangeCostReturn(unit) <= remainingRangeCost &&
                    gridToCheck.rangeCostToHere <= remainingRangeCost - gridToCheck.RangeCostReturn(unit))
                {
                    gridToNextCheckList.Add(gridToCheck);    // 다음 체크할 리스트에 추가
                    gridToCheck.attackableHere = true;       // 발판에 공격 가능한 상태
                    gridToCheck.rangeCostToHere = remainingRangeCost - gridToCheck.RangeCostReturn(unit); // 발판에 도달한 뒤 남는 코스트 재설정
                    gridToCheck.Attack_Range.SetActive(true);
                    attackableGrid.Add(gridToCheck); // 최종 선택 플로어 리스트에 추가 (유닛이 현재 공격 가능한 플로어 리스트)
                }
            }
        }

        for (int i = 0; i < gridToNextCheckList.Count; i++)
        {
            SearchAttackableGrids(unit, remainingRangeCost - gridToNextCheckList[i].RangeCostReturn(unit), gridToNextCheckList[i]);
        }
    }

    // 현재 움직임 & 공격 영역으로 판정되어 리스트에 들어있는 그리드들을 전부 초기화시킵니다.
    public void InitGrids()
    {
        for(int i = 0; i < arrivableGrid.Count; i++)
        {
            arrivableGrid[i].initGrid();
        }
        for(int i= 0; i < attackableGrid.Count; i++)
        {
            attackableGrid[i].initGrid();
        }
        arrivableGrid.Clear();
        attackableGrid.Clear();
    }

    // 유닛 테스트를 위해 초기 세팅을 해주는 임시 코드입니다.
    // 추후 삭제되거나 수정될 여지가 있습니다.
    // 변동사항 : 원래 Player 또는 Enemy가 생성될 경우 Start함수에서 초기값이 설정되었는데, Initialize 함수를 통해 명시적으로 초기회시켜주는 방식으로 변경시켰습니다.
    public void UnitSettingForPlay()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        GridManager grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        Pos_Player = new Vector3(0.0f, 0.0f, -3.0f);
        Pos_Enemy = new Vector3((float)(grid.getWidth() - 1), (float)(grid.getHeight() - 1), -2.0f);

        /* 테스트 환경에 사용될 Player Object를 임의로 생성시켜주었습니다. */
        GameObject Player = DataManager.Instance.GetObject_Unit(Unit_Type.Armored, "Unit_Tank");

        /* Player 오브젝트의 초기 설정 */
        Player.transform.position = Pos_Player;
        Player.transform.SetParent(grid.GridMap[0][0].transform);
        Player.GetComponent<Unit>().Current_Grid = grid.GridMap[0][0].GetComponent<Grid>();
        Player.transform.name = "Tanks";
        Player.transform.tag = "Player";
        Player.GetComponent<Unit>().Init_Inventory();

        /* GameManager의 오브젝트 리스트에 추가 */
        game.playerUnits.Add(Player);

        /*테스트 환경에 사용되는 Enemy Object를 임의로 생성시켜주었습니다. */
        GameObject Enemy = DataManager.Instance.GetObject_Unit(Unit_Type.Infantry, "Unit_Attacker");

        /* Enemy 오브젝트의 초기 설정 */
        Enemy.transform.position = Pos_Enemy;
        Enemy.transform.SetParent(grid.GridMap[(int)Pos_Enemy.x][(int)Pos_Enemy.y].transform);
        Enemy.GetComponent<Unit>().Current_Grid = grid.GridMap[(int)Pos_Enemy.x][(int)Pos_Enemy.y].GetComponent<Grid>();
        Enemy.transform.name = "Soldiers";

        /* GameManager의 리스트에 추가 */
        game.enemyUnits.Add(Enemy);
    }

    // 새로운 UI에 맞도록 기존 Update함수에서 분류하고 해당 함수들을 버튼에 할당시켜주었습니다.
    public void ShowMovableRange()
    {
        if(unitToControl != null)
        {
            GameObject Selected_Sign = unitToControl.transform.Find("Selected Sign").gameObject;
            Selected_Sign.SetActive(true);
            Unit unitScript = unitToControl.GetComponent<Unit>();
            SearchArrivableGrids(unitScript, unitScript.movingCost, unitToControl.transform.parent.GetComponent<Grid>());
        }

        else
        {
            InitGrids();
        }
    }

    public void ShowAttackRange()
    {
        if(unitToControl != null)
        {
            Unit unitScript = unitToControl.GetComponent<Unit>();
            SearchAttackableGrids(unitScript, unitScript.attackRange, unitToControl.transform.parent.GetComponent<Grid>());
        }

        else
        {
            InitGrids();
        }
    }
}
