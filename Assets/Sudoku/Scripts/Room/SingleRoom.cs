using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SingleRoom : MonoBehaviour {

    public int roomID = 100;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Handles.Label(transform.position, roomID.ToString());
    }

    /// <summary>
    /// 房间是否包含传入的位置
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool Contains(Vector3 pos)
    {
        Vector3 offset = transform.position - pos;
        Vector3 size = transform.lossyScale / 2f;
        if (offset.x < size.x && offset.x > -size.x
            && offset.y < size.y && offset.y > -size.y
            && offset.z < size.z && offset.z > -size.z)
            return true;
        else
            return false;
    }
}
