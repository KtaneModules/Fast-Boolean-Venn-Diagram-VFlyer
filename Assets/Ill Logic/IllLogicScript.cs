using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IllLogicScript : MonoBehaviour {

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
    private string sickness = "ABCDʌV▵|↧≡↦↤()";
    private int[] sets = new int[4] { 0, 1, 2, 3};
    private int[] ops = new int[3];
    private int order;
    private bool pressable;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

	private void Awake ()
    {
        moduleID = ++moduleIDCounter;
        sickness = string.Join("", sickness.ToCharArray().Shuffle().Select(x => x.ToString()).ToArray());
        Debug.LogFormat("[Ill Logic #{0}] The displayed characters have the corresponding meanings:", moduleID);
        for (int i = 0; i < 14; i++)
            Debug.LogFormat("[Ill Logic #{0}] {1} \u2192 {2}", moduleID, sickness[i], "ABCDʌV▵|↧≡↦↤()"[i]);
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                if (!moduleSolved && pressable)
                {
                    button.AddInteractionPunch(0.1f);
                    truth[1][b] ^= true;
                    brends[b].material = mats[truth[1][b] ? 1 : 0];
                    StartCoroutine(Sound(b > 7, (b / 4) % 2 > 0, (b / 2) % 2 > 0, b % 2 > 0));
                }
                return false;
            };
        }
        StartCoroutine("Do");
	}

    private IEnumerator Sound(bool a, bool b, bool c, bool d)
    {
        int i = new bool[4] { a, b, c, d }.Count(x => x);
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
        if(d)
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

    private IEnumerator Do()
    {
        float e = 0;
        string disp = "";
        while (!moduleSolved)
        {
            pressable = true;
            timerend.material.color = new Color(0, 1, 0);
            display.color = new Color(1, 1, 0);
            for (int i = 0; i < 16; i++)
            {
                brends[i].material = mats[0];
                truth[1][i] = false;
            }
            sets = sets.Shuffle();
            ops = new int[3] { Random.Range(0, 8), Random.Range(0, 8), Random.Range(0, 8) };
            disp = "ABCD"[sets[0]].ToString() + "ʌV▵|↧≡↦↤"[ops[0]].ToString() + "ABCD"[sets[1]].ToString() + "ʌV▵|↧≡↦↤"[ops[1]].ToString() + "ABCD"[sets[2]].ToString() + "ʌV▵|↧≡↦↤"[ops[2]].ToString() + "ABCD"[sets[3]].ToString(); ;
            order = Random.Range(0, 5);
            // 1 2 3 4
            // 1 (2 3) 4
            // 1 (2 3 4)
            // 1 (2 (3 4))
            // 1 2 (3 4)
            switch (order)
            {
                case 1: disp = disp.Insert(5, ")").Insert(2, "("); break;
                case 2: disp = disp.Insert(2, "(") + ")"; break;
                case 3: disp = disp.Insert(4, "(").Insert(2, "(") + "))"; break;
                case 4: disp = disp.Insert(4, "(") + ")"; break;
            }
            display.text = string.Join("", disp.Select(x => sickness["ABCDʌV▵|↧≡↦↤()".IndexOf(x.ToString())].ToString()).ToArray());
            Debug.LogFormat("[Ill Logic #{0}] The display reads \"{1}\".", moduleID, display.text);
            Debug.LogFormat("[Ill Logic #{0}] Decoding the display yields \"{1}\".", moduleID, disp);
            for (int i = 0; i < 16; i++)
            {
                bool[] a = new bool[4] { i % 2 > 0, (i / 2) % 2 > 0, (i / 4) % 2 > 0, i > 7 };
                switch (order)
                {
                    case 0: truth[0][i] = Op(Op(Op(a[sets[0]], a[sets[1]], ops[0]), a[sets[2]], ops[1]), a[sets[3]], ops[2]); break;
                    case 1: truth[0][i] = Op(Op(a[sets[0]], Op(a[sets[1]], a[sets[2]], ops[1]), ops[0]), a[sets[3]], ops[2]); break;
                    case 2: truth[0][i] = Op(a[sets[0]], Op(Op(a[sets[1]], a[sets[2]], ops[1]), a[sets[3]], ops[1]), ops[0]); break;
                    case 3: truth[0][i] = Op(a[sets[0]], Op(a[sets[1]], Op(a[sets[2]], a[sets[3]], ops[2]), ops[1]), ops[0]); break;
                    default: truth[0][i] = Op(Op(a[sets[0]], a[sets[1]], ops[0]), Op(a[sets[2]], a[sets[3]], ops[2]), ops[1]); break;
                }
            }
            Debug.LogFormat("[Ill Logic #{0}] Expectation: {1}.", moduleID, string.Join(", ", venn.Where((x, i) => truth[0][i]).ToArray()));
            e = 90;
            while (e > 0)
            {
                e -= Time.deltaTime;
                timerend.material.color = new Color(Mathf.Min(1, (90 - e) / 45), Mathf.Max(0, e / 45), 0);
                timer.localPosition = new Vector3(Mathf.Lerp(0.95f, 0, e / 90), 0, 0.1f);
                timer.localScale = new Vector3(Mathf.Lerp(0, 0.19f, e / 90), 1, 0.15f);
                yield return null;
            }
            for (int i = 0; i < 16; i++)
                brends[i].material = mats[(truth[1][i] ? 2 : 0) + (truth[0][i] ? 1 : 2)];
            Debug.LogFormat("[Ill Logic #{0}] Submitted{1}.", moduleID, truth[1].All(x => !x) ? " nothing" : (": " + string.Join(", ", venn.Where((x, i) => truth[1][i]).ToArray())));
            if(Enumerable.Range(0, 16).Select(x => truth[0][x] == truth[1][x]).All(x => x))
            {
                moduleSolved = true;
                timerend.enabled = false;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                display.text = disp;
                module.HandlePass();
            }
            else
            {
                if (Enumerable.Range(0, 16).Select(x => !truth[0][x] && truth[1][x]).Any(x => x))
                    module.HandleStrike();
                pressable = false;
                while(e < 15)
                {
                    e += Time.deltaTime;
                    timer.localPosition = new Vector3(Mathf.Lerp(0.95f, 0, e / 15), 0, 0.1f);
                    timer.localScale = new Vector3(Mathf.Lerp(0, 0.19f, e / 15), 1, 0.15f);
                    yield return null;
                }
            }
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} O/<ABCD> [Selects segment. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToUpperInvariant().Split(' ');
        List<int> s = new List<int> { };
        for(int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if(commands[i].All(x => "OABCD".Contains(x.ToString())))
            {
                if (commands[i].Contains("O"))
                {
                    if (commands[i] == "O")
                        s.Add(0);
                    else
                    {
                        yield return "sendtochaterror!f \"" + commands[i] + "\" is not a valid segment.";
                        continue;
                    }
                }
                else
                {
                    if(commands[i].GroupBy(x => x).Any(x => x.Count() > 1))
                    {
                        yield return "sendtochaterror!f \"" + commands[i] + "\" is not a valid segment. Segments must not contain duplicate characters.";
                        continue;
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
                        yield return "sendtochat!f \"" + commands[i] + "\" appears more than once in the command.";
                        continue;
                    }
                    s.Add(d);
                }
            }
            else
                yield return "sendtochaterror!f \"" + commands[i] + "\" is not a valid segment.";
        }
        for(int i = 0; i < s.Count(); i++)
        {
            yield return new WaitForSeconds(0.1f);
            buttons[s[i]].OnInteract();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for(int i = 0; i < 16; i++)
        {
            if (truth[0][i] ^ truth[1][i])
                buttons[i].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
