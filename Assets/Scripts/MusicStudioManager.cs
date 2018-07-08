using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Windows.Forms;
using System.Linq;

public class MusicStudioManager : MonoBehaviour
{
    public static MusicStudioManager instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public GameObject studioNotePrefab;
    private GameObject noteCanvas;
    public AudioSource musicAudioSource;
    private static float[] verticalPositions = { 60, 30, 0, -30, -60 };
    private List<NoteStruct> notesList = new List<NoteStruct>();
    private SpriteRenderer[] fretsList = new SpriteRenderer[5];
    private Sprite[] studioFrets = new Sprite[5];
    private Sprite[] studioFretsPressed = new Sprite[5];
    private Sprite[] studioNotes = new Sprite[5];
    private float noteOffset = 0;
    private int noteCanvasResolution = 500;
    bool pause = true;

    struct NoteStruct
    {
        public int time, note, length;
    }

    public void Initialize()
    {
        noteCanvas = GameObject.Find("NoteCanvas");
        SetUiFunctions();
        FindSprites();
        CheckSongName("");
    }

    private void LoadMusic()
    {
        StreamReader sr = new StreamReader(MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/info.ini");
        string str = "";
        str = sr.ReadLine();
        GameObject.Find("InputFieldName").GetComponent<InputField>().text = str.Substring(str.IndexOf("=") + 1);
        str = sr.ReadLine();
        GameObject.Find("InputFieldAuthor").GetComponent<InputField>().text = str.Substring(str.IndexOf("=") + 1);
        str = sr.ReadLine();
        GameObject.Find("InputFieldCharter").GetComponent<InputField>().text = str.Substring(str.IndexOf("=") + 1);
        CheckSongName("");
        GameObject.Find("ResolutionSlider").GetComponent<Slider>().value = 0;
        GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value = 0;
        GameObject.Find("InputField").GetComponent<InputField>().text = 1.ToString();
        noteOffset = 0;
        GameObject.Find("InputFieldOffset").GetComponent<InputField>().text = 0.ToString();
        SoundManager.instance.DestroyAllSounds();
        StopAllCoroutines();
        WWW audioLoad = new WWW("file:///" + MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/music" + MainManager.instance.songsFormat);
        while (!audioLoad.isDone) { }
        musicAudioSource = SoundManager.instance.PrepareSound(audioLoad.audioClip);
        while (!musicAudioSource.isActiveAndEnabled) { }
        ReadChart();
        PrepareNoteCanvas(0);
        GameObject.Find("TimeSamplesSlider").GetComponent<Slider>().maxValue = musicAudioSource.clip.samples;
        noteCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(musicAudioSource.clip.samples / noteCanvasResolution, 170);
        StartCoroutine(SyncNoteCanvas());
    }

    public void SetUiFunctions()
    {
        GameObject.Find("PauseButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { PauseMusicButton(); });
        GameObject.Find("SaveButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { SaveChart(); });
        GameObject.Find("ImportButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { ImportButton(); });
        GameObject.Find("ImportChartButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { ImportChartButton(); });
        GameObject.Find("ImportPicButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { ImportPicButton(); });
        GameObject.Find("TimeSamplesSlider").GetComponent<Slider>().onValueChanged.AddListener(MusicControl);
        GameObject.Find("ResolutionSlider").GetComponent<Slider>().onValueChanged.AddListener(MoveNoteCanvas);
        GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().onValueChanged.AddListener(MoveNoteCanvas);
        GameObject.Find("InputFieldName").GetComponent<InputField>().onEndEdit.AddListener(CheckSongName);
        GameObject.Find("InputField").GetComponent<InputField>().onEndEdit.AddListener(SetResolutionSlider);
        GameObject.Find("InputFieldOffset").GetComponent<InputField>().onEndEdit.AddListener(SetOffset);
    }

    public void PauseMusicButton()
    {
        if (!pause) musicAudioSource.Pause();
        else musicAudioSource.Play();
        pause = !pause;
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

    public void SaveChart()
    {
        FileStream fs = new FileStream(MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/info.ini", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.WriteLine("name=" + GameObject.Find("InputFieldName").GetComponent<InputField>().text);
        sw.WriteLine("author=" + GameObject.Find("InputFieldAuthor").GetComponent<InputField>().text);
        sw.WriteLine("charter=" + GameObject.Find("InputFieldCharter").GetComponent<InputField>().text);
        sw.Close();
        fs.Close();
        FileStream chartFileStream = new FileStream(MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/notes.chart", FileMode.Create);
        StreamWriter chartStreamWriter = new StreamWriter(chartFileStream);
        GameObject[] studioNotes = GameObject.FindGameObjectsWithTag("StudioNote");
        GameObject[] studioNotesSorted;
        studioNotesSorted = studioNotes.OrderBy(studioNote => studioNote.transform.position.x).ToArray();
        for (int i = 0; i < studioNotesSorted.Length; i++)
        {
            chartStreamWriter.WriteLine("T" + (int)(studioNotesSorted[i].transform.localPosition.x * noteCanvasResolution) + "N" + studioNotesSorted[i].GetComponent<StudioNoteScript>().note + "L" + (int)(studioNotesSorted[i].GetComponent<StudioNoteScript>().length * noteCanvasResolution));
        }
        chartStreamWriter.Close();
        chartFileStream.Close();
    }

    private void FindSprites()
    {
        GameObject[] tempFrets = GameObject.FindGameObjectsWithTag("Fret");
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (tempFrets[j].transform.localPosition.y == verticalPositions[i])
                {
                    fretsList[i] = tempFrets[j].GetComponent<SpriteRenderer>();
                    break;
                }
            }
            studioFrets[i] = Resources.LoadAll<Sprite>("Sprites/studiofrets")[i];
            studioFretsPressed[i] = Resources.LoadAll<Sprite>("Sprites/studiofretspressed")[i];
            studioNotes[i] = Resources.LoadAll<Sprite>("Sprites/studionotes")[i];
        }
    }

    private void SetResolutionSlider(string str)
    {
        GameObject.Find("ResolutionSlider").GetComponent<Slider>().value = (float)Convert.ToDouble(GameObject.Find("InputField").GetComponent<InputField>().text);
        GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value = 0;
    }

    private void SetOffset(string str)
    {
        noteOffset = (float)Convert.ToDouble(str);
        MoveNoteCanvas(0);
    }

    private void PrepareNoteCanvas(float value)
    {
        GameObject[] tempStudioNotes = GameObject.FindGameObjectsWithTag("StudioNote");
        if (tempStudioNotes.Length > 0) for (int i = 0; i < tempStudioNotes.Length; i++)
            {
                Destroy(tempStudioNotes[i]);
            }
        float noteCanvasYPosition = 0;
        for (int i = 0; i < notesList.Count; i++)
        {
            GameObject studioNoteClone = Instantiate(studioNotePrefab);
            studioNoteClone.tag = "StudioNote";
            studioNoteClone.transform.SetParent(noteCanvas.transform);
            switch (notesList[i].note)
            {
                case 0: noteCanvasYPosition = 60; break;
                case 1: noteCanvasYPosition = 30; break;
                case 2: noteCanvasYPosition = 0; break;
                case 3: noteCanvasYPosition = -30; break;
                case 4: noteCanvasYPosition = -60; break;
            }
            studioNoteClone.GetComponent<Image>().sprite = studioNotes[notesList[i].note];
            studioNoteClone.GetComponent<StudioNoteScript>().note = notesList[i].note;
            studioNoteClone.GetComponent<StudioNoteScript>().length = (int)(notesList[i].length * (GameObject.Find("ResolutionSlider").GetComponent<Slider>().value + GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value) / noteCanvasResolution);
            studioNoteClone.transform.localPosition = new Vector3(notesList[i].time * (GameObject.Find("ResolutionSlider").GetComponent<Slider>().value + GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value) / noteCanvasResolution + noteOffset, noteCanvasYPosition, 0);
            studioNoteClone.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    private void MoveNoteCanvas(float value)
    {
        GameObject.Find("InputField").GetComponent<InputField>().text = (GameObject.Find("ResolutionSlider").GetComponent<Slider>().value + GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value).ToString();
        GameObject[] studioNotes = GameObject.FindGameObjectsWithTag("StudioNote");
        for (int i = 0; i < studioNotes.Length; i++)
        {
            studioNotes[i].GetComponent<StudioNoteScript>().length = (int)(notesList[i].length * (GameObject.Find("ResolutionSlider").GetComponent<Slider>().value + GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value) / noteCanvasResolution);
            studioNotes[i].transform.localPosition = new Vector3(notesList[i].time * (GameObject.Find("ResolutionSlider").GetComponent<Slider>().value + GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value) / noteCanvasResolution + noteOffset, studioNotes[i].transform.localPosition.y, 0);
        }
    }

    void FretPress(int number, bool pressed)
    {
        if (pressed)
            fretsList[number].sprite = studioFretsPressed[number];
        else
            fretsList[number].sprite = studioFrets[number];
    }

    public void MusicControl(float value)
    {
        musicAudioSource.timeSamples = Convert.ToInt32(value);
    }

    private void CreateNote(int number)
    {
        GameObject studioNoteClone = Instantiate(studioNotePrefab);
        float posY = 0;
        studioNoteClone.tag = "StudioNote";
        studioNoteClone.transform.SetParent(noteCanvas.transform);
        switch (number)
        {
            case 0: posY = 60; break;
            case 1: posY = 30; break;
            case 2: posY = 0; break;
            case 3: posY = -30; break;
            case 4: posY = -60; break;
        }
        studioNoteClone.GetComponent<Image>().sprite = studioNotes[number];
        studioNoteClone.GetComponent<StudioNoteScript>().note = number;
        studioNoteClone.GetComponent<StudioNoteScript>().length = 0;
        studioNoteClone.transform.localPosition = new Vector3(noteCanvas.transform.localPosition.x * -1 - 300, posY, 0);
        studioNoteClone.transform.localScale = new Vector3(1, 1, 1);
    }

    public void CheckSongName(string str)
    {
        if (GameObject.Find("InputFieldName").GetComponent<InputField>().text == "" || GameObject.Find("InputFieldName").GetComponent<InputField>().text == "Song Name")
        {
            GameObject.Find("SaveButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
            GameObject.Find("ImportButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
            GameObject.Find("ImportChartButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
            GameObject.Find("ImportPicButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        else
        {
            GameObject.Find("SaveButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
            GameObject.Find("ImportButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
            GameObject.Find("ImportChartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
            GameObject.Find("ImportPicButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }

    public void ImportButton()
    {
        OpenFileDialog openFile = new OpenFileDialog();
        openFile.Title = "Select two music files in .mp3 and .ogg formats";
        openFile.Multiselect = true;
        openFile.Filter = "Music Files|*.mp3;*.ogg";
        openFile.ShowDialog();
        string[] files = openFile.FileNames;
        if (files.Length != 2) return;
        Directory.CreateDirectory(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text);
        for (int i = 0; i < 2; i++)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(files[i]);
            System.IO.File.WriteAllBytes(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text + "/music" + files[i].Substring(files[i].LastIndexOf(@".")), bytes);
        }
        MainManager.instance.songName = GameObject.Find("InputFieldName").GetComponent<InputField>().text;
        SoundManager.instance.DestroyAllSounds();
        StopAllCoroutines();
        WWW audioLoad = new WWW("file:///" + MainManager.instance.songsPath + @"/Songs/" + MainManager.instance.songName + @"/music" + MainManager.instance.songsFormat);
        while (!audioLoad.isDone) { }
        musicAudioSource = SoundManager.instance.PrepareSound(audioLoad.audioClip);
        GameObject.Find("TimeSamplesSlider").GetComponent<Slider>().maxValue = musicAudioSource.clip.samples;
        noteCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(musicAudioSource.clip.samples / noteCanvasResolution, 170);
        StartCoroutine(SyncNoteCanvas());
    }

    public void ImportChartButton()
    {
        OpenFileDialog openFile = new OpenFileDialog();
        openFile.Title = "Select chart";
        openFile.Filter = "Chart Files|*.chart";
        openFile.ShowDialog();
        Directory.CreateDirectory(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text);
        byte[] bytes = System.IO.File.ReadAllBytes(openFile.FileName);
        System.IO.File.WriteAllBytes(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text + "/notes.chart", bytes);
        MainManager.instance.songName = GameObject.Find("InputFieldName").GetComponent<InputField>().text;
        GameObject.Find("ResolutionSlider").GetComponent<Slider>().value = 0;
        GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value = 0;
        ReadChart();
        PrepareNoteCanvas(0);
    }

    public void ImportPicButton()
    {
        OpenFileDialog openFile = new OpenFileDialog();
        openFile.Title = "Select picture";
        openFile.Filter = "Chart Files|*.png";
        openFile.ShowDialog();
        Directory.CreateDirectory(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text);
        byte[] bytes = System.IO.File.ReadAllBytes(openFile.FileName);
        System.IO.File.WriteAllBytes(MainManager.instance.songsPath + @"\Songs\" + GameObject.Find("InputFieldName").GetComponent<InputField>().text + "/pic.png", bytes);
    }

    private void CreateNewSong()
    {
        GameObject.Find("ResolutionSlider").GetComponent<Slider>().value = 0;
        GameObject.Find("MicroResolutionSlider").GetComponent<Slider>().value = 0;
        GameObject.Find("InputField").GetComponent<InputField>().text = 1.ToString();
        musicAudioSource.clip = null;
        noteOffset = 0;
        GameObject.Find("InputFieldOffset").GetComponent<InputField>().text = 0.ToString();
        GameObject.Find("InputFieldName").GetComponent<InputField>().text = "Song Name";
        GameObject.Find("InputFieldAuthor").GetComponent<InputField>().text = "Author";
        GameObject.Find("InputFieldCharter").GetComponent<InputField>().text = "Charter";
        GameObject.Find("TimeSamplesSlider").GetComponent<Slider>().maxValue = 0;
        noteCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 170);
        GameObject[] tempStudioNotes = GameObject.FindGameObjectsWithTag("StudioNote");
        if (tempStudioNotes.Length > 0)
            for (int i = 0; i < tempStudioNotes.Length; i++)
            {
                Destroy(tempStudioNotes[i]);
            }
        CheckSongName("");
    }

    float mouseXStartPos, mouseXFinishPos;
    public void MusicStudioControl()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            switch (MainManager.instance.layoutState)
            {
                case "STUDIOPAUSE":
                case "SONGSELECT":
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
                case "STUDIOPAUSE":
                case "SONGSELECT":
                    {
                        SoundManager.instance.CreateSound("Sounds/upClick", true, false);
                        UiManager.instance.MoveCursor(false);
                        break;
                    }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (MainManager.instance.layoutState)
            {
                case "STUDIO":
                    {
                        SoundManager.instance.CreateSound("Sounds/escClick", true, true);
                        if (!pause) PauseMusicButton();
                        UiManager.instance.DrawLayout("STUDIOPAUSE");
                        break;
                    }
                case "STUDIOPAUSE":
                case "SONGSELECT":
                    {
                        SoundManager.instance.CreateSound("Sounds/escClick", true, true);
                        UiManager.instance.DrawLayout("STUDIO");
                        break;
                    }
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (MainManager.instance.layoutState)
            {
                case "STUDIOPAUSE":
                    {
                        SoundManager.instance.CreateSound("Sounds/enterClick", true, true);
                        switch (UiManager.instance.GetCurrentSelection())
                        {
                            case "EXIT":
                                {
                                    MainManager.instance.gameMode = "menu";
                                    StopAllCoroutines();
                                    UiManager.instance.CreateLoadingScreen();
                                    SceneManager.LoadSceneAsync("mainScene", LoadSceneMode.Single);
                                    break;
                                }
                            case "NEW SONG":
                                {
                                    CreateNewSong();
                                    UiManager.instance.DrawLayout("STUDIO");
                                    break;
                                }
                            case "OPEN SONG":
                                {
                                    UiManager.instance.DrawLayout("SONGSELECT");
                                    break;
                                }
                        }
                        break;
                    }
                case "SONGSELECT":
                    {
                        SoundManager.instance.CreateSound("Sounds/enterClick", true, true);
                        MainManager.instance.songName = UiManager.instance.GetCurrentSelection();
                        LoadMusic();
                        UiManager.instance.DrawLayout("STUDIO");
                        break;
                    }
                case "STUDIO":
                    {
                        PauseMusicButton();
                        break;
                    }
            }
        }
        if (GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().currentSelectedGameObject != GameObject.Find("InputFieldName") &&
            GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().currentSelectedGameObject != GameObject.Find("InputFieldAuthor") &&
            GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().currentSelectedGameObject != GameObject.Find("InputFieldCharter"))
        {
            if (Input.GetKeyDown(MainManager.instance.keys[0]))
            {
                FretPress(0, true);
                CreateNote(0);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[1]))
            {
                FretPress(1, true);
                CreateNote(1);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[2]))
            {
                FretPress(2, true);
                CreateNote(2);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[3]))
            {
                FretPress(3, true);
                CreateNote(3);
            }
            if (Input.GetKeyDown(MainManager.instance.keys[4]))
            {
                FretPress(4, true);
                CreateNote(4);
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
        Vector2 realPos = Input.mousePosition;
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("UiCanvas").GetComponent<RectTransform>(), new Vector2(realPos.x, realPos.y), GameObject.Find("MainCamera").GetComponent<Camera>(), out canvasPos);
        if (canvasPos.y < 80 && canvasPos.y > -80)
            if (pause)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mouseXStartPos = Input.mousePosition.x;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    mouseXFinishPos = Input.mousePosition.x - mouseXStartPos;
                    noteOffset += mouseXFinishPos;
                    GameObject.Find("InputFieldOffset").GetComponent<InputField>().text = noteOffset.ToString();
                    MoveNoteCanvas(0);
                }
            }
    }

    float xpos = 0;
    IEnumerator SyncNoteCanvas()
    {
        xpos = 0;
        while (true)
        {
            xpos = (musicAudioSource.timeSamples / noteCanvasResolution) - xpos;
            noteCanvas.transform.localPosition = new Vector3(-(musicAudioSource.timeSamples / noteCanvasResolution) - 300, 0, 0);
            GameObject.Find("TimeSamplesSlider").GetComponent<Slider>().value = musicAudioSource.timeSamples;
            yield return 0;
        }

    }
}
