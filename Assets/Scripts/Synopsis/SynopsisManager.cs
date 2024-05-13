/* Pre-Processor Define Area */
// #define DEBUG
// #define PARSE_TRACE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public enum ScenarioType
{
    Prologue,
    Intro,
    Stage_Head,
    Stage_Tail,
    Epilogue,
    NotInitialized
}

public class SynopsisManager : MonoBehaviour
{
    private Synopsis Current_Synopsis;
    private Scenario Current_Scenario;
    private Dialogue Current_Dialogue;
    private GameObject Current_Character;

    private bool printing = false;
    private bool Lock, First;

    [SerializeField] private List<GameObject> Character_Objects;
    [SerializeField] private GameObject Character_SD, Character_LD;
    [SerializeField] private GameObject Positions;
    [SerializeField] private GameObject Background_Object, Fade;
    [SerializeField] private TextMeshProUGUI nameBox, talkBox;
    [SerializeField] private GameObject Click_Panel;
    [SerializeField] private GameObject EventManager;
    [SerializeField] private GameObject SoundManager;
    [SerializeField] private GameObject Tag_Object;

    public void Call_Scenario(ScenarioType type, uint stage)
    {
        Set_CurrentScenario(type, stage);
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();
        EventManager.GetComponent<DialogueEventManager>().Deactivate_Click_Icon();
        this.First = true;
    }

    // for Debug Button
    /*
    public void Call_Scenario_Button()
    {
        Set_CurrentScenario(ScenarioType.Intro, 0);
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();
        EventManager.GetComponent<DialogueEventManager>().Deactivate_Click_Icon();
        this.First = true;
        Debug.Log("Scenario Called!");
    }
    */

    public void Rollback_Button()
    {
        if(this.Current_Scenario.Is_Over())
        {
            foreach(GameObject obj in Character_Objects)
            {
                Destroy(obj);
            }
            this.Character_Objects.Clear();
            this.Current_Character = null;
            this.Background_Object.GetComponent<Image>().sprite = null;
            this.Fade.GetComponent<Image>().color = new Color(0,0,0,0);

            nameBox.text = "";
            talkBox.text = "";

            this.Current_Scenario.Rollback();
            this.Click_Panel.GetComponent<Click_Panel>().Click_Enable();
            this.Lock = false;
            this.First = true;
            ClickPanel_Handler();
        }
    }

    public void Rollback()
    {
        foreach(GameObject obj in Character_Objects)
        {
            Destroy(obj);
        }
        this.Character_Objects.Clear();
        this.Background_Object.GetComponent<Image>().sprite = null;
        this.Fade.GetComponent<Image>().color = new Color(0,0,0,0);
        nameBox.text = "";
        talkBox.text = "";
        this.Lock = false;
        this.First = true;
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();

        this.Current_Scenario.Rollback();
    }

    // Will be Abandoned
    /*
    public void Set_CurrentSynopsis_Debug(string name)
    {
        if(this.Current_Metadata != null) Destroy(this.Current_Metadata);
        this.Current_Metadata = DataManager.Instance.GetObject_SynopsisMetadata(name);
        string path = DataManager.Instance.transform.Find(name).GetComponent<Synopsis_MetaData>().Get_Path("Dialogue");
        StartCoroutine(Synopsis_Parser(path));
    }
    */

