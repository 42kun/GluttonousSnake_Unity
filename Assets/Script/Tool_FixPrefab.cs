using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tool_FixPrefab : MonoBehaviour
{

    public GameObject targetObject;
    public bool work = false;
    public bool delOverlapObject = false;
    public bool useFlodTool = false;
    public string foldName;

    public string[] overlapWhilelist;
    private void Awake()
    {
        if (!work)
        {
            return;
        }
        //Returns the path name relative to the project folder where the asset is stored.
        string PrefabPath = AssetDatabase.GetAssetPath(targetObject);
        //return the Prefab root object
        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        Transform rootTransform = contentsRoot.transform;
        FixAllObjectPosition(rootTransform);
        if (delOverlapObject)
            DelOverlapObject(rootTransform);
        if (useFlodTool)
            foldObject(rootTransform, foldName);
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, PrefabPath);
        PrefabUtility.UnloadPrefabContents(contentsRoot);
    }

    /// <summary>
    /// 删除重叠的游戏体
    /// </summary>
    /// <param name="rootTransform"></param>
    private void DelOverlapObject(Transform rootTransform)
    {
        HashSet<Vector2Int> pos = new HashSet<Vector2Int>();
        List<GameObject> duplicateObjects = new List<GameObject>();
        foreach(Transform child in rootTransform.GetComponentsInChildren<Transform>())
        {
            //排除跟节点
            if (child.transform == rootTransform)
            {
                continue;
            }
            //第一次见到这种赋值方式
            Vector2Int t = new Vector2Int
            {
                x = Mathf.RoundToInt(child.position.x * 10),
                y = Mathf.RoundToInt(child.position.y * 10)
            };
            bool skip_flag = false;
            foreach(string ws in overlapWhilelist)
            {
                if (child.gameObject.name.Contains(ws))
                    skip_flag = true;
            }
            if (skip_flag)
                continue;
            if (pos.Contains(t))
                duplicateObjects.Add(child.gameObject);
            else
                pos.Add(t);
        }
        Debug.Log(string.Format("DelOverlapObject:重复元素数为{0}", duplicateObjects.Count));
        foreach(GameObject obj in duplicateObjects)
        {
            DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// 四舍五入坐标
    /// </summary>
    /// <param name="rootTransform"></param>
    private void FixAllObjectPosition(Transform rootTransform)
    {
        foreach(Transform child in rootTransform.GetComponentsInChildren<Transform>())
        {
            Vector2 p = child.position;
            p = roundPosition(p);
            child.position = p;
        }
    }

    /// <summary>
    /// 0.1的四舍五入
    /// </summary>
    /// <param name="originPosition"></param>
    /// <returns></returns>
    private Vector2 roundPosition(Vector2 originPosition)
    {
        originPosition.x = Mathf.Round(originPosition.x * 10) / 10;
        originPosition.y = Mathf.Round(originPosition.y * 10) / 10;
        return originPosition;
    }

    /// <summary>
    /// 根据对象名折叠对象
    /// 我似乎想到了一些更好的做法，所以这个方法我并没用它
    /// </summary>
    /// <param name="rootTransform"></param>
    /// <param name="name"></param>
    private void foldObject(Transform rootTransform, string name)
    {
        GameObject fatherObject = new GameObject(name);
        fatherObject.transform.SetParent(rootTransform);
        foreach (Transform child in rootTransform.GetComponentsInChildren<Transform>())
        {
            if(child!=rootTransform && child.gameObject.name.Contains(name))
            {
                child.SetParent(fatherObject.transform);
            }
        }

    }
}
