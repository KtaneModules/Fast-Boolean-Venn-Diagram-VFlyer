using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FastBooleanVennDiagramScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Renderer[] brends;
    public Material[] mats;
    public Transform timer;
    public Renderer timerend;
    public TextMesh display;

    private string[] venn = new string[16] { "O", "A", "B", "AB", "C", "AC", "BC", "ABC", "D", "AD", "BD", "ABD", "CD", "ACD", "BCD", "ABCD"};
    private bool[][] truth = new bool[2][] { new bool[16], new bool[16]};
    private int[] sets = new int[] { 0, 1, 2, 3 };
    private int[] ops;
    private int order;
    private bool pressable, timerRunning, accelerateTimer = false;

    float timeAllowedNormal = 30f, timeAllowedTP = 60f;
    const string sectors = "ABCD";
    const string operators = "ʌV▵|↧≡↦↤";

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;
    FBVDSettings storedSettings = new FBVDSettings();

    void QuickLogDebug(string toLog, params object[] args)
    {
        Debug.LogFormat("<Fast Boolean Venn Diagram #{0}> {1}", moduleID, string.Format(toLog, args));
    }
    void QuickLog(string toLog, params object[] args)
    {
        Debug.LogFormat("[Fast Boolean Venn Diagram #{0}] {1}", moduleID, string.Format(toLog, args));
    }
    public class FBVDSettings
    {
        public float startTimeNonTP = 30f;
        public float startTimeTP = 60f;
    }
    private void Awake()
    {
        try
        {
            var FBVDCurSettings = new ModConfig<FBVDSettings>("FastBooleanVennDiagram");
            storedSettings = FBVDCurSettings.Settings;
            FBVDCurSettings.Settings = storedSettings;
            timeAllowedNormal = storedSettings.startTimeNonTP;
            timeAllowedTP = storedSettings.startTimeTP;
        }
        catch
        {
            Debug.LogWarning("<Fast Boolean Venn Diagram Settings> Settings do not work as intended! Using default settings!");
            timeAllowedNormal = 30f;
            timeAllowedTP = 60f;
        }
    }

	private void Start()
    {
        moduleID = ++moduleIDCounter;
        QuickLogDebug("Max time for TP: {0}; Max time otherwise: {1}", timeAllowedTP, timeAllowedNormal);
        for (int i = 0; i < buttons.Count; i++)
        {
            KMSelectable button = buttons[i];
            int b = i;
            button.OnInteract = delegate ()
            {
                if (!moduleSolved)
                    if (pressable)
                    {
                        button.AddInteractionPunch(0.1f);
                        truth[1][b] ^= true;
                        brends[b].material = mats[truth[1][b] ? 1 : 0];
                        StartCoroutine(Sound(b > 7, (b / 4) % 2 > 0, (b / 2) % 2 > 0, b % 2 > 0));
                    }
                    else if (!timerRunning)
                    {
                        button.AddInteractionPunch(0.1f);
                        StartCoroutine(ActivateTimer());
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                    }
                return false;
            };
        }
        for (int i = 0; i < 16; i++)
        {
            brends[i].material = mats[0];
            truth[1][i] = false;
        }
        QuickLog("Press any sector to activate the module.");
    }

    private IEnumerator Sound(bool a, bool b, bool c, bool d)
    {
        int i = new bool[4] { a, b, c, d }.Count(x => x);
        if (i == 0)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
            yield break;
        }
        if (a)
        {
            Audio.PlaySoundAtTransform("tock", transform);
            yield return new WaitForSeconds(0.2f / i);
        }
        if (b)
        {
            Audio.PlaySoundAtTransform("tack", transform);
            yield return new WaitForSeconds(0.2f / i);
        }
        if (c)
        {
            Audio.PlaySoundAtTransform("teck", transform);
            yield return new WaitForSeconds(0.2f / i);
        }
        if (d)
            Audio.PlaySoundAtTransform("tick", transform);
    }

    private bool Op(bool a, bool b, int o)
    {
        switch (o)
        {
            case 0: return a && b;
            case 1: return a || b;
            case 2: return a ^ b;
            case 3: return !a || !b;
            case 4: return !a && !b;
            case 5: return a == b;
            case 6: return !a || b;
            default: return a || !b;
        }
    }

    IEnumerator ActivateTimer()
    {
        timerRunning = true;
        float e = 0;
        string disp = "";
        pressable = true;
        timerend.material.color = new Color(0, 1, 0);
        display.color = new Color(1, 1, 0);
        for (int i = 0; i < 16; i++)
        {
            brends[i].material = mats[0];
            truth[1][i] = false;
        }
        sets.Shuffle();
        ops = new int[3] { Random.Range(0, 8), Random.Range(0, 8), Random.Range(0, 8) };
        disp = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
        order = Random.Range(0, 5);
        // 1 2 3 4
        // 1 (2 3) 4
        // 1 (2 3 4)
        // 1 (2 (3 4))
        // 1 2 (3 4)
        switch (order)
        {
            case 1: disp = string.Format("{0}{1}({2}{3}{4}){5}{6}",
                sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]); break;
            case 2:
                disp = string.Format("{0}{1}({2}{3}{4}{5}{6})",
            sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
            operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]); break;
            case 3:
                disp = string.Format("{0}{1}({2}{3}({4}{5}{6}))",
            sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
            operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]); break;
            case 4:
                disp = string.Format("{0}{1}{2}{3}({4}{5}{6})",
                sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]); break;
        }
        display.text = disp;
        QuickLog("The expression displayed is \"{0}\".", disp);
        var loggingOperations = string.Format("(({0}{1}{2}){3}{4}){5}{6}",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
        switch (order)
        {
            case 1:
                loggingOperations = string.Format("({0}{1}({2}{3}{4})){5}{6}",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
                break;
            case 2:
                loggingOperations = string.Format("{0}{1}(({2}{3}{4}){5}{6})",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
                break;
            case 3:
                loggingOperations = string.Format("{0}{1}({2}{3}({4}{5}{6}))",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
                break;
            case 4:
                loggingOperations = string.Format("({0}{1}{2}){3}({4}{5}{6})",
                        sectors[sets[0]], operators[ops[0]], sectors[sets[1]],
                        operators[ops[1]], sectors[sets[2]], operators[ops[2]], sectors[sets[3]]);
                break;

        }
        QuickLog("The expression if left-to-right rules were visualized is \"{0}\".", loggingOperations);
        for (int i = 0; i < 16; i++)
        {
            bool[] a = new bool[4] { (i & 1) == 1, (i >> 1 & 1) == 1, (i >> 2 & 1) == 1, i > 7 };
            switch (order)
            {
                default:
                case 0: truth[0][i] = Op(Op(Op(a[sets[0]], a[sets[1]], ops[0]), a[sets[2]], ops[1]), a[sets[3]], ops[2]); break;
                case 1: truth[0][i] = Op(Op(a[sets[0]], Op(a[sets[1]], a[sets[2]], ops[1]), ops[0]), a[sets[3]], ops[2]); break;
                case 2: truth[0][i] = Op(a[sets[0]], Op(Op(a[sets[1]], a[sets[2]], ops[1]), a[sets[3]], ops[2]), ops[0]); break;
                case 3: truth[0][i] = Op(a[sets[0]], Op(a[sets[1]], Op(a[sets[2]], a[sets[3]], ops[2]), ops[1]), ops[0]); break;
                case 4: truth[0][i] = Op(Op(a[sets[0]], a[sets[1]], ops[0]), Op(a[sets[2]], a[sets[3]], ops[2]), ops[1]); break;
            }
        }
        QuickLog("I expect the following sections to be pressed: {0}.", string.Join(", ", venn.Where((x, i) => truth[0][i]).ToArray()));
        var curMaxTime = TwitchPlaysActive ? timeAllowedTP : timeAllowedNormal;
        e = curMaxTime;
        while (e > 0)
        {
            e -= Time.deltaTime * (accelerateTimer ? 10f : 1f);
            timerend.material.color = new Color(Mathf.Min(1, (curMaxTime - e) / (curMaxTime / 2)), Mathf.Max(0, e / (curMaxTime / 2)), 0);
            timer.localPosition = new Vector3(Mathf.Lerp(0.95f, 0, e / curMaxTime), 0, 0.1f);
            timer.localScale = new Vector3(Mathf.Lerp(0, 0.19f, e / curMaxTime), 1, 0.15f);
            yield return null;
        }
        for (int i = 0; i < 16; i++)
            brends[i].material = mats[(truth[1][i] ? 2 : 0) + (truth[0][i] ? 1 : 2)];
        QuickLog("Submitted{0}.", truth[1].All(x => !x) ? " nothing" : (": " + string.Join(", ", venn.Where((x, i) => truth[1][i]).ToArray())));
        if(Enumerable.Range(0, 16).Select(x => truth[0][x] == truth[1][x]).All(x => x))
        {
            moduleSolved = true;
            timerend.enabled = false;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            module.HandlePass();
        }
        else
        {
            if (Enumerable.Range(0, 16).Any(x => truth[0][x] ^ truth[1][x]))
                module.HandleStrike();
            pressable = false;
            while(e < 15)
            {
                e += Time.deltaTime;
                timer.localPosition = new Vector3(Mathf.Lerp(0.95f, 0, e / 15), 0, 0.1f);
                timer.localScale = new Vector3(Mathf.Lerp(0, 0.19f, e / 15), 1, 0.15f);
                yield return null;
            }
            timerend.material.color = Color.green;
            timer.localScale = new Vector3(0.19f, 1, 0.15f);
            timerRunning = false;
            for (int i = 0; i < 16; i++)
            {
                brends[i].material = mats[0];
                truth[1][i] = false;
            }
            display.text = "";
        }
    }
#pragma warning disable 414
    private bool TwitchPlaysActive;
    private readonly string TwitchHelpMessage = @"!{0} activate [Activates the module.] | !{0} O/<ABCD> [Selects a section corresponding to A, B, C, D; order does not matter. Multiple sections can be chained with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToUpperInvariant().Split(' ');
        if (command.Trim().EqualsIgnoreCase("activate"))
        {
            if (timerRunning)
            {
                yield return "sendtochaterror The timer is already active! Get your sections in before time runs out!";
                yield break;
            }
            yield return null;
            buttons.PickRandom().OnInteract();
            yield return "strike";
            yield break;
        }
        List<int> s = new List<int>();
        for(int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if (commands[i].All(x => "OABCD".Contains(x.ToString())))
            {
                if (commands[i].Contains("O"))
                {
                    if (commands[i] == "O")
                        s.Add(0);
                    else
                    {
                        yield return "sendtochaterror \"" + commands[i] + "\" is not a valid segment.";
                        yield break;
                    }
                }
                else
                {
                    if (commands[i].GroupBy(x => x).Any(x => x.Count() > 1))
                    {
                        yield return "sendtochaterror \"" + commands[i] + "\" is not a valid segment. Segments must not contain duplicate characters.";
                        yield break;
                    }
                    int d = 0;
                    if (commands[i].Contains("D"))
                        d = 8;
                    if (commands[i].Contains("C"))
                        d += 4;
                    if (commands[i].Contains("B"))
                        d += 2;
                    if (commands[i].Contains("A"))
                        d += 1;
                    if (s.Contains(d))
                    {
                        yield return "sendtochaterror \"" + commands[i] + "\" appears more than once in the command.";
                        yield break;
                    }
                    s.Add(d);
                }
            }
            else
            {
                yield return "sendtochaterror \"" + commands[i] + "\" is not a valid segment.";
                yield break;
            }
        }
        if (s.Any())
        {
            if (!timerRunning)
            {
                yield return "sendtochaterror The timer is not active! Use \"!{1} activate\" to initiate the module.";
                yield break;
            }
            yield return null;
            for (int i = 0; i < s.Count(); i++)
            {
                buttons[s[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            yield return truth[0].SequenceEqual(truth[1]) ? "solve" : "strike";
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            if (!timerRunning)
            {
                buttons.PickRandom().OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            while (!truth[0].SequenceEqual(truth[1]))
            {
                accelerateTimer = false;
                for (int i = 0; i < 16; i++)
                {
                    if (truth[0][i] ^ truth[1][i])
                    {
                        buttons[i].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            accelerateTimer = true;
            yield return true;
        }
    }
}
