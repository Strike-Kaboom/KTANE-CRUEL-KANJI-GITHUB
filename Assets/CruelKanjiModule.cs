using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using KModkit;

public class CruelKanjiModule : MonoBehaviour
{
    //General
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMSelectable[] Keys;
    public KMSelectable[] SubmitButtons;
    public TextMesh[] KeysText;
    public TextMesh[] SubmitText;
    public TextMesh ScreenText;
    public GameObject Underline;
    public Material[] ButtonColor;
    public Renderer[] KeyRender;
    public Renderer[] SubmitRender;

    private bool ModuleSolved;
    private bool Calculating;
    private bool Unicorn;
    private int Stage = 1;

    private List<string> UserInputs = new List<string>();

    private static readonly string[] Stage1Words = { "きく", "あいだ", "わたし", "わすれる", "はな", "ところ", "すき", "やすい", "はつ", "すべて", "ざつ", "みどり", "むらさき", "けん", "あん" };
    private static readonly string[] Stage2Words = { "門耳", "門日", "禾厶", "亠亡心", "化匕艾", "一尸戸斤", "女子", "女宀", "二儿癶", "个王入", "九木隹", "ヨ水糸", "匕小幺止糸", "人个刈口", "女宀木" };
    private static readonly string[] Stage3Words = { "I Am The Bomb", };
    private static readonly string[] Stage1Radicals = { "耳", "門", "日", "厶", "禾", "亠", "亡", "心", "匕", "亻", "戸", "斤", "女", "子", "宀", "二", "儿", "癶", "个", "王", "九", "木", "隹", "小", "幺", "ヨ", "匕", "止", "人", "刈", "口", "艾", "一", "尸", "糸" };
    private static readonly string[] Stage2Radicals = { "きく", "あいだ", "わたし", "わすれる", "はな", "ところ", "すき", "やすい", "はつ", "すべて", "ざつ", "みどり", "むらさき", "けん", "あん", "つぎ" };
    private static readonly string[] Stage3Radicals = { "私", "は", "爆弾", "です", "流", "行", "性", "感", "冒" };
    private static readonly string[] SubmitButtonWords = { "へん", "つくり", "かんむり", "あし", "かまえ", "たれ", "にょう", "確認"};

    readonly Dictionary<string, List<string>> Combinations = new Dictionary<string, List<string>>()
    {
        //Stage 1
        {"きく", new List<string> {"耳", "門"}},
        {"あいだ", new List<string> {"日", "門"}},
        {"わたし", new List<string> {"厶", "禾"}},
        {"わすれる", new List<string> {"亠", "亡", "心"}},
        {"はな", new List<string> {"化", "匕", "艾"}},
        {"ところ", new List<string> {"一", "尸", "戸", "斤" }},
        {"すき", new List<string> {"女", "子"}},
        {"やすい", new List<string> {"女", "宀"}},
        {"はつ", new List<string> {"二", "儿", "癶"}},
        {"すべて", new List<string> {"个", "王", "入"}},
        {"ざつ", new List<string> {"九", "木", "隹"}},
        {"みどり", new List<string> {"糸", "ヨ", "水"}},
        {"むらさき", new List<string> {"匕", "小", "幺", "止", "糸"}},
        {"けん", new List<string> {"人", "个", "刈", "口"}},
        {"あん", new List<string> {"女", "宀", "木"}},

        //Stage 2
        {"門耳", new List<string> {"きく"}},
        {"門日", new List<string> {"あいだ"}},
        {"禾厶", new List<string> {"わたし"}},
        {"亠亡心", new List<string> {"わすれる"}},
        {"化匕艾", new List<string> {"はな"}},
        {"一尸戸斤", new List<string> {"ところ"}},
        {"女子", new List<string> {"すき"}},
        {"女宀", new List<string> {"やすい"}},
        {"二儿癶", new List<string> {"はつ"}},
        {"个王入", new List<string> {"すべて"}},
        {"九木隹", new List<string> {"ざつ"}},
        {"ヨ水糸", new List<string> {"みどり"}},
        {"匕小幺止糸", new List<string> {"むらさき"}},
        {"人个刈口", new List<string> {"けん"}},
        {"女宀木", new List<string> {"あん"}},

        //Stage 3
        {"I am the bomb", new List<string> {"私", "は", "爆弾", "です"}},
        //....
    };

