using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour {

    GestureRecognizer recognizer;
    public GameObject man;
    public GameObject heart;
    public GameObject root;
    public float scaleSpeed = 1.01f;
    public float manCutoff = 20;
    public float heartCutoff = 40;
    public bool isScaling = false;
    public bool scaleUp = false;

    // Use this for initialization
    private void Awake()
    {
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            Debug.Log("tap");
            isScaling = true;
            scaleUp = !scaleUp;
        };
        recognizer.StartCapturingGestures();
    }

    private void TweakAlpha(GameObject go, float amount)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            var mat = r.material;
            var c = mat.color;
            c.a += amount;
            mat.color = c;
        }
    }

    private void Update()
    {
        if (isScaling)
        {
            if (scaleUp)
            {
                root.transform.localScale *= scaleSpeed;
                if (root.transform.localScale.magnitude < manCutoff)
                {
                    TweakAlpha(man, -0.01f);
                }
                else if (root.transform.localScale.magnitude >= manCutoff)
                {
                    man.SetActive(false);
                    TweakAlpha(heart, -0.01f);
                }
                if (root.transform.localScale.magnitude > heartCutoff)
                {
                    heart.SetActive(false);
                    isScaling = false;
                }
            }
            else
            {
                root.transform.localScale /= scaleSpeed;
                if (root.transform.localScale.magnitude > manCutoff)
                {
                    heart.SetActive(true);
                    TweakAlpha(heart, 0.01f);
                } else if (root.transform.localScale.magnitude > Vector3.one.magnitude)
                {
                    man.SetActive(true);
                    TweakAlpha(man, 0.01f);
                } else
                {
                    isScaling = false;
                }
            }
        }
    }
}
