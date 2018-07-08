using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject notePrefab, tailPrefab;
    private GameObject noteCanvas;
    private List<VisualizerFall> visualizers = new List<VisualizerFall>();
    public GameObject visualizerPrefab;
    private FlashScript flash;
    private AudioSource musicAudioSource;
    private Text scoreText;
    private int score = 0;
    private static int maxNotes = 200, maxTails = 1000;
    private static float[] horizontalPositions = { -110, -55, 0, 55, 110 };
    private List<NoteStruct> notesList = new List<NoteStruct>();
    private List<GameObject> notePool = new List<GameObject>(maxNotes);
    private List<GameObject> tailPool = new List<GameObject>(maxTails);
    private SpriteRenderer[] fretsList = new SpriteRenderer[5];
    private SpriteRenderer[] fireList = new SpriteRenderer[5];
    private SpriteRenderer[] hitList = new SpriteRenderer[5];
    private Sprite[] pressedFretsList = new Sprite[5];
    private Sprite[] unpressedFretsList = new Sprite[5];
    private Sprite[] noteSpritesList = new Sprite[5];
    private Sprite[] tailSpritesList = new Sprite[5];
    private Sprite[][] fretHitAnimation = new Sprite[5][];
    private Sprite[] tailHitAnimation = new Sprite[16];
    private Sprite[] fireredAnimation = new Sprite[16];
    private IEnumerator[] fretCoroutines = new IEnumerator[5];
    private IEnumerator[] forcedBreakCoroutines = new IEnumerator[5];
    private bool playing = true;

    struct NoteStruct
    {
        public int time, note, length;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(this);
    }

    public void Initialize()
    {
        score = 0;
        FindSprites();
        noteCanvas = GameObject.Find("NoteCanvas");
        ScaleCanvas();
        scoreText = GameObject.Find("scoreText").GetComponent<Text>();
        flash = GameObject.Find("flash").GetComponent<FlashScript>();
        if (MainManager.instance.mods["ACCELERATED"]) MainManager.instance.speed = 2f;
        else if (MainManager.instance.mods["SLOWED"]) MainManager.instance.speed = 0.5f;
        else MainManager.instance.speed = 1f;
        if (MainManager.instance.mods["PERFORMANCE"]) GameObject.FindGameObjectWithTag("Neck").SetActive(false);
        ReadChart();
        PoolsFill();
        WWW audioLoad = new WWW("file:///" + MainManager.instance.songsPath.Replace(@"\", @"/") + @"/Songs/" + MainManager.instance.songName + @"/music" + MainManager.instance.songsFormat);
        while (!audioLoad.isDone) { }
        musicAudioSource = SoundManager.instance.PrepareSound(audioLoad.audioClip);
        if (MainManager.instance.mods["DEAF"]) musicAudioSource.volume = 0;
        UiManager.instance.DestroyAllText();
        UiManager.instance.CreateBackground();
        UiManager.instance.CreateTexts("ARE YOU READY?");
    }

    void ScaleCanvas()
    {
        if (MainManager.instance.platform == RuntimePlatform.Android)
        {
            MainManager.instance.resolutionScaler = 2f;
            GameObject[] tempGO = GameObject.FindGameObjectsWithTag("Fret");
            for (int i = 0; i < tempGO.Length; i++)
            {
                tempGO[i].transform.localPosition = new Vector3(tempGO[i].transform.localPosition.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localPosition.y, tempGO[i].transform.localPosition.z);
                tempGO[i].transform.localScale = new Vector3(tempGO[i].transform.localScale.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localScale.y * MainManager.instance.resolutionScaler, tempGO[i].transform.localScale.z);
            }
            tempGO = GameObject.FindGameObjectsWithTag("Fire");
            for (int i = 0; i < tempGO.Length; i++)
            {
                tempGO[i].transform.localPosition = new Vector3(tempGO[i].transform.localPosition.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localPosition.y, tempGO[i].transform.localPosition.z);
                tempGO[i].transform.localScale = new Vector3(tempGO[i].transform.localScale.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localScale.y * MainManager.instance.resolutionScaler, tempGO[i].transform.localScale.z);
            }
            tempGO = GameObject.FindGameObjectsWithTag("Neck");
            for (int i = 0; i < tempGO.Length; i++)
            {
                tempGO[i].transform.localPosition = new Vector3(tempGO[i].transform.localPosition.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localPosition.y, tempGO[i].transform.localPosition.z);
                tempGO[i].transform.localScale = new Vector3(tempGO[i].transform.localScale.x * MainManager.instance.resolutionScaler, tempGO[i].transform.localScale.y, tempGO[i].transform.localScale.z);
            }
        }
    }

    private void PoolsFill()
    {
        notePool.RemoveRange(0, notePool.Count);
        tailPool.RemoveRange(0, tailPool.Count);
        for (int i = 0; i < maxNotes; i++)
        {
            GameObject tempGameObject = Instantiate(notePrefab);
            tempGameObject.transform.SetParent(noteCanvas.transform);
            tempGameObject.layer = 5;
            gameObject.tag = "Note";
            tempGameObject.transform.localScale = new Vector3(28 * MainManager.instance.resolutionScaler, 28 * MainManager.instance.resolutionScaler, 0);
            notePool.Add(tempGameObject);
        }
        for (int i = 0; i < maxTails; i++)
        {
            GameObject tempGameObject = Instantiate(tailPrefab);
            tempGameObject.transform.SetParent(noteCanvas.transform);
            tempGameObject.layer = 5;
            gameObject.tag = "Tail";
            if (MainManager.instance.mods["ACCELERATED"]) tempGameObject.transform.localScale = new Vector3(28 * MainManager.instance.resolutionScaler, 100 * MainManager.instance.resolutionScaler, 0);
            else tempGameObject.transform.localScale = new Vector3(28 * MainManager.instance.resolutionScaler, 50 * MainManager.instance.resolutionScaler, 0);
            tailPool.Add(tempGameObject);
        }
        for (int i = 0; i < fretCoroutines.Length; i++)
        {
            fretCoroutines[i] = TailHitCoroutine(i);
            forcedBreakCoroutines[i] = TailHitForcedBreakCoroutine(i, 0);
        }
    }

    void ReadChart()
    {
        notesList.RemoveRange(0, notesList.Count);
        FileStream chartFileStream = new FileStream(MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/notes.chart", FileMode.Open);
        StreamReader chartStreamReader = new StreamReader(chartFileStream);
        string tempReadLine = "";
        tempReadLine = chartStreamReader.ReadLine();
        while (tempReadLine != null)
        {
            NoteStruct tempNoteStruct = new NoteStruct();
            tempNoteStruct.time = Convert.ToInt32(tempReadLine.Substring(tempReadLine.IndexOf("T") + 1).Remove(tempReadLine.Substring(tempReadLine.IndexOf("T") + 1).IndexOf("N")));
            tempNoteStruct.note = Convert.ToInt32(tempReadLine.Substring(tempReadLine.IndexOf("N") + 1).Remove(tempReadLine.Substring(tempReadLine.IndexOf("N") + 1).IndexOf("L")));
            tempNoteStruct.length = Convert.ToInt32(tempReadLine.Substring(tempReadLine.IndexOf("L") + 1));
            notesList.Add(tempNoteStruct);
            tempReadLine = chartStreamReader.ReadLine();
        }
        chartStreamReader.Close();
        chartFileStream.Close();
    }

    void FindSprites()
    {
        GameObject[] tempFrets = GameObject.FindGameObjectsWithTag("Fret");
        GameObject[] tempFires = GameObject.FindGameObjectsWithTag("Fire");
        GameObject[] tempHits = GameObject.FindGameObjectsWithTag("Hit");
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (tempFrets[j].transform.localPosition.x == horizontalPositions[i])
                {
                    fretsList[i] = tempFrets[j].GetComponent<SpriteRenderer>();
                    break;
                }
            }
            for (int j = 0; j < 5; j++)
            {
                if (tempFires[j].transform.localPosition.x == horizontalPositions[i])
                {
                    fireList[i] = tempFires[j].GetComponent<SpriteRenderer>();
                    break;
                }
            }
            for (int j = 0; j < 5; j++)
            {
                if (tempHits[j].transform.localPosition.x == horizontalPositions[i])
                {
                    hitList[i] = tempHits[j].GetComponent<SpriteRenderer>();
                    if (i == 0) tempHits[j].transform.localPosition = new Vector3(-114.2f * MainManager.instance.resolutionScaler, tempHits[j].transform.localPosition.y, 0);
                    if (i == 1) tempHits[j].transform.localPosition = new Vector3(-56.79f * MainManager.instance.resolutionScaler, tempHits[j].transform.localPosition.y, 0);
                    if (i == 2) tempHits[j].transform.localPosition = new Vector3(0f, tempHits[j].transform.localPosition.y, 0);
                    if (i == 3) tempHits[j].transform.localPosition = new Vector3(56.9f * MainManager.instance.resolutionScaler, tempHits[j].transform.localPosition.y, 0);
                    if (i == 4) tempHits[j].transform.localPosition = new Vector3(112.5f * MainManager.instance.resolutionScaler, tempHits[j].transform.localPosition.y, 0);
                    break;
                }
            }
            unpressedFretsList[i] = Resources.LoadAll<Sprite>("Sprites/frets")[i];
            pressedFretsList[i] = Resources.LoadAll<Sprite>("Sprites/frets")[i + 5];
            noteSpritesList[i] = Resources.LoadAll<Sprite>("Sprites/notes")[i];
            tailSpritesList[i] = Resources.LoadAll<Sprite>("Sprites/tails")[i];
            fretHitAnimation[i] = new Sprite[6];
            for (int j = 0; j < 6; j++)
            {
                fretHitAnimation[i][j] = Resources.LoadAll<Sprite>("Sprites/frethit" + i.ToString())[j];
            }
        }
        for (int i = 0; i < 16; i++)
        {
            fireredAnimation[i] = Resources.LoadAll<Sprite>("Sprites/firered")[i];
            tailHitAnimation[i] = Resources.LoadAll<Sprite>("Sprites/hit")[i];
        }
    }

    void DropNote(int number, int length)
    {
        for (int i = 0; i < maxNotes; i++)
        {
            if (!notePool[i].activeInHierarchy)
            {
                notePool[i].GetComponent<NoteScript>().number = number;
                notePool[i].GetComponent<NoteScript>().tail = length;
                switch (number)
                {
                    case 0: notePool[i].transform.localPosition = new Vector3(-110f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 1: notePool[i].transform.localPosition = new Vector3(-55f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 2: notePool[i].transform.localPosition = new Vector3(0f, 0f, 500f); break;
                    case 3: notePool[i].transform.localPosition = new Vector3(+55f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 4: notePool[i].transform.localPosition = new Vector3(+110f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                }
                notePool[i].GetComponent<SpriteRenderer>().sprite = noteSpritesList[number];
                notePool[i].SetActive(true);
                break;
            }
        }
    }

    void DropTail(int number)
    {
        for (int i = 0; i < maxTails; i++)
        {
            if (!tailPool[i].activeInHierarchy)
            {
                switch (number)
                {
                    case 0: tailPool[i].transform.localPosition = new Vector3(-110f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 1: tailPool[i].transform.localPosition = new Vector3(-55f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 2: tailPool[i].transform.localPosition = new Vector3(0f, 0f, 500f); break;
                    case 3: tailPool[i].transform.localPosition = new Vector3(+55f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                    case 4: tailPool[i].transform.localPosition = new Vector3(+110f * MainManager.instance.resolutionScaler, 0f, 500f); break;
                }
                tailPool[i].GetComponent<SpriteRenderer>().sprite = tailSpritesList[number];
                tailPool[i].SetActive(true);
                break;
            }
        }
    }

    void GamePause(bool on)
    {
        playing = !on;
        if (playing) musicAudioSource.UnPause();
        else musicAudioSource.Pause();
        for (int i = 0; i < maxNotes; i++)
        {
            notePool[i].GetComponent<NoteScript>().playing = playing;
        }
        for (int i = 0; i < maxTails; i++)
        {
            tailPool[i].GetComponent<NoteScript>().playing = playing;
        }
    }

    void FretHit(int number)
    {
        StartCoroutine(FretFireCoroutine(number));
        StartCoroutine(FretHitCoroutine(number, 0));
    }
    void TailHitStart(int number)
    {
        TailHitBreak(number);
        StartCoroutine(fretCoroutines[number]);
        StartCoroutine(FretHitCoroutine(number, 1));
    }
    void TailHitBreak(int number)
    {
        StopCoroutine(fretCoroutines[number]);
        if (hitList[number].sprite != null)
        {
            StartCoroutine(FretHitCoroutine(number, 2));
            hitList[number].sprite = null;
        }
    }

    void FretPress(int number, bool pressed)
    {
        if (pressed)
            fretsList[number].sprite = pressedFretsList[number];
        else
        {
            if (fretsList[number].sprite.name == "frethit" + number.ToString() + "_2")
            {
                TailHitBreak(number);
            }
            else
                fretsList[number].sprite = unpressedFretsList[number];
        }
    }

    void NoteIsHere(int fret)
    {
        for (int i = 0; i < maxNotes; i++)
        {
            if (notePool[i].activeInHierarchy)
                if (notePool[i].transform.localPosition.y < -170 && notePool[i].transform.localPosition.y > -187 && notePool[i].GetComponent<NoteScript>().number == fret)
                {
                    notePool[i].SetActive(false);
                    if (notePool[i].GetComponent<NoteScript>().tail > 0)
                    {
                        TailHitStart(fret);
                        StopCoroutine(forcedBreakCoroutines[fret]);
                        forcedBreakCoroutines[fret] = TailHitForcedBreakCoroutine(fret, notePool[i].GetComponent<NoteScript>().tail);
                        StartCoroutine(forcedBreakCoroutines[fret]);
                    }
                    else
                    {
                        FretHit(fret);
                        score += 95;
                        scoreText.text = score.ToString();
                    }
                    if (MainManager.instance.mods["EPILEPTIC"]) flash.Flash(fret);
                    break;
                }
        }
    }

    private void SongEnd()
    {
        scoreText.text = "";
        UiManager.instance.DrawLayout("WIN");
        UiManager.instance.CreateBackground();
        UiManager.instance.CreateTexts("YOU WIN! " + score.ToString() + " SCORED TOTAL!");

        SoundManager.instance.CreateSound("Sounds/intro" + UnityEngine.Random.Range(0, 6), true, false);
    }

    int c = 0;
    private void CreateVisualizer()
    {
        GameObject[] tempVisualizers = GameObject.FindGameObjectsWithTag("Visualizer");
        if (tempVisualizers.Length > 0)
            for (int i = 0; i < tempVisualizers.Length; i++)
            {
                Destroy(tempVisualizers[i]);
            }
        visualizers.RemoveRange(0, visualizers.Count);
        for (int i = 330; i > (-335); i = i - 55)
        {
            c++;
            GameObject tempVisualizer = Instantiate(visualizerPrefab);
            tempVisualizer.name = "visualizer" + c.ToString();
            tempVisualizer.tag = "Visualizer";
            tempVisualizer.transform.SetParent(GameObject.Find("BackgroundCanvas").transform);
            tempVisualizer.transform.localPosition = new Vector3(i, -200, 0);
            tempVisualizer.transform.localScale = new Vector3(1, 1, 1);
            tempVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);
            visualizers.Add(tempVisualizer.GetComponent<VisualizerFall>());
        }
        StartCoroutine(MusicVisualizerCoroutine());
    }

    private void Restart()
    {
        for (int i = 0; i < maxNotes; i++)
        {
            if (notePool[i].activeInHierarchy) notePool[i].SetActive(false);
        }
        for (int i = 0; i < maxTails; i++)
        {
            if (tailPool[i].activeInHierarchy) tailPool[i].SetActive(false);
        }
        StopAllCoroutines();
        UiManager.instance.DrawLayout("PLAY");
        CreateVisualizer();
        musicAudioSource.timeSamples = 0;
        score = 0;
        scoreText.text = score.ToString();
        GamePause(false);
        musicAudioSource.Play();
        StartCoroutine(DropNotesCoroutine());
        if (MainManager.instance.mods["AUTO"]) StartCoroutine(AutoPlay());
    }

    public void GameControl()
    {
        if (MainManager.instance.platform == RuntimePlatform.Android)
        {
            Touch[] touches;
            touches = Input.touches;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Vector2 touchPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(noteCanvas.GetComponent<RectTransform>(), new Vector2(touches[i].rawPosition.x, touches[i].rawPosition.y), GameObject.Find("NoteCamera").GetComponent<Camera>(), out touchPos);
                if (touches[i].phase == TouchPhase.Began)
                {
                    switch (MainManager.instance.layoutState)
                    {
                        case "PAUSE":
                            {
                                SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                                if (touchPos.y < 130 && touchPos.y > 70) UiManager.instance.cursorPosition = 0;
                                if (touchPos.y < 70 && touchPos.y > 10) UiManager.instance.cursorPosition = 1;
                                if (touchPos.y < 10 && touchPos.y > -50) UiManager.instance.cursorPosition = 2;
                                switch (UiManager.instance.cursorPosition)
                                {
                                    case 2:
                                        {
                                            MainManager.instance.gameMode = "menu";
                                            StopAllCoroutines();
                                            GamePause(false);
                                            UiManager.instance.CreateLoadingScreen();
                                            SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                                            break;
                                        }
                                    case 1:
                                        {
                                            Restart();
                                            break;
                                        }
                                    case 0:
                                        {
                                            GamePause(false);
                                            UiManager.instance.DrawLayout("PLAY");
                                            break;
                                        }
                                }
                                break;
                            }
                        case "GAME":
                            {
                                UiManager.instance.DrawLayout("PLAY");
                                musicAudioSource.Play();
                                CreateVisualizer();
                                StartCoroutine(DropNotesCoroutine());
                                if (MainManager.instance.mods["AUTO"]) StartCoroutine(AutoPlay());
                                break;
                            }
                        case "WIN":
                            {
                                MainManager.instance.gameMode = "menu";
                                StopAllCoroutines();
                                GamePause(false);
                                UiManager.instance.CreateLoadingScreen();
                                SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                                break;
                            }
                        case "PLAY":
                            {
                                if (touchPos.x < -200 && touchPos.y > 130)
                                {
                                    SoundManager.instance.CreateSound("Sounds/escClick", true, true);
                                    GamePause(true);
                                    UiManager.instance.DestroyAllText();
                                    UiManager.instance.CreateBackground();
                                    UiManager.instance.DrawLayout("PAUSE");
                                }
                                if (touchPos.x > -250 && touchPos.x < -140 && touchPos.y < 0)
                                {
                                    NoteIsHere(0);
                                    FretPress(0, true);
                                }
                                if (touchPos.x > -140 && touchPos.x < -30 && touchPos.y < 0)
                                {
                                    NoteIsHere(1);
                                    FretPress(1, true);
                                }
                                if (touchPos.x > -30 && touchPos.x < 30 && touchPos.y < 0)
                                {
                                    NoteIsHere(2);
                                    FretPress(2, true);
                                }
                                if (touchPos.x > 30 && touchPos.x < 140 && touchPos.y < 0)
                                {
                                    NoteIsHere(3);
                                    FretPress(3, true);
                                }
                                if (touchPos.x > 140 && touchPos.x < 250 && touchPos.y < 0)
                                {
                                    NoteIsHere(4);
                                    FretPress(4, true);
                                }
                                break;
                            }
                    }
                }
                if (touches[i].phase == TouchPhase.Ended)
                {
                    switch (MainManager.instance.layoutState)
                    {
                        case "PLAY":
                            {
                                if (touchPos.x > -250 && touchPos.x < -140 && touchPos.y < 0)
                                {
                                    FretPress(0, false);
                                }
                                if (touchPos.x > -140 && touchPos.x < -30 && touchPos.y < 0)
                                {
                                    FretPress(1, false);
                                }
                                if (touchPos.x > -30 && touchPos.x < 30 && touchPos.y < 0)
                                {
                                    FretPress(2, false);
                                }
                                if (touchPos.x > 30 && touchPos.x < 140 && touchPos.y < 0)
                                {
                                    FretPress(3, false);
                                }
                                if (touchPos.x > 140 && touchPos.x < 250 && touchPos.y < 0)
                                {
                                    FretPress(4, false);
                                }
                                break;
                            }
                    }
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "PAUSE":
                        {
                            SoundManager.instance.CreateSound("Sounds/upClick", true, false);
                            UiManager.instance.MoveCursor(true);
                            break;
                        }
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "PAUSE":
                        {
                            SoundManager.instance.CreateSound("Sounds/upClick", true, false);
                            UiManager.instance.MoveCursor(false);
                            break;
                        }
                }
            }
            if (Input.anyKeyDown)
            {
                switch (MainManager.instance.layoutState)
                {
                    case "GAME":
                        {
                            UiManager.instance.DrawLayout("PLAY");
                            musicAudioSource.Play();
                            CreateVisualizer();
                            StartCoroutine(DropNotesCoroutine());
                            if (MainManager.instance.mods["AUTO"]) StartCoroutine(AutoPlay());
                            break;
                        }
                    case "WIN":
                        {
                            MainManager.instance.gameMode = "menu";
                            StopAllCoroutines();
                            GamePause(false);
                            UiManager.instance.CreateLoadingScreen();
                            SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                            break;
                        }
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "PAUSE":
                        {
                            SoundManager.instance.CreateSound("Sounds/enterClick", true, true);
                            switch (UiManager.instance.GetCurrentSelection())
                            {
                                case "EXIT":
                                    {
                                        MainManager.instance.gameMode = "menu";
                                        StopAllCoroutines();
                                        GamePause(false);
                                        UiManager.instance.CreateLoadingScreen();
                                        SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                                        break;
                                    }
                                case "RESTART":
                                    {
                                        Restart();
                                        break;
                                    }
                                case "CONTINUE":
                                    {
                                        GamePause(false);
                                        UiManager.instance.DrawLayout("PLAY");
                                        break;
                                    }
                            }
                            break;
                        }
                    case "PLAY": break;
                    case "GAME":
                        {
                            UiManager.instance.DrawLayout("PLAY");
                            musicAudioSource.Play();
                            CreateVisualizer();
                            StartCoroutine(DropNotesCoroutine());
                            if (MainManager.instance.mods["AUTO"]) StartCoroutine(AutoPlay());
                            break;
                        }
                    case "WIN":
                        {
                            MainManager.instance.gameMode = "menu";
                            StopAllCoroutines();
                            GamePause(false);
                            UiManager.instance.CreateLoadingScreen();
                            SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                            break;
                        }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "PLAY":
                        {
                            SoundManager.instance.CreateSound("Sounds/escClick", true, true);
                            GamePause(true);
                            UiManager.instance.DrawLayout("PAUSE");
                            break;
                        }
                    case "PAUSE":
                        {
                            GamePause(false);
                            UiManager.instance.DrawLayout("PLAY");
                            break;
                        }
                }
            }
            if (Input.GetKeyDown(MainManager.instance.keys[0]))
            {
                NoteIsHere(0);
                FretPress(0, true);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[1]))
            {
                NoteIsHere(1);
                FretPress(1, true);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[2]))
            {
                NoteIsHere(2);
                FretPress(2, true);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[3]))
            {
                NoteIsHere(3);
                FretPress(3, true);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[4]))
            {
                NoteIsHere(4);
                FretPress(4, true);
            }

            if (Input.GetKeyUp(MainManager.instance.keys[0]))
            {
                FretPress(0, false);
            }
            if (Input.GetKeyUp(MainManager.instance.keys[1]))
            {
                FretPress(1, false);
            }
            if (Input.GetKeyUp(MainManager.instance.keys[2]))
            {
                FretPress(2, false);
            }
            if (Input.GetKeyUp(MainManager.instance.keys[3]))
            {
                FretPress(3, false);
            }
            if (Input.GetKeyUp(MainManager.instance.keys[4]))
            {
                FretPress(4, false);
            }
        }
    }

    void OnApplicationFocus(bool focusStatus)
    {
        if (!focusStatus)
        {
            if (MainManager.instance.layoutState == "PLAY")
            {
                SoundManager.instance.CreateSound("Sounds/escClick", true, true);
                GamePause(true);
                UiManager.instance.DestroyAllText();
                UiManager.instance.CreateBackground();
                UiManager.instance.DrawLayout("PAUSE");
            }
        }
    }

    IEnumerator TailHitCoroutine(int number)
    {
        while (true)
        {
            for (int i = 0; i < 16; i++)
            {
                hitList[number].sprite = tailHitAnimation[i];
                yield return new WaitForSecondsRealtime(0.02f);
                score += 95 / 4;
                scoreText.text = score.ToString();
            }
            yield return 0;
        }
    }

    IEnumerator TailHitForcedBreakCoroutine(int number, int length)
    {
        int nSample = musicAudioSource.timeSamples + length;
        while (musicAudioSource.timeSamples < nSample)
        {
            while (!playing) { yield return 0; }
            yield return 0;
        }
        TailHitBreak(number);
    }

    IEnumerator FretHitCoroutine(int number, int mode)
    {
        switch (mode)
        {
            case 0:
                {
                    for (int i = 0; i < 6; i++)
                    {
                        fretsList[number].sprite = fretHitAnimation[number][i];
                        yield return new WaitForSeconds(0.02f);
                    }
                    break;
                }
            case 1:
                {
                    for (int i = 0; i < 3; i++)
                    {
                        fretsList[number].sprite = fretHitAnimation[number][i];
                        yield return new WaitForSeconds(0.02f);
                    }
                    break;
                }
            case 2:
                {
                    for (int i = 3; i < 6; i++)
                    {
                        fretsList[number].sprite = fretHitAnimation[number][i];
                        yield return new WaitForSeconds(0.02f);
                    }
                    break;
                }
        }

    }
    IEnumerator FretFireCoroutine(int number)
    {
        for (int i = 0; i < 16; i++)
        {
            fireList[number].sprite = fireredAnimation[i];
            yield return new WaitForSeconds(0.01f);
        }
    }
    IEnumerator DropTailsCoroutine(int noteCounter)
    {
        int nSample = notesList[noteCounter].time + notesList[noteCounter].length - (int)(73500 / MainManager.instance.speed);
        while (musicAudioSource.timeSamples < nSample)
        {
            while (!playing) { yield return 0; }
            DropTail(notesList[noteCounter].note);
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator DropNotesCoroutine()
    {
        for (int noteCounter = 0; noteCounter < notesList.Count; noteCounter++)
        {
            int nSample = notesList[noteCounter].time - (int)(73500 / MainManager.instance.speed);
            while (musicAudioSource.timeSamples < nSample) yield return 0;
            while (!playing) { yield return 0; }
            DropNote(notesList[noteCounter].note, notesList[noteCounter].length);
            if (notesList[noteCounter].length > 0) StartCoroutine(DropTailsCoroutine(noteCounter));
        }
        StartCoroutine(CheckSongEnd());
    }
    IEnumerator AutoPlay()
    {
        for (int noteCounter = 0; noteCounter < notesList.Count; noteCounter++)
        {
            int nSample = notesList[noteCounter].time - 700;
            while (musicAudioSource.timeSamples < nSample) yield return 0;
            while (!playing) { yield return 0; }
            NoteIsHere(notesList[noteCounter].note);
        }
    }

    IEnumerator CheckSongEnd()
    {
        while (musicAudioSource.timeSamples < musicAudioSource.clip.samples) { yield return 0; }
        SongEnd();
    }

    float[] spectrum = new float[64];

    IEnumerator MusicVisualizerCoroutine()
    {
        while (true)
        {
            musicAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            int i = 1;
            while (i < visualizers.Count + 1)
            {
                visualizers[i - 1].Raise(spectrum[i] * 3000);
                i++;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}