    readonly Dictionary<string, string> SubmitCom = new Dictionary<string, string>
    {
        //Stage 1
        { "きく", "かまえ" },
        { "あいだ", "かまえ" },
        { "わたし", "へん" },
        { "わすれる", "あし" },
        { "はな", "かんむり" },
        { "ところ", "へん" },
        { "すき", "へん" },
        { "やすい", "かんむり" },
        { "はつ", "かんむり" },
        { "すべて", "かんむり" },
        { "ざつ", "つくり" },
        { "みどり", "へん" },
        { "むらさき", "かまえ" },
        { "けん", "つくり" },
        { "あん", "あし" },

        //Stage 2
        { "門耳", "かまえ" },
        { "門日", "かまえ" },
        { "禾厶", "へん" },
        { "亠亡心", "あし" },
        { "化匕艾", "かんむり" },
        { "一尸戸斤", "つくり" },
        { "女子", "へん" },
        { "女宀", "かんむり" },
        { "二儿癶", "かんむり" },
        { "个王入", "かんむり" },
        { "九木隹", "つくり" },
        { "ヨ水糸", "へん" },
        { "匕小幺止糸", "かまえ" },
        { "人个刈口", "つくり" },
        { "女宀木", "あし" },

        //Stage 3
        { "私は爆弾です", "確認" },
        //....
    };

    private List<string> moduleNames = new List<string>();

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        List<string> Words = new List<string>(SubmitButtonWords);
        List<TextMesh> Texts = new List<TextMesh>(SubmitText);

        foreach (TextMesh Key in Texts)
        {
            string Word = Words[UnityEngine.Random.Range(0, Words.Count)];
            Words.Remove(Word);
            switch (Word.Length)
            {
                case 2:
                    Key.fontSize = 80;
                    break;
                case 3:
                    Key.fontSize = 55;
                    break;
                case 4:
                    Key.fontSize = 40;
                    break;
            }
            Key.text = Word;
        }

        foreach (KMSelectable Key in Keys)
        {
            KMSelectable PressedKey = Key;
            Key.OnInteract += delegate () { KeyPress(PressedKey); return false; };
            Key.transform.Find("KeyPHL").gameObject.SetActive(false);
        }

