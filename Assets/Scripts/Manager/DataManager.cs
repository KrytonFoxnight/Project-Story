using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.Networking;
/*
    DataManager 원칙
        1. ROOT_xxx는 게임에 사용되는 Prefab의 원본을 담는 Dictionary임.
        2. ROOT_xxx는 어떠한 경우에도 수정되지 못하도록 private로 선언해주었음.
        3. 유일하게 Directory의 변동사항이 있을 경우에만 다시 Init 해주는 방식으로 DB를 변경할 수 있도록 만들 것.(추후 구현)

    - 진행목록 -
    
    아래 기술된 DB를 구성하는 요소들의 초기값이 각 Script에 기술된 Init 함수에 의해 결정되도록 해줌.
    다만 Item의 경우에 Init이 아닌 생성자를 통해 초기화됨. 이는 Delegate의 미적용 문제로 조치될 예정.

    Grid
        Terrain(기존 Basic), Sign, Border, Custom Grid Prefab별로 Dictionary를 생성해주어 ROOT로 보관함.
        GetObject_Grid를 통해 원하는 종류의 Grid 프리팹을 '복사'해서 넘겨줌(원본의 불변성을 보장)
        기존의 주먹구구 방식이 아닌 해당 path의 prefab 타입 오브젝트들을 모두 추가시켜주므로 유동적임.
        다만 Grid ID등의 일부 요소가 중첩될 수 있음. 이 점 유의할 것.

    Unit
        기존 Unit_Type을 개편하였음.
        같은 종류의 Unit이 Player와 Enemy 모두에게 사용할 수 있어야 하므로 '병과'별로 구분지어줌.
        이제 해당 Unit이 Player인지 Enemy인지 판단해주는 기준은 현재로선 'GameManager에 있는 배열'밖에 없음.
        사용법은 Grid와 동일함.

    Item
        Item도 위의 두 DB 구성법과 같은 방식으로 만들어졌음.

    DataManager를 만들면서 Assets의 폴더 구조도 체계적으로 개편해주었음.
    각 Path들은 불변 상대 경로 이므로 가능하면 변경하지 말것!!!!!!!
*/

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // Data Paths
    // Grid Manager
    private string Mapdata_Path, TerrainGrid_Path, SignGrid_Path, BorderGrid_Path, CustomGrid_Path, Synopsis_Path;

    // Items
    private string ConsumableItem_Path, ArmorItem_Path;

    // Units
    private string ArmoredUnit_Path, InfantryUnit_Path, SupportUnit_Path;

    // Synopsis
    [SerializeField] private GameObject Synopsis_MetaData;
    private string Effect_Sound_Path;

    // Data Containers
    // Scene Controller
    private Dictionary<string, GameState> ROOT_SceneState;

    // Grid Manager
    private Dictionary<int, GameObject> ROOT_TerrainGrid = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_SignGrid = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_BorderGrid = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_CustomGrid = new Dictionary<int, GameObject>();

    // Item
    private Dictionary<int, GameObject> ROOT_ConsumableItem = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_ArmorItem = new Dictionary<int, GameObject>();

    // Unit
    private Dictionary<int, GameObject> ROOT_ArmoredUnit = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_InfantryUnit = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> ROOT_SupportUnit = new Dictionary<int, GameObject>();

    //Dialogue
    private Dictionary<int, GameObject> ROOT_SynopsisMetaData = new Dictionary<int, GameObject>();

    // Strong Design : Singleton
    // SceneController에 의해 Scene이 전환되는 경우, 가장 먼저 생성된 객체를 유지시켜 이전 참조가 missing되지 않도록 수정
    // 중재자 패턴도 있으나, 대부분 Singleton으로 구현되었으므로 다음과 같이 수정하였음.
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else if(Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Mapdata_Path = "Assets/SaveFiles/";
        TerrainGrid_Path = "Assets/Prefabs/Prefab_for_Grid/Terrains";
        SignGrid_Path = "Assets/Prefabs/Prefab_for_Grid/Signs";
        BorderGrid_Path = "Assets/Prefabs/Prefab_for_Grid/Borders";
        CustomGrid_Path = "Assets/Prefabs/Prefab_for_Grid/Customs";

        ConsumableItem_Path = "Assets/Prefabs/Prefab_for_Item/Consumable";
        ArmorItem_Path = "Assets/Prefabs/Prefab_for_Item/Armor";

        ArmoredUnit_Path = "Assets/Prefabs/Prefab_for_Unit/Armored";
        InfantryUnit_Path = "Assets/Prefabs/Prefab_for_Unit/Infantry";
        SupportUnit_Path = "Assets/Prefabs/Prefab_for_Unit/Support";

        Synopsis_Path = "Assets/Stages";
        Effect_Sound_Path = "Assets/Resources/Synopsis/Effect/Sound";

        /* Database Initialize */
        Init_Database_Scene();
        Init_Database_Grid();
        Init_Database_Item();
        Init_Database_Unit();
        Init_Database_Synopsis();
    }

    // SceneController Parts
    void Init_Database_Scene()
    {
        ROOT_SceneState = new Dictionary<string, GameState>
        {
            {"Main_Scene", GameState.Idle},
            {"Ingame_Scene", GameState.DialogueHead},
            {"MapEdit_Scene", GameState.Idle},
            {"Setting_Scene", GameState.Idle},
            {"SynopsisSelection_Scene", GameState.SelectSynopsis},
            {"StageSelection_Scene", GameState.SelectStage}
        };
    }

    public GameState Get_SceneState(string SceneName)
    {
        if(ROOT_SceneState.TryGetValue(SceneName, out GameState state)) return state;
        else return GameState.Error;
    }
    // SceneController Parts End

    // GridManager Parts
    void Init_Database_Grid()
    {
        string[] TerrainPaths = AssetDatabase.FindAssets("t:prefab", new[] {TerrainGrid_Path});
        string[] SignPaths = AssetDatabase.FindAssets("t:prefab", new[] {SignGrid_Path});
        string[] BorderPaths = AssetDatabase.FindAssets("t:prefab", new[] {BorderGrid_Path});
        string[] CustomPaths = AssetDatabase.FindAssets("t:prefab", new[] {CustomGrid_Path});

        foreach(string path in TerrainPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Grid>().Init();
            ROOT_TerrainGrid.Add(prefab.GetComponent<Grid>().grid_idx, prefab);
        }

        foreach(string path in SignPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Grid>().Init();
            ROOT_SignGrid.Add(prefab.GetComponent<Grid>().grid_idx, prefab);
        }

        foreach(string path in BorderPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Grid>().Init();
            ROOT_BorderGrid.Add(prefab.GetComponent<Grid>().grid_idx, prefab);
        }

        foreach(string path in CustomPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Grid>().Init();
            ROOT_CustomGrid.Add(prefab.GetComponent<Grid>().grid_idx, prefab);
        }
    }

    public string GetPath_Mapdata()
    {
        return Mapdata_Path;
    }

    public Dictionary<int, GameObject> GetDatabase_Grid(Grid_Type type)
    {
        Dictionary<int, GameObject> copied;
        switch(type)
        {
            case Grid_Type.Terrain:
                copied = new Dictionary<int, GameObject>(ROOT_TerrainGrid);
                break;

            case Grid_Type.Sign:
                copied = new Dictionary<int, GameObject>(ROOT_SignGrid);
                break;

            case Grid_Type.Border:
                copied = new Dictionary<int, GameObject>(ROOT_BorderGrid);
                break;

            case Grid_Type.Custom:
                copied = new Dictionary<int, GameObject>(ROOT_CustomGrid);
                break;

            default:
                copied = null;
                break;
        }
        return copied;
    }

    public GameObject GetObject_Grid(Grid_Type type, string name)
    {
        Dictionary<int, GameObject> target = GetDatabase_Grid(type);

        foreach(KeyValuePair<int, GameObject> item in target)
        {
            if(item.Value.GetComponent<Grid>().grid_name == name)
            {
                GameObject clone = Instantiate(item.Value);
                clone.name = item.Value.name;
                return clone;
            }
        }

        return null;
    }
    // GridManager Part End

    // Item Parts
    void Init_Database_Item()
    {
        string[] ConsumablePaths = AssetDatabase.FindAssets("t:prefab", new[] {ConsumableItem_Path});
        string[] ArmorPaths = AssetDatabase.FindAssets("t:prefab", new[] {ArmorItem_Path});

        foreach(string path in ConsumablePaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<ConsumableItem>().Init();
            ROOT_ConsumableItem.Add(prefab.GetComponent<ConsumableItem>().Get_ItemID(), prefab);
        }

        foreach(string path in ArmorPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Armor>().Init();
            ROOT_ArmorItem.Add(prefab.GetComponent<Armor>().Get_ItemID(), prefab);
        }
    }

    public Dictionary<int, GameObject> GetDatabase_Item(Inventory_Type type)
    {
        Dictionary<int, GameObject> copied;
        switch(type)
        {
            case Inventory_Type.ConsumableItems:
                copied = new Dictionary<int, GameObject>(ROOT_ConsumableItem);
                break;

            case Inventory_Type.Armors:
                copied = new Dictionary<int, GameObject>(ROOT_ArmorItem);
                break;

            default:
                copied = null;
                break;
        }
        return copied;
    }

    public GameObject GetObject_Item(Inventory_Type type, string name)
    {
        Dictionary<int, GameObject> target = GetDatabase_Item(type);

        foreach(KeyValuePair<int, GameObject> item in target)
        {
            if(item.Value.GetComponent<Item>().Get_ItemName() == name)
            {
                GameObject clone = Instantiate(item.Value);
                clone.name = item.Value.name;
                return clone;
            }
        }
        return null;
    }
    // Item Parts End

    // Unit Parts
    void Init_Database_Unit()
    {
        string[] ArmoredUnitPaths = AssetDatabase.FindAssets("t:prefab", new[] {ArmoredUnit_Path});
        string[] InfantryUnitPaths = AssetDatabase.FindAssets("t:prefab", new[] {InfantryUnit_Path});
        string[] SupportUnitPaths = AssetDatabase.FindAssets("t:prefab", new[] {SupportUnit_Path});

        foreach(string path in ArmoredUnitPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Unit>().Init();
            ROOT_ArmoredUnit.Add(prefab.GetComponent<Unit>().Get_UnitID(), prefab);
        }

        foreach(string path in InfantryUnitPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Unit>().Init();
            ROOT_InfantryUnit.Add(prefab.GetComponent<Unit>().Get_UnitID(), prefab);
        }

        foreach(string path in SupportUnitPaths)
        {
            string realPath = AssetDatabase.GUIDToAssetPath(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
            prefab.GetComponent<Unit>().Init();
            ROOT_SupportUnit.Add(prefab.GetComponent<Unit>().Get_UnitID(), prefab);
        }
    }

    public Dictionary<int, GameObject> GetDatabase_Unit(Unit_Type type)
    {
        Dictionary<int, GameObject> copied;
        switch(type)
        {
            case Unit_Type.Armored:
                copied = new Dictionary<int, GameObject>(ROOT_ArmoredUnit);
                break;

            case Unit_Type.Infantry:
                copied = new Dictionary<int, GameObject>(ROOT_InfantryUnit);
                break;

            case Unit_Type.Support:
                copied = new Dictionary<int, GameObject>(ROOT_SupportUnit);
                break;

            default:
                copied = null;
                break;
        }
        return copied;
    }

    public GameObject GetObject_Unit(Unit_Type type, string name)
    {
        Dictionary<int, GameObject> target = GetDatabase_Unit(type);

        foreach(KeyValuePair<int, GameObject> unit in target)
        {
            if(unit.Value.GetComponent<Unit>().Get_UnitName() == name)
            {
                GameObject clone = Instantiate(unit.Value);
                clone.name = unit.Value.name;
                return clone;
            }
        }
        return null;
    }
    // Unit Parts End

    // Synopsis Parts Start
    void Init_Database_Synopsis()
    {
        int idx = 1;
        foreach(string dir in Directory.GetDirectories(Synopsis_Path))
        {
            GameObject MetaData = Instantiate(Synopsis_MetaData);
            MetaData.GetComponent<Synopsis_MetaData>().Init_Synopsis(dir);
            MetaData.transform.SetParent(this.transform);
            ROOT_SynopsisMetaData.Add(idx++, MetaData);
        }
    }

    public Dictionary<int, GameObject> GetDatabase_Synopsis()
    {
        return new Dictionary<int, GameObject>(ROOT_SynopsisMetaData);
    }

    public GameObject GetObject_SynopsisMetadata(string name)
    {
        Transform target = this.transform.Find(name);
        if(target != null) return target.gameObject;
        else return null;
    }

    public Sprite Get_Sprite(string path, string name)
    {
        if(File.Exists(path))
        {
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = name;
            return sprite;
        }

        else
        {
            Debug.Log("DataManager - Get_Sprite : No File Error\n" + "PATH : " + path);
            return null;
        }
    }

    public IEnumerator Play_Sound(string name, AudioSource source)
    {
        string newPath = "file://" + Path.Combine(Application.dataPath.Replace("/Assets",""), Effect_Sound_Path) + "/" + name + ".wav";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(newPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                source.clip = DownloadHandlerAudioClip.GetContent(www);
                source.Play();
            }
        }
    }
    // Synopsis Parts End
}
