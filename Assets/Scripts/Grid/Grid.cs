using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject Grid_Highlight;
    [SerializeField] private GameObject Grid_Edit;
    [SerializeField] public GameObject Movable_Range;
    [SerializeField] public GameObject Attack_Range;
    [SerializeField] private GameObject Unsearched;
    [SerializeField] private GameObject Searched;

    public string grid_name;
    public int grid_idx;
    public Vector3 grid_pos;
    public int grid_movement_cost;
    public int grid_vision_cost;
    public Grid_Type grid_type;

    public Grid_State grid_state;

    // Floor.cs
    public bool arrivableHere; // 선택된 유닛이 이 발판으로 이동할 수 있을 때 true
    public bool attackableHere; //선택된 유닛이 이 발판까지 공격할 수 있을 때 true

    public int movementCostAfterArrive; // 이 발판에 도착한 후 남는 코스트 (플레이어 이동 시 사용)
    public int rangeCostToHere; // 이 발판까지 도달한 뒤 남는 사거리 코스트 (0 이하가 되면 공격 못하는 발판임)
    public int visionCostAfterArrive;

    // 아래 변수들은 A* 알고리즘에 사용 (EnemyController 클래스)
    // 참조 : https://www.youtube.com/watch?v=tqwsnUkUleA

    public int G;   // G : 시작으로부터 이동했던 거리
    public int H;   //H : |가로|+|세로| 장애물 무시하여 목표까지의 거리
    public int F;   //F : G + H
    public Grid parentFloor;   // parent Floor : 현재 그리드 직전에 확인했던 그리드 

    public virtual void Init()
    {

    }

    // 유닛이 발판 지날 때 필요한 코스트를 반환하는 함수
    public virtual int MovementCostReturn(Unit unit)
    {
        return grid_movement_cost;
    }

    public virtual int GetVisionCost()
    {
        return grid_vision_cost;
    }

    public virtual int RangeCostReturn(Unit unit)
    {
        return 1;
    }

    // Grid 스크립트에 있는 값을 초기화 시켜줍니다.
    // Unit 움직임에 관련된 요소 초기화.
    public virtual void initGrid()
    {
        movementCostAfterArrive = 0;
        visionCostAfterArrive = 0;
        rangeCostToHere = 0;
        parentFloor = null;
        arrivableHere = false;
        attackableHere = false;
        Movable_Range.SetActive(false);
        Attack_Range.SetActive(false);
    }
    
    
    void OnMouseOver()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if(Input.GetMouseButton(0) && UI.EditMode == true)
        {
            if(!EventSystem.current.IsPointerOverGameObject())
            {
                Grid New_GridScript = UI.Selected_Grid.GetComponent<Grid>();
                Grid.ChangeGrid(New_GridScript.grid_idx, New_GridScript.grid_movement_cost, New_GridScript.grid_vision_cost, (int)grid_pos.x, (int)grid_pos.y, New_GridScript.grid_type);
            }
        }
    }

    void OnMouseEnter()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        if(UI.EditMode == false)
        {  
            if(!EventSystem.current.IsPointerOverGameObject() && grid_type != Grid_Type.Border)
            {
                Grid_Highlight.SetActive(true);
            }
        }
        
        // map edit ui를 키고 끄는 과정 중에서 Clone으로 생성된 Grid 스크립트가 삭제되어 Selected Object가 Missing Object로 변경되어 발생하는 오류를 수정해주었습니다.
        else
        {
            if(!EventSystem.current.IsPointerOverGameObject() && grid_type != Grid_Type.Border && UI.Selected_Grid != null)
            {
                SpriteRenderer edit_grid = Grid_Edit.GetComponent<SpriteRenderer>();
                SpriteRenderer selected_grid = UI.Selected_Grid.GetComponent<SpriteRenderer>();
                edit_grid.sprite = selected_grid.sprite;
                Grid_Edit.SetActive(true);
            }
        }
    }

    void OnMouseExit()
    {
        Grid_Highlight.SetActive(false);
        Grid_Edit.SetActive(false);
    }

    // for fog system
    public void GridState_Handler(Grid_State gridState)
    {
        switch(gridState)
        {
            case Grid_State.Unsearched:
                Unsearched.SetActive(true);
                Searched.SetActive(false);
                break;

            case Grid_State.Searched:
                Unsearched.SetActive(false);
                Searched.SetActive(true);
                break;

            case Grid_State.Checkable:
                Unsearched.SetActive(false);
                Searched.SetActive(false);
                break;

            default:
                Debug.Log("Invalid Grid Status (Grid.cs)");
                break;
        }
    }

    public virtual int CostReturn(Unit unit)
    {
        return grid_movement_cost;
    }

    public GameObject GetUnit(string camp)
    {
        GameObject Unit = null;
        foreach(Transform child in this.transform)
        {
            if(child.tag == camp)
            {
                Unit = child.gameObject;
                break;
            }
        }
        return Unit;
    }

    public bool IsThereUnit()
    {
        foreach(Transform child in this.transform)
        {
            if(child.tag == "Player" || child.tag == "Enemy") return true;
        }
        return false;
    }
}
