using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public enum Grid_Type
{
    Terrain,
    Sign,
    Unit,
    Custom,
    Special,
    Border
}

public enum Grid_State
{
    Unsearched,
    Searched,
    Checkable
}

public class GridManager : MonoBehaviour
{
    // Map Info Data
    [SerializeField] private string map_name;
    [SerializeField] private int map_width, map_height;
    
    // Top objects managed by grid type
    // 인스턴스로 생성된 그리드들을 종류별로 관리해주는 Dictionary
    private Dictionary<int, GameObject> Terrain_Groups;
    private Dictionary<int, GameObject> Sign_Groups;
    private Dictionary<int, GameObject> Border_Groups;

    // Noise map for generating arbitrary maps
    // 맵의 임의 생성을 위해 사용되는 노이즈맵, 원소는 그리드의 종류를 나타내주는 Idx(Grid_Database의 key)가 담겨있다.
    List<List<int>> PerlinNoiseMap = new List<List<int>>();

    // 2D GameObjects List where created grid instances are stored
    // 실제 생성된 Grid Instance가 저장되는 배열
    public List<List<GameObject>> GridMap = new List<List<GameObject>>();

    // PerlinNoise Parameters.
    // PerlinNoise를 통한 맵의 생성에 사용되는 인자
    [SerializeField] private float magnification = 8.0f;
    [SerializeField] private int x_offset = 0;
    [SerializeField] private int y_offset = 0;

    // 프로그램 실행시 초기화 목록
    void Awake()
    {
        map_width = 20;
        map_height = 20;
        SetAllGroups();
    }
    
    /* ---------------------------------------------- Key Functions ---------------------------------------------- */
    // Code placed in parent object to manage created instances.
    // 생성된 인스턴스들을 관리해주도록 상위 오브젝트에 담아주는 코드.
    // 초기 실행, 새로운 map 호출 상황에서 사용된다.
    void SetAllGroups()
    {
        SetGridGroups(Grid_Type.Terrain);
        SetGridGroups(Grid_Type.Sign);
        SetGridGroups(Grid_Type.Border);
    }

    // 생성된 Grid들을 종류별로 Empty GameObject에 담아주는 코드
    // 만약 새로운 맵을 호출한 경우 ClearGroup을 통해 기존 인스턴스들을 Clear해준다.
    void SetGridGroups(Grid_Type type)
    {
        switch(type)
        {
            case Grid_Type.Terrain:
                if(Terrain_Groups == null)
                {
                    // Grid를 종류별로 보관해주는 데이터베이스 생성
                    Terrain_Groups = new Dictionary<int, GameObject>();
                    foreach(KeyValuePair<int, GameObject> Grid_Pair in DataManager.Instance.GetDatabase_Grid(type))
                    {
                        GameObject Terrain_Group = new GameObject(Grid_Pair.Value.name);
                        Terrain_Group.transform.parent = gameObject.transform;
                        Terrain_Group.transform.localPosition = new Vector3(0,0,0);
                        Terrain_Groups.Add(Grid_Pair.Key, Terrain_Group);
                    }
                }

                else
                {
                    Dictionary<int, GameObject> TerrainDB = DataManager.Instance.GetDatabase_Grid(type);
                    foreach(KeyValuePair<int, GameObject> grid in TerrainDB)
                    {
                        ClearGroup(GameObject.Find(grid.Value.name));
                    }
                }
                break;
            
            case Grid_Type.Sign:
                if(Sign_Groups == null)
                {
                    // Grid를 종류별로 보관해주는 데이터베이스 생성
                    Sign_Groups = new Dictionary<int, GameObject>();
                    foreach(KeyValuePair<int, GameObject> Grid_Pair in DataManager.Instance.GetDatabase_Grid(type))
                    {
                        GameObject Sign_Group = new GameObject(Grid_Pair.Value.name);
                        Sign_Group.transform.parent = gameObject.transform;
                        Sign_Group.transform.localPosition = new Vector3(0,0,0);
                        Sign_Groups.Add(Grid_Pair.Key, Sign_Group);
                    }
                }

                else
                {
                    Dictionary<int, GameObject> SignDB = DataManager.Instance.GetDatabase_Grid(type);
                    foreach(KeyValuePair<int, GameObject> grid in SignDB)
                    {
                        ClearGroup(GameObject.Find(grid.Value.name));
                    }
                }

                break;

            case Grid_Type.Border:
                if(Border_Groups == null)
                {
                    // Grid를 종류별로 보관해주는 데이터베이스 생성
                    Border_Groups = new Dictionary<int, GameObject>();
                    foreach(KeyValuePair<int, GameObject> Grid_Pair in DataManager.Instance.GetDatabase_Grid(type))
                    {
                        GameObject Border_Group = new GameObject(Grid_Pair.Value.name);
                        Border_Group.transform.parent = gameObject.transform;
                        Border_Group.transform.localPosition = new Vector3(0,0,0);
                        Border_Groups.Add(Grid_Pair.Key, Border_Group);
                    }
                }

                else
                {
                    Dictionary<int, GameObject> BorderDB = DataManager.Instance.GetDatabase_Grid(type);
                    foreach(KeyValuePair<int, GameObject> grid in BorderDB)
                    {
                        ClearGroup(GameObject.Find(grid.Value.name));
                    }
                }
                break;

            default:
                break;
        }
    }

