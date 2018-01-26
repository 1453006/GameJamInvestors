using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour {

    float speed = 2.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float correctSpeed = speed + 1.5f * GamePlay.instance.hardness;
        transform.Translate(-Vector3.forward * correctSpeed * ArCore.instance.arWorldScale* Time.deltaTime);
        if(!FBUtils.PointInOABB(transform.position,GamePlay.instance.wrapupBoxCollider))
        {
            FBPoolManager.instance.returnObjectToPool(this.gameObject);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
          
            GamePlay.instance.SetState(GamePlay.STATE.GameOver);
        }
    }

   
}
