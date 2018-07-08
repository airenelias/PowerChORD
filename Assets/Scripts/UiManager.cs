using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    public GameObject textPrefab;
    public GameObject songNamePrefab;
    public GameObject modPrefab;
    public GameObject controlsPrefab;
    private List<GameObject> menuObjectsList = new List<GameObject>();
    private List<string> menuItemsList = new List<string>();
    GameObject uiCanvas;
    public int cursorPosition = 0;
    GameObject background, loadingScreen;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Start()
    {
        uiCanvas = GameObject.Find("UiCanvas");
    }

    public void ShakeText(string name, bool enabled)
    {
        GameObject textClone = GameObject.Find(name);
        TextScript script = textClone.GetComponent<TextScript>();
        Text text = textClone.GetComponent<Text>();
        if (script == null || text == null)
        {
            script = textClone.GetComponentInChildren<TextScript>();
            text = textClone.GetComponentInChildren<Text>();
        }
        script.enabled = enabled;
        if (enabled) text.color = new Color32(255, 170, 30, 255);
        else text.color = Color.white;
    }

    public void CreateTextElement(string text, Vector3 pos, bool center, bool header)
    {
        GameObject textClone = Instantiate(textPrefab);
        textClone.name = text;
        textClone.tag = "MenuElement";
        textClone.transform.SetParent(uiCanvas.transform);
        textClone.transform.localPosition = pos;
        textClone.transform.Rotate(0, 0, UnityEngine.Random.Range(-1f, 1f));
        textClone.transform.localScale = new Vector3(1f, 1f, 1f);
        Text textText = textClone.GetComponent<Text>();
        textText.text = text;
        if (center)
        {
            textText.fontSize = 50;
            textText.alignment = TextAnchor.MiddleCenter;
        }
        if (header)
        {
            textText.color = new Color32(255, 90, 0, 255);
            textText.fontSize = 60;
            textText.alignment = TextAnchor.MiddleRight;
            textClone.transform.localPosition = new Vector3(0, textClone.transform.localPosition.y, 0);
            textClone.GetComponent<TextMove>().header = true;
        }
    }

    public void CreateLoadingScreen()
    {
        DestroyAllText();
        loadingScreen = Instantiate((GameObject)Resources.Load("Prefabs/loadingScreenPrefab"));
        loadingScreen.transform.SetParent(uiCanvas.transform);
        loadingScreen.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        loadingScreen.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        loadingScreen.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        loadingScreen.transform.localPosition = new Vector3(0, 0, 0);
        loadingScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        loadingScreen.transform.localScale = new Vector3(1, 1, 1);
    }

    public void CreateBackground()
    {
        background = Instantiate((GameObject)Resources.Load("Prefabs/backgroundPrefab"));
        background.transform.SetParent(uiCanvas.transform);
        background.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        background.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        background.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        background.transform.localPosition = new Vector3(0, 0, 0);
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        background.transform.localScale = new Vector3(1, 1, 1);
    }

    public void CreateTexts(string header, string[] menuItems)
    {
        CreateTextElement(header, new Vector3(300f, 160), false, true);
        for (int i = 0, h = 100; i < menuItems.Length; i++, h -= 60)
        {
            menuItemsList.Add(menuItems[i]);
            CreateTextElement(menuItemsList[i], new Vector3(-320f, h), false, false);
        }
        cursorPosition = 0;
        if (MainManager.instance.platform != RuntimePlatform.Android) ShakeText(menuItemsList[cursorPosition], true);
    }

    public void CreateTexts(string header)
    {
        menuItemsList.Add(header);
        CreateTextElement(header, new Vector3(0, 0), true, false);
        cursorPosition = 0;
        ShakeText(menuItemsList[cursorPosition], true);
    }

    public void MoveCursor(bool up)
    {
        ShakeText(menuItemsList[cursorPosition], false);
        if (up) cursorPosition--;
        else cursorPosition++;
        if (cursorPosition < 0) cursorPosition = menuItemsList.Count - 1;
        if (cursorPosition > menuItemsList.Count - 1) cursorPosition = 0;
        ShakeText(menuItemsList[cursorPosition], true);
    }

    public string GetCurrentSelection()
    {
        if (menuItemsList[cursorPosition] != null) return menuItemsList[cursorPosition];
        else return null;
    }

    public void DestroyAllText()
    {
        Destroy(background);
        Destroy(loadingScreen);
        GameObject[] tempGameObjects = GameObject.FindGameObjectsWithTag("MenuElement");
        if (tempGameObjects.Length > 0) for (int i = 0; i < tempGameObjects.Length; i++)
            {
                Destroy(tempGameObjects[i]);
            }
        menuItemsList.RemoveRange(0, menuItemsList.Count);
        menuObjectsList.RemoveRange(0, menuObjectsList.Count);
    }

    public void CreateModElement(Sprite pic, string name, Vector3 pos, bool selected)
    {
        GameObject modClone = Instantiate(modPrefab);
        modClone.name = name;
        modClone.tag = "MenuElement";
        modClone.transform.SetParent(uiCanvas.transform);
        modClone.transform.localPosition = pos;
        modClone.transform.localScale = new Vector3(1f, 1f, 1f);
        modClone.GetComponentInChildren<Image>().sprite = pic;
        Text text = modClone.GetComponentInChildren<Text>();
        text.text = name;
        if (selected)
            modClone.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 255);
        else
            modClone.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 100);
        menuObjectsList.Add(modClone);
    }

    public void ModSelect(string name, bool selected)
    {
        if (selected)
            GameObject.Find(name).GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 255);
        else
            GameObject.Find(name).GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 100);
    }

    public void CreateSongElement(Texture2D pic, string name, string author, string charter, Vector3 pos)
    {
        GameObject songNameClone = Instantiate(songNamePrefab);
        songNameClone.name = name;
        songNameClone.tag = "MenuElement";
        songNameClone.transform.SetParent(uiCanvas.transform);
        songNameClone.transform.localPosition = pos;
        songNameClone.transform.localScale = new Vector3(1f, 1f, 1f);
        songNameClone.GetComponentInChildren<RawImage>().texture = pic;
        Text[] tempTextArray = songNameClone.GetComponentsInChildren<Text>();
        tempTextArray[0].text = name;
        tempTextArray[1].text = author;
        tempTextArray[2].text = charter;
    }

    public void CreateControlsElement(string controlsname, string controlskey, Vector3 pos)
    {
        GameObject controlsClone = Instantiate(controlsPrefab);
        controlsClone.name = controlsname;
        controlsClone.tag = "MenuElement";
        controlsClone.transform.SetParent(uiCanvas.transform);
        controlsClone.transform.localPosition = pos;
        controlsClone.transform.localScale = new Vector3(1f, 1f, 1f);
        Text[] tempTextArray = controlsClone.GetComponentsInChildren<Text>();
        tempTextArray[0].text = controlsname;
        tempTextArray[1].text = controlskey;
    }

    public void ControlsSelect(int menuItem)
    {
        GameObject controlsClone = GameObject.Find(menuItemsList[menuItem]);
        Text[] tempTextArray = controlsClone.GetComponentsInChildren<Text>();
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                tempTextArray[1].text = kcode.ToString();
                MainManager.instance.keys[menuItem] = kcode;
                PlayerPrefs.SetInt(menuItemsList[menuItem], (int)(kcode));
            }
        }
    }

    public void CreateMods()
    {
        string[] menuItems = { "PLAY" };
        string[] modNames = MainManager.instance.mods.Keys.ToArray();
        Sprite[] picArray = new Sprite[modNames.Length];
        int i = 0;
        for (i = 0; i < picArray.Length; i++)
        {
            picArray[i] = Resources.Load<Sprite>("Sprites/modpic" + modNames[i]);
        }
        CreateTexts("select mods", menuItems);
        i = 0;
        int h = 25;
        while (h > -200)
        {
            menuItemsList.Add(modNames[i]);
            CreateModElement(picArray[i], modNames[i], new Vector3(-250f, h, 0), MainManager.instance.mods[modNames[i]]);
            i++; h -= 60;
            if (h < -200 || i == modNames.Length) break;
        }
        h = 25;
        while (h > -200 && i != modNames.Length)
        {
            menuItemsList.Add(modNames[i]);
            CreateModElement(picArray[i], modNames[i], new Vector3(100f, h, 0), MainManager.instance.mods[modNames[i]]);
            i++; h -= 60;
            if (h < -200 || i == modNames.Length) break;
        }
    }

    public void CreateSongs()
    {
        string[] dirs = Directory.GetDirectories(MainManager.instance.songsPath + @"/Songs/");
        List<string> songArray = new List<string>();
        for (int i = 0; i < dirs.Length; i++)
            if (File.Exists(dirs[i] + "/music.mp3") && File.Exists(dirs[i] + "/music.ogg") && File.Exists(dirs[i] + "/info.ini") && File.Exists(dirs[i] + "/pic.png"))
                songArray.Add(dirs[i]);
        CreateTextElement("select a song", new Vector3(-320, 160), false, true);
        for (int i = 0, h = 85; i < songArray.Count; i++, h -= 60)
        {
            FileStream iniFileStream = new FileStream(songArray[i] + "/info.ini", FileMode.Open);
            StreamReader iniStreamReader = new StreamReader(iniFileStream);
            string tempString = iniStreamReader.ReadLine();
            string name = tempString.Substring(tempString.IndexOf("=") + 1);
            tempString = iniStreamReader.ReadLine();
            string author = tempString.Substring(tempString.IndexOf("=") + 1);
            tempString = iniStreamReader.ReadLine();
            string charter = tempString.Substring(tempString.IndexOf("=") + 1);
            WWW picLoad = new WWW("file:///" + MainManager.instance.songsPath.Replace(@"\", @"/") + @"/Songs/" + name + @"/pic.png");
            Texture2D pic = new Texture2D(500, 500);
            while (!picLoad.isDone) { }
            pic = picLoad.texture;
            CreateSongElement(pic, name, author, charter, new Vector3(-250f, h, 0));
            menuItemsList.Add(name);
            iniStreamReader.Close();
            iniFileStream.Close();
        }
        cursorPosition = 0;
        if (MainManager.instance.platform != RuntimePlatform.Android) ShakeText(menuItemsList[cursorPosition], true);
    }

    public void CreateControls()
    {
        DestroyAllText();
        CreateTextElement("CONTROLS", new Vector3(-320, 160), false, true);
        string[] controlsList = { "GREEN FRET", "RED FRET", "YELLOW FRET", "BLUE FRET", "ORANGE FRET" };
        for (int i = 0, h = 85; i < 5; i++, h -= 60)
        {
            menuItemsList.Add(controlsList[i]);
            CreateControlsElement(controlsList[i], MainManager.instance.keys[i].ToString(), new Vector3(-250f, h, 0));
        }
        cursorPosition = 0;
        if (MainManager.instance.platform != RuntimePlatform.Android) ShakeText(menuItemsList[cursorPosition], true);
    }

    public void CreateArrowBack()
    {
        GameObject arrowbackClone = Instantiate(Resources.Load("Prefabs/arrowback") as GameObject);
        arrowbackClone.tag = "MenuElement";
        arrowbackClone.transform.SetParent(uiCanvas.transform);
        arrowbackClone.transform.localPosition = new Vector3(-337f, 158f, 0f);
        arrowbackClone.transform.localScale = new Vector3(1f, 1f, 1f);
        arrowbackClone.GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 40f);
    }

    public void DrawLayout(string layout)
    {
        DestroyAllText();
        switch (layout)
        {
            case "INTRO":
                {
                    CreateTexts("PRESS ANY BUTTON TO CONTINUE");
                    break;
                }
            case "MAIN":
                {
                    if (MainManager.instance.platform != RuntimePlatform.Android)
                    {
                        string[] menuItems = { "SINGLEPLAYER", "MUSIC STUDIO", "OPTIONS", "EXIT" };
                        CreateTexts("POWERCHORD", menuItems);

                    }
                    else
                    {
                        string[] menuItems = { "SINGLEPLAYER", "EXIT" };
                        CreateTexts("POWERCHORD", menuItems);
                    }
                    break;
                }
            case "OPTIONS":
                {
                    string[] menuItems = { "VOLUME " + (Mathf.Ceil((MainManager.instance.volume * 100))).ToString(), "CONTROLS" };
                    CreateTexts("OPTIONS", menuItems);
                    break;
                }
            case "OPTIONSCONTROLS":
                {
                    CreateControls();
                    break;
                }
            case "SINGLEPLAYER":
                {
                    CreateSongs();
                    break;
                }
            case "MODS":
                {
                    CreateMods();
                    break;
                }
            case "STUDIO":
                {
                    break;
                }
            case "STUDIOPAUSE":
                {
                    CreateBackground();
                    string[] menuItems = { "NEW SONG", "OPEN SONG", "EXIT" };
                    CreateTexts("MUSIC STUDIO", menuItems);
                    break;
                }
            case "PLAY":
            case "GAME":
                {
                    break;
                }
            case "PAUSE":
                {
                    CreateBackground();
                    string[] menuItems = { "CONTINUE", "RESTART", "EXIT" };
                    CreateTexts("PAUSE", menuItems);
                    break;
                }
            case "WIN":
                {
                    break;
                }
            case "SONGSELECT":
                {
                    CreateBackground();
                    CreateSongs();
                    break;
                }
            default:
                {
                    return;
                }
        }
        MainManager.instance.layoutState = layout;
    }
}
