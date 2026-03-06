using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelTip_P : PanelBase
{
    TextMeshProUGUI showText;
    private Coroutine coroutine;

    private void OnEnable()
    {
        SwitchText(1);
    }

    public void SwitchText(int choice)
    {
        switch (choice)
        {
            case 1:
                if (showText != null) showText.enabled = false;
                showText = GetControl<TextMeshProUGUI>("TextP");
                showText.enabled = true;
                break;

            case 2:
                showText.enabled = false;
                showText = GetControl<TextMeshProUGUI>("TextEP");
                showText.enabled = true;
                if (coroutine !=  null)
                {
                    StopCoroutine(coroutine);
                }
                coroutine = StartCoroutine(Disappeare());

                break;
        }
    }

    private IEnumerator Disappeare()
    {
        Color originalColor = showText.color;
        float restTime = 2;
        while(restTime > 0)
        {
            yield return null;
            restTime -= Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0,1 - restTime/2);//
            showText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        showText.enabled = false;
        // 可选：重置文本Alpha，下次显示时恢复正常
        showText.color = originalColor;
        coroutine = null;

        UIMgr.Instance.HidePanel("PanelTip_P");
    }
}