    public IEnumerator Synopsis_Parser(string path)
    {
        GameManager game = GameObject.Find("GameManager").GetComponent<GameManager>();
        Synopsis_MetaData meta = game.Get_CurrentMetadata();
        Scenario[] newScenarios = new Scenario[2 * meta.Get_NumStages() + 3];

        string[] CSV_Files = Directory.GetFiles(path, "*.csv");

        foreach(string csv_path in CSV_Files)
        {
            /* Build A Synopsis */
            if(File.Exists(csv_path))
            {
                using (var reader = new StreamReader(csv_path))
                {
                    string[] values;
                    while(!reader.EndOfStream)
                    {
                        Read_Line(reader, out values);
                        switch(values[0])
                        {
                            // 해당 Dialogue의 대사 출력 전에 사전에 설정해주고 싶은 요소들을 명시적으로 처리해주는 명령어
                            // 배경음, 배경화면 등
                            case "SET_PROPERTY":
                                Set_Handler(values[1], values[2]);
                                break;

                            // Dialogue를 구성하는 대사 정보들을 한 장면 단위로 구성하는 정보들의 시작을 지시
                            case "DIALOGUE_DATA_START":
                                uint idx;
                                Scenario newScenario = Dialogue_Handler(reader, csv_path, out idx);
                                newScenarios[idx] = newScenario;
                                break;

                            // 주석일 경우 & 목자 지시표 예외 처리
                            case "COMMENT":
                            case "Command":
                                break;

                            default:
                                #if DEBUG
                                    Debug.Log("Synopsis_Parse default reached at " + values[0]);
                                #endif
                                break;
                        }
                    }
                }
            }
        }

        /* Test Integrity */
        #if DEBUG
            Debug.Log("Parse Complete.");
        #endif
        Synopsis newSynopsis = ScriptableObject.CreateInstance<Synopsis>();
        newSynopsis.Init_Synopsis(0, newScenarios);
        this.Current_Synopsis = newSynopsis;
        yield return null;
    }

    // Dialogue 정보를 처리해주어 오브젝트로 만들어 관리해주도록 구성
    // 한 파일 당 한 번씩만 실행됨
    Scenario Dialogue_Handler(StreamReader reader, string path, out uint idx)
    {
        GameManager game = GameObject.Find("GameManager").GetComponent<GameManager>();
        Synopsis_MetaData meta = game.Get_CurrentMetadata();

        idx = 0;
        string[] values;
        ScenarioType type = ScenarioType.NotInitialized;
        Sprite sprite = null;

        Scenario newScenario = ScriptableObject.CreateInstance<Scenario>();
        List<Dialogue> newDialogues = new List<Dialogue>();

        string file_name = Path.GetFileNameWithoutExtension(path);
        
        // csv 파일 이름을 가지고 해당 Dialogue가 어떤 종류인지 처리해주는 구문
        // 파일 이름 형식
        // Prologue : 게임이 처음 시작될 때 앞으로의 이야기를 설명해주는 장면 구성
        // Intro : 스테이지 선택창에서 나오는 간략한 서사
        // Epilogue : 보스 스테이지 종료 후 나오는 에필로그
        // Stage_#Stage_(Head or Tail) : 해당 스테이지를 시작할 때 진행 전에 출력되는, 또는 스테이지 종료 후에 출력할 대사 정보
        switch(file_name)
        {
            // 사전 예약어에 따른 고유 Dialogue의 설정
            case "Prologue":
                idx = (uint)(meta.Get_NumStages() * 2 + 1);
                type = ScenarioType.Prologue;
                break;

            case "Intro":
                idx = 0;
                type = ScenarioType.Intro;
                break;

            case "Epilogue":
                idx = (uint)(meta.Get_NumStages() * 2 + 2);
                type = ScenarioType.Epilogue;
                break;

            // 기본 Stage를 위핸 Dialogue 구성
            default:
                values = file_name.Split('_');
                if(values[0] == "Stage")
                {
                    if(values[2] == "Head")         { type = ScenarioType.Stage_Head; idx = uint.Parse(values[1]) * 2 - 1; }
                    else if(values[2] == "Tail")    { type = ScenarioType.Stage_Tail; idx = uint.Parse(values[1]) * 2; }
                }
                break;
        }
        
        // 현재 열린 csv 파일의 파싱 시작
        do
        {
            Read_Line(reader, out values);
            switch(values[0])
            {
                // Dialgue 대사 정보 입력 종료
                case "DIALOGUE_DATA_END":
                    break;

                // 해당 Dialogue 진행 시 필요한 별도의 효과 적용
                case "EVENT":
                    Event_Handler(newDialogues[newDialogues.Count - 1], values);
                    break;

                // 대사 정보 처리
                default:
                    Dialogue newDialogue = ScriptableObject.CreateInstance<Dialogue>();
                    string[] path_buf;
                    string spritePath = null;

                    if(values[1] != "DUMMY_DIALOGUE" && values[2] != "")
                    {
                        path_buf = values[2].Split('/');
                        spritePath = meta.Get_Path(path_buf[0]) + "/" + path_buf[1];
                        sprite = DataManager.Instance.Get_Sprite(spritePath, values[2]);
                    }
                    
                    switch(type)
                    {
                        case ScenarioType.Prologue:
                        case ScenarioType.Epilogue:
                        case ScenarioType.Stage_Head:
                        case ScenarioType.Stage_Tail:
                        case ScenarioType.Intro:
                            newDialogue.Init_Dialogue(values[1], sprite, values[3], values[4]);
                            break;

                        default:
                            #if DEBUG
                                Debug.Log("Dialogue Handler: Invalid Type Detected");
                            #endif
                            break;
                    }
                    newDialogues.Add(newDialogue);
                    break;
            }


        } while(!reader.EndOfStream && values[0] != "DIALOGUE_DATA_END");

        if(type != ScenarioType.NotInitialized) newScenario.Init_Scenario(newDialogues.ToArray(), type);
        #if DEBUG
            else Debug.Log("Dialogue_Handler: Setting Scenario Failed! type is NotInitialized!");
        #endif

        return newScenario;
    }

