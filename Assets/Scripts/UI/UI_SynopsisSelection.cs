using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SynopsisSelection : MonoBehaviour
{
    [SerializeField] private GameObject Player_Image, Player_Name, Player_Description, Enemy_Image, Enemy_Name, Enemy_Description, Synopsis_Image, Synopsis_Name, Synopsis_Description, Synopsis_BGM_Name;
    private List<Synopsis_MetaData> Metadata;
    [SerializeField] private int Current_Idx;

    // DataManager로부터 모든 Metadata를 읽어와 UI에 표시하기 위한 정보를 수집함
    public void Set_Metadata()
    {
        GameObject DataManager = GameObject.Find("DataManager");
        Metadata = new List<Synopsis_MetaData>();
        foreach(Transform child in DataManager.transform)
        {
            Metadata.Add(child.gameObject.GetComponent<Synopsis_MetaData>());
        }
        Current_Idx = 0;
        Set_UI();
    }

    public void Prev_Synopsis()
    {
        this.Current_Idx--;
        if(this.Current_Idx < 0) this.Current_Idx = Metadata.Count - 1;
        Set_UI();
    }

    public void Next_Synopsis()
    {
        this.Current_Idx++;
        if(this.Current_Idx >= Metadata.Count) Current_Idx = 0;
        Set_UI();
    }

    public void Set_UI()
    {
        Synopsis_MetaData meta = this.Metadata[Current_Idx];
        string[] synopsis_metadata_string = meta.Get_SynopsisStringInfo();
        Sprite[] synopsis_metadata_sprite = meta.Get_SynopsisSpriteInfo();

        Synopsis_Image.GetComponent<Image>().sprite = synopsis_metadata_sprite[0];
        Player_Image.GetComponent<Image>().sprite = synopsis_metadata_sprite[1];
        Enemy_Image.GetComponent<Image>().sprite = synopsis_metadata_sprite[2];

        Synopsis_Name.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[0];
        Synopsis_Description.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[1];
        Synopsis_BGM_Name.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[2];
        Player_Name.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[3];
        Player_Description.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[4];
        Enemy_Name.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[5];
        Enemy_Description.GetComponent<TextMeshProUGUI>().text = synopsis_metadata_string[6];

        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        game.Set_CurrentMetadata(Metadata[Current_Idx]);
    }

    public Synopsis_MetaData Get_CurrentMetadata()
    {
        return Metadata[Current_Idx];
    }
}
