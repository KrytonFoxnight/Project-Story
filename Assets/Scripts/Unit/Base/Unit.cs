using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Unit_Type
{
    Armored,
    Infantry,
    Support
}

public class Unit : MonoBehaviour
{
    // DB 구성을 위한 개별 ID
    protected int Unit_Id;
    protected string Unit_Name;

    //최대 이동코스트, 최대 체력, 최대 공격 횟수
    // 추후 상수 개념으로 이용하는 것으로 하면 어떨까?
    public int maxMovingCost;
    public int maxHp;
    public int maxAttackCnt;

    // 현재 값들(Item System을 위해 작성됨)
    public int Current_HP;
    public int Current_MVCost;
    public int Current_ATTCnt;

    public int movingCost;  // 보유한 이동 코스트
    public Unit_Type type;   // 유닛 타입
    public int attackPower; // 공격력
    public int armor;       // 방어막
    public int attackRange; // 공격 사거리
    public int attackCnt;   // 한 턴에 가능한 공격 횟수

    // Unit에게 부여된 Inventory들
    public List<Inventory> Unit_Inventorys;

    // 현재 선택된 Unit이 무엇인지 알려주기 위한 효과
    public GameObject Selected_Sign;

    public Grid Current_Grid;
    public List<Grid> Vision_Area = new List<Grid>();

    // 시야 계산을 위한 변수
    public int visionCost;

    public virtual void Init()
    {
        Unit_Id = -1;
    }

    //턴 시작 시 이동 코스트, 공격횟수를 초기화해주는 함수
    public void NewTurn()
    {
        movingCost = maxMovingCost;
        attackCnt = maxAttackCnt;
    }

    // 발판 이동 함수
    public virtual void moveTo(GameObject targetGrid){
        // Script Extract
        Grid targetGridScript = targetGrid.GetComponent<Grid>();
        Grid currentGridScript = transform.parent.GetComponent<Grid>();

        movingCost = targetGridScript.movementCostAfterArrive; // 현재 발판에서 타겟 발판까지 이동한 후 코스트

        // 이동한 유닛의 부모를 타겟발판으로 설정하고 위치 재설정
        // 이동 애니메이션 추가 필요(스무스하게 움직이도록)
        transform.SetParent(targetGrid.transform);
        this.transform.position = new Vector3(targetGrid.transform.position.x, targetGrid.transform.position.y, this.transform.position.z);
        this.Current_Grid = targetGrid.GetComponent<Grid>();
    }

    //유닛 피격 함수
    public virtual void OnDamage(int damage)
    {
        // 만약 방어막이 공격을 감당 못하면 방어막, 체력 둘다 피격 처리
        if (armor < damage)
        {
            damage -= armor;
            armor = 0;
            Current_HP -= damage;
        }

        // 방어막이 공격을 막을 수 있으면 방어막만 피격처리
        else
            armor -= damage;

        // hp가 0에 도달하면 사망처리
        if(Current_HP <= 0)
            OnDie();
    }

    // 유닛 공격 함수
    public virtual void Attack(Unit targetUnit)
    {
        // 남은 공격횟수가 있을 때만 실행
        if (attackCnt > 0)
        {
            // 타겟 유닛에서 공격하는 유닛의 공격력만큼 피격 처리
            targetUnit.OnDamage(attackPower);
            // 남은 공격 횟수 차감
            attackCnt--;
        }
    }

    //유닛 사망 처리 함수
    public void OnDie()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        
        // 오브젝트 파괴
        if(this.tag == "Player")
        {
            game.playerUnits.Remove(gameObject);
            Destroy(gameObject);
        }

        else if(this.tag == "Enemy")
        {
            game.enemyUnits.Remove(gameObject);
            Destroy(gameObject);
        }

