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
    /// ɾ���ص�����Ϸ��
    /// </summary>
    /// <param name="rootTransform"></param>
    private void DelOverlapObject(Transform rootTransform)
    {
        HashSet<Vector2Int> pos = new HashSet<Vector2Int>();
        List<GameObject> duplicateObjects = new List<GameObject>();
        foreach(Transform child in rootTransform.GetComponentsInChildren<Transform>())
        {
            //�ų����ڵ�
            if (child.transform == rootTransform)
            {
                continue;
            }
            //��һ�μ������ָ�ֵ��ʽ
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
        Debug.Log(string.Format("DelOverlapObject:�ظ�Ԫ����Ϊ{0}", duplicateObjects.Count));
        foreach(GameObject obj in duplicateObjects)
        {
            DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// ������������
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
    /// 0.1����������
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
    /// ���ݶ������۵�����
    /// ���ƺ��뵽��һЩ���õ�������������������Ҳ�û����
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
