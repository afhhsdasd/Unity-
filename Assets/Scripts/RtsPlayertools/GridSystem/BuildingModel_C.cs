using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum E_StructureType
{
    Center,
    Point,
}

public class BuildingModel_C
{
    public string structureName;
    public float occupySize;
    public float heightOffset;
    public E_StructureType structureType;
    //storage
    public int sID;
    public SoldierStructureBase storageStructure;
}

public class BuildingMenu_C
{
    public Dictionary<string, BuildingModel_C> structureMenu = new Dictionary<string, BuildingModel_C>()
    {
        {
            "基地", new BuildingModel_C()
            {
                structureName = "基地",
                occupySize = 3f,       // 自定义你的参数
                heightOffset = 0.5f,
                structureType = E_StructureType.Center,
                ////sID = 1,
                //prefab = basePrefab
            }
        },
        {
            "电池", new BuildingModel_C()
            {
                structureName = "电池",
                occupySize = 1f,
                heightOffset = 0.5f,
                structureType = E_StructureType.Center,
                //sID = 2,
                //prefab = powerPlantPrefab
            }
        },
        {
            "挖掘机", new BuildingModel_C()
            {
                structureName = "挖掘机",
                occupySize = 2f,
                heightOffset = 0.5f,
                structureType = E_StructureType.Point,
                //sID = 3,
                //prefab = minerPrefab
            }
        },
        {
            "兵营", new BuildingModel_C()
            {
                structureName = "兵营",
                occupySize = 3f,
                heightOffset = 0.5f,
                structureType = E_StructureType.Center,
                //sID = 3,
                //prefab = minerPrefab
            }
        },
        {
            "炮塔", new BuildingModel_C()
            {
                structureName = "炮塔",
                occupySize = 1f,
                heightOffset = 0.5f,
                structureType = E_StructureType.Center,
                //sID = 3,
                //prefab = minerPrefab
            }
        },
        {
            "传送带", new BuildingModel_C()
            {
                structureName = "传送带",
                occupySize = 1f,
                heightOffset = 0.5f,
                structureType = E_StructureType.Center,
                //sID = 3,
                //prefab = minerPrefab
            }
        },
        // 继续添加兵营、炮塔、传送带的配置...
    };

    public E_StructureType structureType;

    public (float,float,E_StructureType) ActivateStructure(Action<SoldierStructureBase> structure,string structureName)
    {
        SoldierStructureBase storage = structureMenu[structureName].storageStructure;
        if (storage != null)
        {

            structure?.Invoke(MonoController.Instance.GetInstantiate(storage));

        }
        else
        {
            AddressableMgr.Instance.LoadAssetAsync<GameObject>((callback) =>
            {

                storage = callback.GetComponent<SoldierStructureBase>();
                if(storage == null)
                {
                    Debug.Log($"{storage}_这个不是建筑");
                }
                structure.Invoke(MonoController.Instance.GetInstantiate(storage));

            }, Addressables.MergeMode.Intersection, structureName, "Structure");
        }
        return (structureMenu[structureName].occupySize/2, structureMenu[structureName].heightOffset, structureMenu[structureName].structureType);
    }
}
