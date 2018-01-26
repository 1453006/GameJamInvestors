﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GoogleARCore;


public class ArCore : MonoBehaviour {

    public enum STATE
    {
        DetectingPlane,
        Play
    }

    public enum ARFeature
    {
        NONE,               // no AR
        FAKE,               // fake camera
        REAL                // real AR
    }

    public ARFeature feature;
    public GameObject prefab;
    public Vector3 touchedPoint = Vector3.zero;
    public static ArCore instance;
    public STATE state;
    public GameObject wrapupGame;
    public GameObject fakePlane;
    public Camera freeCam;
    public Camera m_firstPersonCamera;
    public Transform anchor;

    public float arWorldScale ;

    

    private void Awake()
    {
        instance = this;
    }

    void Start () {
        state = STATE.DetectingPlane;
        wrapupGame.SetActive(false);
        
        switch(feature)
        {
            case ARFeature.REAL:
                {
                    fakePlane.SetActive(false);
                    freeCam.gameObject.SetActive(false);
                    m_firstPersonCamera.gameObject.SetActive(true);
                    Camera.SetupCurrent(m_firstPersonCamera);
                    

                }
                break;
            case ARFeature.FAKE:
                {
                    fakePlane.SetActive(true);
                    freeCam.gameObject.SetActive(true);
                    m_firstPersonCamera.gameObject.SetActive(false);
                    Camera.SetupCurrent(freeCam);
                }
                break;
                
        }
    }
	

	void Update () {

        switch (feature)
        {
            case ARFeature.NONE:
                break;
            case ARFeature.FAKE:
                UpdateFakeAR();
                break;
            case ARFeature.REAL:
                UpdateReal();
                break;

        }

	}

#region Fake
    void UpdateFakeAR()
    {

        if (state != STATE.DetectingPlane)
        {
            return;
        }

#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
            //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
            if (Physics.Raycast(ray, out hit, float.MaxValue))//, LayerMask.NameToLayer("ARPlane")))
            {
                Debug.Log(hit.point);
                //Instantiate(prefab, hit.point, Quaternion.identity);
                prefab.transform.position = hit.point;
                touchedPoint = hit.point;
                switch(state)
                {
                    case STATE.DetectingPlane:
                        {
                            anchor.transform.position = hit.point;
                            wrapupGame.transform.SetParent(anchor);
                            wrapupGame.transform.localPosition = Vector3.zero;

                            wrapupGame.transform.localScale *= arWorldScale;
                            wrapupGame.SetActive(true);
                            float scaleY = wrapupGame.transform.localScale.y;
                            Sequence sequence = DOTween.Sequence();
                            sequence.Append(wrapupGame.transform.DOScaleY(scaleY * 2, 0.25f).SetEase(Ease.OutSine));
                            sequence.Append(wrapupGame.transform.DOScaleY(scaleY, 0.5f).SetEase(Ease.OutBounce));

                            fakePlane.SetActive(false);
                            
                            GamePlay.instance.SetState(GamePlay.STATE.CountDown);
                            state = STATE.Play;
                            break;
                        }
                    
                }
                
            }
        }
#endif

    }
    #endregion


#region Real
    private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

    private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();
    private List<GameObject> visualPlanes = new List<GameObject>();
    public GameObject m_trackedPlanePrefab;
  

    void UpdateReal()
    {
        if (state != STATE.DetectingPlane)
        {
            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

       
        Frame.GetNewPlanes(ref m_newPlanes);
        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        for (int i = 0; i < m_newPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                transform);
            visualPlanes.Add(planeObject);
            planeObject.GetComponent<GoogleARCore.HelloAR.TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);

            // Apply a random color and grid rotation.
            planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", new Color(0f, 0.588f, 0.533f));
            planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
        }


        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;
        
        if (Session.Raycast(m_firstPersonCamera.ScreenPointToRay(touch.position), raycastFilter, out hit))
        {
            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = Session.CreateAnchor(hit.Point, Quaternion.identity);

            switch (state)
            {
                case STATE.DetectingPlane:
                    {
                        wrapupGame.transform.position = hit.Point;
                        wrapupGame.transform.SetParent(anchor.transform);
                        //wrapupGame.transform.localPosition = Vector3.zero;
                        wrapupGame.transform.localScale *= arWorldScale;
                        wrapupGame.SetActive(true);
                        float scaleY = wrapupGame.transform.localScale.y;
                        Sequence sequence = DOTween.Sequence();
                        sequence.Append(wrapupGame.transform.DOScaleY(scaleY * 2, 0.25f).SetEase(Ease.OutSine));
                        sequence.Append(wrapupGame.transform.DOScaleY(scaleY, 0.5f).SetEase(Ease.OutBounce));
                        
                        
                        foreach (GameObject obj in visualPlanes)
                            obj.SetActive(false);

                        GamePlay.instance.SetState(GamePlay.STATE.CountDown);
                        state = STATE.Play;
                        break;
                    }
             
            }

        }



    }

#endregion


}
