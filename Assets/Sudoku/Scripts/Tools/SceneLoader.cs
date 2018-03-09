using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class SceneLoader : MonoBehaviour {

    public GameObject[] prefabs;

    [Button(ButtonSizes.Medium)]
    void InitScene()
    {
        var rooms = GameObject.FindObjectsOfType<SingleRoom>();
        foreach(var pre in prefabs)
        {
            var part = pre.GetComponent<roomParts>();
            if (part != null)
            {
                foreach(var room in rooms)
                {
                    if(room.roomID == part.roomID)
                    {
                        GameObject go = Instantiate(pre, room.transform.position + part.localPosition, Quaternion.identity);
                        go.transform.parent = room.transform;
                    }
                }               
            }
        }
    }
}
