using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using UnityEditor;
using SimpleLightProbePlacer;
public class LightProbeInfo : MonoBehaviour {

    SphericalHarmonicsL2[] shs;
    Vector3[] poss;
    [Button(ButtonSizes.Medium)]
    void Save()
    {
        shs = LightmapSettings.lightProbes.bakedProbes;
        poss = LightmapSettings.lightProbes.positions;
        print(shs.Length + " " + poss.Length);

        SHStrorage[] storages = GameObject.FindObjectsOfType<SHStrorage>();
        for (int i = 0; i < poss.Length; i++)
        {
            foreach (var s in storages)
            {
                LightProbeVolume vol = s.GetComponentInChildren<LightProbeVolume>();
                if(vol != null)
                {
                    if(vol.GetBounds().Contains(poss[i]))
                    {
                        s.sh_Indexs.Add(i);
                        s.sh_datas.Add(shs[i]);
                        break;
                    }
                }
            }
        }
    }

    [Button(ButtonSizes.Medium)]
    void Swap()
    {
        SHStrorage[] storages = GameObject.FindObjectsOfType<SHStrorage>();
        Vector3 temp = storages[0].transform.position;
        storages[0].transform.position = storages[1].transform.position;
        storages[1].transform.position = temp;
        if (storages[0].sh_Indexs.Count != storages[1].sh_Indexs.Count)
        {
            Debug.LogError("sh count not equal");
            return;
        }

        int stepLength = storages[0].sh_datas.Count;
        List<SphericalHarmonicsL2> new_shs = new List<SphericalHarmonicsL2>(shs.Length);
        foreach (var s in storages)
        {
            s.Inserted = false;
        }

        for (int i = 0; i < poss.Length; i++)
        {
            foreach (var s in storages)
            {
                if (s.Inserted)
                    continue;

                LightProbeVolume vol = s.GetComponentInChildren<LightProbeVolume>();
                if (vol != null)
                {
                    if (vol.GetBounds().Contains(poss[i]))
                    {
                        int offset = Mathf.FloorToInt((float)i / (float)s.sh_Indexs.Count);
                        new_shs.InsertRange(offset * stepLength, s.sh_datas);
                        s.Inserted = true;
                    }
                }
            }
        }

            //if (storages[0].sh_Indexs.Count == storages[1].sh_Indexs.Count)
            //{
            //    for (int i = 0; i < storages[0].sh_Indexs.Count; i++)
            //    {
            //        var temp = shs[storages[0].sh_Indexs[i]];
            //        shs[storages[0].sh_Indexs[i]] = shs[storages[1].sh_Indexs[i]];
            //        shs[storages[1].sh_Indexs[i]] = temp;
            //    }
            //}
            //else
            //{
            //    Debug.LogError("sh count not equal");
            //}
            //for (int i = 0; i < 8; i++)
            //{
            //    var temp = shs[i];
            //    shs[i] = shs[i + 8];
            //    shs[i + 8] = temp;
            //}
            LightmapSettings.lightProbes.bakedProbes = new_shs.ToArray();
    }

    [Button(ButtonSizes.Medium)]
    void ClearStorage()
    {
        SHStrorage[] storages = GameObject.FindObjectsOfType<SHStrorage>();
        foreach (var s in storages)
            s.sh_Indexs.Clear();
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
