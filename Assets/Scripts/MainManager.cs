using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class MainManager : MonoBehaviour
{
    public static MainManager instance;
    public float volume = 1;
    public KeyCode[] keys = new KeyCode[6];
    public Dictionary<string, bool> mods = new Dictionary<string, bool>();
    public string songName = "";
    public string layoutState = "";
    public string gameMode = "launch";
    public RuntimePlatform platform;
    public string songsPath;
    public string songsFormat;
    public float resolutionScaler = 1f;
    public float speed = 1f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        SettingsLoad();
        SceneManager.LoadScene("mainScene");
    }

    public void SettingsLoad()
    {
        string[] controlsList = { "GREEN FRET", "RED FRET", "YELLOW FRET", "BLUE FRET", "ORANGE FRET" };
        if (PlayerPrefs.HasKey("volume") && PlayerPrefs.HasKey("GREEN FRET"))
        {
            volume = PlayerPrefs.GetFloat("volume");
            for (int i = 0; i < controlsList.Length; i++)
            {
                keys[i] = (KeyCode)PlayerPrefs.GetInt(controlsList[i]);
            }
        }
        else
        {
            SetDefaultKeys();
            
        }
        SetPlatform();
        SetDefaultMods();
        Application.runInBackground = true;
    }

    public void SetDefaultKeys()
    {
        keys[0] = KeyCode.LeftShift;
        keys[1] = KeyCode.Z;
        keys[2] = KeyCode.X;
        keys[3] = KeyCode.C;
        keys[4] = KeyCode.V;
    }

    public void SetDefaultMods()
    {
        mods.Add("LONGSIGHT", false);
        mods.Add("PERFORMANCE", false);
        mods.Add("DEAF", false);
        mods.Add("EPILEPTIC", false);
        mods.Add("SLOWED", false);
        mods.Add("ACCELERATED", false);
        mods.Add("AUTO", false);
    }

    private void SetPlatform()
    {
        platform = Application.platform;
        if (platform == RuntimePlatform.Android)
        {
            songsPath = Application.persistentDataPath;
            songsFormat = ".mp3";
            resolutionScaler = 2f;
        }
        else if (platform == RuntimePlatform.WindowsPlayer)
        {
            songsPath = Application.dataPath;
            songsFormat = ".ogg";
            resolutionScaler = 1f;
        }
        else
        {
            songsPath = Directory.GetCurrentDirectory();
            songsFormat = ".ogg";
            resolutionScaler = 1f;
        }
    }

    public void ImplementManagers()
    {
        UiManager.instance.Start();
        SoundManager.instance.Start();
        switch (gameMode)
        {
            case "launch":
                {
                    MenuManager.instance.Launch();
                    break;
                }
            case "menu":
                {
                    MenuManager.instance.Initialize();
                    break;
                }
            case "game":
                {
                    GameManager.instance.Initialize();
                    break;
                }
            case "studio":
                {
                    MusicStudioManager.instance.Initialize();
                    break;
                }
        }
    }

    private void UpdateControl()
    {
        switch (gameMode)
        {
            case "launch":
                {
                    MenuManager.instance.LaunchControl();
                    break;
                }
            case "menu":
                {
                    MenuManager.instance.MenuControl();
                    break;
                }
            case "game":
                {
                    GameManager.instance.GameControl();
                    break;
                }
            case "studio":
                {
                    MusicStudioManager.instance.MusicStudioControl();
                    break;
                }
        }
    }

    void Update()
    {
        UpdateControl();

    }
}
