using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEngine;
using UnityEngine.InputSystem;



public class BuildingPlacer : SingletonLocal<BuildingPlacer>
{
    //四条射线，一个盒子；左上角的点和那个点最近，吸附过去，再计算物体中心点的位置
    [Header("基础配置")]
    public bool isBuildMenu;
    public bool isStruct;
    public bool isReady;
    //public Coroutine inBuildMenu;

    //public GameObject preFeb;
    [Header("建筑配置")]
    public GameObject structure;
    public BuildingMenu_C structureMenuInit = new();
    public float halfOccupySize;
    public float heightOffset;
    public float halfStructureSize;
    public E_StructureType structureType = E_StructureType.Center;

    [Header("地图配置")]
    public float cellSize;
    public Terrain terrain;
    public GridDraw gridDraw;
    private KDTree kDTree;

    public Dictionary<Vector2,float> ligalCenterPosDic;
    public Dictionary<Vector2,float> ligalPosDic;

    public List<Vector2> ligalCenterPosList;
    public List<Vector2> ligalPosList;

    [Header("其他配置")]
    public Coroutine coroutine;

    public void PlacerInit(float _cellSize, Terrain _terrain, GridDraw _gridDraw
        , Dictionary<Vector2, float> _ligalCenterPosDic, Dictionary<Vector2, float> _ligalPosDic
        , List<Vector2> _ligalCenterPosList, List<Vector2> _ligalPosList)
    {
        cellSize = _cellSize;

        terrain = _terrain;
        gridDraw = _gridDraw;
        //
        ligalCenterPosDic = _ligalCenterPosDic;
        ligalPosDic = _ligalPosDic;

        ligalCenterPosList = _ligalCenterPosList;
        ligalPosList = _ligalPosList;
    }

    public void Clear()
    {
        ligalCenterPosDic.Clear();
        ligalPosDic.Clear ();

        ligalCenterPosList.Clear ();
        ligalPosList.Clear ();

        kDTree = null;

    }

    protected override void Awake()
    {
        base.Awake ();
        //preFeb = GameObject.Find ("Cube");
        
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<KeyCode> ("某键按下", ShowBuildingMenu);
    }

    private void OnDisable()
    {
        
        //EventCenter.Instance.RemoveEventListener<KeyCode> ("某键按下", ShowBuildingMenu);
        
        
    }

    public IEnumerator InBuildMenu()
    {
        
        while(isBuildMenu)
        {
            yield return null;
        }

    }

    private IEnumerator InStruct()
    {
        //float height = 0;

        Collider[] results = new Collider[10];
        
        Vector3[] _5point = new Vector3[5];

        while(isStruct)
        {
            yield return null;
            //检测地形合法性
            if(!Physics.Raycast (Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue ()), out RaycastHit hitInfo, 1000, LayerMask.GetMask ("Environment"), QueryTriggerInteraction.Ignore)
                || structure == null)
            {
                continue;
            }
            

            //检测位置占用合法性
            Vector3 halfExtents = new (halfStructureSize, halfStructureSize, halfStructureSize);//根据建筑大小
            int hits = Physics.OverlapBoxNonAlloc (hitInfo.point, halfExtents, results, Quaternion.identity, ~LayerMask.GetMask ("Environment"), QueryTriggerInteraction.Ignore);
            if(hits > 1)
            {
                Vector3 setPos = hitInfo.point;
                setPos.y = heightOffset;
                structure.transform.position = setPos;
                continue;
            }

            //hitinfo数据清洗
            Vector3 hitPoint = hitInfo.point;
            Terrain terrain = Terrain.activeTerrain;
            hitPoint.y = terrain.SampleHeight (hitInfo.point);

            GridAttached (hitPoint);

        }

