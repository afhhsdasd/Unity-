using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;

public class SoldierMove                   
{
    public RaycastHit hitinfo;
    private List<ArmorBody> soldiers;
    public float spacing;

    //private Vector3 lastPos = Vector3.zero;

    public SoldierMove(float spacing , E_Camp camp)
    {
        this.spacing = spacing;

        string mylayer = camp.ToString ();

        soldiers = LineDrawSelect.Instance.SelectedSoldiers;

        EventCenter.Instance.AddEventListener<KeyCode> ("鼠标某键按下",keycode =>
        {
            if(keycode != KeyCode.Mouse1 || soldiers.Count < 0) return;
            
            if(Physics.Raycast (Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()),
            out hitinfo, 1000, ~ LayerMask.GetMask (mylayer)))
            {
                SoldierStructureBase target = hitinfo.collider.GetComponent<SoldierStructureBase> ();   
                if(target != null)
                {
                    //如果是中立单位呢？
                    foreach(var soldier in soldiers)
                    {
                        soldier.SetTarget (target);
                    }
                }
            }

            if(Physics.Raycast(Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()), 
            out hitinfo, 1000, LayerMask.GetMask ("Environment")))
            {
                
                if(hitinfo.transform.gameObject.CompareTag("SceneObj")) return;
                        
                int count = soldiers.Count;
                var (rows, cols) = CalculateRectRowsCols (count);
                Vector3 endPos = hitinfo.point;
                Vector3 startOffset = new ( -(cols - 1) * spacing / 2f ,0, -(rows - 1) * spacing / 2f );
                Quaternion rot = CalculateRot (endPos, count);
                SortSoldiers ();//可对列表进行排序，达到让某种士兵排序在前的目的

                for(int i = 0; i < count; i++)//开始对列表中的士兵计算位置
                {
                    int row = i / cols;
                    int col = i % cols;

                    Vector3 offset = new(col * spacing, 0, row * spacing);

                    Vector3 targetPos = endPos + rot * startOffset + rot * offset;
                    targetPos.y = endPos.y; 

                    soldiers[i].ArmorMove (targetPos);
                    //soldier.ArmorMove (hitinfo.point);
                }
            }
        });


    }


    private (int rows, int cols) CalculateRectRowsCols(int soldierCount)
    {
        if(soldierCount <= 0) return (0, 0);

        // 行数 = 向上取整(√士兵数量)
        //int rows = Mathf.CeilToInt (Mathf.Sqrt (soldierCount));
        // 列数 = 向上取整(士兵数量 / 行数)
        //int cols = Mathf.CeilToInt ((float)soldierCount / rows);

        int cols = Mathf.CeilToInt (Mathf.Sqrt (soldierCount));//列数优先

        int rows = Mathf.CeilToInt ((float)soldierCount / cols);

        return (rows, cols);
    }

    private Quaternion CalculateRot(Vector3 endPos,int count)
    {
        Vector3 beginPos = Vector3.zero;
        foreach(var soldier in soldiers)
        {
            beginPos += soldier.transform.position;
        }
        beginPos = beginPos/ count; 
        Vector3 endDir = endPos - beginPos;
        endDir.y = 0;
        endDir = endDir.normalized;
        Quaternion rot = Quaternion.LookRotation(endDir);
        return rot;
    }

    private void SortSoldiers()
    {

    }
}


