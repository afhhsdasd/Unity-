using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public enum E_Camp
{
    Blue,
    Red,
    Yellow
}

public class LineDrawSelect : SingletonLocal<LineDrawSelect>
{
    public E_Camp playerCamp = E_Camp.Blue;

    public float spacing;

    private LineRenderer line;

    private Vector3 beginPos;
    private Vector3 endPos;
    private Vector3 secondPos;
    private Vector3 thirdPos;

    private RaycastHit beginHit;
    private RaycastHit endHit;

    private Coroutine inspectSoldier;
    private SoldierMove soldierMove;
    private readonly List<ArmorBody> selectedSoldiers = new();
    private bool isStart;

    //[Header ("回调")]
    //public UnityAction<KeyCode> MouseLeftDown;
    //public UnityAction<KeyCode> MouseLeftUp;

    public List<ArmorBody> SelectedSoldiers => selectedSoldiers;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        //beginPos.z = 5;
        //endPos.z = 5;
        line = this.GetComponent<LineRenderer>();
        line.loop = true;
        InputMgr.Instance.isStart = true;

        soldierMove = new SoldierMove (spacing , playerCamp);
        
    }

    
    void Start()
    {
        
        
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<KeyCode> ("鼠标某键按下", MouseLeftDown);

        EventCenter.Instance.AddEventListener<KeyCode> ("鼠标某键抬起", MouseLeftUp);
    }

    private void OnDisable()
    {
        
        //EventCenter.Instance.RemoveEventListener<KeyCode> ("鼠标某键按下", MouseLeftDown);

        //EventCenter.Instance.RemoveEventListener<KeyCode> ("鼠标某键抬起", MouseLeftUp);
        
        
    }

    private IEnumerator StartDraw()
    {
        beginPos = Mouse.current.position.ReadValue ();
        beginPos.z = 5;
        while(isStart)
        {
            secondPos.x = endPos.x; secondPos.y = beginPos.y; secondPos.z = 5;
            thirdPos.x = beginPos.x; thirdPos.y = endPos.y; thirdPos.z = 5;

            endPos = Mouse.current.position.ReadValue ();
            endPos.z = 5;
            line.SetPosition (0, Camera.main.ScreenToWorldPoint (beginPos));
            line.SetPosition (1, Camera.main.ScreenToWorldPoint (secondPos));
            
            line.SetPosition (2, Camera.main.ScreenToWorldPoint (endPos));
            line.SetPosition (3, Camera.main.ScreenToWorldPoint (thirdPos));
            yield return null;
        }
    }

    private IEnumerator InspectSoldier()
    {
        while(selectedSoldiers.Count != 0)
        {
            foreach(var soldier in selectedSoldiers)
            {
                if(soldier.isDead == true)
                {
                    selectedSoldiers.Remove(soldier);
                }
            }
            yield return null;
        }
        inspectSoldier = null;
    }
    /// <summary>
    /// 回调
    /// </summary>
    /// <param name="keycode"></param>
    private void MouseLeftDown(KeyCode keycode)
    {
        if(keycode == KeyCode.Mouse0)
        {
            foreach(var soldier in selectedSoldiers)
            {
                soldier.FootEffect.SetActive (false);
            }
            selectedSoldiers.Clear ();

            isStart = true;
            line.positionCount = 4;

            StartCoroutine (StartDraw ());

            if(Physics.Raycast (Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()), out beginHit, 1000, LayerMask.GetMask ("Environment"),QueryTriggerInteraction.Ignore))
            {

            }
            //beginHit = Physics.Raycast(Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()),)
        }
    }

    private void MouseLeftUp(KeyCode keycode)
    {
        if(keycode == KeyCode.Mouse0)
        {
            isStart = false;
            line.positionCount = 0;
            if(Physics.Raycast (Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()), out endHit, 1000, LayerMask.GetMask ("Environment"),QueryTriggerInteraction.Ignore))
            {

                Vector3 centerPos = new ((beginHit.point.x + endHit.point.x) / 2, (beginHit.point.y + endHit.point.y) / 2, (beginHit.point.z + endHit.point.z) / 2);
                Vector3 SelectHalfRange = new (Mathf.Abs (endHit.point.x - beginHit.point.x)/2, 100, Mathf.Abs (endHit.point.z - beginHit.point.z)/2);
                Collider[] colliders = Physics.OverlapBox (centerPos, SelectHalfRange);
                ArmorBody soldier;
                foreach(Collider collider in colliders)
                {
                    soldier = collider.gameObject.GetComponent<ArmorBody> ();

                    if(soldier != null)
                    {
                        soldier.FootEffect.SetActive (true);
                        selectedSoldiers.Add (soldier);
                    }
                }
                inspectSoldier ??= StartCoroutine (InspectSoldier ());

            }

        }
    }
}
