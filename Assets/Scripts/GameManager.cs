using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("---------------< Core >")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("---------------< Object Pooling >")]
    public GameObject planetPrefab;
    public Transform planetGroup;
    public List<Planet> planetPool;

    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Planet lastPlanet;

    [Header("---------------< Audio >")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over};
    int sfxCursor;

    [Header("---------------< UI >")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    void Awake()
    {
        Application.targetFrameRate = 60;

        planetPool = new List<Planet>();
        effectPool = new List<ParticleSystem>();
        for (int index = 0; index < poolSize; index++)
        {
            MakePlanet();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextPlanet", 1.5f); // 1.5초 정도 후에 NextPlanet 함수 실행
    }

    Planet MakePlanet()
    {
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 부모를 만들어 원하는 축을 고정값으로 만듦
        GameObject instantPlanetObj = Instantiate(planetPrefab, planetGroup);
        instantPlanetObj.name = "Planet " + planetPool.Count;
        Planet instantPlanet = instantPlanetObj.GetComponent<Planet>();
        instantPlanet.manager = this;       // 게임매니저 초기화
        instantPlanet.effect = instantEffect;
        planetPool.Add(instantPlanet);

        return instantPlanet;
    }
    Planet GetPlanet()
    {
        for (int index = 0; index < planetPool.Count; index++)
        {
            poolCursor = (poolCursor +1) % planetPool.Count;
            if (!planetPool[poolCursor].gameObject.activeSelf) return planetPool[poolCursor];

        }
        return MakePlanet();
    }

    void NextPlanet()
    {
        if (isOver) return;

        lastPlanet = GetPlanet();
        lastPlanet.level = Random.Range(0,maxLevel);
        lastPlanet.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastPlanet != null)
        {
            yield return null;
        }

        // return null : 한 프레임 쉬기
        yield return new WaitForSeconds(2.5f);

        NextPlanet();
    }
    public void TouchDown()
    {
        if (lastPlanet == null) return;

        lastPlanet.Drag();
    }
    public void TouchUp()
    {
        if (lastPlanet == null) return;

        lastPlanet.Drop();
        lastPlanet = null;
    }

    public void GameOver()
    {
        if (isOver) return;

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        GameObject panelObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = panelObj.GetComponent<ParticleSystem>();

        yield return new WaitForSeconds(1f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(GameManager.Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        { 
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }

}
