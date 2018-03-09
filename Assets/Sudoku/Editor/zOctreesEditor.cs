using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
public class zOctreesEditor : EditorWindow {

    public GUISkin skin;

    private static zOctreesEditor myWindow;
    private static zOctrees root;

    /// <summary>
    /// 需要划分的物体root
    /// </summary>
    private Transform rootTransform;
    /// <summary>
    /// 划分的最小尺寸
    /// </summary>
    private float minSize;
    /// <summary>
    /// 只绘制叶子，绘制正方体，自定义初始bounds
    /// </summary>
    private bool OnlyDrawLeaf, normalize, customBounds;
    /// <summary>
    /// 自定义初始bounds的center和size
    /// </summary>
    private Vector3 customCenter, customSize;
    /// <summary>
    /// 整个八叉树的位移
    /// </summary>
    private Vector3 offset;
    /// <summary>
    /// 初始bounds的最小和最大值位移
    /// </summary>
    private Vector3 minOffset, maxOffset;

    /// <summary>
    /// 节点列表
    /// </summary>
    private static List<zOctrees> nodeList = new List<zOctrees>();
    /// <summary>
    /// 最终划分区域
    /// </summary>
    private static List<GameObject> sceneParts = new List<GameObject>();
    /// <summary>
    /// 预制体保存路径
    /// </summary>
    private static string savePath;