    void Set_Handler(string set_flag, string path)
    {
        GameManager game = GameObject.Find("GameManager").GetComponent<GameManager>();
        Synopsis_MetaData meta = game.Get_CurrentMetadata();
        
        switch(set_flag)
        {
            case "MUSIC":
                break;

            case "BACKGROUND":
                string[] path_buf = path.Split('/');
                string spritePath = meta.Get_Path(path_buf[0]) + "/" + path_buf[1];
                Sprite sprite = DataManager.Instance.Get_Sprite(spritePath, "Background");
                this.Background_Object.GetComponent<Image>().sprite = sprite;
                break;

            default:
                Debug.Log("Set_Handler: Invalid Set Flag! - " + set_flag + " | " + path);
                break;
        }
    }

    void Event_Handler(Dialogue dialogue, string[] values)
    {
        switch(values[1])
        {
            case "ANIMATION":
                dialogue.Set_Animation(values[2], values[3], values[4]);
                break;

            case "SOUND":
                dialogue.Set_Sound(values[2], values[4]);
                break;

            case "EFFECT":
                dialogue.Set_Effect(values[2], values[3], values[4]);
                break;

            default:
                Debug.Log("Event_Handler Error : Invalid EVENT Flag");
                break;
        }
    }

    void Read_Line(StreamReader reader, out string[] values)
    {
        var line = reader.ReadLine();
        values = line.Split(',');

        #if PARSE_TRACE
            if(values.Length == 3) Debug.Log("Current Line parameter 3 : " + values[0] + " | " + values[1] + " | " + values[2]);
            else if(values.Length == 4) Debug.Log("Current Line parameter 4: " + values[0] + " | " + values[1] + " | " + values[2] + "|" + values[3]);
        #endif
    }

    public void Set_CurrentScenario(ScenarioType type, uint stage_num)
    {
        switch(type)
        {
            case ScenarioType.Prologue:
            case ScenarioType.Intro:
            case ScenarioType.Epilogue:
                this.Current_Scenario = this.Current_Synopsis.Get_Scenario(type, 0);
                break;

            case ScenarioType.Stage_Head:
            case ScenarioType.Stage_Tail:
                this.Current_Scenario = this.Current_Synopsis.Get_Scenario(type, stage_num);
                break;

            default:
                #if DEBUG
                    Debug.Log("Set_CurrentScenario: Invalid Type Input");
                #endif
                break;
        }

        if(this.Current_Scenario != null)
        {
            Rollback();
        }
    }

