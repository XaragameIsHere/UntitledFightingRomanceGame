using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;




public class enemyScripting : MonoBehaviour
{
    ParticleSystem.NoiseModule noice;
    ParticleSystem.ShapeModule shape;
    ParticleSystem.MainModule main;
    ParticleSystem.ForceOverLifetimeModule forceOverLifetime;
    ParticleSystem.EmissionModule emission;
    ParticleSystem.SubEmittersModule explody;
    [SerializeField] GameObject fallingObjectArea;
    [SerializeField] playerMovement playerScript;
    [SerializeField] playerUIController Controller;
    [SerializeField] AudioClip FightMusic;
    [SerializeField] AudioClip dialogueMusic;
    [SerializeField] LayerMask bulletLayer;
    [SerializeField] Canvas death;
    [SerializeField] GameObject bigGuySpawn;
    [SerializeField] GameObject bigGuy;
    [SerializeField] Vector3 playerPostion;
    [SerializeField] Vector3 enemyPosition;
    [SerializeField] tutorialScripting tutorial;
    [SerializeField] GameObject tutorialKey;
    [SerializeField] GameObject eKey;
    [HideInInspector] public dialogueParsing.Dialogue dialogueRoot;
    [HideInInspector] public bool hit = false;
    public GameObject enemyObject;
    [ColorUsage(true, true)]
    public Color someHDRColor;
    [ColorUsage(true, true)]
    private Color newHDRColor = Color.white;

    [SerializeField] GameObject attack1Sprite; 
    public Sprite attack2sprite;
    [SerializeField] Volume postProcess;
    ColorAdjustments colorAd;

    public TextAsset jsonFile;
    public int test;
    public int enemyHealth = 10;
    private ParticleSystem shooter;
    public BoxCollider2D boxTrigger;
    public PolygonCollider2D playerCollider;
    public GameObject player;
    private AudioSource audioSystem;
    private SpriteRenderer spriteComponent;
    private LineRenderer lasersBitch;
    private AudioSource audio;
    private RaycastHit2D snipeHit;

    // Start is called before the first frame update
    void Start()
    {

        postProcess.profile.TryGet(out colorAd);
        colorAd.colorFilter.hdr = false;
        shooter = GetComponent<ParticleSystem>();
        emission = shooter.emission;
        forceOverLifetime = shooter.forceOverLifetime;
        main = shooter.main;
        noice = shooter.noise;
        shape = shooter.shape;
        explody = shooter.subEmitters;
        shooter.Play();
        
        lasersBitch = GetComponent<LineRenderer>();
        spriteComponent = GetComponent<SpriteRenderer>();
        audioSystem = GetComponent<AudioSource>();

        dialogueRoot = JsonUtility.FromJson<dialogueParsing.Dialogue>(jsonFile.text);
    }

    IEnumerator slowMoParry()
    {

        yield return new WaitForSeconds(.65f);
        shape.rotation = new Vector3(0, -90, 0);
        shape.arc = 1;
        main.startSpeed = 30;
        emission.rateOverTime = .75f;
        shooter.Emit(1);
        yield return new WaitForSeconds(.2f);

        tutorial.dropHint(tutorialKey);

        colorAd.colorFilter.Override(someHDRColor);
        main.simulationSpeed = .02f;
        yield return new WaitUntil(() => hit == true);
        colorAd.colorFilter.Override(newHDRColor);
        main.simulationSpeed = .6f;

        loopFight();

    }

    public void initializeFight()
    {
        //player.GetComponent<AudioSource>().Stop();
        //audioSystem.Play();
        playerScript.inDialogue = true;
        player.transform.DOMove(playerPostion, .5f);
        Controller.startDialogue(dialogueRoot);



    }
    
    IEnumerator drop()
    {
        List<GameObject> falling = new List<GameObject>();
        for (int i = 0; i < 6; i++)
        {

            GameObject bullet = Instantiate(attack1Sprite);
            falling.Add(bullet);

        }

        for (int v = 0; v < 4; v++)
        {
            foreach (GameObject fallingObject in falling)
            {
                fallingObject.transform.position = fallingObjectArea.transform.GetChild(Mathf.RoundToInt(Random.Range(1, 11))).position;
            }

            yield return new WaitForSeconds(3);

            foreach (GameObject fallingObject in falling)
            {
                fallingObject.GetComponent<Rigidbody2D>().simulated = true;
            }

            yield return new WaitForSeconds(2f);

            foreach (GameObject fallingObject in falling)
            {
                fallingObject.GetComponent<Rigidbody2D>().simulated = false;
            }

        }
        loopFight();
    }
    

    IEnumerator timer(int waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (go >= Mathf.RoundToInt(Random.Range(3, 5)))
        {
            shooter.Stop();
            loopFight();
        }
        else
        {
            shootyshooty();
        }
    }

