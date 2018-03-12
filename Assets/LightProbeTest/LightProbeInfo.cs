using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using UnityEditor;
public class LightProbeInfo : MonoBehaviour {

    SphericalHarmonicsL2[] shs;
    Vector3[] poss;
    [Button(ButtonSizes.Medium)]
    void Save()
    {
        shs = LightmapSettings.lightProbes.bakedProbes;
        poss = LightmapSettings.lightProbes.positions;
        print(shs.Length + " " + poss.Length);
    }

    [Button(ButtonSizes.Medium)]
    void Swap()
    {
        for (int i = 0; i < 8; i++)
        {
            var temp = shs[i];
            shs[i] = shs[i + 8];
            shs[i + 8] = temp;
        }
        LightmapSettings.lightProbes.bakedProbes = shs;
    }

    private void OnDrawGizmos()
    {
        if(poss.Length > 0)
        {
            for (int i = 0; i < poss.Length; i++)
            {
                Handles.Label(poss[i], i.ToString());
            }
        }
    }
}
