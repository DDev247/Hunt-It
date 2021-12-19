using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary> This script is the only one in the project! It controlls everything. </summary>
public class OSJ : MonoBehaviour
{
    public bool IsMenu;
    public bool IsMenu2;
    public bool LastScene;

    [Header("Menu.cs section")]

    // Public fields
    public Slider volSlider;
    public AudioMixer mixer;
    public TMPro.TextMeshProUGUI VolText;
    public AudioClip Music;
    public AudioMixerGroup mixerGroup;
    public TMPro.TextMeshProUGUI FunFact;
    public string[] facts;
    public TMPro.TextMeshProUGUI sensText;
    public Slider sensSlider;
    public TMPro.TextMeshProUGUI verText;

    // Private fields
    private bool canGoBack;

    public void QuitGame()
    {
        PlayerPrefs.Save();
        StartCoroutine(Quit());
    }

    public void PlayGame()
    {
        PlayerPrefs.Save();
        Fader.SetTrigger("Fade Out");
        StartCoroutine(Fading());
    }

    public void LoadSettings()
    {
        sensSlider.value = PlayerPrefs.GetFloat("gunsens");
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    IEnumerator Quit()
    {
        int i = 0;

        GameObject[] obj = GameObject.FindGameObjectsWithTag("music");
        Fader.SetTrigger("Fade Quit");
        while(i < 60)
        {
            obj[0].GetComponent<AudioSource>().volume -= 1.0f / 60.0f;
            yield return new WaitForFixedUpdate();
            i++;
        }

        i = 0;
        while(i < 30)
        {
            yield return new WaitForFixedUpdate();
            i++;
        }
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    IEnumerator FunnyText()
    {
        int i = 0;

        while(i < 250)
        {
            yield return new WaitForFixedUpdate();
            i++;
        }

        SceneManager.LoadScene(1);
    }

    [Header("Main.cs section")]
    public GameObject Player;
    public GameObject Camera;
    public float maxCameraDelta;

    public bool hasFailed;
    public bool hasWon;

    public GameObject FailMenu;
    public GameObject WinMenu;
    public GameObject PauseMenu;

    public int EnemiesLeft;
    public float Health;
    public Animator HealthOverlay;
    public Animator Fader;
    public TMPro.TextMeshProUGUI Text;
    public TMPro.TextMeshProUGUI PressText;
    public bool hasPressed;

    private float healthTime;
    public float regenTime;

    public bool isPaused;
    public bool isSlowed;

    // Start is called before the first frame update
    void Start()
    {
        if(!IsMenu && !IsMenu2)
        {
            if(!hasFailed && !hasWon)
            {
                movStart();
                wepStart();
                aiStart();
            }
        }
        else if(!IsMenu2)
        {
            // Do menus
            verText.SetText("v" + Application.version);
            if(!PlayerPrefs.HasKey("gunsens"))
            {
                PlayerPrefs.SetFloat("gunsens", 150);
                PlayerPrefs.Save();
            }
            FunFact.SetText(facts[Random.Range(0, facts.Length - 1)]);
        }
        else
        {
            GameObject obj = new GameObject("Music");
            obj.tag = "music";
            AudioSource a = obj.AddComponent<AudioSource>();
            a.clip = Music;
            a.loop = true;
            a.outputAudioMixerGroup = mixerGroup;
            a.Play();
            DontDestroyOnLoad(obj);
            StartCoroutine(FunnyText());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsMenu && !IsMenu2)
        {
            rotTarget.transform.position = Camera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));

            if(!isPaused)
            {
                if(Input.GetButtonDown("Pause"))
                {
                    isPaused = true;
                    canGoBack = false;
                }
                PauseMenu.SetActive(false);
                HealthOverlay.SetFloat("Health", Health);
                Text.SetText("health:" + Mathf.Round(Health) + "\n" + "left:" + EnemiesLeft);

                if(Input.GetButtonDown("SlowMotion"))
                {
                    if(isSlowed)
                    {
                        isSlowed = false;
                    }
                    else
                    {
                        isSlowed = true;
                    }
                }

                if(isSlowed)
                {
                    Vector2 curTime = new Vector2(0.0f, Time.timeScale);
                    Vector2 wanted = new Vector2(0.0f, 0.35f);
                    float maxDelta = 5.0f * Time.deltaTime; 
                    Time.timeScale = Vector2.MoveTowards(curTime, wanted, maxDelta).y;
                }
                else
                {
                    Vector2 curTime = new Vector2(0.0f, Time.timeScale);
                    Vector2 wanted = new Vector2(0.0f, 1f);
                    float maxDelta = 5.0f * Time.deltaTime; 
                    Time.timeScale = Vector2.MoveTowards(curTime, wanted, maxDelta).y;
                }

                if(Health > 100)
                {            
                    Health = 100f;
                }

                if(Health <= 0)
                {
                    DeclareLoser();
                }

                if(EnemiesLeft == 0)
                {
                    DeclareWinner();
                }

                if(hasFailed)
                {
                    if(!hasPressed && Input.GetButtonDown("Submit"))
                    {
                        hasPressed = true;
                        Fader.SetTrigger("Fade Out");
                        StartCoroutine(Fading2());
                    }
                    FailMenu.SetActive(true);
                }
                else
                {
                    FailMenu.SetActive(false);
                }

                if(hasWon && !hasFailed)
                {
                    if(LastScene)
                    {
                        PressText.SetText("Press Enter to go to the menu");
                    }
                    else
                    {
                        PressText.SetText("Press Enter to continue");
                    }
                    if(!hasPressed && Input.GetButtonDown("Submit"))
                    {
                        hasPressed = true;
                        Fader.SetTrigger("Fade Out");
                        StartCoroutine(Fading());
                    }
                    WinMenu.SetActive(true);
                }
                else
                {
                    WinMenu.SetActive(false);
                }

                if(!hasFailed && !hasWon)
                {
                    movUpdate();
                    wepUpdate();
                    aiUpdate();
                }
            }
            else
            {
                PauseMenu.SetActive(true);

                if(Input.GetButtonDown("Pause") && canGoBack)
                {
                    isPaused = false;
                    canGoBack = false;
                }
                if(Input.GetButtonDown("Submit"))
                {
                    Fader.SetTrigger("Fade Out");
                    StartCoroutine(Fading2());
                }
                if(Input.GetButtonDown("GoBack"))
                {
                    Fader.SetTrigger("Fade Out");
                    LastScene = true;
                    StartCoroutine(Fading());
                }
                canGoBack = true;
            }
        }
        else if(!IsMenu2)
        {
            // Do menus
            mixer.SetFloat("Volume", volSlider.value);
            VolText.SetText("Volume:\n" + volSlider.value);

            PlayerPrefs.SetFloat("gunsens", sensSlider.value);
            sensText.SetText("Sensitivity:" + PlayerPrefs.GetFloat("gunsens"));
        }
        else
        {
            
        }
    }