        structure = null;
    }

    private void GridAttached(Vector3 hitPoint)
    {
        
        float height = hitPoint.y;
        Vector2 bestLigalPoint;
        //检测网格占用合法性
        Vector2[] _5point = new Vector2[5];
        if (structureType == E_StructureType.Center)
        {
            kDTree = KDTree.BuildKDTree(ligalCenterPosList, 0);
            bestLigalPoint = kDTree.FindNearest(kDTree, new(hitPoint.x, hitPoint.z), 0);
            _5point[0] = new (bestLigalPoint.x, bestLigalPoint.y);
            _5point[1] = new (_5point[0].x - halfStructureSize, _5point[0].y - halfStructureSize);
            _5point[2] = new (_5point[0].x + halfStructureSize, _5point[0].y - halfStructureSize);
            _5point[3] = new (_5point[0].x - halfStructureSize, _5point[0].y + halfStructureSize);
            _5point[4] = new (_5point[0].x + halfStructureSize, _5point[0].y + halfStructureSize);
        }
        else
        {
            kDTree = KDTree.BuildKDTree(ligalPosList, 0);
            bestLigalPoint = kDTree.FindNearest(kDTree, new(hitPoint.x, hitPoint.z), 0);
            _5point[0] = new(bestLigalPoint.x, bestLigalPoint.y);
            _5point[1] = new(_5point[0].x - halfStructureSize, _5point[0].y - halfStructureSize);
            _5point[2] = new(_5point[0].x + halfStructureSize, _5point[0].y - halfStructureSize);
            _5point[3] = new(_5point[0].x - halfStructureSize, _5point[0].y + halfStructureSize);
            _5point[4] = new(_5point[0].x + halfStructureSize, _5point[0].y + halfStructureSize);
        }
        
        bool isHit = true;
        for (int i = 1; i < 5; i++)
        {
            if(!ligalPosDic.ContainsKey (_5point[i]) || ligalPosDic[_5point[i]] != height)
            {
                isHit = false; break;
            }
        }
        //float distance = new Vector2(hitPoint.x - bestLigalPoint.x, hitPoint.z - bestLigalPoint.y).sqrMagnitude;

        isReady = false;
        if (!isHit)
        {

            Vector3 setPos = hitPoint;
            setPos.y = heightOffset;
            structure.transform.position = setPos;

        }
        else
        {
            structure.transform.position = new (bestLigalPoint.x, height + heightOffset, bestLigalPoint.y);
            //按下左键放置建筑
            isReady = true;
        }
    }

    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="keycode"></param>
    private void ShowBuildingMenu(KeyCode keycode)
    {
        //暂停进入携程,展示网格
        //如果后期有多块地形，改为直接找
        if(keycode == KeyCode.B)
        {
            if(!isBuildMenu && !isStruct)
            {
                gridDraw.ShowGrid (true);
                UIMgr.Instance.ShowPanel<PanelTip_P>("PanelTip_P");

                isBuildMenu = true;
                StartCoroutine (InBuildMenu ());

                Time.timeScale = 0f; // 全局暂停
                Time.fixedDeltaTime = 0f;
                EventCenter.Instance.AddEventListener<KeyCode> ("某键按下", KeyBoardDown);
            }
        }
    }
    
    private void KeyBoardDown(KeyCode keycode)
    {
        isBuildMenu = false;
        isStruct = true;//锁协程
        //EventCenter.Instance.RemoveEventListener<KeyCode> ("某键按下", KeyBoardDown);

        switch(keycode)
        {
            case KeyCode.B:
                //取消暂停 
                UIMgr.Instance.GetPanel<PanelTip_P>("PanelTip_P").SwitchText(2);
                Time.timeScale = 1f; // 全局暂停
                Time.fixedDeltaTime = 1f;
                gridDraw.ShowGrid (false);
                isStruct = false;
                EventCenter.Instance.RemoveEventListener<KeyCode>("某键按下", KeyBoardDown);
                return;

            case KeyCode.D:
                //放置后扣除资源
                Selection("挖掘机");
                break;
            case KeyCode.W:
                Selection("兵营");
                break;
            case KeyCode.T:
                Selection("炮塔");
                break;
            case KeyCode.C:
                Selection("传送带");
                break;
        }
        
        if(ligalCenterPosDic.Count == 0 || ligalPosDic.Count == 0)
        {
            isStruct = false;
            gridDraw.ShowGrid (false);
            //取消暂停，异常退出
            Debug.Log ("无法建造");
            return;
        }

        EventCenter.Instance.AddEventListener<KeyCode>("鼠标某键按下",Construction);
        EventCenter.Instance.AddEventListener<KeyCode>("鼠标某键按下",DisConstruction);

        if(coroutine == null)
        {
            coroutine = StartCoroutine (InStruct());

        }
        
    }

    private void Selection(string name)
    {
        if (structure != null)
        {
            Destroy(structure);
            structure = null;
        }
        (halfOccupySize, heightOffset, structureType) = structureMenuInit.ActivateStructure((structure) =>
        {
            this.structure = structure.gameObject;
            SetStructurePreviewMode(this.structure, true);
        }, name);
        halfStructureSize = halfOccupySize * cellSize;
    }

    //private void 

    public void Construction(KeyCode keyCode)
    {
        if(keyCode == KeyCode.Mouse0 && isReady == true)
        {
            SetStructurePreviewMode(structure, false);
            structure = null;
        }
    }

    public void DisConstruction(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Mouse2 && structure != null)
        {
            Destroy(structure);
            structure = null;
        }
    }

    private void SetStructurePreviewMode(GameObject structureObj, bool isPreview)
    {
        if (structureObj == null) return;

        // 1. 处理碰撞：预览态禁用所有碰撞体，正常态恢复
        Collider[] colliders = structureObj.GetComponentsInChildren<Collider>(true); // 包含子物体
        foreach (var col in colliders)
        {
            col.enabled = !isPreview;
        }

        // 2. 处理视觉：预览态半透明，正常态恢复原透明度
        Renderer[] renderers = structureObj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            // 复制材质（避免修改原预制体的材质）
            Material mat = new Material(renderer.material);
            if (isPreview)
            {
                // 半透明（alpha=0.5，可根据需求调整）
                Color color = mat.color;
                color.a = 0.5f;
                mat.color = color;
                // 开启透明渲染（关键：否则alpha不生效）
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else
            {
                // 恢复原透明度
                Color color = mat.color;
                color.a = 1f;
                mat.color = color;
                // 恢复正常渲染
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue = 2000;
            }
            renderer.material = mat;
        }

        // 3. 可选：预览态禁用建筑的逻辑脚本（避免预览时触发业务逻辑）
        MonoBehaviour[] scripts = structureObj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var script in scripts)
        {
            // 排除自身（BuildingPlacer）和必要的核心脚本
            if (script is not BuildingPlacer)
            {
                script.enabled = !isPreview;
            }
        }
    }

}
            