using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class Synopsis_MetaData : MonoBehaviour
{
    [SerializeField] private uint Synopsis_NumStages, Synopsis_OpenedStages;
    [SerializeField] private string Path_SynopsisRepo, Path_SynopsisDialogue, Path_SynopsisMapdata, Path_SynopsisSound, Path_SynopsisUnits, Path_SynopsisMetaData, Path_SynopsisIllustImage, Path_SynopsisCharImage, Path_SynopsisSystemImage;
    [SerializeField] private string Synopsis_Name, Synopsis_Description, Synopsis_Player_Name, Synopsis_Player_Description, Synopsis_Enemy_Name, Synopsis_Enemy_Description, Synopsis_BGM_Name;
    [SerializeField] private AudioClip Synopsis_BGM;
    [SerializeField] private Sprite Synopsis_Cover, Synopsis_Player, Synopsis_Enemy;

    public void Init_Synopsis(string Path)
    {
        Path_SynopsisRepo = Path;
        Path_SynopsisDialogue = Path + "/Dialogue";
        Path_SynopsisMapdata = Path + "/Mapdata";
        Path_SynopsisSound = Path + "/Sound";
        Path_SynopsisUnits = Path + "/Unit";
        Path_SynopsisIllustImage = Path + "/Image/Illustration";
        Path_SynopsisCharImage = Path + "/Image/Character";
        Path_SynopsisSystemImage = Path + "/Image/System";
        Path_SynopsisMetaData = Path + "/Meta.csv";

        if(File.Exists(Path_SynopsisMetaData))
        {
            using (var reader = new StreamReader(Path_SynopsisMetaData))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    switch(values[0])
                    {
                        case "Title":
                            Synopsis_Name = values[1];
                            Synopsis_Description = CSV_String_Corrector(values[2]);
                            string Path_SynopsisLogo = Path_SynopsisIllustImage + "/" + values[3];
                            if (File.Exists(Path_SynopsisLogo)) this.Synopsis_Cover = DataManager.Instance.Get_Sprite(Path_SynopsisLogo, "Synopsis_Logo");
                            break;

                        case "Player":
                            Synopsis_Player_Name = values[1];
                            Synopsis_Player_Description = CSV_String_Corrector(values[2]);
                            string Path_PlayerImage = Path_SynopsisCharImage + "/" + values[3];
                            if (File.Exists(Path_PlayerImage)) this.Synopsis_Player = DataManager.Instance.Get_Sprite(Path_PlayerImage, "Synopsis_Player");
                            break;

                        case "Enemy":
                            Synopsis_Enemy_Name = values[1];
                            Synopsis_Enemy_Description = CSV_String_Corrector(values[2]);
                            string Path_EnemyImage = Path_SynopsisCharImage + "/" + values[3];
                            if (File.Exists(Path_EnemyImage)) this.Synopsis_Enemy = DataManager.Instance.Get_Sprite(Path_EnemyImage, "Synopsis_Enemy");
                            break;

                        case "BGM":
                            Synopsis_BGM_Name = values[1];
                            string Path_SynopsisBGM = Path_SynopsisSound + "/" + values[3];
                            if (File.Exists(Path_SynopsisBGM)) StartCoroutine(SetAudio(Path_SynopsisBGM, "Synopsis_BGM"));
                            break;

                        case "#Stages":
                            Synopsis_OpenedStages = uint.Parse(values[1]);
                            Synopsis_NumStages = uint.Parse(values[2]);
                            break;
                        default:
                            break;
                    }
                }
            }
            this.name = Synopsis_Name;
        }

        else this.name = "NO_METAFILE";
    }

    string CSV_String_Corrector(string str)
    {
        string corrected = str.Replace("\"\"", "\"");
        if(str.Length >= 2 && corrected[0] == '"' && corrected[corrected.Length-1] == '"') return corrected.Substring(1, corrected.Length - 2);
        else return corrected;
    }

    public string[] Get_SynopsisStringInfo()
    {
        string[] info = new string[]
        {
            Synopsis_Name,
            Synopsis_Description,
            Synopsis_BGM_Name,
            Synopsis_Player_Name,
            Synopsis_Player_Description,
            Synopsis_Enemy_Name,
            Synopsis_Enemy_Description
        };
        return info;
    }

    public Sprite[] Get_SynopsisSpriteInfo()
    {
        Sprite[] info = new Sprite[]
        {
            Synopsis_Cover,
            Synopsis_Player,
            Synopsis_Enemy
        };

        return info;
    }

    public AudioClip Get_BGM()      { return this.Synopsis_BGM; }
    public uint Get_NumStages()     { return this.Synopsis_NumStages; }
    public uint Get_OpenedStages()  { return this.Synopsis_OpenedStages; }

    IEnumerator SetAudio(string path, string name)
    {
        string newPath = "file://" + Path.Combine(Application.dataPath.Replace("/Assets",""), path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(newPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                this.Synopsis_BGM = DownloadHandlerAudioClip.GetContent(www);
                this.Synopsis_BGM.name = Synopsis_BGM_Name;
            }
        }
    }

    public string Get_Path(string path)
    {
        string target = "";
        switch(path)
        {
            case "Dialogue":
                target = this.Path_SynopsisDialogue;
                break;

            case "Unit":
                target = this.Path_SynopsisUnits;
                break;

            case "Illustration":
                target = this.Path_SynopsisIllustImage;
                break;

            case "Character":
                target = this.Path_SynopsisCharImage;
                break;

            case "System":
                target = this.Path_SynopsisSystemImage;
                    break;

            default:
                Debug.Log("Invalid call at Metadata.Get_Path with " + path);
                break;
        }
        return target;
    }
}
