using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class ParticleGenerator : MonoBehaviour
{
	public GameObject arrowPrefab;
    private Dictionary<int, List<Dictionary<string, Vector3>>> tslices = new Dictionary<int, List<Dictionary<string, Vector3>>>();
    private int highestFrame = 0;
    private int currentFrame = 0;
    public int skipFactor = 200;
    public float maxM = 0f;
    private string[] separator = new string[] { ", " };
    
    // Use this for initialization
    void Start()
    {
        ParseCSVS();
        LoadFrame(0);
    }

    // Update is called once per frame
    void Update()
    {
        int f = (int)(Time.time * 10) % (highestFrame + 10);
        if (f != currentFrame)
        {
            Debug.Log("Loading frame: " + f);
            LoadFrame(f);
            currentFrame = f;
        }
    }

    void ParseCSVS()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Debug.Log("reading csvs");
        var csvs = Resources.LoadAll("animation");
        foreach (TextAsset csvObject in csvs)
        {
            var s = csvObject.name;
            s = s.Substring(s.Length - 3);
            int ts = int.Parse(s) - 405;
            Debug.Log(ts);
            if (ts > highestFrame)
            {
                highestFrame = ts;
            }
            tslices[ts] = new List<Dictionary<string, Vector3>>();
            var csv = csvObject.text.Split('\n');
            var headers = csv[5].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            Vector3 modelOrigin = Vector3.zero;
            var count = 0;
            for (int i = 6; i < csv.Length; i += skipFactor)
            {
                count++;
                var line = csv[i];
                if (line.Length == 0) continue;
                var bits = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var d = new Dictionary<string, float>();
                for (var j = 0; j < headers.Length; j++)
                {
                    try
                    {
                        d[headers[j].Trim()] = float.Parse(bits[j]);
                    }
                    catch (FormatException)
                    {

                    }
                }
                var pos = new Vector3(d["X [ m ]"], d["Y [ m ]"], d["Z [ m ]"]);
                if (modelOrigin == Vector3.zero)
                {
                    modelOrigin = pos;
                    pos = Vector3.zero;
                } else
                {
                    pos -= modelOrigin;
                }
                var direction = new Vector3(d["Velocity u [ m s^-1 ]"], d["Velocity v [ m s^-1 ]"], d["Velocity w [ m s^-1 ]"]);
                if (direction.magnitude > maxM)
                {
                    maxM = direction.magnitude;
                }
                    tslices[ts].Add(new Dictionary<string, Vector3>() {
                    { "pos", pos },
                    { "direction", direction }
                });
            }
        }
        Debug.Log(csvs.Length + " files loaded in " + sw.Elapsed + ". Maxm: " + maxM);
    }

    void LoadFrame(int frame)
    {
        var keys = tslices.Keys.ToList();
        int a = 0;
        int b = 0;
        float factor = 0;
        if (frame < highestFrame)
        {
            for (int i = 0; i < keys.Count - 1; i++)
            {
                a = keys[i];
                b = keys[i + 1];
                if (a <= frame && b >= frame)
                {
                    break;
                }
            }
            factor = 1.0f * (frame - a) / (b - a);
        }
        else
        {
            factor = (frame - highestFrame) / 10f;
            a = highestFrame;
            b = 0;
        }
        for (int i = 0; i < tslices[a].Count; i++) {
            var pta = tslices[a][i];
            var ptb = tslices[b][i];
            var pos = Vector3.Lerp(pta["pos"], ptb["pos"], factor);
            var direction = Vector3.Lerp(pta["direction"], ptb["direction"], factor);
            var m = direction.magnitude;

            // Arrow glyph
            GameObject glyph;
            if (gameObject.transform.childCount <= i)
            {
                glyph = Instantiate(arrowPrefab);
                glyph.transform.parent = gameObject.transform;
            } else
            {
                glyph = gameObject.transform.GetChild(i).gameObject;
            }
            glyph.transform.localPosition = pos;
            glyph.transform.localRotation = Quaternion.LookRotation(direction);
            glyph.transform.localScale = Vector3.one * m;
            
            foreach (var rend in glyph.GetComponentsInChildren<Renderer>())
            {
                float h = 1 - m / maxM;
                var color = Color.HSVToRGB(h, 1, 1);
                //rend.material.color = color;
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetColor("_Color", color);
                rend.SetPropertyBlock(props);
            }
            /*
            try
            {
                var ps = glyph.GetComponent<ParticleSystem>();
                var ma = ps.main;
                ma.startSpeed = direction.magnitude * 10;
            } catch
            {
            }
            */
        }
    }
}