    private IEnumerator snipe()
    {
        for (int i = 0; i < 12; i++)
        {
            rotate = Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(player.transform.position.y - transform.position.y) / Mathf.Abs(player.transform.position.x - transform.position.x));

            snipeHit = Physics2D.Raycast(transform.position, new Vector2(-rotate, 0));
            if (snipeHit.collider != null)
            {
                
                lasersBitch.enabled = true;
                lasersBitch.SetPositions( new Vector3[] {transform.localPosition, player.transform.localPosition});
                yield return new WaitForSeconds(.75f);

                shape.rotation = new Vector3(-rotate, -90, 0);
                shape.arc = 1;
                main.startSpeed = 100;
                emission.rateOverTime = .75f;
                shooter.Emit(1);
                yield return new WaitForSeconds(.5f);
                lasersBitch.enabled = false;

            }
        }

        loopFight();

    }

    float rotate;
    private IEnumerator doubleShot()
    {

        emission.rateOverTime = 0;
        shooter.Play();
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1];
        bursts[0].count = 6;
        bursts[0].repeatInterval = 0.1f;
        bursts[0].time = 1;
        bursts[0].cycleCount = 2;
        bursts[0].probability = 1;
        emission.SetBursts(bursts);

        shape.angle = 15;
        shape.radius = 2;
        ParticleSystem.EmitParams emitOverride = new ParticleSystem.EmitParams();
            emitOverride.startLifetime = 10f;
        
        for (int i = 0; i < 12; i++)
        {
            rotate = Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(player.transform.position.y - transform.position.y) / Mathf.Abs(player.transform.position.x - transform.position.x));
            shape.rotation = new Vector3(-rotate, -90, 0);
            shooter.Emit(emitOverride, 12);
            yield return new WaitForSeconds(.5f);
        }
            
        emission.SetBursts(new ParticleSystem.Burst[0]);
        
        print("sasdfs");
        loopFight();
    }

    private void spray()
    {
        transform.DORotate(new Vector3(0, 0, 0), .5f);
        shape.angle = 45;
        shape.arc = 360;
        noice.enabled = true;
        noice.frequency = 1.59f;
        noice.scrollSpeed = 1.6f;
        noice.strength = 0.09f;
        StartCoroutine(timer(Random.Range(8, 25)));

    }

    private void narrow()
    {
        int fruityLoops = Mathf.RoundToInt(Random.Range(3, 7));
        DOTween.To(() => shape.angle, x => shape.angle = x, 10, 2);
        transform.Rotate(new Vector3(0, 0, -35));
        transform.DORotate(new Vector3(0, 0, 30), 2).SetLoops(fruityLoops, LoopType.Yoyo);
        StartCoroutine(timer(fruityLoops * 2));
    }

    private IEnumerator throwable()
    {
        shape.rotation = new Vector3(-60, -90, 0);
        shape.arc = 1;
        emission.rateOverTime = .5f;
        forceOverLifetime.enabled = true;

        explody.SetSubEmitterEmitProbability(0, 1);
        for (int i = 0; i < 6; i++)
        {
            main.startSpeed = Mathf.Clamp((player.transform.localPosition.y/12) * 140, 60, 120);
            shooter.Emit(1);
            yield return new WaitForSeconds(1.25f);
        }


        forceOverLifetime.enabled = false;
        explody.SetSubEmitterEmitProbability(0, 0);
        loopFight();

    }

    bool shootyState = true;
    int go = 0;
    private void shootyshooty()
    {
        go += 1;
        shooter.Play();
        shootyState = !shootyState;
        switch (shootyState)
        {
            case true:
                spray();
                break;
            case false:
                narrow();
                break;

        }
    }

    IEnumerator dropInTheBigGuy()
    {
        GameObject newBigGuy = Instantiate(bigGuy);
        newBigGuy.transform.position = bigGuySpawn.transform.position;

        for (int v = 0; v < 8; v++)
        {
            yield return new WaitForSeconds(3);
            newBigGuy.GetComponent<Rigidbody2D>().AddForce(new Vector2(5000 * player.transform.position.x - newBigGuy.transform.position.x, 30000));
            print("bee");
        }

        newBigGuy.GetComponent<Rigidbody2D>().simulated = false;
        Tween tween = newBigGuy.transform.DOLocalMoveY(46, 2.5f);
        
        loopFight();
        yield return tween.WaitForCompletion();
        Destroy(newBigGuy);
    }

    [SerializeField] bool FirstLevel;
    int currentState = 1;
    public void continualizeFight()
    {

        print("start");

        //audioSystem.clip = FightMusic;
        //audioSystem.Play();
        transform.DOMove(enemyPosition, 1);
        playerScript.playerCamera.orthographicSize = 7;
        playerScript.inFight = true;

        if (FirstLevel)
            StartCoroutine(slowMoParry());
        else
            loopFight();

    }

    
    IEnumerator check()
    {
        yield return new WaitForSeconds(5);
        if (!playerScript.inDialogue)
            loopFight();
    }

    public void loopFight()
    {
        print("loop");
        if (enemyHealth > 0)
        {
            switch (currentState)
            {
                case 1:
                    StartCoroutine(snipe());
                    break;
                case 2:
                    StartCoroutine(throwable());
                    break;
                case 3:
                    StartCoroutine(doubleShot());
                    break;

            }

            currentState = Mathf.RoundToInt(Random.Range(1, 3));
        }
        else
        {
            tutorial.dropHint(eKey);
            StartCoroutine(check());
        }
    }


}
