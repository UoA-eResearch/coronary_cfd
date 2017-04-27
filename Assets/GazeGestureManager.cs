using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour {

    GestureRecognizer recognizer;
    public GameObject man;
    public GameObject root;
    public float scaleSpeed = 1.01f;
    public bool isScaling = false;

    // Use this for initialization
    private void Awake()
    {
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            Debug.Log("tap");
            isScaling = true;
        };
        recognizer.StartCapturingGestures();
    }

    private void Update()
    {
        if (isScaling)
        {
            root.transform.localScale *= scaleSpeed;
            if (root.transform.localScale.magnitude > 10)
            {
                isScaling = false;
                man.SetActive(false);
            }
        }
    }
}