    public void ClickPanel_Handler()
    {
        // Tag Animation Part
        if(First)
        {
            StartCoroutine(EventManager.GetComponent<DialogueEventManager>().Default_Effect_Tag(Tag_Object));
            StartCoroutine(EventManager.GetComponent<DialogueEventManager>().Default_Click_Icon());
            First = false;
        }

        // Talking Printing Handling Conditions
        if(!this.printing)
        {
            this.Current_Dialogue = Current_Scenario.Next_Dialogue();

            if(this.Current_Dialogue != null)
            {
                string char_name = "NOT_INITIALIZED";
                string talking = "NOT_INITIALIZED";
                string position = "NOT_INITIALIZED";
                Sprite sprite = null;
                
                // Extract Dialogue Data
                this.Current_Dialogue.Set_Value(out char_name, out talking, out sprite, out position);

                #if DEBUG
                    //Debug.Log("\n" + "Max CNT " + this.Current_Scenario.Get_Max_Dialogue() + "\n" + "Current Dialogue ID " + this.Current_Scenario.Get_Current_Dialogue() + "\n" + char_name + "\n" + talking + "\n" + position);
                #endif

                GameObject target = null;

                // Background Check
                if(position == "BACKGROUND")
                {
                    this.Background_Object.GetComponent<Image>().sprite = sprite;
                }

                // 캐릭터 이름이 없으나 현재 시나리오에 적어도 하나의 오브젝트가 존재하는 경우
                else if(char_name == "" && Current_Character != null)
                {
                    target = this.Current_Character;
                }

                // 캐릭터 오브젝트가 없는 경우 새로 만들어주어 target에 할당
                else
                {
                    // 현재 동일한 오브젝트가 있는지 먼저 확인, 기준은 캐릭터의 이름이 같은 경우 같은 오브젝트로 처리
                    target = this.Character_Objects.Find(obj => obj.name == char_name);
                    if(target == null && char_name != "" && char_name != "DUMMY_DIALOGUE" && sprite != null)
                    {
                        // 이미지가 SD인지 LD인지 판정해주는 기준, 첫 포지션 입력 시 결정되며 변경되지 않음.
                        string character_scale = position.Split("_")[1];

                        // 새로 생성되는 Character Object의 값들을 설정해줍니다.
                        // LD로 명시되어 있는 경우에 LD용 프리팹을 인스턴스로 생성해주고, 그 이외의 경우는 모두 SD로 취급하여 생성됨.
                        if(character_scale == "LD") target = Instantiate(this.Character_LD);
                        else target = Instantiate(this.Character_SD);

                        target.name = char_name;
                        target.GetComponent<Image>().sprite = sprite;

                        // Character Object 크기를 재설정해줍니다.
                        target.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);

                        // 생성된 캐릭터 오브젝트를 입력받은 위치에 해당하는 오브젝트에 상속시켜줍니다.
                        // 만약 입력값이 없는 경우 중앙에 배치하도록 강제합니다.
                        if(position != "") EventManager.GetComponent<DialogueEventManager>().Set_Position(target, Positions.transform.Find(position));
                        else EventManager.GetComponent<DialogueEventManager>().Set_Position(target, Positions.transform.Find("4"));

                        // 생성된 Object를 추적하기 위해 배열에 추가해줍니다.
                        this.Character_Objects.Add(target);
                    }
                }

                // Dummy Dialogue Part
                if(char_name == "DUMMY_DIALOGUE")
                {
                    string[] effect = Current_Dialogue.Get_Effect();
                    if(effect != null) this.EventManager.GetComponent<DialogueEventManager>().Play_Effect(null, effect);
                }

                // Character Object Processor part
                if(target != null)
                {
                    // 새로 생성된 경우를 위해 항상 초기화시켜줌
                    this.Current_Character = target;
                    Transform parent = target.transform.parent;
                    string[] effect = Current_Dialogue.Get_Effect();
                    string[] sound = Current_Dialogue.Get_Sound();
                    string[] animation = Current_Dialogue.Get_Animation();

                    // 효과 적용 우선순위
                    // 1. 대사 여부에 따른 캐릭터 명조 변경
                    // 2. 실행할 이펙트가 존재하는 경우 - 명시된 Effect 표시
                    // 3. 캐릭터의 위치가 바뀌는 경우 - 기본 이동 애니메이션 실행
                    // 4. 파일에 명시된 애니메이션을 현재 target에 적용

                    // Character Moving Animation Part
                    // 현재 이야기를 하고 있는 캐릭터의 색상을 원래대로 돌려두고 나머지 오브젝트는 어둡게 처리해줍니다.
                    EventManager.GetComponent<DialogueEventManager>().Focus_Handler(Current_Character, Character_Objects);

                    // Character Effect Part
                    // Dialogue 파싱에 설정된 Effect를 적용해줍니다.
                    if(effect != null)
                    {
                        this.EventManager.GetComponent<DialogueEventManager>().Play_Effect(target, effect);
                    }

                    if(sound != null)
                    {
                        this.EventManager.GetComponent<DialogueEventManager>().Play_Effect_Sound(sound);
                    }

                    // Character Position Part
                    // 이미 존재하는 캐릭터 오브젝트가 다른 포지션으로 이동해야할 경우 기본 이동 애니메이션을 실행합니다.
                    if(position != "" && parent != null && parent.name != position)
                    {
                        if(position == "OUT") EventManager.GetComponent<DialogueEventManager>().Set_Position(target, Positions.transform.Find("OUT"));
                        else StartCoroutine(EventManager.GetComponent<DialogueEventManager>().Default_Effect_Movement(target.transform, Positions.transform.Find(position)));
                    }

                    // Character Object Animation Part
                    // Dialogue 파싱에 설정된 애니메이션을 실행합니다.
                    else if(animation != null)
                    {
                        this.EventManager.GetComponent<DialogueEventManager>().Play_Animation(target, animation);
                    }
                }

                // Character Sound Part

                // Character Name Part
                if(char_name == "DUMMY_DIALOGUE") this.nameBox.text = "";
                else if(char_name != "") this.nameBox.text = char_name;
                    
                // Character Talking Part
                if(char_name == "DUMMY_DIALOGUE") this.talkBox.text = "";
                else if(talking != "" && !this.Current_Scenario.Is_Over()) StartCoroutine(Talking_Print(talking));
            }
        }

