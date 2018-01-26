using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class mainCharacter : MonoBehaviour {

    private Animator animator;
    public float rotationSpeed = 0.5f;
    public float speed = 0f;
    public static mainCharacter instance;

    private  bool shouldMove = false;
    private Vector3 currentTarget;
    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start () {
        animator = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
       

    }

    public void MoveTo(Vector3 target)
    {
        animator.SetFloat("Speed_f", 1f);
        transform.DOKill();
        transform.DOMove(target,(float) (Vector3.Distance(transform.position, target) / (speed * ArCore.instance.arWorldScale))).SetEase(Ease.OutFlash).OnUpdate(() =>
        {
            if (Vector3.Distance(target, transform.position) <= 0.1f  *ArCore.instance.arWorldScale)
            {
                transform.position = target;
                animator.SetFloat("Speed_f", 0f);
                transform.DOComplete();
            }
        });
       
        Vector3 lookPos = (target - transform.position).normalized;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);

        transform.DORotate(new Vector3(0,
            rotation.eulerAngles.y ,
            0), 0.5f);
      
    }
}
