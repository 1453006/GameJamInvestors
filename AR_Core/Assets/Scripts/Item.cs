using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour {

    public enum TYPE
    {
        OBJECT,
        LINE
    }
    float speed = 2.0f;
    TYPE type;
    public float timetoReturn;

  
    public void setType(TYPE set)
    {
        type = set;
        switch (type)
        {
            case TYPE.OBJECT:
                {

                }
                break;
            case TYPE.LINE:
                {

                }
                break;
            default:
                break;
        }
    }
	// Use this for initialization
	void Start () {
		
	}

  
    private void OnEnable()
    {
        if(type == TYPE.LINE)
        {
            BoxCollider collider = gameObject.addMissingComponent<BoxCollider>();
            Transform start = transform.GetChild(0);
            Transform end = transform.GetChild(1);

            //boxcollider
            Vector3 size = new Vector3();
            size.x = Mathf.Abs(start.localPosition.x - end.localPosition.x) * 2 > 0 ? Mathf.Abs(start.localPosition.x - end.localPosition.x) * 2 : 1;
            size.y = Mathf.Abs(start.localPosition.y - end.localPosition.y) * 2 > 0 ? Mathf.Abs(start.localPosition.y - end.localPosition.y) * 2 : 1;
            size.z = Mathf.Abs(start.localPosition.z - end.localPosition.z) * 2 > 0 ? Mathf.Abs(start.localPosition.z - end.localPosition.z) * 2 : 1;

            collider.size = size;
            collider.isTrigger = true;

            //rigidbody
            Rigidbody rb = gameObject.addMissingComponent<Rigidbody>();
            rb.isKinematic = true;

            //return this after n sec
            Invoke("ReturnObject", timetoReturn);
        }
    }

    void ReturnObject()
    {
        FBPoolManager.instance.returnObjectToPool(this.gameObject);
        foreach(GameObject go in GamePlay.instance.holders)
        {
            FBPoolManager.instance.returnObjectToPool(go);
        }
        //GamePlay.instance.holders.Clear();


    }
    // Update is called once per frame
    void Update () {
        if (type == TYPE.OBJECT)
        {
            float correctSpeed = speed + 1.5f * GamePlay.instance.hardness;
            transform.Translate(-Vector3.forward * correctSpeed * ArCore.instance.arWorldScale * Time.deltaTime);
            if (!FBUtils.PointInOABB(transform.position, GamePlay.instance.wrapupBoxCollider))
            {
                FBPoolManager.instance.returnObjectToPool(this.gameObject);
            }
        }
        else if(type == TYPE.LINE)
        {
            //do nothing
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GamePlay.instance.SetState(GamePlay.STATE.GameOver);
        }
    }

   
}
