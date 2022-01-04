using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SFX
{
    levelUp,
    pop,
    drop,
    over,
}

public class GameManager : MonoBehaviour
{
    [Header("===== Pooler =====")]
    public List<Circle> circlePool;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;

    [Header("===== Circle =====")]
    public Circle lastCircle;
    public GameObject circlePrefab;
    public Transform circleGroup;

    [Header("===== Particle System =====")]
    public GameObject effectPrefab;
    public Transform effectGroup;

    [Header("===== Audio =====")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayers;

    public AudioClip[] popClip;
    public AudioClip levelUPClip;
    public AudioClip dropClip;
    public AudioClip overClip;
    [HideInInspector]
    public int sfxCursor;

    [Header("===== UI =====")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI finalScore;
    public GameObject startPanel;
    public GameObject overPanel;
    public GameObject startFruit;
    public GameObject line;
    public GameObject floor;

    [Header("===== ETC =====")]
    public int score;
    public int MaxLevel;
    public bool isOver;

    void Awake()
    {
        // 프레임 고정
        Application.targetFrameRate = 60;

        circlePool = new List<Circle>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateCircle();
        }

        if (!PlayerPrefs.HasKey("HighScore"))
            PlayerPrefs.SetInt("HighScore", 0);

        highScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
    }

    public void GameStart()
    {
        line.SetActive(true);
        floor.SetActive(true);
        scoreText.gameObject.SetActive(true);
        highText.gameObject.SetActive(true);
        highScoreText.gameObject.SetActive(true);
        startPanel.SetActive(false);

        bgmPlayer.Play();
        Invoke("NextCircle", 1.5f);
    }

    void FixedUpdate()
    {
        startFruit.transform.Rotate(0, 0, -60 * Time.deltaTime);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
    }

    void LateUpdate()
    {
        scoreText.text = "Score\n" + score;
    }

    Circle CreateCircle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 오브젝트 생성
        GameObject instantCircleObj = Instantiate(circlePrefab, circleGroup);
        instantCircleObj.name = "Circle " + circlePool.Count;
        Circle instantCircle = instantCircleObj.GetComponent<Circle>();

        instantCircle.manager = this;
        instantCircle.effect = instantEffect;

        circlePool.Add(instantCircle);

        return instantCircle;
    }

    Circle GetCircle()
    {
        for (int i = 0; i < circlePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % circlePool.Count;

            if (!circlePool[poolCursor].gameObject.activeSelf)
            {
                return circlePool[poolCursor];
            }
        }

        return CreateCircle();
    }

    void NextCircle()
    {
        if (isOver)
            return;

        lastCircle = GetCircle();
        lastCircle.level = Random.Range(0, MaxLevel);
        lastCircle.gameObject.SetActive(true);

        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastCircle != null)
            yield return null;

        yield return new WaitForSeconds(2.5f);

        NextCircle();
    }

    public void TouchDown()
    {
        if (lastCircle == null)
            return;

        lastCircle.Drag();
    }

    public void TouchUp()
    {
        if (lastCircle == null)
            return;

        lastCircle.Drop();
        lastCircle = null;
    }

    public void GameOver()
    {
        if (isOver)
            return;

        isOver = true;
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // 모든 circle 가져오기
        Circle[] circles = FindObjectsOfType<Circle>();

        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].rigid.simulated = false;
        }

        // 하나씩 삭제
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].Hide(circles[i].transform.position);
            yield return new WaitForSeconds(.1f);
        }

        yield return new WaitForSeconds(1f);

        // 최고 점수 갱신
        int highScore = Mathf.Max(score, PlayerPrefs.GetInt("HighScore"));
        PlayerPrefs.SetInt("HighScore", highScore);

        finalScore.text = score.ToString();
        overPanel.SetActive(true);

        bgmPlayer.Stop();
    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }
    
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
    }

    public void SFXPlay(SFX type)
    {
        switch (type)
        {
            case SFX.levelUp:
                break;
            case SFX.pop:
                break;
            case SFX.drop:
                break;
            case SFX.over:
                break;
            default:
                break;
        }
    }
}
