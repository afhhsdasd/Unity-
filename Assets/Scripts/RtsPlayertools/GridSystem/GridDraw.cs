using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDraw:MonoBehaviour
{
    public int cellSize = 2;
    public int PointsNum = 10;

    float[,] cellHeight;
    public Vector3 terrainPos;

    public Terrain terrain;
    public TerrainData terrainData; 
    public Material material;

    private GameObject gridObj;

    public Dictionary<Vector2, float> ligalCenterPosDic = new Dictionary<Vector2, float> ();
    public Dictionary<Vector2, float> ligalPosDic = new Dictionary<Vector2, float> ();

    public List<Vector2> ligalCenterPosList = new List<Vector2> ();
    public List<Vector2> ligalPosList = new List<Vector2> ();
    //private KDTree kDTree;
    public List<Vector3> squareVertices = new ();
    private List<Vector2> allUV = new ();
    public List<int> triangleIndex = new ();
    //public float 

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;

         
    }

    private void Start()
    {
        if(ligalCenterPosDic.Count != 0 && ligalCenterPosList.Count != 0) return;

        int fakeGridWidth = Mathf.CeilToInt(terrainData.size.x) / cellSize + 1;
        int fakeGridLength = Mathf.CeilToInt(terrainData.size.z) / cellSize + 1;
        
        CalculatecellHeight (fakeGridWidth, fakeGridLength); 
        

        bool[,] isCell = new bool[fakeGridWidth,fakeGridLength];

        for(int x = 0; x < fakeGridWidth; x++)
        {
            for(int z = 0; z < fakeGridLength; z++)
            {
                if(!isCell[x,z] && cellHeight[x, z]>=0)
                {
                    GridBFS ( x, z,isCell,fakeGridWidth,fakeGridLength);
                }
            }
        }

        GridInit ();
    }

    private void GridInit()
    {
        Mesh GridMesh = new Mesh ();
        GridMesh.vertices = squareVertices.ToArray ();
        GridMesh.triangles = triangleIndex.ToArray ();
        GridMesh.uv = allUV.ToArray ();
        GridMesh.RecalculateNormals ();

        gridObj = new GameObject ("CombinedGridMesh");
        //gridObj.isStatic = true;

        gridObj.transform.position = new Vector3 (0, 0.1f, 0);

        gridObj.AddComponent<MeshFilter> ().mesh = GridMesh;
        gridObj.AddComponent<MeshRenderer> ().material = material;
        //gridObj.AddComponent<MeshCollider>().sharedMesh = GridMesh;

        //gridObj.GetComponent<MeshCollider> ().convex = false;
        gridObj.SetActive (false);
        // adress list:
        //ligalCenterPosList.Clear ();
        ligalCenterPosList.AddRange (ligalCenterPosDic.Keys);
        //ligalPosList.Clear ();
        ligalPosList.AddRange (ligalPosDic.Keys);

        BuildingPlacer.Instance.PlacerInit (cellSize,terrain,this,ligalCenterPosDic,ligalPosDic,ligalCenterPosList,ligalPosList);

        
        //JsonMgr.Instance.SaveData (this, "Map:坠落点");
    }

    public void OnEnable()
    {
        if(ligalCenterPosDic.Count == 0 || ligalCenterPosList.Count == 0) return;

        BuildingPlacer.Instance.PlacerInit (cellSize / 2, terrain, this, ligalCenterPosDic, ligalPosDic, ligalCenterPosList, ligalPosList);
    }

    public void OnDisable()
    {
        //BuildingPlacer.Instance.Clear ();
    }

    public void OnDestroy()
    {
        //BuildingPlacer.Instance.Clear ();
    }

    public void CalculatecellHeight(int fakeGridWidth, int fakeGridLength)
    {
        RaycastHit hitinfo;
        Vector3 rayPos = terrain.transform.position;
        rayPos.y += terrainData.size.y;

        cellHeight = new float[fakeGridWidth, fakeGridLength];

        for(int x = 0; x < fakeGridWidth; x++)
        {
            for(int z = 0; z < fakeGridLength; z++)
            {
                rayPos.x = terrainPos.x + x * cellSize;
                rayPos.z = terrainPos.z + z * cellSize;
                cellHeight[x, z] = Physics.Raycast (rayPos, Vector3.down, out hitinfo
                    , 1000, LayerMask.GetMask ("Environment"), QueryTriggerInteraction.Ignore) 
                    ? hitinfo.point.y : -1;
            }
        }
    }

    public void GridBFS(int x,int z, bool[,] isCell,int fakeGridWidth, int fakeGridLength)
    {
        isCell[x,z] = true;

        float startHeight = cellHeight[x,z];
        List<Vector2Int> fakeGridPos = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Vector2Int pos = new(x,z);

        queue.Enqueue(pos);fakeGridPos.Add(pos);
        while(queue.Count > 0)
        {
            Vector2Int nowPos = queue.Dequeue ();
            int a;int b;
            foreach(var dir in new Vector2Int[]
            {
                new Vector2Int(1,0),new Vector2Int(0,1),
                new Vector2Int(-1,0),new Vector2Int(0,-1),
            })
            {
                a = nowPos.x + dir.x;
                b = nowPos.y + dir.y;
                Vector2Int _nowPos = new (a,b);
                if(a >= 0 && b >= 0 
                    && a < fakeGridWidth && b < fakeGridLength
                    && startHeight == cellHeight[a,b] 
                    && !isCell[a, b])
                {
                    isCell[a,b] = true;
                    queue.Enqueue(_nowPos);
                    fakeGridPos.Add(_nowPos);
                }
            }
        }
        if(fakeGridPos.Count > PointsNum)
        {
            GridHandle (fakeGridPos, startHeight);
        }
    }

    public void GridHandle(List<Vector2Int> fakeGridPos, float startHeight)
    {//二次加工成索引和真正的值，顺便补充ligalPosDic
        //List<Vector3> realGridPos = new ();

        Dictionary<Vector2Int, Vector3> dic = new Dictionary<Vector2Int, Vector3> ();

        Vector3 realPos;
        foreach(var pos in fakeGridPos)
        {
            realPos = terrain.transform.position;
            realPos.y = startHeight;
            realPos.x += pos.x * cellSize;
            realPos.z += pos.y * cellSize;
            //realGridPos.Add (realPos);
            dic[new Vector2Int (pos.x, pos.y)] = realPos;

            ligalPosDic[new Vector2 (realPos.x, realPos.z)] = realPos.y;
        }

        MeshBuild (dic);
    }

    public void MeshBuild(Dictionary<Vector2Int, Vector3> dic)
    {
        //最终加工成mesh所需材料
        Vector3[] squareVertice = new Vector3[4];
        Vector2 centerPos = Vector2.zero;

        foreach(var pos in dic.Keys)
        {
            if(dic.ContainsKey (new (pos.x + 1, pos.y))
                && dic.ContainsKey (new (pos.x + 1, pos.y + 1))
                && dic.ContainsKey (new (pos.x, pos.y + 1)))
            {
                squareVertice[0] = dic[pos];
                centerPos.x = squareVertice[0].x; centerPos.y = squareVertice[0].z;
                allUV.Add (new (0,0));

                squareVertice[1] = dic[new (pos.x + 1, pos.y)];
                centerPos.x += squareVertice[1].x; centerPos.y += squareVertice[1].z;
                allUV.Add(new (1,0));

                squareVertice[2] = dic[new (pos.x + 1, pos.y + 1)];
                centerPos.x += squareVertice[2].x; centerPos.y += squareVertice[2].z;
                allUV.Add(new(1,1));

                squareVertice[3] = dic[new (pos.x, pos.y + 1)];
                centerPos.x += squareVertice[3].x; centerPos.y += squareVertice[3].z;
                allUV.Add (new (0,1));

                centerPos /= 4;      
                ligalCenterPosDic[centerPos] = dic[pos].y;
            }
            else continue;

            int index = squareVertices.Count;
            squareVertices.AddRange (squareVertice);

            triangleIndex.Add (index);
            triangleIndex.Add (index + 2);
            triangleIndex.Add (index + 1);

            triangleIndex.Add (index);
            triangleIndex.Add (index + 3);
            triangleIndex.Add (index + 2);
        }

        
    }

    public void ShowGrid(bool isShow)
    {//显示动态计算的网格
        gridObj.SetActive (isShow);
    }
}