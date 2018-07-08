using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    private List<VisualizerFall> visualizers = new List<VisualizerFall>();
    public AudioSource musicAudioSource;
    public GameObject visualizerPrefab;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Launch()
    {
        musicAudioSource = SoundManager.instance.PrepareSound((AudioClip)Resources.Load("Sounds/menumusic"));
        UiManager.instance.DrawLayout("INTRO");
        SoundManager.instance.CreateSound("Sounds/intro" + UnityEngine.Random.Range(0, 6), true, false);
    }

    public void Initialize()
    {
        if (SoundManager.instance.preparedAudioSource == null) musicAudioSource = SoundManager.instance.PrepareSound((AudioClip)Resources.Load("Sounds/menumusic"));
        SoundManager.instance.PlayPreparedSound();
        CreateVisualizer();
        SoundManager.instance.StopSounds();
        UiManager.instance.DrawLayout("MAIN");
    }

    private void CreateVisualizer()
    {
        visualizers.RemoveRange(0, visualizers.Count);
        for (int i = 335, c = 0; i > (-335); i = i - 50, c++)
        {
            GameObject tempVisualizer = Instantiate(visualizerPrefab);
            tempVisualizer.name = "visualizer" + c.ToString();
            tempVisualizer.tag = "Visualizer";
            tempVisualizer.transform.SetParent(GameObject.Find("UiCanvas").transform);
            tempVisualizer.transform.localPosition = new Vector3(i, -200, 0);
            tempVisualizer.transform.localScale = new Vector3(1, 1, 1);
            tempVisualizer.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);
            visualizers.Add(tempVisualizer.GetComponent<VisualizerFall>());
        }
        StartCoroutine(MusicVisualizerCoroutine());
    }

    public void LaunchControl()
    {
        if (MainManager.instance.platform == RuntimePlatform.Android)
        {
            Touch touch;
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                MainManager.instance.gameMode = "menu";
                Initialize();
            }
        }
        else
        {
            if (Input.anyKeyDown)
            {
                MainManager.instance.gameMode = "menu";
                Initialize();
            }
        }
    }

    public void MenuControl()
    {
        if (MainManager.instance.platform == RuntimePlatform.Android)
        {
            Touch touch;
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("UiCanvas").GetComponent<RectTransform>(), new Vector2(touch.rawPosition.x, touch.rawPosition.y), GameObject.Find("MainCamera").GetComponent<Camera>(), out touchPos);
                switch (MainManager.instance.layoutState)
                {
                    case "INTRO":
                        {
                            CreateVisualizer();
                            SoundManager.instance.StopSounds();
                            SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                            SoundManager.instance.PlayPreparedSound();
                            UiManager.instance.DrawLayout("MAIN");
                            break;
                        }
                    case "MAIN":
                        {
                            SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                            if (touchPos.y < 130 && touchPos.y > 70) UiManager.instance.cursorPosition = 0;
                            if (touchPos.y < 70 && touchPos.y > 10) UiManager.instance.cursorPosition = 1;
                            if (touchPos.y < 10 && touchPos.y > -50) UiManager.instance.cursorPosition = 2;
                            switch (UiManager.instance.cursorPosition)
                            {
                                case 1: Application.Quit(); break;
                                case 0: SoundManager.instance.CreateSound("Sounds/enterClick", true, false); UiManager.instance.DrawLayout(UiManager.instance.GetCurrentSelection()); break;
                            }
                            break;
                        }
                    case "SINGLEPLAYER":
                        {
                            if (touchPos.y > 130 && touchPos.x < -200) { SoundManager.instance.CreateSound("Sounds/escClick", true, true); UiManager.instance.DrawLayout("MAIN"); }
                            else
                            {
                                if (touchPos.y < 130 && touchPos.y > 70) UiManager.instance.cursorPosition = 0;
                                if (touchPos.y < 70 && touchPos.y > 10) UiManager.instance.cursorPosition = 1;
                                if (touchPos.y < 10 && touchPos.y > -50) UiManager.instance.cursorPosition = 2;
                                if (touchPos.y < -50 && touchPos.y > -110) UiManager.instance.cursorPosition = 3;
                                if (touchPos.y < -110 && touchPos.y > -170) UiManager.instance.cursorPosition = 4;

                                MainManager.instance.songName = UiManager.instance.GetCurrentSelection();
                                UiManager.instance.DrawLayout("MODS");
                            }
                            break;
                        }
                    case "MODS":
                        {
                            if (touchPos.y > 130 && touchPos.x < -200) { SoundManager.instance.CreateSound("Sounds/escClick", true, true); UiManager.instance.DrawLayout("MAIN"); }
                            else
                            {
                                if (touchPos.y < 130 && touchPos.y > 70 && touchPos.x < 0) UiManager.instance.cursorPosition = 0;
                                if (touchPos.y < 70 && touchPos.y > 10 && touchPos.x < 0) UiManager.instance.cursorPosition = 1;
                                if (touchPos.y < 10 && touchPos.y > -50 && touchPos.x < 0) UiManager.instance.cursorPosition = 2;
                                if (touchPos.y < -50 && touchPos.y > -110 && touchPos.x < 0) UiManager.instance.cursorPosition = 3;
                                if (touchPos.y < -110 && touchPos.y > -170 && touchPos.x < 0) UiManager.instance.cursorPosition = 4;
                                if (touchPos.y < 70 && touchPos.y > 10 && touchPos.x > 0) UiManager.instance.cursorPosition = 5;
                                if (touchPos.y < 10 && touchPos.y > -50 && touchPos.x > 0) UiManager.instance.cursorPosition = 6;
                                if (touchPos.y < -50 && touchPos.y > -110 && touchPos.x > 0) UiManager.instance.cursorPosition = 7;
                            }
                            switch (UiManager.instance.GetCurrentSelection())
                            {
                                case "PLAY":
                                    {
                                        MainManager.instance.gameMode = "game";
                                        UiManager.instance.DrawLayout("GAME");
                                        UiManager.instance.CreateLoadingScreen();
                                        SceneManager.LoadSceneAsync("gameScene", LoadSceneMode.Single);
                                        break;
                                    }
                                default:
                                    {
                                        SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                                        MainManager.instance.mods[UiManager.instance.GetCurrentSelection()] = !MainManager.instance.mods[UiManager.instance.GetCurrentSelection()];
                                        UiManager.instance.ModSelect(UiManager.instance.GetCurrentSelection(), MainManager.instance.mods[UiManager.instance.GetCurrentSelection()]);
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
        }
        else
        {
            if (Input.anyKeyDown)
            {
                if (MainManager.instance.layoutState == "OPTIONSCONTROLS")
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                    { }
                    else
                    {
                        SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                        UiManager.instance.ControlsSelect(UiManager.instance.cursorPosition);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SoundManager.instance.CreateSound("Sounds/upClick", true, false);
                UiManager.instance.MoveCursor(true);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SoundManager.instance.CreateSound("Sounds/downClick", true, false);
                UiManager.instance.MoveCursor(false);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "INTRO":
                        {
                            CreateVisualizer();
                            SoundManager.instance.StopSounds();
                            SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                            SoundManager.instance.PlayPreparedSound();
                            UiManager.instance.DrawLayout("MAIN");
                            break;
                        }
                    case "MAIN":
                        {
                            SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                            switch (UiManager.instance.GetCurrentSelection())
                            {
                                case "EXIT": Application.Quit(); break;

                                case "MUSIC STUDIO":
                                    {
                                        MainManager.instance.gameMode = "studio";
                                        UiManager.instance.DrawLayout("STUDIO");
                                        UiManager.instance.CreateLoadingScreen();
                                        SceneManager.LoadSceneAsync("musicStudioScene", LoadSceneMode.Single);
                                        break;
                                    }

                                default: UiManager.instance.DrawLayout(UiManager.instance.GetCurrentSelection()); break;
                            }
                            break;
                        }
                    case "SINGLEPLAYER":
                        {
                            MainManager.instance.songName = UiManager.instance.GetCurrentSelection();
                            UiManager.instance.DrawLayout("MODS");
                            break;
                        }
                    case "MODS":
                        {
                            switch (UiManager.instance.GetCurrentSelection())
                            {
                                case "PLAY":
                                    {
                                        MainManager.instance.gameMode = "game";
                                        UiManager.instance.DrawLayout("GAME");
                                        UiManager.instance.CreateLoadingScreen();
                                        SceneManager.LoadSceneAsync("gameScene", LoadSceneMode.Single);
                                        break;
                                    }
                                default:
                                    {
                                        SoundManager.instance.CreateSound("Sounds/enterClick", true, false);
                                        MainManager.instance.mods[UiManager.instance.GetCurrentSelection()] = !MainManager.instance.mods[UiManager.instance.GetCurrentSelection()];
                                        UiManager.instance.ModSelect(UiManager.instance.GetCurrentSelection(), MainManager.instance.mods[UiManager.instance.GetCurrentSelection()]);
                                        break;
                                    }
                            }
                            break;
                        }
                    case "OPTIONS":
                        {
                            switch (UiManager.instance.GetCurrentSelection())
                            {
                                case "CONTROLS": UiManager.instance.DrawLayout("OPTIONSCONTROLS"); break;
                            }
                            break;
                        }
                }

            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "OPTIONS":
                        {
                            if (UiManager.instance.GetCurrentSelection().Contains("VOLUME") && MainManager.instance.volume > 0)
                            {
                                SoundManager.instance.CreateSound("Sounds/upClick", true, false);
                                MainManager.instance.volume -= 0.1f;
                                PlayerPrefs.SetFloat("volume", MainManager.instance.volume);
                                SoundManager.instance.AdjustVolume();
                                UiManager.instance.DrawLayout("OPTIONS");
                            }
                            break;
                        }
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                switch (MainManager.instance.layoutState)
                {
                    case "OPTIONS":
                        {
                            if (UiManager.instance.GetCurrentSelection().Contains("VOLUME") && MainManager.instance.volume < 1)
                            {
                                SoundManager.instance.CreateSound("Sounds/downClick", true, false);
                                MainManager.instance.volume += 0.1f;
                                PlayerPrefs.SetFloat("volume", MainManager.instance.volume);
                                SoundManager.instance.AdjustVolume();
                                UiManager.instance.DrawLayout("OPTIONS");
                            }
                            break;
                        }
                }

            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (MainManager.instance.layoutState)
                {

                    case "SINGLEPLAYER": case "OPTIONS": SoundManager.instance.CreateSound("Sounds/escClick", true, true); UiManager.instance.DrawLayout("MAIN"); break;
                    case "MODS": SoundManager.instance.CreateSound("Sounds/escClick", true, true); UiManager.instance.DrawLayout("SINGLEPLAYER"); break;
                    case "OPTIONSCONTROLS": SoundManager.instance.CreateSound("Sounds/escClick", true, true); UiManager.instance.DrawLayout("OPTIONS"); break;
                }
            }
        }
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
