#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class NavMeshBakeEditor : MonoBehaviour
{
    public NavMeshSurface navMeshSurface, navMeshSurface1;

    private BuyPoint[] GetBuyPoints()
    {
        BuyPoint[] all = Resources.FindObjectsOfTypeAll<BuyPoint>();
        List<BuyPoint> list = new List<BuyPoint>();
        foreach (BuyPoint bp in all)
        {
            if (bp != null && bp.gameObject.scene.IsValid())
                list.Add(bp);
        }
        return list.ToArray();
    }

    [ContextMenu("Refresh BuyPoints (扫描并重新编号)")]
    public void RefreshBuyPoints()
    {
        BuyPoint[] points = GetBuyPoints();

        // 按 hierarchy 同级顺序排序
        System.Array.Sort(points, (a, b) =>
        {
            int orderA = a.transform.GetSiblingIndex();
            int orderB = b.transform.GetSiblingIndex();
            return orderA.CompareTo(orderB);
        });

        for (int i = 0; i < points.Length; i++)
        {
            BuyPoint bp = points[i];
            bp.srNo = i;
            bp.gameObject.name = "BuyPoint " + i;

#if UNITY_EDITOR
            PrefabUtility.RecordPrefabInstancePropertyModifications(bp.gameObject);
            EditorUtility.SetDirty(bp);
#endif
        }

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log($"NavMeshBakeEditor: 扫描完成，找到 {points.Length} 个 BuyPoint，编号已刷新");
#endif
    }

    [ContextMenu("Bake NavMesh (全量烘焙)")]
    public void Bake()
    {
        BuyPoint[] points = GetBuyPoints();

        if (points.Length == 0)
        {
            Debug.LogWarning("NavMeshBakeEditor: 场景中没有找到 BuyPoint，跳过烘焙");
            return;
        }

        // 第1步：临时激活所有 objectToUnlock，隐藏 BuyPoint 本体
        foreach (BuyPoint bp in points)
        {
            if (bp.objectToUnlock != null)
                bp.objectToUnlock.SetActive(true);
            bp.gameObject.SetActive(false);
        }

        // 第2步：烘焙 NavMesh
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();
        if (navMeshSurface1 != null)
            navMeshSurface1.BuildNavMesh();

        // 第3步：恢复物体状态
        foreach (BuyPoint bp in points)
        {
            if (bp.objectToUnlock != null)
            {
                if (bp.objectToUnlock.GetComponent<Table>())
                    bp.objectToUnlock.GetComponent<BoxCollider>().enabled = false;

                bp.objectToUnlock.SetActive(false);
            }
            bp.gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log($"NavMeshBakeEditor: 烘焙完成，共处理 {points.Length} 个 BuyPoint");
#endif
    }
}