    void FixedUpdate()
    {
        if(!IsMenu && !IsMenu2)
        {
            if(!isPaused)
            {
                healthTime -= Time.fixedDeltaTime;
                if(healthTime < 0)
                {
                    if(Health < 100)
                    {            
                        Health += 0.5f;
                    }
                }
                if(!hasFailed && !hasWon)
                {
                    movFixed();
                    wepFixed();
                    aiFixed();
                }
            }
        }
        else if(!IsMenu2)
        {
            // Do menus
        }
        else
        {

        }
    }

    void DeclareWinner()
    {
        hasWon = true;

        // Do win stuff
    }

    void DeclareLoser()
    {
        hasFailed = true;

        // Do fail stuff
    }

    IEnumerator Fading()
    {
        int i = 0;

        while(i < 60)
        {
            yield return new WaitForFixedUpdate();
            i++;
        }

        if(!LastScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    IEnumerator Fading2()
    {
        int i = 0;

        while(i < 60)
        {
            yield return new WaitForFixedUpdate();
            i++;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [Header("Movement.cs section")]

    // Public fields
    public float MovSpeed;
    public Animator PlayerAnim;

    // Private fields
    private float sideInput;
    private float vertInput;

    private Rigidbody2D PlayerRB;

    Vector2 move;

    void movStart()
    {
        PlayerRB = Player.GetComponent<Rigidbody2D>();
    }

    void movUpdate()
    {
        sideInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
        move.x = sideInput;
        move.y = vertInput;
        if(move.x == 1 && move.y == 1)
        {
            move.x = 0.75f;
            move.y = 0.75f;
        }
        else if(move.x == -1 && move.y == -1)
        {
            move.x = -0.75f;
            move.y = -0.75f;
        }
        else if(move.x == 1 && move.y == -1)
        {
            move.x = 0.75f;
            move.y = -0.75f;
        }
        else if(move.x == -1 && move.y == 1)
        {
            move.x = -0.75f;
            move.y = 0.75f;
        }

        if(Input.GetButton("up"))
        {
            PlayerAnim.speed = 1;
            PlayerAnim.SetFloat("Dir", 1);
        }
        else if(Input.GetButton("down"))
        {
            PlayerAnim.speed = 1;
            PlayerAnim.SetFloat("Dir", 2);
        }
        else if(Input.GetButton("left"))
        {
            PlayerAnim.speed = 1;
            PlayerAnim.SetFloat("Dir", 3);
        }
        else if(Input.GetButton("right"))
        {
            PlayerAnim.speed = 1;
            PlayerAnim.SetFloat("Dir", 4);
        }
        else
        {
            //PlayerAnim.SetFloat("Dir", 0);
            PlayerAnim.speed = 0;
        }
    }

    void movFixed()
    {
        PlayerRB.MovePosition(PlayerRB.position + move * MovSpeed * Time.fixedDeltaTime);
        Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, Player.transform.position, maxCameraDelta * Time.fixedDeltaTime);
        Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, 0);
    }

    [Header("Weapons.cs section")]

    // Public fields
    public float timeLeftGun;
    public float timeLeftAxe;

    public GameObject WeaponParent;
    public GameObject[] Weapons;
    public GameObject[] WeaponOrigin;
    public int WeaponSelection;

    public float maxWepRotDelta;

    public GameObject WeaponFlash;
    public GameObject ImpactEffect;
    public GameObject BloodEffect;

    public float gunSens;

    public AudioSource ShootSound;

    public bool mouseCtrls;

    // Private fields
    private float timeGun = 0.15f;
    private float timeAxe = 0.5f;
    private float gunRot;
    private GameObject rotTarget;

    void wepStart()
    {
        timeLeftGun = 0;
        timeLeftAxe = 0;

        gunSens = PlayerPrefs.GetFloat("gunsens");
        rotTarget = new GameObject("mouseTarget"); 
    }

    void wepUpdate()
    {
        if(!mouseCtrls)
        {
            gunRot -= Input.GetAxisRaw("Aim") * gunSens * Time.deltaTime;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            gunRot -= Input.GetAxisRaw("Mouse X") * gunSens * Time.deltaTime;
        }

        float scroller = 0;
        if(Input.GetButtonDown("Switch Up"))
        { scroller = 1; }
        else if(Input.GetButtonDown("Switch Down"))
        { scroller = -1; }
        else
        { scroller = 0; }

        if(scroller == 1)
        {
            if(WeaponSelection < 1)
            {
                WeaponSelection += 1;
            }
            else
            {
                WeaponSelection = 0;
            }
        }
        else if(scroller == -1)
        {
            if(WeaponSelection > 0)
            {
                WeaponSelection -= 1;
            }
            else
            {
                WeaponSelection = 1;
            }
        }

        if(Input.GetButtonDown("Fire") && timeLeftGun < 0)
        {
            timeLeftGun = timeGun;
            ShootSound.Play();
            RaycastHit2D hit = Physics2D.Raycast(WeaponOrigin[WeaponSelection].transform.position, WeaponOrigin[WeaponSelection].transform.up);
            Weapons[WeaponSelection].GetComponent<Animator>().SetTrigger("Shoot");

            GameObject flash = Instantiate(WeaponFlash, WeaponOrigin[WeaponSelection].transform.position, WeaponOrigin[WeaponSelection].transform.rotation);
            Destroy(flash, 0.5f);
            if(hit)
            {
                Debug.Log(hit.transform.gameObject.name);
                if(hit.transform.tag == "enemy") { Destroy(hit.transform.gameObject, 0.1f); hit.transform.tag = "Untagged"; hit.transform.GetChild(2).transform.GetChild(Random.Range(0, 1)).GetComponent<AudioSource>().Play(); EnemiesLeft--; healthTime = regenTime; Health -= Random.Range(7.0f, 14.0f); GameObject blood = Instantiate(BloodEffect, hit.point, Quaternion.identity); blood.transform.position = new Vector3(blood.transform.position.x, blood.transform.position.y, 1); Destroy(blood, 1f); Camera.GetComponent<Animator>().SetTrigger("Kill"); }
                if(hit.transform.tag == "powerup") { Destroy(hit.transform.gameObject, 1f); hit.transform.tag = "Untagged"; hit.transform.GetComponent<Animator>().SetTrigger("OnCollect"); hit.transform.GetChild(2).transform.GetChild(Random.Range(0, 1)).GetComponent<AudioSource>().Play(); Health += Random.Range(14.0f, 28.0f); }
                GameObject impact = Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                Destroy(impact, 0.5f);
            }
        }
    }

    void wepFixed()
    {
        timeLeftGun -= Time.fixedDeltaTime;
        timeLeftAxe -= Time.fixedDeltaTime;

        //Vector3 diff = rotTarget.transform.position - WeaponParent.transform.position;
        //float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //WeaponParent.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotZ);

        //WeaponParent.transform.LookAt(rotTarget.transform, Vector3.forward);
        //WeaponParent.transform.eulerAngles = new Vector3(0, 0, -WeaponParent.transform.eulerAngles.z);

        WeaponParent.transform.eulerAngles = new Vector3(0, 0, gunRot);
    }

    [Header("AI.cs section")]

    // Public fields
    public GameObject[] AiObjects;
    public AIMovementInfo[] infos;

    // Private fields


    void aiStart()
    {
        foreach(GameObject g in AiObjects)
        {
            g.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Random.ColorHSV(0.0f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f);
        }

        int i = 0;
        List<AIMovementInfo> list = new List<AIMovementInfo>();
        while(i < AiObjects.Length)
        {
            list.Add(new AIMovementInfo());
            i++;
        }
        infos = list.ToArray();

        int count = 0;
        foreach(AIMovementInfo info in infos)
        {
            info.attached = AiObjects[count];
            info.attachedRB = AiObjects[count].GetComponent<Rigidbody2D>();
            count++;
        }
    }

    void aiUpdate()
    {
        foreach(AIMovementInfo info in infos)
        {
            if(info.time == 0)
            {
                //Decide or wait
                int rand1 = Random.Range(0, 1);
                if(rand1 == 0)
                {
                    //Decide
                    int randDecide = Random.Range(1, 4);
                    float time = Random.Range(0.5f, 1.0f);

                    info.time = time;
                    info.type = randDecide;
                }
                else 
                {
                    //Apply time to info
                    info.time = Random.Range(0.5f, 1.5f);
                    info.type = 0;
                }
            }
        }
    }

    void aiFixed()
    {
        foreach(AIMovementInfo info in infos)
        {
            info.updt();
        }
    }
}

public class AIMovementInfo
{
    public GameObject attached;
    public Rigidbody2D attachedRB;
    public float time = 0;
    public int type = 0;

    public Vector2 move;
    public float MoveSpeed = 2.5f;

    public void updt()
    {
        if(attached != null)
        {
            time -= Time.fixedDeltaTime;
            if(time < 0)
            {
                time = 0;
            }
            aiSetAnim();
            aiMovement();
        }
    }

    void aiMovement()
    {
        if(type == 1) { move.x = 0; move.y = 1; }
        if(type == 2) { move.x = 0; move.y = -1; }
        if(type == 3) { move.x = -1; move.y = 0; }
        if(type == 4) { move.x = 1; move.y = 0; }
        
        attachedRB.MovePosition(attachedRB.position + move * MoveSpeed * Time.fixedDeltaTime);

    }

    void aiSetAnim()
    {
        Animator anim = attached.GetComponent<Animator>();

        if(type == 1)
        {
            anim.speed = 1;
            anim.SetFloat("Dir", 1);
        }
        else if(type == 2)
        {
            anim.speed = 1;
            anim.SetFloat("Dir", 2);
        }
        else if(type == 3)
        {
            anim.speed = 1;
            anim.SetFloat("Dir", 3);
        }
        else if(type == 4)
        {
            anim.speed = 1;
            anim.SetFloat("Dir", 4);
        }
        else if(type == 0)
        {
            anim.speed = 0;
        }
    }
}
