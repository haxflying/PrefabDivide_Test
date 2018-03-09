using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zOctrees  {

    [SerializeField]
    public string name;
    public Vector3 min, max;
    [SerializeField]
    public zOctrees u1, u2, u3, u4, d1, d2, d3, d4;
    [HideInInspector]
    public List<Transform> objs = new List<Transform>();
    [SerializeField]
    public zOctrees parent;

    public Vector3 center
    {
        get
        {
            return (min + max) * 0.5f;
        }
    }

    public bool IsLeaf;
    
    public bool IsValid
    {
        get
        {
            return IsLeaf && objs.Count > 0;
        }
    }
}
