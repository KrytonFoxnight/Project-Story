using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_StageSelection : MonoBehaviour
{
    [SerializeField] GameObject Stage_Content, Stage_Button;

    public void Stage_Generation()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        DataManager data = GameObject.Find("DataManager").GetComponent<DataManager>();
        SynopsisManager synopsis = GameObject.Find("SynopsisManager").GetComponent<SynopsisManager>();
        ButtonManager button = GameObject.Find("Buttons").GetComponent<ButtonManager>();
        Synopsis_MetaData meta = game.Get_CurrentMetadata();

        uint NumStages = meta.Get_NumStages();
        uint OpenedStages = meta.Get_OpenedStages();

        // Clear Stage_Content's Child Obj
        foreach(Transform child in Stage_Content.transform)
        {
            Destroy(child.gameObject);
        }

        // Button Generation
        for(uint i = 1; i <= NumStages; ++i)
        {
            // Resources
            GameObject newButton = Instantiate(Stage_Button);
            GameObject Button_Image = newButton.transform.Find("Image").gameObject;
            GameObject Button_Locked = newButton.transform.Find("Locked").gameObject;
            GameObject Button_Text = newButton.transform.Find("Text").gameObject;
            Sprite stage_image = data.Get_Sprite(meta.Get_Path("System") + "/Stage_Image.png", "Stage_Image_" + i.ToString());
            Sprite stage_locked = data.Get_Sprite(meta.Get_Path("System") + "/Stage_Locked.png", "Stage_Locked_" + i.ToString());

            // Setting
            newButton.name = "Button-Stage " + i.ToString();
            if(stage_image != null) Button_Image.GetComponent<Image>().sprite = stage_image;
            if(stage_locked != null) Button_Locked.GetComponent<Image>().sprite = stage_locked;
            Button_Text.GetComponent<TextMeshProUGUI>().text = "Stage " + i.ToString();
            
            if(i <= OpenedStages)
            {
                newButton.GetComponent<Button>().interactable = true;
                Button_Locked.SetActive(false);
            }
            else
            {
                newButton.GetComponent<Button>().interactable = false;
                Button_Locked.SetActive(true);
            }

            // closure로 인한 duplication 문제를 해소시키기 위해 복사하여 설정 
            uint copied = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => {
                button.Button_LoadSceneAsync("Ingame_Scene");
                synopsis.Call_Scenario(ScenarioType.Stage_Head, copied);
            });

            // Transform
            newButton.transform.SetParent(Stage_Content.transform);
        }
    }
}
