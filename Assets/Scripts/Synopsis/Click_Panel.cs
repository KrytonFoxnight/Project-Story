using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Click_Panel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject SynopsisManager;
    [SerializeField] private bool click = false;
    private float padding = 0.25f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(click)
        {
            SynopsisManager.GetComponent<SynopsisManager>().ClickPanel_Handler();
            StartCoroutine(Click_Padding());
        }
    }

    IEnumerator Click_Padding()
    {
        this.click = false;
        yield return new WaitForSeconds(padding);
        if(SynopsisManager.GetComponent<SynopsisManager>().Is_Over())
        {
            this.click = false;
            GameObject UI_Synopsis = SynopsisManager.transform.Find("Synopsis UI").gameObject;
            UI_Synopsis.SetActive(false);
        }
        else this.click = true;
    }

    public void Click_Enable()  { click = true;     }
    public void Click_Disable() { click = false;    }
}
