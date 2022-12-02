using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PawnUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _hpText;
    [SerializeField]
    private Image _poisonIcon;

    public void UpdateHP(int hp, int additiveHp)
    {
        _hpText.text = hp.ToString() + (additiveHp > 0 ? (" + " + additiveHp.ToString()) : "");
    }

    public void HideAllUI()
    {
        foreach (RectTransform uiObj in GetComponentsInChildren<RectTransform>())
        {
            uiObj.gameObject.SetActive(false);
        }
    }

    public void ShowAllUI()
    {
        foreach (RectTransform uiObj in GetComponentsInChildren<RectTransform>(true))
        {
            uiObj.gameObject.SetActive(true);
        }
    }

    public void ActivatePoison()
    {
        _poisonIcon.enabled = true;
    }

    public void DeactivatePoison()
    {
        _poisonIcon.enabled = false;
    }
}
