using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Ingame : MonoBehaviour
{
    // For Singleton
    [SerializeField] GameObject ButtonManager;

    /* Map Selection */
    public FileInfo[] Files;
    public List<string> FileNames;
    public GameObject MapSelectUI;
    public GameObject PFB_MapButton;
    public Transform MapSelect_ScrollViewContent;

    /* Input Save Map Name*/
    public GameObject MapNameUI;
    public GameObject inputField;
    string savemap_name;

    /* In-Game Map Editting */
    public GameObject PFB_GridButton;
    public GameObject MapEditorUI;
    public Transform MapEditor_ScrollViewContent;
    public GameObject Selected_Grid;
    public bool EditMode;

    /* Pause UI */
    public GameObject PauseUI;
    public GameObject RestartButton;

    /* Item System */
    public GameObject PFB_Item_Button;
    public GameObject PFB_Inventory_Button;
    public Transform Inventory_Item_ScrollViewContent;
    public Transform Inventory_Select_ScrollViewContent;

    void Awake()
    {
        EditMode = false;
    }

    /* Map Selection UI Part */

    // A function that reflects changes in the save_path folder each time you run code that dynamically generates a button.
    // 버튼을 동적으로 생성해주는 코드를 실행할 때마다 save_path 폴더의 변동 사항을 반영해주는 함수.
    public void UpdateDirectory()
    {
        DirectoryInfo dir = new DirectoryInfo(DataManager.Instance.GetPath_Mapdata());
        Files = dir.GetFiles();
        FileNames.Clear();

        ClearButton(MapSelect_ScrollViewContent);

        foreach(FileInfo file in Files)
        {
            if(file.Extension == ".mapdata")
            {
                FileNames.Add(file.Name);
            }
        }
    }

    // A function that dynamically generates a button to select files in the save_path folder and load the map.
    // save_path 폴더의 파일들을 선택해 해당 맵을 불러와주는 버튼을 동적으로 생성시켜주는 함수.
    public void GenerateMapSelectUI()
    {
        UpdateDirectory();
        foreach(string map_name in FileNames)
        {
            GameObject MapButton = Instantiate(PFB_MapButton) as GameObject;
            MapButton.name = map_name.Substring(0, map_name.Length - ".mapdata".Length);
            MapButton.transform.SetParent(MapSelect_ScrollViewContent.transform);
            MapButton.GetComponentInChildren<TextMeshProUGUI>().text = map_name.Substring(0, map_name.Length - ".mapdata".Length);

            MapButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ButtonManager.GetComponent<ButtonManager>().Button_LoadGridMap(MapButton.name);
            });
        }
    }

    // Function to turn on and off MapSelect UI. Performed by wrapping in Button Manager.
    // MapSelectUI을 키고 끄는 함수. ButtonManager에서 Wrapping하여 수행된다.
    public void MapSelectUIOnOff()
    {
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();

        if(MapSelectUI.activeSelf == true)
        {
            MapSelectUI.SetActive(false);
            camera.CameraMovementActivate = true;
        }
        else
        {
            GenerateMapSelectUI();
            MapSelectUI.SetActive(true);
            camera.CameraMovementActivate = false;
        }
    }

    // It is the same as GenerateMapSelectUI, but adds a function that changes GameState to the generated button.
    // GenerateMapSelectUI와 같지만 생성되는 버튼에 GameState를 변경시켜주는 기능을 추가시킴.
    public void SelectMapForPlay()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();

        UpdateDirectory();
        foreach(string map_name in FileNames)
        {
            GameObject MapButton = Instantiate(PFB_MapButton) as GameObject;
            MapButton.name = map_name.Substring(0, map_name.Length - ".mapdata".Length);
            MapButton.transform.SetParent(MapSelect_ScrollViewContent.transform);
            MapButton.GetComponentInChildren<TextMeshProUGUI>().text = map_name.Substring(0, map_name.Length - ".mapdata".Length);

            MapButton.GetComponent<Button>().onClick.AddListener(() => 
            {
                ButtonManager.GetComponent<ButtonManager>().Button_LoadGridMap(MapButton.name);
                game.ChangeState(GameState.InitUnits);
                game.currentMap = MapButton.name;
            });
        }

        MapSelectUI.SetActive(true);
        camera.CameraMovementActivate = false;
    }

    /* Save Map Part */
    
    // A function that creates a save file that names the string received from the in-game
    // 인게임에서 받은 파일명을 갖는 .mapdata 파일을 생성시켜주는 함수.
    public void SaveWithMapName()
    {
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
        savemap_name = inputField.GetComponent<TextMeshProUGUI>().text;

        // 특이하게 아무것도 없는 문자열인데 string.Length 메소드의 반환값이 1로 나옴.
        // /n, /r, /f, %u200C, %u2063 등 모두 테스트를 진행했지만 조건에 맞지 않았음.
        // 가장 가능성이 높은 것은 string.Length가 char의 갯수가 아닌 instance의 갯수를 반환한다는 점에서 메모리 영역 할당을 위해 무언가가 있다로 잠정 결론(문제 생길 여지 있음)
        // 따라서 지금은 길이가 2 이상(적어도 한글자) 및 공백으로만 이루어진 경우만 잡아주는 걸로 작성함.
        // 정답은 폭 없는 공백 문자(유니코드 200B)가 기본적으로 들어가있는 상태였기 때문에 디버그로는 잡지 못하고 길이는 1로 나오는 것이었다.
        // Input Field에서 작성된 문자열 중 공백으로만 이루어진 문자열은 0020 0020 0020 .... 200B꼴로 이루어져 있었다.
        // 마지막에 폭 없는 공백 문자가 있어서 IsNullOrWhiteSpace가 정상동작하지 않았다! 그래서 마지막 문자를 제거해주고 필터링 함수를 굴려주면 정상작동한다.
        if(savemap_name != "\u200B" && !string.IsNullOrWhiteSpace(savemap_name.TrimEnd('\u200B')))
        {
            if(Grid.SaveGridMap(savemap_name)) MapNameUI.SetActive(false);
            CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
            camera.CameraMovementActivate = true;
        }
        else
        {
            Debug.Log("Error:Invalid Map Name Input!");
        }
    }

    /* Map Editting Part */

    // A function that allows you to select Grid as Button in the MapEditUI to edit the current Map with that Grid.
    // MapEditUI에서 Button으로 Grid를 선택해 해당 Grid로 현재 Map을 편집할 수 있도록 해주는 함수.
    public void ChangeEditGrid(GameObject PFB)
    {
        Selected_Grid = PFB;
    }

    // A function that dynamically creates a button for selecting Basic Grid.
    // Basic Grid를 선택하는 버튼을 동적으로 만들어주는 함수.
    public void GenerateBasicGridButton()
    {
        foreach(KeyValuePair<int, GameObject> entry in DataManager.Instance.GetDatabase_Grid(Grid_Type.Terrain))
        {
            GameObject GridButton = Instantiate(PFB_GridButton) as GameObject;
            GameObject PFB_Grid = entry.Value;
            GameObject Copied = Instantiate(PFB_Grid);
            Copied.transform.SetParent(GridButton.transform);
            Copied.SetActive(false);

            GridButton.transform.SetParent(MapEditor_ScrollViewContent.transform);
            Sprite GridSprite = Copied.GetComponent<SpriteRenderer>().sprite;
            Image Button_Image = GridButton.GetComponent<Image>();
            Button_Image.sprite = GridSprite;

            string DisplayText = Copied.name.StartsWith("PFB_") ? Copied.name.Substring(4) : Copied.name;
            DisplayText = DisplayText.Replace("(Clone)", "");
            GridButton.GetComponentInChildren<TextMeshProUGUI>().text = DisplayText;
            GridButton.name = DisplayText + " Button";

            GridButton.GetComponent<Button>().onClick.AddListener(() => ChangeEditGrid(Copied));
        }
    }

    // A function that dynamically creates a button for selecting Sign Grid.
    // Sign Grid를 선택하는 버튼을 동적으로 만들어주는 함수.
    public void GenerateSignGridButton()
    {
        foreach(KeyValuePair<int, GameObject> entry in DataManager.Instance.GetDatabase_Grid(Grid_Type.Sign))
        {
            GameObject GridButton = Instantiate(PFB_GridButton) as GameObject;
            GameObject PFB_Grid = entry.Value;
            GameObject Copied = Instantiate(PFB_Grid);
            Copied.transform.SetParent(GridButton.transform);
            Copied.SetActive(false);

            GridButton.transform.SetParent(MapEditor_ScrollViewContent.transform);
            Sprite GridSprite = Copied.GetComponent<SpriteRenderer>().sprite;
            Image Button_Image = GridButton.GetComponent<Image>();
            Button_Image.sprite = GridSprite;

            string DisplayText = Copied.name.StartsWith("PFB_") ? Copied.name.Substring(4) : Copied.name;
            DisplayText = DisplayText.Replace("(Clone)", "");
            GridButton.GetComponentInChildren<TextMeshProUGUI>().text = DisplayText;
            GridButton.name = DisplayText + " Button";

            GridButton.GetComponent<Button>().onClick.AddListener(() => ChangeEditGrid(Copied));
        }
    }

    // Function to turn on and off MapEdit UI. Performed by wrapping in Button Manager.
    // MapEdit UI를 키고 끄는 함수.
    public void MapEditorUIOnOff()
    {
        if(MapEditorUI.activeSelf == true)
        {
            MapEditorUI.SetActive(false);
            EditMode = false;
        }
        else
        {
            ClearButton(MapEditor_ScrollViewContent);
            GenerateBasicGridButton();
            GenerateSignGridButton();
            EditMode = true;
            MapEditorUI.SetActive(true);
        }
    }

    /* Pause UI Part */
    public void PauseUIOnOff()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();

        if(PauseUI.activeSelf == false)
        {
            camera.CameraMovementActivate = false;
            RestartButton.GetComponent<Button>().onClick.RemoveAllListeners();
            RestartButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if(game.currentMap != "")
                {
                    ButtonManager.GetComponent<ButtonManager>().Button_LoadGridMap(game.currentMap);
                    game.ChangeState(GameState.InitUnits);
                    PauseUI.SetActive(false);
                }
            });

            PauseUI.SetActive(true);
        }

        else
        {
            PauseUI.SetActive(false);
            camera.CameraMovementActivate = true;
        }
    }

    /* Unit Inventory Part */
    public void InitializeUnitUI()
    {
        ClearButton(Inventory_Item_ScrollViewContent);
        ClearButton(Inventory_Select_ScrollViewContent);
    }

    public void InventorySelectUIOnOff()
    {
        ClearButton(Inventory_Select_ScrollViewContent);
        GenerateInventorySelectUI();
    }

    public void GenerateInventorySelectUI()
    {
        Unit unit = UnitManager.Instance.unitToControl.GetComponent<Unit>();

        if(unit != null)
        {
            foreach(Inventory inven in unit.Unit_Inventorys)
            {
                GameObject newButton = Instantiate(PFB_Inventory_Button);
                newButton.transform.SetParent(Inventory_Select_ScrollViewContent);
                newButton.GetComponentInChildren<TextMeshProUGUI>().text = inven.Inventory_Name;
                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GenerateSingleInventoryUI(inven);
                });
            }
        }
    }

    public void GenerateSingleInventoryUI(Inventory inven)
    {
        ClearButton(Inventory_Item_ScrollViewContent);
        foreach(Item item in inven.Inventory_ItemsList)
        {
            GameObject newItemButton = GenerateItemButton(item);
            newItemButton.transform.SetParent(Inventory_Item_ScrollViewContent);
        }
    }

    public GameObject GenerateItemButton(Item item)
    {
        GameObject Item_Button = Instantiate(PFB_Item_Button);
        
        Sprite Item_Sprite = item.Get_ItemSprite();
        Image Button_Image = Item_Button.GetComponent<Image>();
        Button_Image.sprite = Item_Sprite;

        switch(item)
        {
            case NormalItem:
                break;

            case ConsumableItem consumableItem:
                
                TextMeshProUGUI maxCount = Item_Button.transform.Find("Max Count").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI currentCount = Item_Button.transform.Find("Current Count").GetComponent<TextMeshProUGUI>();
                
                maxCount.text = consumableItem.Max_Usage.ToString();
                currentCount.text = consumableItem.Current_Usage.ToString();

                Item_Button.GetComponent<Button>().onClick.AddListener(() => 
                {
                    item.Invoke_Item_Effect();
                    currentCount.text = consumableItem.Current_Usage.ToString();
                });
                break;

            default:
                Debug.Log("Error : Invalid Item Type (UIMANAGER)");
                break;
        }

        return Item_Button;
    }

    /* Universal Helper Function */

    // 동적으로 버튼을 생성해주는 UI를 부를 때마다 목록을 초기화해주는 함수.
    // 해당 함수를 수행해야 동일한 버튼이 중복되지 않는다.
    public void ClearButton(Transform ScrollViewContent)
    {
        for (int i = ScrollViewContent.childCount - 1; i >= 0; i--)
        {
            Transform child = ScrollViewContent.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