    // PerlinNoise를 통해 임의의 Map을 생성해준다.
    public void GenerateMap()
    {
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();
        camera.CameraMovementActivate = true;
        x_offset = Random.Range(-100, 100);
        y_offset = Random.Range(-100, 100);
        
        // 기존 Map이 있는 경우를 대비해 Child를 제거
        SetAllGroups();

        // 주어진 인자에 따라 새로운 Map을 만들어준다.
        GenerateGridMap();
        camera.MoveCameraToCenter();
    }

    // 새로운 PerlinNoiseMap과 GridMap을 생성하여 덮어씌워준다.
    // 기존 GridMap을 재사용하는 방식은 오류가 발생하여 덮어씌우는 방식으로 수정.
    void GenerateGridMap()
    {
        List<List<int>> New_PerlinNoiseMap = new List<List<int>>();
        List<List<GameObject>> New_GridMap = new List<List<GameObject>>();
        for (int x = 0; x < map_width; x++)
        {
            New_PerlinNoiseMap.Add(new List<int>());
            New_GridMap.Add(new List<GameObject>());

            for (int y = 0; y < map_height; y++)
            {
                int Grid_Id = GetIdUsingPerlin(x, y);
                New_PerlinNoiseMap[x].Add(Grid_Id);
                GameObject grid = CreateGrid(Grid_Type.Terrain, Grid_Id, x, y);
                New_GridMap[x].Add(grid);
            }
        }
        CreateBorder();
        PerlinNoiseMap = New_PerlinNoiseMap;
        GridMap = New_GridMap;
    }

    // 함수에 주어진 인자에 알맞는 한 개의 Grid Instance를 생성해준다.
    // 이때, Save와 Load에 사용되는 데이터가 Grid 스크립트를 기준으로 하므로 반드시 Grid의 값을 설정해주어야 한다.
    GameObject CreateGrid(Grid_Type type, int id, int x, int y)
    {
        GameObject PFB_Grid = null;
        GameObject Grid_Group = null;
        switch(type)
        {
            case Grid_Type.Terrain:
                PFB_Grid = DataManager.Instance.GetDatabase_Grid(Grid_Type.Terrain)[id];
                Grid_Group = Terrain_Groups[id];
                break;

            case Grid_Type.Sign:
                PFB_Grid = DataManager.Instance.GetDatabase_Grid(Grid_Type.Sign)[id];
                Grid_Group = Sign_Groups[id];
                break;

            case Grid_Type.Unit:
                break;

            default:
                Debug.Log("Error:Invalid Grid Type");
                break;
        }
        GameObject grid = Instantiate(PFB_Grid, Grid_Group.transform);

        // 각 Grid에 공통적으로 할당된 스크립트
        Grid Grid_Data = grid.GetComponent<Grid>();
        Grid_Data.grid_name = string.Format("tile_x{0}_y{1}", x, y);
        Grid_Data.grid_pos = new Vector3(x, y, 0);
        grid.name = Grid_Data.grid_name;
        grid.transform.localPosition = Grid_Data.grid_pos;
        
        return grid;
    }

    // Abandoned!
    // Player의 이동 논리 호환을 위해 작성된 코드. 논리 수정으로 인해 사용되지 않음.
    void CreateBorder()
    {
        GameObject Border_Group = Border_Groups[-1];
        for(int x = -1; x < map_width + 1; x++)
        {
            for(int y = -1; y < map_height + 1; y++)
            {
                if(x == -1 || x == map_width || y == -1 || y == map_height)
                {
                    GameObject border = DataManager.Instance.GetObject_Grid(Grid_Type.Border, "Border_Grid");
                    border.transform.parent = Border_Group.transform;
                    Grid Border_Data = border.GetComponent<Grid>();
                    Border_Data.grid_name = string.Format("border_x{0}_y{1}", x, y);
                    Border_Data.grid_idx = -1;
                    Border_Data.grid_pos = new Vector3(x, y, 0);
                    Border_Data.grid_movement_cost = 9999;
                    Border_Data.grid_vision_cost = 9999;

                    border.name = Border_Data.grid_name;
                    border.transform.localPosition = Border_Data.grid_pos;
                }
            }
        }
    }