        foreach (KMSelectable Submit in SubmitButtons)
        {
            Submit.OnInteract += delegate ()
            {
                if (!ModuleSolved && !Calculating)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Submit.transform);
                    Submit.AddInteractionPunch(.4f);
                    var i = string.Join(",", UserInputs.ToArray());
                    Debug.LogFormat("[Cruel Kanji #{0}] You submitted {1}", moduleId, i);
                    var RequiredButtons = Combinations[ScreenText.text];
                    var RequiredSubmitButton = SubmitCom[ScreenText.text];
                    var SubmitT = Submit.GetComponentInChildren<TextMesh>().text;
                    var j = string.Join(",", RequiredButtons.ToArray());

                    foreach (KMSelectable Key in Keys)
                    {
                        Key.transform.Find("KeyPHL").gameObject.SetActive(false);
                    }

                    if (UserInputs.Count != RequiredButtons.Count)
                    {
                        Debug.LogFormat("[Cruel Kanji #{0}] Incorrect! The amount of Radicals needed to pass this stage was not satisfactory. The correct sequence was {1}. Strike issued and resetting stage...", moduleId, j);
                        StartCoroutine(HandleStrike());
                        return false;
                    }

                    if (!UserInputs.All(input => RequiredButtons.Contains(input)))
                    {
                        Debug.LogFormat("[Cruel Kanji #{0}] Incorrect! The required Radicals were not pressed at the time of submission. The correct sequence was {1}. Strike issued and resetting stage...", moduleId, j);
                        StartCoroutine(HandleStrike());
                        return false;
                    }

                    if (SubmitT != RequiredSubmitButton)
                    {
                        Debug.LogFormat("[Cruel Kanji #{0}] Incorrect! The required submit button was not pressed at the time of submission. The correct submit button was {1}. Strike issued and resetting stage...", moduleId, RequiredSubmitButton);
                        StartCoroutine(HandleStrike());
                        return false;
                    }
                    Calculating = true;
                    if (Stage == 4)
                        Debug.LogFormat("[Cruel Kanji #{0}] Module Solved!", moduleId);
                    else
                        Debug.LogFormat("[Cruel Kanji #{0}] Correct! Advancing stage...", moduleId);
                    Stage++;
                    HandleAnimations();
                    return false;
                }
                return false;
            };
        }
    }

    // Initialization
    void Start()
    {
        moduleNames = Bomb.GetModuleNames();
        for (int i = 0; i < moduleNames.Count; i++)
        {
            if (moduleNames[i] == "Kanji")
            {
                Unicorn = true;
            }
        }
        PickChar();
    }

    void PickChar()
    {
        Debug.LogFormat("[Cruel Kanji #{0}] Current Stage is {1}.", moduleId, Stage);
        UserInputs.Clear();
        var WordList = new[] { Stage1Words, Stage2Words, Stage3Words };
        var StageWords = WordList[Stage - 1];
        ScreenText.text = StageWords[UnityEngine.Random.Range(0, StageWords.Count())];
        var RadicalList = new[] { Stage1Radicals, Stage2Radicals, Stage3Radicals };
        List<string> StageRadicals = new List<string>(RadicalList[Stage - 1]);
        List<TextMesh> Texts = new List<TextMesh>(KeysText);

        foreach (string CorrectRadical in Combinations[ScreenText.text])
        {
            TextMesh TextMesh = Texts[UnityEngine.Random.Range(0, Texts.Count)];
            Texts.Remove(TextMesh);
            switch (CorrectRadical.Length)
            {
                case 2:
                    TextMesh.fontSize = 70;
                    break;
                case 3:
                    TextMesh.fontSize = 60;
                    break;
                case 4:
                    TextMesh.fontSize = 40;
                    break;
                case 5:
                    TextMesh.fontSize = 20;
                    break;
            }
            TextMesh.text = CorrectRadical;
            StageRadicals.Remove(CorrectRadical);
        }

        foreach (TextMesh Key in Texts)
        {
            string FakeRadical = StageRadicals[UnityEngine.Random.Range(0, StageRadicals.Count)];
            StageRadicals.Remove(FakeRadical);
            switch (FakeRadical.Length)
            {
                case 2:
                    Key.fontSize = 70;
                    break;
                case 3:
                    Key.fontSize = 60;
                    break;
                case 4:
                    Key.fontSize = 40;
                    break;
                case 5:
                    Key.fontSize = 20;
                    break;
            }
            Key.text = FakeRadical;
        }
        Calculating = false;
    }

    void KeyPress(KMSelectable Key)
    {
        if (!ModuleSolved && !Calculating)
        {
            string KeyRadical = Key.GetComponentInChildren<TextMesh>().text;

            if (UserInputs.Contains(KeyRadical))
            {
                UserInputs.Remove(KeyRadical);
                Debug.LogFormat("[Cruel Kanji #{0}] Removed '{1}'.", moduleId, KeyRadical);
            }
            else
            {
                UserInputs.Add(KeyRadical);
                Debug.LogFormat("[Cruel Kanji #{0}] Added '{1}'.", moduleId, KeyRadical);
            }

            Key.transform.Find("KeyPHL").gameObject.SetActive(UserInputs.Contains(KeyRadical));
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Key.transform);
            Key.AddInteractionPunch(.4f);
        }
        else
        {
            return;
        }
    }

    IEnumerator HandleStrike()
    {
        int a = 0;
        for (int i = 0; i < 2; i++)
        {
            switch (i)
            {
                case 0:
                    foreach (Renderer Key in KeyRender)
                    {
                        Key.material = ButtonColor[6];
                    }
                    ScreenText.color = new Color(1f, 0.15f, 0);
                    break;
                case 1:
                    foreach (Renderer Key in KeyRender)
                    {
                        Key.material = ButtonColor[0];
                    }
                    ScreenText.color = new Color(1f, 1f, 1f);
                    break;
            }
            if (i == 1 && a < 3)
            {
                i = -1;
                a++;
            }
            yield return new WaitForSeconds(0.1f);
        }
        GetComponent<KMBombModule>().HandleStrike();
        PickChar();
    }

    void HandleAnimations()
    {
        if (Stage == 4)
        {
            ModuleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            foreach (KMSelectable Key in Keys)
            {
                Key.GetComponentInChildren<TextMesh>().text = "";
            }

            foreach (KMSelectable Key in SubmitButtons)
            {
                Key.GetComponentInChildren<TextMesh>().text = "";
            }

            ScreenText.text = "";
            Underline.SetActive(false);
            StartCoroutine(SubmitAnim());
            StartCoroutine(SubButAnim());
        }
        if (Stage <= 3)
        {
            StartCoroutine(Flower());
        }
    }

    IEnumerator SubButAnim()
    {
        for (int i = 0; i < 14; i++)
        {

            switch (i)
            {
                case 0:
                    SubmitRender[0].material = ButtonColor[1];
                    break;
                case 1:
                    SubmitRender[1].material = ButtonColor[1];
                    break;
                case 2:
                    SubmitRender[2].material = ButtonColor[1];
                    break;
                case 3:
                    SubmitRender[3].material = ButtonColor[1];
                    break;
                case 4:
                    SubmitRender[4].material = ButtonColor[1];
                    break;
                case 5:
                    SubmitRender[5].material = ButtonColor[1];
                    break;
                case 6:
                    SubmitRender[6].material = ButtonColor[1];
                    break;
                case 7:
                    SubmitRender[0].material = ButtonColor[0];
                    break;
                case 8:
                    SubmitRender[1].material = ButtonColor[0];
                    break;
                case 9:
                    SubmitRender[2].material = ButtonColor[0];
                    break;
                case 10:
                    SubmitRender[3].material = ButtonColor[0];
                    break;
                case 11:
                    SubmitRender[4].material = ButtonColor[0];
                    break;
                case 12:
                    SubmitRender[5].material = ButtonColor[0];
                    break;
                case 13:
                    SubmitRender[6].material = ButtonColor[0];
                    break;
            }
            if (i == 13)
                i = -1;
            yield return new WaitForSeconds(0.1f);
        }

    }

    IEnumerator Flower()
    {
        foreach (KMSelectable Key in Keys)
        {
            Key.transform.Find("KeyText").gameObject.SetActive(false);
        }
        for (int i = 0; i < 27; i++)
        {
            switch (i)
            {
                case 1:
                    KeyRender[1].material = ButtonColor[7];
                    break;
                case 2:
                    KeyRender[2].material = ButtonColor[7];
                    break;
                case 3:
                    KeyRender[7].material = ButtonColor[7];
                    break;
                case 4:
                    KeyRender[11].material = ButtonColor[7];
                    break;
                case 5:
                    KeyRender[14].material = ButtonColor[7];
                    break;
                case 6:
                    KeyRender[13].material = ButtonColor[7];
                    break;
                case 7:
                    KeyRender[8].material = ButtonColor[7];
                    break;
                case 8:
                    KeyRender[4].material = ButtonColor[7];
                    break;
                case 9:
                    KeyRender[5].material = ButtonColor[6];
                    break;
                case 10:
                    KeyRender[6].material = ButtonColor[6];
                    break;
                case 11:
                    KeyRender[10].material = ButtonColor[6];
                    break;
                case 12:
                    KeyRender[9].material = ButtonColor[6];
                    break;
                case 13:
                    KeyRender[0].material = ButtonColor[2];
                    KeyRender[3].material = ButtonColor[2];
                    KeyRender[12].material = ButtonColor[2];
                    KeyRender[15].material = ButtonColor[2];
                    yield return new WaitForSeconds(1f);
                    KeyRender[0].material = ButtonColor[0];
                    KeyRender[3].material = ButtonColor[0];
                    KeyRender[12].material = ButtonColor[0];
                    KeyRender[15].material = ButtonColor[0];
                    break;
                case 14:
                    KeyRender[1].material = ButtonColor[0];
                    break;
                case 15:
                    KeyRender[2].material = ButtonColor[0];
                    break;
                case 16:
                    KeyRender[7].material = ButtonColor[0];
                    break;
                case 17:
                    KeyRender[11].material = ButtonColor[0];
                    break;
                case 18:
                    KeyRender[14].material = ButtonColor[0];
                    break;
                case 19:
                    KeyRender[13].material = ButtonColor[0];
                    break;
                case 20:
                    KeyRender[8].material = ButtonColor[0];
                    break;
                case 21:
                    KeyRender[4].material = ButtonColor[0];
                    break;
                case 22:
                    KeyRender[5].material = ButtonColor[0];
                    break;
                case 23:
                    KeyRender[6].material = ButtonColor[0];
                    break;
                case 24:
                    KeyRender[10].material = ButtonColor[0];
                    break;
                case 25:
                    KeyRender[9].material = ButtonColor[0];
                    break;
            }
            yield return new WaitForSeconds(0.1f);
            if (i == 25 && Stage == 4)
            {
                i = -1;
            }
        }

        if (Stage < 3)
        {
            foreach (KMSelectable Key in Keys)
            {
                Key.transform.Find("KeyText").gameObject.SetActive(true);
            }
        }

        if (Stage == 3)
        {
            ScreenText.fontSize = 50;
            int i = 0;
            foreach (KMSelectable j in SubmitButtons)
            {
                while (i != 30)
                {
                    yield return new WaitForSeconds(0.01f);
                    j.transform.localPosition = j.transform.localPosition + Vector3.up * -0.00021f;
                    i++;
                }
            }
        }
        PickChar();
    }

    IEnumerator SubmitAnim()
    {
        foreach (KMSelectable Key in Keys)
        {
            Key.transform.Find("KeyText").gameObject.SetActive(false);
        }
        foreach (KMSelectable Key in SubmitButtons)
        {
            Key.GetComponentInChildren<TextMesh>().text = "";
        }
        ScreenText.text = "";
        Underline.SetActive(false);
        int aa = 0;
        for (int i = 0; i < 1; i++)
        {
            int a = UnityEngine.Random.Range(0, 16);
            int b = UnityEngine.Random.Range(1, 7);
            switch (i)
            {
                case 0:
                    KeyRender[a].material = ButtonColor[b];
                    yield return new WaitForSeconds(0.15f);
                    KeyRender[a].material = ButtonColor[0];
                    break;
            }
            if (i == 0 && aa < 50)
            {
                i = -1;
                aa++;
            }
            else
                StartCoroutine(Flower());
        }
    }

    IEnumerator Tetris()
    {
        if (ModuleSolved)
        {
            StopCoroutine(Flower());
            StopCoroutine(SubButAnim());
            //Tetris starts here
            for (int i = 0; i < 14; i++)
            {
                switch (i)
                {
                    case 0:
                        KeyRender[1].material = ButtonColor[4];
                        KeyRender[4].material = ButtonColor[4];
                        KeyRender[5].material = ButtonColor[4];
                        KeyRender[6].material = ButtonColor[4];
                        yield return new WaitForSeconds(1);
                        break;
                    case 1:
                        KeyRender[1].material = ButtonColor[0];
                        KeyRender[4].material = ButtonColor[0];
                        KeyRender[6].material = ButtonColor[0];
                        KeyRender[8].material = ButtonColor[4];
                        KeyRender[9].material = ButtonColor[4];
                        KeyRender[10].material = ButtonColor[4];
                        yield return new WaitForSeconds(1);
                        break;
                    case 2:
                        KeyRender[5].material = ButtonColor[0];
                        KeyRender[8].material = ButtonColor[0];
                        KeyRender[10].material = ButtonColor[0];
                        KeyRender[12].material = ButtonColor[4];
                        KeyRender[13].material = ButtonColor[4];
                        KeyRender[14].material = ButtonColor[4];
                        yield return new WaitForSeconds(1);
                        break;
                    case 3:
                        KeyRender[2].material = ButtonColor[3];
                        KeyRender[5].material = ButtonColor[3];
                        KeyRender[6].material = ButtonColor[3];
                        KeyRender[10].material = ButtonColor[3];
                        yield return new WaitForSeconds(0.9f);
                        break;
                    case 4:
                        KeyRender[5].material = ButtonColor[0];
                        KeyRender[2].material = ButtonColor[0];
                        KeyRender[10].material = ButtonColor[0];
                        KeyRender[3].material = ButtonColor[3];
                        KeyRender[7].material = ButtonColor[3];
                        KeyRender[11].material = ButtonColor[3];
                        yield return new WaitForSeconds(0.8f);
                        break;
                    case 5:
                        KeyRender[3].material = ButtonColor[0];
                        KeyRender[6].material = ButtonColor[0];
                        KeyRender[10].material = ButtonColor[3];
                        KeyRender[15].material = ButtonColor[3];
                        yield return new WaitForSeconds(0.8f);
                        break;
                    case 6:
                        KeyRender[12].material = ButtonColor[0];
                        KeyRender[13].material = ButtonColor[0];
                        KeyRender[14].material = ButtonColor[0];
                        KeyRender[15].material = ButtonColor[0];
                        yield return new WaitForSeconds(0.08f);
                        break;
                    case 7:
                        KeyRender[9].material = ButtonColor[0];
                        KeyRender[7].material = ButtonColor[0];
                        KeyRender[10].material = ButtonColor[0];
                        KeyRender[13].material = ButtonColor[4];
                        KeyRender[14].material = ButtonColor[3];
                        KeyRender[15].material = ButtonColor[3];
                        KeyRender[11].material = ButtonColor[3];
                        yield return new WaitForSeconds(0.8f);
                        break;
                    case 8:
                        KeyRender[0].material = ButtonColor[6];
                        KeyRender[4].material = ButtonColor[6];
                        KeyRender[8].material = ButtonColor[6];
                        KeyRender[9].material = ButtonColor[6];
                        yield return new WaitForSeconds(0.9f);
                        break;
                    case 9:
                        KeyRender[0].material = ButtonColor[0];
                        KeyRender[4].material = ButtonColor[0];
                        KeyRender[12].material = ButtonColor[6];
                        KeyRender[10].material = ButtonColor[6];
                        yield return new WaitForSeconds(0.9f);
                        break;
                    case 10:
                        KeyRender[12].material = ButtonColor[0];
                        KeyRender[13].material = ButtonColor[0];
                        KeyRender[14].material = ButtonColor[0];
                        KeyRender[15].material = ButtonColor[0];
                        yield return new WaitForSeconds(0.7f);
                        break;
                    case 11:
                        KeyRender[8].material = ButtonColor[0];
                        KeyRender[9].material = ButtonColor[0];
                        KeyRender[10].material = ButtonColor[0];
                        KeyRender[11].material = ButtonColor[0];
                        KeyRender[12].material = ButtonColor[6];
                        KeyRender[13].material = ButtonColor[6];
                        KeyRender[14].material = ButtonColor[6];
                        KeyRender[15].material = ButtonColor[3];
                        yield return new WaitForSeconds(0.6f);
                        break;
                    case 12:
                        KeyRender[12].material = ButtonColor[0];
                        KeyRender[13].material = ButtonColor[0];
                        KeyRender[14].material = ButtonColor[0];
                        KeyRender[15].material = ButtonColor[0];
                        yield return new WaitForSeconds(1);
                        break;
                    case 13:
                        StartCoroutine(Flower());
                        StartCoroutine(SubButAnim());
                        break;
                }

            }
        }
        yield break;
    }
}