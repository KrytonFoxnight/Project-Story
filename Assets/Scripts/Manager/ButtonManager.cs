using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    public void Button_LoadSceneAsync(string scene_name)
    {
        SceneController target = GameObject.FindWithTag("SceneController").GetComponent<SceneController>();
        target.SceneControllerLoadAsync(scene_name);
    }

    public void Button_Terminate()
    {
        Application.Quit();
    }

    public void Button_LoadGridMap(string map_name)
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        GridManager grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();

        if(map_name != "")
        {
            grid.LoadGridMap(map_name);
            UI.MapSelectUI.SetActive(false);

            CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
            camera.CameraMovementActivate = true;
        }
    }

    public void Button_OnSaveGridMap()
    {
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        
        UI.MapNameUI.SetActive(true);
        camera.CameraMovementActivate = false;
    }

    public void Button_OffSaveGridMap()
    {
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        
        UI.MapNameUI.SetActive(false);
        camera.CameraMovementActivate = true;
    }

    public void Button_GenerateMap()
    {
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
        Grid.GenerateMap();
    }

    public void Button_MapSelectUI()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.MapSelectUIOnOff();
    }

    public void Button_MapEditUI()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.MapEditorUIOnOff();
    }

    public void Button_StartGame()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        game.ChangeState(GameState.SelectMap);
    }

    public void Button_ChangeTurn()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        game.ChangeTurn();
    }

    public void Button_PauseUI()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.PauseUIOnOff();
    }

    // Better Control
    public void Button_Attack()
    {
        UnitManager.Instance.ShowAttackRange();
    }

    public void Button_Move()
    {
        UnitManager.Instance.ShowMovableRange();
    }

    // 해당 Unit의 Inventory 리스트를 고를 수 있도록 해주는 함수.
    public void Button_UnitsInventory()
    {
        UI_Ingame UI = GameObject.FindWithTag("UI").GetComponent<UI_Ingame>();
        UI.InventorySelectUIOnOff();
    }

    // Item System
    public void Button_ItemApply()
    {

    }

}
