using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.IO;
using UnityEngine.UI;
using GoogleARCore;
using DoozyUI;
public class Route
{
    Transform startPoint;
    Transform endPoint;
    float speed;
}

public class GamePlay : MonoBehaviour {

    public enum STATE
    {
        NONE,
        CountDown,
        Play,
        GameOver
    }
    public static GamePlay instance;

    public STATE state;
    public int hardness;
    private static int MAX_HARDNESS = 5;
    public TextAsset txtAsset;
    public Transform[] listSpawnPoint;
    public List<List<int>> listCase = new List<List<int>>();
    public GameObject[] car;
    public BoxCollider wrapupBoxCollider;
    public GameObject player;
    public GameObject visualClickedPoint;
    private mainCharacter playerController;
    public List<GameObject> holders = new List<GameObject>();
    private float timer = 0f;
    public float countdownTime;
    public float countdownTimer;
    private int score = 0;

    //effects :
    GameObject bloodEfx;


    #region UI
    public Text resultScore;
    public Text displayScore;
    public Text txtScore;
    public Transform panelResult;
    public Text txtDebug;
    public Text txtCountDown;
    public Text txtHighScore;
    public void OnRestartBtnClicked()
    {
        score = 0;
        timer = 0;
        countdownTimer = countdownTime;
        txtCountDown.text = countdownTimer.ToString();
        SetState(STATE.CountDown);
        panelResult.gameObject.GetComponent<UIElement>().Hide(false);
     }


#endregion

    private void OnDisable()
    {
        SetState(STATE.NONE);
        
    }

    private void Awake()
    {
        instance = this;
        countdownTimer = countdownTime;
    }
    // Use this for initialization
    void Start () {

        hardness = 0;
        LoadGameCase();
        playerController = player.GetComponent<mainCharacter>();
        visualClickedPoint.SetActive(false);
        
    }

    void LoadGameCase()
    {
        
        StringReader reader = new StringReader(txtAsset.text);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] route = line.Split('\t');
            List<int> list = new List<int>();
            foreach(string str in route)
            {
                list.Add(int.Parse(str));
            }
            listCase.Add(list);
        }
    }
	
    public void SetState(STATE st)
    {
        state = st;
        switch (st)
        {
            case STATE.NONE:
                {
                    txtCountDown.gameObject.SetActive(false);
                    timer = 0;
                    countdownTimer = countdownTime;
                    FBPoolManager.instance.returnAllObjectsToPool();
                    break;
                }
            case STATE.CountDown:
                {
                    //close effect
                    if (bloodEfx)
                        FBPoolManager.instance.returnObjectToPool(bloodEfx);

                    //UI
                    txtCountDown.gameObject.SetActive(true);
                    txtCountDown.text = countdownTimer.ToString();
                    //panelResult.gameObject.SetActive(false);

                   // StartCoroutine(StartCountDown());

                    //random player visual 
                    int rand = Random.Range(0, player.transform.childCount);
                    for(int i=0;i< player.transform.childCount;i++)
                    {
                        if (i == rand)
                        {
                            mainCharacter script = player.GetComponent<mainCharacter>();
                            
                            player.transform.GetChild(i).gameObject.SetActive(true);
                            script.animator = player.GetComponentInChildren<Animator>();

                        }
                        else
                            player.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    //sound 
                    SoundManager.instance.stopBgm();
                    SoundManager.instance.playBgm("intro1", false);
                        break;
                }
            case STATE.Play:
                {                    
                    //UI

                    txtCountDown.gameObject.SetActive(false);
                    txtScore.text = "0";
                    displayScore.text = "0";
                    txtScore.gameObject.GetComponent<UIElement>().Show(false);
                    panelResult.gameObject.SetActive(false);

                    //SHOW FULL-SCREEN CANVAS
                    player.GetComponentInChildren<Animator>().SetBool("Death_b", false);
                    player.transform.DOKill(true);

                    //sound background
                    SoundManager.instance.stopBgm();
                    SoundManager.instance.playBgm("soundBgm1", true);
                    break;
                }
            case STATE.GameOver:
                {
                    //effects 
                    //BloodSprayEffect
                    if (!bloodEfx)
                        bloodEfx = FBPoolManager.instance.getPoolObject("PlasmaExplosionEffect");
                    else
                    {
                        FBPoolManager.instance.returnObjectToPool(bloodEfx);
                        bloodEfx = FBPoolManager.instance.getPoolObject("PlasmaExplosionEffect");
                    }
                    bloodEfx.transform.position = player.transform.position;
                    //if (bloodEfx.transform.localScale.x > 10)
                    //    bloodEfx.transform.localScale *= 0.1f;

                    //if (bloodEfx.transform.localScale.x > 10)
                    //    bloodEfx.transform.localScale *= 0.1f;
                  
                    bloodEfx.transform.SetParent(player.transform);
                    bloodEfx.transform.localScale = new Vector3(1, 1, 1);
                    bloodEfx.SetActive(true);



                    hardness = 0;
                    //UI
                    txtScore.gameObject.SetActive(true);
                    
                    //SHOW GAMEOVER CANVAS
                    int highScore = PlayerPrefs.GetInt("highScore", 0);
                    if (score > highScore)
                    {
                        highScore = score;
                        PlayerPrefs.SetInt("highScore", score);
                    }
                    txtHighScore.text = "High Score: "+highScore.ToString();
                    resultScore.text =  "Score             " + score.ToString();

                    player.GetComponentInChildren<Animator>().SetBool("Death_b", true);
                    player.transform.DOKill(true);
                    Invoke("ShowResultPanel", 1f);


                    //stop audio
                    SoundManager.instance.stopBgm();
                    SoundManager.instance.playBgm("loseSfx", true);
                    break;
                }
        }
        
    }
    public void setDisplayScore() {
        displayScore.text= score.ToString();
        displayScore.gameObject.SetActive(true);

    }

    public IEnumerator StartCountDown()
    {
        while(countdownTimer > 0)
        {
            yield return new WaitForSeconds(1.0f);
            --countdownTimer;
            if (countdownTimer == 0)
            {
                SetState(STATE.Play);
               
            }
            txtCountDown.text = countdownTimer.ToString();
        }
      
        
    }

    void ShowResultPanel()
    {
        panelResult.gameObject.SetActive(true);
        panelResult.gameObject.GetComponent<UIElement>().Show(false);
    }
    // Update is called once per frame
    void Update()
    {
        
        if(state == STATE.CountDown)
        {
            timer += Time.deltaTime;
            if(timer >= 1f)
            {
                countdownTimer--;
                txtCountDown.text = countdownTimer.ToString();
                timer = 0;
            }
           
            if(countdownTimer <= 0)
            {
                SetState(STATE.Play);
            }
        }

        if (state != STATE.Play)
            return;

        float timeToSpawn = 4f - 0.2f * hardness;
        timer += Time.deltaTime;
        if (timer >= timeToSpawn)
        {
            timer = 0f;
            score++;
          
            if (score % 5 == 0)
            {
                hardness = (hardness < MAX_HARDNESS) ? hardness+1 : MAX_HARDNESS;
            }
            txtScore.text = score.ToString();
            
            txtScore.gameObject.GetComponent<UIElement>().Show(false);
            
            //setDisplayScore();
            GenerateCar();
        }

        //mouse click handel

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
   
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
               
                playerController.MoveTo(hit.point);
                visualClickedPoint.transform.position = hit.point;
                visualClickedPoint.SetActive(true);
            }
          
           
        }