        // Race 문제있으면 여기 먼저 확인
        game.CheckResult();
    }

    public int reducedHP()
    {
        return maxHp - Current_HP;
    }

    // 힐 받기 처리
    public virtual void OnHeal(int healPoint)
    {
        Current_HP += healPoint;

        if (maxHp < Current_HP)
        {
            Current_HP = maxHp;
        }
    }

    // Item System Helper Func.
    public void Init_Inventory()
    {
        this.Unit_Inventorys = new List<Inventory>();
        this.Add_Inventory("Armor inventory", Inventory_Type.Armors, 3);
        this.Add_Inventory("Consumable inventory", Inventory_Type.ConsumableItems, 3);

        // 아이템을 추가시킬 때, 프리팹을 복사해서 추가시켜주어야 Count값의 독립성이 보장되므로 다음과 같이 작성할 것.
        this.Get_Item(Inventory_Type.ConsumableItems, "Item_Syringe", 2);
    }

    public void Get_Item(Inventory_Type type, string name, int num)
    {
        foreach(Inventory inven in this.Unit_Inventorys)
        {
            for(int i = 1; i <= num; ++i)
            {
                if(inven.Type == type && inven.Inventory_ItemsList.Count < inven.Inventory_Size)
                {
                    GameObject PFB_Item = DataManager.Instance.GetObject_Item(type, name);
                    Item item = PFB_Item.GetComponent<Item>();
                    inven.Inventory_ItemsList.Add(item);
                    Destroy(PFB_Item);
                }
            }
        }
        
    }

    public void Add_Inventory(string inven_name, Inventory_Type inven_type, int inven_size)
    {
        Inventory inven = ScriptableObject.CreateInstance<Inventory>();
        inven.Inventory_Name = inven_name;
        inven.Type = inven_type;
        inven.Inventory_Size = inven_size;
        inven.Inventory_ItemsList = new List<Item>();
        this.Unit_Inventorys.Add(inven);
    }

    public void SelectSignOn()
    {
        this.Selected_Sign.SetActive(true);
    }

    public void SelectSignOff()
    {
        this.Selected_Sign.SetActive(false);
    }

    public void CalculateVision()
    {
        SetCurrentVisionState(Grid_State.Searched);
        
        this.Vision_Area.Clear();
        this.CalculateCurrentVision(); 

        SetCurrentVisionState(Grid_State.Checkable);
        
    }

    public void SetCurrentVisionState(Grid_State state)
    {
        foreach(Grid grid in this.Vision_Area)
        {
            grid.GridState_Handler(state);
        }
    }
    
    // 재귀 함수 방식을 채용할 경우, IEnumerator와 Coroutine을 사용해도 Race문제로 인해 시야 계산이 비정상적으로 동작했습니다.
    // 이를 위해 한 함수의 실행에 시야 계산 완료가 보장되도록 함수를 최적화하였습니다.
    // 만약 Attack 또는 Move에 Race문제가 생길 경우 다음 코드를 참고하면 되겠습니다.
    // 나중에 Vision과 관련된 것들 제거할 것
    public void CalculateCurrentVision()
    {
        GridManager grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
        int remainingCost = this.visionCost - 1;
        Grid gridToCheck; // 확인할 발판
        List<Grid> gridToNextCheckList = new List<Grid>(); // 다음 재귀에서 확인할 발판의 중앙 발판 리스트들 (다음 재귀에서 이 리스트에 들어있는 발판들의 동서남북 발판을 확인하는거임/ 뭐라 설명을 못하겠네ㄷㄷ)
        List<Grid> newCheckList = new List<Grid>();

        int[] dx = {-1, 1, 0, 0};
        int[] dy = {0, 0, -1, 1};

        for(int i = 0; i < 4; ++i)
        {
            gridToCheck = grid.GridValidity((int)this.Current_Grid.grid_pos.x + dx[i], (int)this.Current_Grid.grid_pos.y + dy[i]);
            if(gridToCheck != null)
            {
                gridToNextCheckList.Add(gridToCheck);
            }
        }

        while(gridToNextCheckList.Count > 0 && remainingCost >= 0)
        {
            foreach(Grid g in gridToNextCheckList)
            {
                if (g.GetVisionCost() <= remainingCost)
                {
                    for(int i = 0; i < 4; ++i)
                    {
                        gridToCheck = grid.GridValidity((int)g.grid_pos.x + dx[i], (int)g.grid_pos.y + dy[i]);
                        if(gridToCheck != null)
                        {
                            newCheckList.Add(gridToCheck);    // 다음 체크할 리스트에 추가
                        }
                    }
                        
                    if (!this.Vision_Area.Contains(g))
                    {
                        this.Vision_Area.Add(g); // 최종 선택 플로어 리스트에 추가 (유닛이 현재 도달 가능한 플로어 리스트)
                    }
                }
            }
            
            gridToNextCheckList.AddRange(newCheckList);
            newCheckList.Clear();
            remainingCost--;
        }
        
    }

    public int Get_UnitID()
    {
        return this.Unit_Id;
    }

    public string Get_UnitName()
    {
        return Unit_Name;
    }
}
