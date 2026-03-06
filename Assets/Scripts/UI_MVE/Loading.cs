using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIMgr.Instance.ShowPanel<PanelBegin> ("PanelBegin", E_UI_Layer.Bottom);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