    // 맵 편집 시 사용되는 코드.
    // 클릭한 위치에 있는 Grid를 없애고 현재 선택된 Grid로 변경시켜준다.
    public void ChangeGrid(int id, int Grid_Movement_Cost, int Grid_Vision_Cost, int x, int y, Grid_Type type)
    {
        if(type != Grid_Type.Border && x >= 0 && y >= 0 && x < map_width && y < map_height)
        {
            GameObject New_Grid = CreateGrid(type, id, x, y);
            Destroy(GridMap[x][y]);
            GridMap[x][y] = New_Grid;
        }
    }

    // 현재 GridMap에 저장된 데이터를 기준으로 .mapdata 파일을 만들어 저장한다.
    // 추후 Map Editor 기능이 생긴 이후에 발생할 수 있는 예외 처리는 이후에 작성될 예정.
    // .mapdata는 가시성을 위해 최대한 간편한 형식으로 작성됨.
    public bool SaveGridMap(string savemap_name)
    {
        if(GridMap.Count != 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("\tMapData\n");
            sb.Append("\t{\n");
            sb.Append("\t\tMapName:" + savemap_name + "\n");
            sb.Append("\t\tMapSize:" + map_width + "," + map_height + "\n");
            sb.Append("\t}\n");

            sb.Append("\tGridData\n");
            sb.Append("\t{\n");
            for (int i = 0; i < GridMap.Count; i++)
            {
                sb.Append("\t\t{\n");
                for (int j = 0; j < GridMap[i].Count; j++)
                {
                    sb.Append("\t\t\t{");

                    Grid Grid_Data = GridMap[i][j].GetComponent<Grid>();

                    sb.Append("\n\t\t\t\tTileName:" + Grid_Data.grid_name);
                    sb.Append("\n\t\t\t\tTileType:" + Grid_Data.grid_type);
                    sb.Append("\n\t\t\t\tTileIdx:" + Grid_Data.grid_idx);
                    sb.Append("\n\t\t\t\tTileLocation:(" + Grid_Data.grid_pos.x + "," + Grid_Data.grid_pos.y + "," + Grid_Data.grid_pos.z + ")");
                    sb.Append("\n\t\t\t\tTileMovementCost:" + Grid_Data.grid_movement_cost);
                    sb.Append("\n\t\t\t\tTileVisionCost:" + Grid_Data.grid_vision_cost);
                    sb.Append("\n\t\t\t}\n");
                }
                sb.Append("\t\t}\n");
            }
            sb.Append("\t}\n");
            sb.Append("}");
            string mapdata = sb.ToString();
            string path = DataManager.Instance.GetPath_Mapdata() + savemap_name + ".mapdata";
            File.WriteAllText(path, mapdata);
            return true;
        }
        else
        {
            Debug.Log("Error:Invalid Map Status");
            return false;
        }
    }

    // SaveGridMap을 통해 저장된 .mapdata파일을 읽어와서 GridMap을 만들어주고 덮어씌워주는 코드.
    // PerlinNoiseMap에 대한 정보는 없어서
    public void LoadGridMap(string loadmap_name)
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        CameraManager camera = GameObject.FindWithTag("IngameCamera").GetComponent<CameraManager>();

        // .mapdata 파일의 경로
        // 지금은 뼈대 구축이 목적이므로 임의로 작성됨. 추후 수정 예정.
        string filePath = DataManager.Instance.GetPath_Mapdata() + loadmap_name + ".mapdata";

