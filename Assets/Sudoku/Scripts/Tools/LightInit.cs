using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 测试用
/// </summary>
[ExecuteInEditMode]
public class LightInit : MonoBehaviour {

    public Transform root;
    public GameObject[] lights;
    public int count = 10;
	[Button(ButtonSizes.Medium)]
    void RandomLight()
    {
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, lights.Length);
            Vector3 randomPos = Random.insideUnitSphere * 5f;
            randomPos.y = 1.5f;
            var go = Instantiate(lights[index], randomPos, Quaternion.identity);
            go.transform.parent = root;
            go.isStatic = true;
        }       
    }
}
