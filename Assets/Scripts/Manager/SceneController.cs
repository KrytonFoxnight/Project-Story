using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // Custom Loading Canvas Object & Slider for display loading steps.
    [SerializeField] private GameObject LoaderCanvas; // 로딩 화면을 가진 캔버스
    [SerializeField] private Slider ProgressBar; // 로딩 진행률을 표현해주는 슬라이더 컴포넌트

    // Scene이 로드될 때마다 초기 함수 처리를 위해 작성
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 해당 함수를 통해 특정 Scene이 로드될 때마다 해주어야 하는 초기 처리문을 실행할 수 있음.
    // 해당 Scene이 완전히 로드되어야만 실행되므로 객체 접근에 안전성이 있음.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject synopsis = GameObject.Find("SynopsisManager");
        GameObject UI_Scene = GameObject.Find("UI");
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        GameState state = game.currentState;

        switch(state)
        {
            case GameState.SelectSynopsis:
                UI_Scene.GetComponent<UI_SynopsisSelection>().Set_Metadata();
                break;

            case GameState.SelectStage:
                // Setting Synopsis Data
                Synopsis_MetaData meta = game.Get_CurrentMetadata();
                string path = meta.Get_Path("Dialogue");
                StartCoroutine(synopsis.GetComponent<SynopsisManager>().Synopsis_Parser(path));

                UI_Scene.GetComponent<UI_StageSelection>().Stage_Generation();

                // Intro랑 Prologue를 실행시킨다.
                break;

            case GameState.DialogueHead:
                GameObject UI_Synopsis = synopsis.transform.Find("Synopsis UI").gameObject;
                UI_Synopsis.SetActive(true);
                break;
        }

        LoaderCanvas.SetActive(false);
    }

    // Function that uses async function to load corresponding Scene synchronously.
    // 동기화 기능을 사용해 Scene Loading을 동기적으로 수행.
    public async void SceneControllerLoadAsync(string SceneName)
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        GameObject Prev_UI = GameObject.Find("UI");
        Prev_UI.SetActive(false);

        var scene = SceneManager.LoadSceneAsync(SceneName);
        scene.allowSceneActivation = false;
        LoaderCanvas.SetActive(true);
        do
        {
            await Task.Delay(100);
            ProgressBar.value = scene.progress;
        }
        while(scene.progress < 0.9f);
        await Task.Delay(1000);
        
        scene.allowSceneActivation = true;

        GameState state = DataManager.Instance.GetComponent<DataManager>().Get_SceneState(SceneName);
        game.ChangeState(state);
    }
}
