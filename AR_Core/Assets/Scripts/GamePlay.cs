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
            case STATE.CountDown:
                {
                    //close effect
                    if (bloodEfx)
                        FBPoolManager.instance.returnObjectToPool(bloodEfx);

                    //UI
                    txtCountDown.gameObject.SetActive(true);

                    //panelResult.gameObject.SetActive(false);

                    StartCoroutine(StartCountDown());

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
                    player.GetComponent<Animator>().SetBool("Death_b", false);
                    player.transform.DOKill(true);

                    //sound background
                    SoundManager.instance.stopBgm();
                    SoundManager.instance.playBgm("soundBgm1", true);
                    break;
                }
            case STATE.GameOver:
                {
                    //effects 
                    
                    if (!bloodEfx)
                        bloodEfx = FBPoolManager.instance.getPoolObject("BloodSprayEffect");
                    else
                    {
                        FBPoolManager.instance.returnObjectToPool(bloodEfx);
                        bloodEfx = FBPoolManager.instance.getPoolObject("BloodSprayEffect");
                    }
                    if (bloodEfx.transform.localScale.x > 10)
                        bloodEfx.transform.localScale *= 0.1f;

                    if (bloodEfx.transform.localScale.x > 10)
                        bloodEfx.transform.localScale *= 0.1f;

                    bloodEfx.SetActive(true);
                    bloodEfx.transform.SetParent(player.transform);
                    bloodEfx.transform.position = player.transform.position;
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

                    player.GetComponent<Animator>().SetBool("Death_b", true);
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
        if (state != STATE.Play)
            return;

        float timeToSpawn = 4f - 0.2f * hardness;
        timer += Time.deltaTime;
        if (timer >= timeToSpawn)
        {
            timer = 0f;
            score++;
            if(score % 5 == 0)
            {
                hardness = (hardness < MAX_HARDNESS) ? hardness+1 : MAX_HARDNESS;
            }
            txtScore.text = score.ToString();
            txtScore.gameObject.GetComponent<UIElement>().Show(false);
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
            Transform pos = listSpawnPoint[index-1];
            StartCoroutine(routeAction(pos));
        }
    }

    IEnumerator routeAction(Transform pos)
    {
        Debug.Log("route action called");
        pos.gameObject.SetActive(true);
        float delay = 2f - (0.2f * hardness);
        yield return new WaitForSeconds(delay);
        pos.gameObject.SetActive(false);
        GameObject obj = FBPoolManager.instance.getPoolObject(FBPoolManager.instance.itemsToPool[Random.Range(0,6)].name);
       
        obj.transform.localScale = new Vector3(1,1,1) * ArCore.instance.arWorldScale;
        obj.transform.position = pos.position;
        obj.transform.rotation = pos.rotation;
        obj.addMissingComponent<Item>();
        obj.SetActive(true);
        
    }

}