#else
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = ArCore.instance.m_firstPersonCamera.ScreenPointToRay(touch.position);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
               
                playerController.MoveTo(hit.point);
                visualClickedPoint.transform.position = hit.point;
                visualClickedPoint.SetActive(true);
            }
            
           
        }
#endif

    }

    void GenerateCar()
    {
        int rand = Random.Range(0,listCase.Count-1);
        foreach(int index in listCase[rand])
        {
            Transform pos = listSpawnPoint[index];
            StartCoroutine(routeAction(pos,index));
        }
    }

    void returnObjectToPool(GameObject obj)
    {
        FBPoolManager.instance.returnObjectToPool(obj);
    }
    IEnumerator routeAction(Transform pos,int index)
    {
        bool shouldHasOpposite = false;
        Debug.Log("route action called");
        float delay = 2f - (0.2f * hardness);
        //Random.Range(7, 7)
        GameObject obj = FBPoolManager.instance.getPoolObject(car[Random.Range(0, car.Length)].gameObject.name);
        //if this isline visual opposite pos
        int oppositeInex = FBUtils.FindOppositeSpawnpoint(index);
        Transform opposite = listSpawnPoint[oppositeInex];
        if (obj.name.Contains("line"))
        {
            //display
            shouldHasOpposite = true;

            GameObject holder = FBPoolManager.instance.getPoolObject(obj.name +"_holder");
            GameObject oppositeHolder = FBPoolManager.instance.getPoolObject(obj.name + "_holder");
            holders.Add(holder);
            holders.Add(oppositeHolder);

            holder.transform.localScale = new Vector3(1, 1, 1) * ArCore.instance.arWorldScale;
            holder.transform.position = pos.position;
            holder.transform.rotation = pos.rotation;
            holder.SetActive(true);

            oppositeHolder.transform.localScale = new Vector3(1, 1, 1) * ArCore.instance.arWorldScale;
            oppositeHolder.transform.position = opposite.position;
            oppositeHolder.transform.rotation = opposite.rotation;
            oppositeHolder.SetActive(true);

            
            opposite.gameObject.SetActive(false);
            pos.gameObject.SetActive(false);
        }
        else
        {
           
     
            pos.gameObject.SetActive(true);
            opposite.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(delay);

        if (shouldHasOpposite)
        {
            //obj.transform.localScale = new Vector3(1, 1, 1) * ArCore.instance.arWorldScale;
            obj.transform.localScale = new Vector3(1, 1, 1) * ArCore.instance.arWorldScale;
            obj.transform.position = pos.position;
            DigitalRuby.LightningBolt.LightningBoltScript lineScript = obj.GetComponent<DigitalRuby.LightningBolt.LightningBoltScript>();
            lineScript.StartObject.transform.position = pos.position;
            lineScript.EndObject.transform.position = opposite.position;
            Item script = obj.addMissingComponent<Item>();
            script.setType(Item.TYPE.LINE);
            script.timetoReturn = 2f - (0.2f * hardness);
            obj.SetActive(true);

        }
        else
        {

        obj.transform.localScale = new Vector3(1,1,1) * ArCore.instance.arWorldScale;
        obj.transform.position = pos.position;
        obj.transform.rotation = pos.rotation;
        Item script = obj.addMissingComponent<Item>();
        script.setType(Item.TYPE.OBJECT);
        obj.SetActive(true);

        pos.gameObject.SetActive(false);
        opposite.gameObject.SetActive(false);
        }

    }

}