        // 파일 존재 확인
        if (File.Exists(filePath))
        {
            // 기존 맵에 생성된 인스턴스를 제거하지 않고 부를 경우 타일이 중첩되어 보이지 않는 현상 발생
            // 따라서 해당 코드를 추가해 기존의 모든 인스턴스를 제거해주어야한다.
            SetAllGroups();
            // 파일의 내용을 string 형식으로 모드 읽어온다.
            string fileContents = File.ReadAllText(filePath);

            // \n으로 한 번 분리
            string[] lines = fileContents.Split('\n');

            // 해당 데이터에서 추출할 map data를 담을 인자.
            string mapName = "";
            int mapWidth = 0;
            int mapHeight = 0;

            // 해당 데이터를 추출해서 만들어질 GridMap
            List<List<GameObject>> LoadedGridMap = new List<List<GameObject>>();

            for (int i = 0; i < lines.Length; i++)
            {
                // 공백 제거
                string line = lines[i].Trim();

                // 맵에 대한 정보들 추출
                if (line.StartsWith("MapName:"))
                {
                    mapName = line.Substring("MapName:".Length);
                }
                else if (line.StartsWith("MapSize:"))
                {
                    string[] sizeValues = line.Substring("MapSize:".Length).Split(',');
                    mapWidth = int.Parse(sizeValues[0]);
                    mapHeight = int.Parse(sizeValues[1]);
                }

                // Grid에 대한 정보 추출
                else if (line == "GridData")
                {
                    // 저장을 위해 새로운 GameObject List를 각 width마다 추가
                    for (int x = 0; x < mapWidth; x++)
                    {
                        LoadedGridMap.Add(new List<GameObject>());
                    }

                    // 다음 줄로 이동("{","}" 넘어가는 용도), 이하 동문
                    i++;
                    for (int x = 0; x < mapWidth; x++)
                    {
                        i++;
                        for (int y = 0; y < mapHeight; y++)
                        {
                            i++;

                            // grid에 대한 정보를 담아줄 변수들
                            string grid_name = "";
                            Grid_Type grid_type = Grid_Type.Terrain;
                            int grid_idx = 0;
                            Vector3 grid_pos = Vector3.zero;
                            int grid_movement_cost = 0;
                            int grid_vision_cost = 0;

                            while (lines[i].Trim() != "}")
                            {
                                line = lines[i].Trim();
                                if (line.StartsWith("TileName:"))
                                {
                                    grid_name = line.Substring("TileName:".Length);
                                }
                                else if (line.StartsWith("TileType:"))
                                {
                                    grid_type = (Grid_Type)System.Enum.Parse(typeof(Grid_Type), line.Substring("TileType:".Length));
                                }
                                else if (line.StartsWith("TileLocation:"))
                                {
                                    string[] locationValues = line.Substring("TileLocation:(".Length, line.Length - "TileLocation:(".Length - 1).Split(',');
                                    grid_pos.x = float.Parse(locationValues[0]);
                                    grid_pos.y = float.Parse(locationValues[1]);
                                    grid_pos.z = float.Parse(locationValues[2]);
                                }
                                else if (line.StartsWith("TileIdx:"))
                                {
                                    grid_idx = int.Parse(line.Substring("TileIdx:".Length));
                                }
                                else if (line.StartsWith("TileMovementCost:"))
                                {
                                    grid_movement_cost = int.Parse(line.Substring("TileMovementCost:".Length));
                                }
                                else if (line.StartsWith("TileVisionCost:"))
                                {
                                    grid_vision_cost = int.Parse(line.Substring("TileVisionCost:".Length));
                                }
                                i++;
                            }

                            // 하나의 grid에 대한 정보를 모두 추출해주었으므로 해당 grid를 생성 및 저장.
                            GameObject grid = CreateGrid(grid_type, grid_idx, x, y);

                            if(game != null && game.isPlaying == true)
                            {
                                grid.GetComponent<Grid>().GridState_Handler(Grid_State.Unsearched);
                            }

                            LoadedGridMap[x].Add(grid);
                            i++;
                        }
                        i++;
                    }
                }
            }

            // .mapdata에서 추출해낸 모든 정보를 GridManager로 덮어씌움.
            map_name = mapName;
            map_width = mapWidth;
            map_height = mapHeight;
            GridMap = LoadedGridMap;
            CreateBorder();
            if(LoadedGridMap[0][0] == null) Debug.Log("not exitst");
            camera.MoveCameraToCenter();
        }
    }

    // 주어진 2차원 좌표를 토대로 perlin noise algorithm으로 특정 grid_idx를 결정해주는 코드
    int GetIdUsingPerlin(int x, int y)
    {
        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
        );

        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * DataManager.Instance.GetDatabase_Grid(Grid_Type.Terrain).Count;

        if(scaled_perlin == 4)
        {
            scaled_perlin = 3;
        }
        return Mathf.FloorToInt(scaled_perlin);
    }    

    // Grid를 종류별로 저장해주는 오브젝트의 Child를 모두 제거해주는 함수
    // 제거해주지 않으면 Grid가 중복되는 현상 발생
    void ClearGroup(GameObject Grid_Group)
    {
        for (int i = Grid_Group.transform.childCount - 1; i >= 0; i--)
        {
            // 해당 Object의 child를 가져옴
            Transform child = Grid_Group.transform.GetChild(i);

            // 해당 Child를 삭제
            Destroy(child.gameObject);
        }
    }

    /* ---------------------------------------------- Helper Functions ---------------------------------------------- */
    public int getWidth()
    {
        return map_width;
    }

    public int getHeight()
    {
        return map_height;
    }

    /* ---------------------------------------------- Unit Functions ---------------------------------------------- */

    // Player 이동 영역 확인 논리에 사용되는 코드.
    // 해당 죄표에 Grid가 있는지 확인하여 해당 Grid 또는 null을 반환.
    public Grid GridValidity(int x, int y)
    {
        //Debug.Log("GridValidity " + x + " " + y);
        if(x > -1 && x < map_width && y > -1 && y < map_height)
        {
            return GridMap[x][y].GetComponent<Grid>();
        }
        else 
        {
            //Debug.Log("Rejected");
            return null;
        }
    }
    

}
