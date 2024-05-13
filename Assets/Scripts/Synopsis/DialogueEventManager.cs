using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueEventManager : MonoBehaviour
{
    [SerializeField] private AudioClip current_clip;
    [SerializeField] private GameObject Click_Panel, Click_Icon, Fade;
    [SerializeField] private GameObject Background_Sound, Effect_Sound;
    private static float Default_Time = 0.2f, Movement_Speed = 0.1f, Effect_Padding = 0.05f;
    private static float Default_Animation_Sustain = 1.0f, Default_Animation_Padding = 3.0f;

    private Color Color_Deactivate = new Color(0.5f, 0.5f, 0.5f, 1.0f), Color_Activate = new Color(1.0f, 1.0f, 1.0f, 1.0f), Color_Tag = new Color(118f / 255f, 118f / 255f, 118f / 255f, 200f / 255f), Color_Text = new Color(67f / 255f, 67f / 255f, 67f / 255f, 200f / 255f);
    
    [SerializeField] private bool Terminate = false, Click_Icon_Flag = false;
    
    public void Play_Animation(GameObject target, string[] param)
    {
        switch(param[0])
        {
            case "VIBRATE":
                if(param[1] == "HORIZONTAL")    StartCoroutine(Effect_Vibrate_Horizontal(target));
                else if(param[1] == "VERTICAL") StartCoroutine(Effect_Vibrate_Vertical(target));
                break;

            default:
                Debug.Log("DialogueEventManager : Invalid Animation Flag");
                break;
        }
    }

    public void Play_Effect(GameObject target, string[] effect)
    {
        switch(effect[0])
        {
            case "FADE":
                switch(effect[1])
                {
                    case "ALL":
                        StartCoroutine(Default_Effect_Fade_All());
                        break;

                    case "IN":
                        StartCoroutine(Default_Effect_Fade_In());
                        break;

                    case "OUT":
                        StartCoroutine(Default_Effect_Fade_Out());
                        break;
                }
                break;

            case "CHARACTER":
                target.GetComponent<Character>().Effect(effect);
                break;

            default:
                Debug.Log("DialogueEventManager : Invalid Effect Flag");
                break;
        }
    }

    public void Play_Effect_Sound(string[] sound)
    {
        AudioSource source = Effect_Sound.GetComponent<AudioSource>();
        StartCoroutine(DataManager.Instance.GetComponent<DataManager>().Play_Sound(sound[0], source));
    }

    public void Deactivate_Click_Icon()
    {
        Click_Icon_Flag = false;
        Image icon = Click_Icon.GetComponent<Image>();
        icon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public IEnumerator Default_Click_Icon()
    {
        Click_Icon_Flag = true;
        Image icon = Click_Icon.GetComponent<Image>();

        while(Click_Icon_Flag)
        {
            for(float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                icon.color = new Color(1.0f, 1.0f, 1.0f, t);
                yield return null;
            }

            icon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            for(float t = 0.5f; t > 0f; t -= Time.deltaTime)
            {
                icon.color = new Color(1.0f, 1.0f, 1.0f, t);
                yield return null;
            }

            icon.color = new Color(1.0f, 1.0f, 1.0f, 0f);
        }

        icon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public IEnumerator Default_Effect_Fade_All()
    {
        Image fade = Fade.GetComponent<Image>();
        Click_Panel.GetComponent<Click_Panel>().Click_Disable();

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0.0f);

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            float normalized_color = t / Default_Animation_Sustain;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, normalized_color);
            yield return null;
        }

        yield return new WaitForSeconds(Default_Animation_Padding);

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            float normalized_color = 1 - t / Default_Animation_Sustain;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, normalized_color);
            yield return null;
        }

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0.0f);
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();
    }

    public IEnumerator Default_Effect_Fade_Out()
    {
        Image fade = Fade.GetComponent<Image>();
        Click_Panel.GetComponent<Click_Panel>().Click_Disable();

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0.0f);

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            float normalized_color = t / Default_Animation_Sustain;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, normalized_color);
            yield return null;
        }

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 1.0f);
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();
    }

    public IEnumerator Default_Effect_Fade_In()
    {
        Image fade = Fade.GetComponent<Image>();
        Click_Panel.GetComponent<Click_Panel>().Click_Disable();

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 1.0f);

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            float normalized_color = 1 - t / Default_Animation_Sustain;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, normalized_color);
            yield return null;
        }

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0.0f);
        Click_Panel.GetComponent<Click_Panel>().Click_Enable();
    }

    public IEnumerator Default_Effect_Tag(GameObject target)
    {
        GameObject Tag_Image = target.transform.Find("Tag").gameObject;
        GameObject Tag_Text = target.transform.Find("Tag_Text").gameObject;

        Image image = Tag_Image.GetComponent<Image>();
        TextMeshProUGUI text = Tag_Text.GetComponent<TextMeshProUGUI>();

        Color image_original = image.color;
        Color text_original = text.color;

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            image.color = Color.Lerp(image_original, Color_Tag, t);
            text.color = Color.Lerp(text_original, Color_Text, t);
            yield return null;
        }

        yield return new WaitForSeconds(Default_Animation_Padding);

        for(float t = 0f; t < Default_Animation_Sustain; t += Time.deltaTime)
        {
            image.color = Color.Lerp(image_original, Color_Tag, 200f/255f - t);
            text.color = Color.Lerp(text_original, Color_Text, 200f/255f - t);
            yield return null;
        }
    }

    public void Set_Position(GameObject target, Transform Position)
    {
        string[] Pos_Arr = Position.name.Split("_");
        int Position_Name;
        
        if(int.TryParse(Pos_Arr[0], out Position_Name) && Position_Name >= 0 && Position_Name <= 8)
        {
            target.transform.SetParent(Position.transform);
            target.transform.localPosition = new Vector3(0,0,0);
        }
        else if(Pos_Arr[0] == "OUT")
        {
            target.transform.SetParent(Position.transform);
            target.transform.localPosition = new Vector3(0,0,0);
        }
        else
        {
            Debug.Log("EffectManager - Invalid Position Input");
        }
    }

    public IEnumerator Default_Effect_Movement(Transform target, Transform dest)
    {
        // Debug.Log("Effect Movement");
        Vector3 startPos = target.position;
        Vector3 destPos = dest.position;
        Vector3 vel = Vector3.zero;

        target.SetParent(dest);

        while(!this.Terminate && target != null && Vector3.Distance(startPos, destPos) > 0.01f)
        {
            target.transform.position = Vector3.SmoothDamp(target.transform.position, destPos, ref vel, Movement_Speed);
            yield return null;
        }
        target.transform.position = destPos;
        this.Terminate = false;
    }

    public void Focus_Handler(GameObject Current_Character, List<GameObject> Character_Objects)
    {
        foreach(GameObject obj in Character_Objects)
        {
            if(Current_Character == obj) StartCoroutine(Default_Effect_Focus(obj));
            else StartCoroutine(Default_Effect_Unfocus(obj));
        }
    }

    public IEnumerator Default_Effect_Focus(GameObject target)
    {
        //Debug.Log("Effect Focus to " + target.name);
        Image image = target.GetComponent<Image>();
        
        if(image != null)
        {
            float elapsedTime = 0;
            Color startColor = image.color;

            while(!this.Terminate && target != null && elapsedTime < Default_Time)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / Default_Time;
                image.color = Color.Lerp(startColor, Color_Activate, t);
                yield return null;
            }
            image.color = Color_Activate;
        }
        this.Terminate = false;
    }

    public IEnumerator Default_Effect_Unfocus(GameObject target)
    {
        Image image = target.GetComponent<Image>();
        if(image != null)
        {
            float elapsedTime = 0;
            Color startColor = image.color;

            while(!this.Terminate && target != null && elapsedTime < Default_Time)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / Default_Time;
                image.color = Color.Lerp(startColor, Color_Deactivate, t);
                yield return null;
            }
            image.color = Color_Deactivate;
        }
        this.Terminate = false;
    }

    public IEnumerator Effect_Vibrate_Horizontal(GameObject target)
    {
        Vector3 originalPos = target.transform.position;
        float diameter = 20.0f;
        int cnt = 5;

        for(int i = 1; !this.Terminate && target != null && i <= cnt; ++i)
        {
            target.transform.position = new Vector3(originalPos.x - diameter, originalPos.y, originalPos.z);
            yield return new WaitForSeconds(Effect_Padding);
            target.transform.position = new Vector3(originalPos.x + diameter, originalPos.y, originalPos.z);
            yield return new WaitForSeconds(Effect_Padding);
            yield return null;
        }
        target.transform.position = originalPos;
        this.Terminate = false;
    }

    public IEnumerator Effect_Vibrate_Vertical(GameObject target)
    {
        Vector3 originalPos = target.transform.position;
        float diameter = 20.0f;
        int cnt = 5;

        for(int i = 1; !this.Terminate && target != null && i <= cnt; ++i)
        {
            target.transform.position = new Vector3(originalPos.x, originalPos.y - diameter, originalPos.z);
            yield return new WaitForSeconds(Effect_Padding);
            target.transform.position = new Vector3(originalPos.x, originalPos.y + diameter, originalPos.z);
            yield return new WaitForSeconds(Effect_Padding);
            yield return null;
        }
        target.transform.position = originalPos;
        this.Terminate = false;
    }

    public void Finish()    { this.Terminate = true; } 
}