    [MenuItem("Window/Octree")]
    public static void OpenWindow()
    {
        Rect rct = new Rect(0, 0, 600, 600);
        myWindow = GetWindowWithRect<zOctreesEditor>(rct);
    }
    void OnFocus()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }
    public void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }
    private List<Transform> transs;
    
    private void OnGUI()
    {
        GUI.skin = skin;

        GUILayout.Label("Octree Baker");
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();

        OnlyDrawLeaf = EditorGUILayout.Toggle("OnlyDrawLeaf", OnlyDrawLeaf);

        minSize = EditorGUILayout.FloatField("min size", minSize);
        rootTransform = (Transform)EditorGUILayout.ObjectField(rootTransform, typeof(Transform), true);

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        
        normalize = EditorGUILayout.Toggle("normalize", normalize);
        customBounds = EditorGUILayout.Toggle("customBounds", customBounds);
        offset = EditorGUILayout.Vector3Field("offset", offset);
        minOffset = EditorGUILayout.Vector3Field("minOffset", minOffset);
        maxOffset = EditorGUILayout.Vector3Field("maxOffset", maxOffset);            

        if(customBounds)
        {
            GUILayout.Space(20);
            customCenter = EditorGUILayout.Vector3Field("customCenter", customCenter);
            customSize = EditorGUILayout.Vector3Field("customSize", customSize);
        }

        GUILayout.Space(20);
        //生成初始bounds
        if (GUILayout.Button("Generate bound"))
        {
            if(rootTransform == null)
            {
                Debug.LogError("Root is null");
                return;
            }
            if (root == null)
            {
                root = new zOctrees();
            }

            transs = new List<Transform>(rootTransform.GetComponentsInChildren<Transform>());
            //calculate whole bounds
            float xmin = float.MaxValue, ymin = float.MaxValue, zmin = float.MaxValue;
            float xmax = float.MinValue, ymax = float.MinValue, zmax = float.MinValue;

            if (!customBounds)
            {
                foreach (Transform t in transs)
                {
                    if (t.position.x < xmin)
                        xmin = t.position.x;
                    if (t.position.y < ymin)
                        ymin = t.position.y;
                    if (t.position.z < zmin)
                        zmin = t.position.z;

                    if (t.position.x > xmax)
                        xmax = t.position.x;
                    if (t.position.y > ymax)
                        ymax = t.position.y;
                    if (t.position.z > zmax)
                        zmax = t.position.z;
                }


                root.min = new Vector3(xmin, ymin, zmin) + offset + minOffset;
                root.max = new Vector3(xmax, ymax, zmax) + offset + maxOffset;                
            }
            else
            {
                root.min = customCenter - customSize / 2f;
                root.max = customCenter + customSize / 2f;
            }
            root.objs = transs;


            if (normalize)
            {
                Vector3 center = root.center;
                Vector3 bound = root.max - root.min;
                float maxEdge = Mathf.Max(bound.x, bound.y, bound.z)/2f;
                Vector3 newHalfEdge = Vector3.one * maxEdge;
                root.min = center - newHalfEdge;
                root.max = center + newHalfEdge;
            }

            nodeList.Add(root);
            Debug.Log(root.min + " " + root.max);
        }

        //递归生成八叉树
        if(GUILayout.Button("Bake Octree"))
        {
            if(root == null)
            {
                Debug.LogError("root node is null");
                return;
            }
            GenerateTree(root);
            Debug.Log("Generated");
        }

        //清除
        if(GUILayout.Button("Clear"))
        {
            nodeList.Clear();
            sceneParts.Clear();
            System.GC.Collect();
            level = 0;
        }       

        //重构场景物体结构
        GUILayout.Space(30);
        if(GUILayout.Button("Reconstruct Scene Structure"))
        {
            ReconstructScene();
        }
        ///还原场景物体结构（其实就是删除新生成的物体，并没有还原之前的父子物体结构）
        if (GUILayout.Button("Revert Scene Structure"))
        {
            RevertSceneStructure();
        }
        //删除空物体
        if (GUILayout.Button("ClearEmpty"))
        {
            ClearEmpty();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("PathToSave");
        savePath = EditorGUILayout.TextField(savePath);
        GUILayout.EndHorizontal();

        //保存为预制体
        if (GUILayout.Button("Save"))
        {
            //string path = System.IO.Path.Combine(Application.dataPath, savePath);
            //if (!System.IO.Directory.Exists(path))
            //    System.IO.Directory.CreateDirectory(path);
            SingleRoom[] rooms = GameObject.FindObjectsOfType<SingleRoom>();

            for (int i = 0; i < rootTransform.childCount; i++)
            {
                roomParts rp = rootTransform.GetChild(i).gameObject.AddComponent<roomParts>();
                rp.SaveLightmap();

                foreach(var room in rooms)
                {
                    if (room.Contains(rp.transform.position))
                    {
                        rp.roomID = room.roomID;
                        rp.localPosition = rp.transform.position - room.transform.position;
                        break;
                    }
                }
                //这里为了方便写死了
                string filepath = "Assets/Prefabs/" + rp.gameObject.name;
                PrefabUtility.CreatePrefab(filepath + ".prefab", rp.gameObject);
            }

        }
    }

    void ClearEmpty()
    {
        if (sceneParts.Count != 0)
        {
            foreach (var g in sceneParts)
            {
                if (g.GetComponentsInChildren<MeshRenderer>().Length == 0)
                    DestroyImmediate(g);
            }
        }
        else
        {
            Transform[] ts = rootTransform.GetComponentsInChildren<Transform>();
            foreach(var t in ts)
            {
                if (t != rootTransform && !t.GetComponent<MeshRenderer>())
                    DestroyImmediate(t.gameObject);
            }
        }
    }
    void RevertSceneStructure()
    {
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            Transform t = rootTransform.GetChild(i);
            if(t.name.StartsWith("scene_part"))
            {
                for (int j = 0; j < t.childCount; j++)
                {
                    t.GetChild(j).SetParent(rootTransform);
                }
                //DestroyImmediate(t.gameObject);
            }
        }
        Debug.Log("Revert Complete");
    }
    void ReconstructScene()
    {
        foreach(var node in nodeList)
        {
            if(node.IsValid)
            {
                GameObject go = new GameObject();
                go.name = "scene_part_" + node.name;
                sceneParts.Add(go);
                go.transform.SetParent(rootTransform);
                go.transform.position = node.center;
                foreach(var t in node.objs)
                {
                    t.SetParent(go.transform);
                }
            }
        }

        Debug.Log("Reconstruct Complete!");
    }

    int level = 0;
    void GenerateTree(zOctrees node)
    {
        Debug.Log("generating: " + minSize / (node.max.x - node.min.x) * 100 + "% with objs count :"
             + node.objs.Count);
        nodeList.Add(node);
        if (node.max.x - node.min.x < minSize || node.objs.Count <= 1)
        {
            node.IsLeaf = true;
            return;
        }

        node.IsLeaf = false;      
        Vector3 c = node.center;
        Vector3 mi = node.min;
        Vector3 ma = node.max;

        #region generate child

        node.u1 = new zOctrees()
        {
            parent = node,
            min = new Vector3(mi.x, c.y, mi.z),
            max = new Vector3(c.x, ma.y, c.z),
            name = level.ToString() + "u1"         
        };

        node.u2 = new zOctrees()
        {
            parent = node,
            min = new Vector3(c.x, c.y, mi.z),
            max = new Vector3(ma.x, ma.y, c.z),
            name = level.ToString() + "u2"
        };

        node.u3 = new zOctrees()
        {
            parent = node,
            min = new Vector3(c.x, c.y, c.z),
            max = new Vector3(ma.x, ma.y, ma.z),
            name = level.ToString() + "u3"
        };

        node.u4 = new zOctrees()
        {
            parent = node,
            min = new Vector3(mi.x, c.y, c.z),
            max = new Vector3(c.x, ma.y, ma.z),
            name = level.ToString() + "u4"
        };

        node.d1 = new zOctrees()
        {
            parent = node,
            min = new Vector3(mi.x, mi.y, mi.z),
            max = new Vector3(c.x, c.y, c.z),
            name = level.ToString() + "d1"
        };

        node.d2 = new zOctrees()
        {
            parent = node,
            min = new Vector3(c.x, mi.y, mi.z),
            max = new Vector3(ma.x, c.y, c.z),
            name = level.ToString() + "d2"
        };

        node.d3 = new zOctrees()
        {
            parent = node,
            min = new Vector3(c.x, mi.y, c.z),
            max = new Vector3(ma.x, c.y, ma.z),
            name = level.ToString() + "d3"
        };

        node.d4 = new zOctrees()
        {
            parent = node,
            min = new Vector3(mi.x, mi.y, c.z),
            max = new Vector3(c.x, c.y, ma.z),
            name = level.ToString() + "d4"
        };

        level++;

        foreach(Transform t in node.objs)
        {
            if(IfContain(node.u1, t))
            {
                node.u1.objs.Add(t);
            }
            if (IfContain(node.u2, t))
            {
                node.u2.objs.Add(t);
            }
            if (IfContain(node.u3, t))
            {
                node.u3.objs.Add(t);
            }
            if (IfContain(node.u4, t))
            {
                node.u4.objs.Add(t);
            }

            if (IfContain(node.d1, t))
            {
                node.d1.objs.Add(t);
            }
            if (IfContain(node.d2, t))
            {
                node.d2.objs.Add(t);
            }
            if (IfContain(node.d3, t))
            {
                node.d3.objs.Add(t);
            }
            if (IfContain(node.d4, t))
            {
                node.d4.objs.Add(t);
            }
        }

        GenerateTree(node.u1);
        GenerateTree(node.u2);
        GenerateTree(node.u3);
        GenerateTree(node.u4);
        GenerateTree(node.d1);
        GenerateTree(node.d2);
        GenerateTree(node.d3);
        GenerateTree(node.d4);
        #endregion
    }

    bool IfContain(zOctrees node, Transform t)
    {
        bool isContain = true;
       
        
        Vector3 p = t.position;

        if (p.x > node.max.x || p.x < node.min.x)
            isContain = false;

        if (p.y > node.max.y || p.y < node.min.y)
            isContain = false;

        if (p.z > node.max.z || p.z < node.min.z)
            isContain = false;
        
        return isContain;
    }    

    public void DrawBounds(zOctrees node)
    {
        Vector3[] lineSeg = new Vector3[] {
            node.min,
            new Vector3(node.max.x, node.min.y, node.min.z),

            new Vector3(node.max.x, node.min.y, node.min.z),
            new Vector3(node.max.x, node.max.y, node.min.z),

            new Vector3(node.max.x, node.max.y, node.min.z),
            new Vector3(node.min.x, node.max.y, node.min.z),

            new Vector3(node.min.x, node.max.y, node.min.z),
            node.min,
            //
            new Vector3(node.min.x, node.min.y, node.max.z),
            new Vector3(node.max.x, node.min.y, node.max.z),

            new Vector3(node.max.x, node.min.y, node.max.z),
            new Vector3(node.max.x, node.max.y, node.max.z),

            new Vector3(node.max.x, node.max.y, node.max.z),
            new Vector3(node.min.x, node.max.y, node.max.z),

            new Vector3(node.min.x, node.max.y, node.max.z),
            new Vector3(node.min.x, node.min.y, node.max.z),
            //
            new Vector3(node.min.x, node.max.y, node.min.z),
            new Vector3(node.min.x, node.max.y, node.max.z),
            //
            new Vector3(node.max.x, node.max.y, node.min.z),
            new Vector3(node.max.x, node.max.y, node.max.z),
            //
            new Vector3(node.max.x, node.min.y, node.min.z),
            new Vector3(node.max.x, node.min.y, node.max.z),
            //
            new Vector3(node.min.x, node.min.y, node.min.z),
            new Vector3(node.min.x, node.min.y, node.max.z),
            };
        Handles.DrawLines(lineSeg);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        if (root != null)
        {
            Handles.color = Color.cyan;
            
            foreach(zOctrees node in nodeList)
            {
                if (OnlyDrawLeaf)
                {
                    if(node.IsValid)
                        DrawBounds(node);
                }
                else
                {
                    DrawBounds(node);
                }
            }
        }
    }
}