        // Talk Printing Auto Finish
        else
        {
            this.printing = false;
            this.EventManager.GetComponent<DialogueEventManager>().Finish();
            this.talkBox.text = this.Current_Dialogue.Get_Text();
        }

        // Scenario Over Handling Part
        if(this.Current_Scenario.Is_Over())
        {
            this.printing = false;
            this.Click_Panel.GetComponent<Click_Panel>().Click_Disable();
            this.EventManager.GetComponent<DialogueEventManager>().Deactivate_Click_Icon();

            foreach(GameObject obj in Character_Objects)
            {
                Destroy(obj); 
            }
            this.Character_Objects.Clear();
            
            this.Current_Character = null;
            this.Background_Object.GetComponent<Image>().sprite = null;
            nameBox.text = "";
            talkBox.text = "";
            this.Lock = true;
        }
    }

    // Talking Animation IEnumerator
    IEnumerator Talking_Print(string text)
    {
        this.printing = true;
        this.talkBox.text = "";

        foreach(char chr in text)
        {
            this.talkBox.text += chr;
            if(!printing)
            {
                this.talkBox.text = "";
                this.talkBox.text = text;
                yield return null;
            }
            else yield return new WaitForSeconds(0.05f);
        }

        this.printing = false;
        yield return null;
    }

    public bool Is_Over()
    {
        return this.Lock;
    }
}