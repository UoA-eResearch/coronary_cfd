using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class ParticleGenerator : MonoBehaviour
{
	public GameObject particleSystemPrefab;
	public GameObject arrowPrefab;
	string[] separator = new string[] { ", " };

	// Use this for initialization
	void Start()
	{
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();
		Debug.Log("reading csv");
		var csvObject = Resources.Load("export-new") as TextAsset;
		var csv = csvObject.text.Split('\n');
		var headers = csv[5].Split(separator, StringSplitOptions.RemoveEmptyEntries);
		Vector3 modelOrigin = Vector3.zero;
		var count = 0;
		float maxM = 0;
		for (int i = 6; i < csv.Length; i+=50)
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
				} catch (FormatException)
				{
					
				}
			}
			var pos = new Vector3(d["X [ m ]"], d["Y [ m ]"], d["Z [ m ]"]);
			var direction = new Vector3(d["Velocity u [ m s^-1 ]"], d["Velocity v [ m s^-1 ]"], d["Velocity w [ m s^-1 ]"]);
			
			if (i == 6)
			{
				modelOrigin = pos;
			}

			// Arrow glyph
			var glyph = Instantiate(arrowPrefab);
			glyph.transform.parent = gameObject.transform;
			glyph.transform.localPosition = pos - modelOrigin;
			glyph.transform.LookAt(glyph.transform.TransformPoint(direction));
			glyph.transform.localScale = Vector3.one * direction.magnitude;
			float m = direction.magnitude;
			if (m > maxM) maxM = m;
			foreach (var rend in glyph.GetComponentsInChildren<Renderer>())
			{
				var color = Color.HSVToRGB(1 - m / .5f, 1, 1);
				//rend.material.color = color;
				MaterialPropertyBlock props = new MaterialPropertyBlock();
				props.SetColor("_Color", color);
				rend.SetPropertyBlock(props);
			}

			// Particle system
			/*
			var p = Instantiate(particleSystemPrefab);
			p.transform.parent = gameObject.transform;
			p.transform.localPosition = pos;
			p.transform.LookAt(p.transform.TransformPoint(direction));
			var ps = p.GetComponent<ParticleSystem>();
			var ma = ps.main;
			ma.startSpeed = direction.magnitude * 10;
			*/
		}
		Debug.Log("plotted " + count + " data points in  " + sw.Elapsed + " - max M: " + maxM);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
