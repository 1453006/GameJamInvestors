﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public static class FBUtils
{
	/// <summary>
	/// swap 2 objects in a list
	/// </summary>
	/// <typeparam name="T">type</typeparam>
	/// <param name="list">list</param>
	/// <param name="idx1">idx of object 1</param>
	/// <param name="idx2">idx of object 2</param>
	public static void swapObject<T>(List<T> list, int idx1, int idx2)
	{
		T obj1 = list[idx1];
		list[idx1] = list[idx2];
		list[idx2] = obj1;
	}

	/// <summary>
	/// add a component of type T and return it; if component exists, return it
	/// </summary>
	/// <typeparam name="T">type</typeparam>
	/// <param name="obj">game object</param>
	/// <returns>added component</returns>
	public static T addMissingComponent<T>(this GameObject obj) where T : Component
	{
		T t = obj.GetComponent<T>();
		if (t == null)
			t = obj.AddComponent<T>();
		return t;
	}

	/// <summary>
	/// recursively find a child with specified name
	/// </summary>
	/// <param name="parent">parent</param>
	/// <param name="name">name to find</param>
	/// <returns>child</returns>
	public static Transform findChildRecursively(this Transform parent, string name)
	{
		Transform result = parent.Find(name);
		if (result)
			return result;
		foreach(Transform child in parent)
		{
			result = findChildRecursively(child, name);
			if (result)
				return result;
		}
		return null;
	}

	/// <summary>
	/// set layer for a game object and its children
	/// </summary>
	/// <param name="obj">game object</param>
	/// <param name="layer">layer id</param>
	public static void setLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform t in obj.transform)
			t.gameObject.setLayerRecursively(layer);
	}

	/// <summary>
	/// detach and destroy all children
	/// </summary>
	/// <param name="parent">parent</param>
	public static void destroyChildren(this Transform parent)
	{
		List<GameObject> list = new List<GameObject>(parent.childCount);
		for (int i = 0; i < parent.childCount; i++)
			list.Add(parent.GetChild(i).gameObject);
		parent.DetachChildren();
		for (int i = 0; i < list.Count; i++)
			GameObject.Destroy(list[i]);
	}

	/// <summary>
	/// remove a component from game object
	/// </summary>
	/// <typeparam name="T">component to remove</typeparam>
	/// <param name="obj">game object</param>
	/// <param name="removeInChildren">remove in children too</param>
	public static void removeComponents<T>(this GameObject obj, bool removeInChildren = false)
	{
		T[] components = null;
		if (removeInChildren)
			components = obj.GetComponentsInChildren<T>();
		else
			components = obj.GetComponents<T>();
		for (int i = 0; i < components.Length; i++)
			GameObject.Destroy(components[i] as UnityEngine.Object);
	}

	/// <summary>
	/// log a string to console
	/// </summary>
	/// <param name="s">string to log</param>
	public static void log(string s)
	{
		//Debug.Log(s);
	}

    public static bool PointInOABB(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }

    #region need review

    public static string Url;
	public static string videoUrl;
	public static string playVideo_previousScene;

	public static void playVideo()
	{
		videoUrl = Url;
		playVideo_previousScene = SceneManager.GetActiveScene ().name;
		Application.LoadLevel ("Video");
	}

	public static bool isValidUrl(string url){
		string myurl = url;
		try {
			if(url.Contains("http://") || url.Contains("https://"))
				return true;
			return false;
		}       
		catch (NullReferenceException ex) {

		}
		return false;
	}

	#endregion
    #region inGAME
    public static int FindOppositeSpawnpoint(int x)
    {
        int result = -1;
        if((x>= 0 && x <= 5) || (x>=12 && x<= 17))
        {
            result = 17 - x;
        }
        else
        {
            result = 29 - x;
        }

        return result;
    }
    #endregion
}
