using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class pipNadoScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] textButtons;
	public KMSelectable topHat;

	public TextMesh mainDisplay;
	public TextMesh timerDisplay;
	public TextMesh[] subDisplays;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	bool countdownTimer;

	private string[] pippyNatoAlphabet = { "Alpha", "Beta", "Crab", "D---", "Echo", "F---", "Glubbers_", "Hotel", "Igloo", "J", "Kilogram", "Lemon", "Margherita", "Nostril", "Octopuuus", "Poggers", "Quack!", "Ronald Reagan", "Sunscreen", "Terraforming", "Umbrella", "Vi-o-lin", "Wisconsin", "Xbox", "YEET", "Zuckerburg" };
	private int[] pippyNatoAlphabetValues = { 35, 88, 16, 42, 39, 91, 28, 55, 31, 68, 74, 57, 55, 77, 43, 49, 68, 15, 24, 95, 82, 18, 97, 30, 31, 89 };
	private string[] natoSequence = new string[4];
	private int[] natoSequenceIndex = new int[4];
	private int[] dispNumbers = new int[3];
	private int offset;
	private int calculatedNumber = 0;
	int stage = 0;

	void Awake()
    {

		moduleId = moduleIdCounter++;


		foreach (KMSelectable button in textButtons)
        {
			button.OnInteract += delegate () { subButtonPress(button); return false; };
        }

		topHat.OnInteract += delegate () { topHatPress(); return false; };

    }

	
	void Start()
    {
        mainDisplay.text = "";
		timerDisplay.text = "";

		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }
		textSelection();
		calcOffset();

    }

	void calcOffset()
    {
		int first = Bomb.GetSerialNumberNumbers().First();
		int last = Bomb.GetSerialNumberNumbers().Last();
		offset = int.Parse(first.ToString() + last.ToString());

		offset = (offset * Bomb.GetModuleNames().Count()) % 100;

		Debug.LogFormat("[Pip-Nado #{0}] The offset is {1}.", moduleId, offset);
    }

	void textSelection()
    {
		for (int i = 0; i < 4; i++)
        {
			natoSequenceIndex[i] = rnd.Range(0, pippyNatoAlphabet.Length);
			natoSequence[i] = pippyNatoAlphabet[natoSequenceIndex[i]];
        }
    }


    void subButtonPress(KMSelectable button)
    {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		button.AddInteractionPunch();
		if (moduleSolved || !countdownTimer)
        {
			return;
        }

		for (int i = 0; i < 3; i++)
        {
			if (button == textButtons[i])
            {
				dispNumbers[i]++;
				if (dispNumbers[i] > 9)
                {
					dispNumbers[i] = 0;
                }
				subDisplays[i].text = dispNumbers[i].ToString();
            }
        }
    }

	void initSubDisplay()
    {
		for (int i = 0; i < 3; i++)
        {
			dispNumbers[i] = 0;
			subDisplays[i].text = dispNumbers[i].ToString();
        }
    }

	void calcMain()
    {
		calculatedNumber = ((pippyNatoAlphabetValues[natoSequenceIndex[0]] + pippyNatoAlphabetValues[natoSequenceIndex[1]] + pippyNatoAlphabetValues[natoSequenceIndex[2]] + pippyNatoAlphabetValues[natoSequenceIndex[3]]) * offset) % 1000;
		Debug.LogFormat("[Pip-Nado #{0}] The final value is {1}.", moduleId, calculatedNumber);
    }

	void topHatPress()
    {
		topHat.AddInteractionPunch();
		if (moduleSolved)
        {
			return;
        }

		if (!countdownTimer && stage != 0) stage = 0;

        switch (stage)
        {
			case 0:
				if (!countdownTimer)
				{
					countdownTimer = true;
					StartCoroutine(countDownTimer());
					StartCoroutine(displaySequence());
					initSubDisplay();
					calcMain();
				}
				stage++;
				break;
			case 1:
				if (countdownTimer)
				{
					StopAllCoroutines();
					checkNumber();

				}
				break;
        }

	

    }

	void checkNumber()
    {
		countdownTimer = false;
		int finalNumber = int.Parse(dispNumbers[0].ToString() + dispNumbers[1].ToString() + dispNumbers[2].ToString());

		if (finalNumber == calculatedNumber)
        {
			StartCoroutine(solveStuff());
        }
        else
        {
			string strike = "Expected " + calculatedNumber.ToString() + ", but inputted " + finalNumber.ToString() + ".";
			strikeMessage(strike);
        }
	}

	IEnumerator solveStuff()
    {
		Debug.LogFormat("[Pip-Nado #{0}] That is correct! Module solved!", moduleId);
		moduleSolved = true;
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		GetComponent<KMBombModule>().HandlePass();
		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }
		timerDisplay.text = "";
		mainDisplay.text = "Solved!";
		yield return new WaitForSeconds(1.5f);
		mainDisplay.text = "";

		yield return null;
    }

	IEnumerator countDownTimer()
    {
		Debug.LogFormat("[Pip-Nado #{0}] Timer has started.", moduleId);
		int seconds = 90;
		while (countdownTimer && seconds != 0)
        {
			timerDisplay.text = seconds.ToString();
			seconds--;
			yield return new WaitForSeconds(1);
        }
		if (seconds == 0)
        {
			timerDisplay.text = "";
			strikeMessage("Time is up!");
			countdownTimer = false;
        }
		yield return null;
    }

	IEnumerator displaySequence()
    {
		Debug.LogFormat("[Pip-Nado #{0}] The displayed words in the sequence are: {1}, {2}, {3}, and {4}.", moduleId, natoSequence[0], natoSequence[1], natoSequence[2], natoSequence[3]);
		while (countdownTimer)
        {
			mainDisplay.text = natoSequence[0].ToString();
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = "";
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = natoSequence[1].ToString();
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = "";
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = natoSequence[2].ToString();
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = "";
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = natoSequence[3].ToString();
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = "";
			yield return new WaitForSeconds(0.4f);
		}
		yield return null;
    }

	void strikeMessage(string reason)
    {
		StopAllCoroutines();
		mainDisplay.text = "";
		timerDisplay.text = "";
		GetComponent<KMBombModule>().HandleStrike();
		Debug.LogFormat("[Pip-Nado #{0}] Strike! {1} Resetting...", moduleId, reason);
		textSelection();
		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }
    }


	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} start to initialize the module. | !{0} submit 0-9 to input your number. Make sure to use spaces when submitting.";
#pragma warning restore 414

	private bool validNumbers(string c)
    {
		string[] valids = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
		if (!valids.Contains(c))
        {
			return false;
        }
		return true;
    }

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] commands = command.ToUpper().Split(' ');
		yield return null;



		if (commands[0].Equals("START"))
        {
			if (!countdownTimer)
            {
				topHat.OnInteract();
            }
			else if (countdownTimer)
            {
				yield return "sendtochaterror The timer has already started!";
            }
			yield break;
        }


		if (commands[0].Equals("SUBMIT"))
        {
			if (!countdownTimer)
            {
				yield return "sendtochaterror You haven't started the module yet!";
				yield break;
            }
			if (commands.Length > 4)
            {
				yield return "sendtochaterror You can't submit over 3 numbers!";
            }
			else if (commands.Length == 4)
            {
				if (validNumbers(commands[1]))
				{
					if (validNumbers(commands[2]))
					{
						if (validNumbers(commands[3]))
						{
							int temp1 = 0;
							int temp2 = 0;
							int temp3 = 0;
							int.TryParse(commands[1], out temp1);
							int.TryParse(commands[2], out temp2);
							int.TryParse(commands[3], out temp3);

							while (temp1 != dispNumbers[0])
                            {
								textButtons[0].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }
							while (temp2 != dispNumbers[1])
                            {
								textButtons[1].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }
							while (temp3 != dispNumbers[2])
                            {
								textButtons[2].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }
							topHat.OnInteract();
						}
                        else
                        {
							yield return "sendtochaterror '" + commands[3] + "' is an invalid value!";
                        }
					}
                    else
                    {
						yield return "sendtochaterror '" + commands[2] + "' is an invalid value!";
                    }
				}
                else
                {
					yield return "sendtochat error '" + commands[1] + "' is an invalid value!";
                }
			}
			else if (commands.Length == 3)
            {
				yield return "sendtochaterror Please specify the value of the third number!";
            }
			else if (commands.Length == 2)
            {
				yield return "sendtochaterror Please specify the values of the second and third numbers!";
            }
			else if (commands.Length == 1)
            {
				yield return "sendtochaterror Please specify the values of all three numbers!";
            }
			yield break;
			
        }





	}

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;

		if (!countdownTimer)
        {
			topHat.OnInteract();
		}

		int temp1 = calculatedNumber / 100;
		int temp2 = (calculatedNumber / 10) % 10;
		int temp3 = calculatedNumber % 10;

		while (temp1 != dispNumbers[0])
        {
			textButtons[0].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		while (temp2 != dispNumbers[1])
        {
			textButtons[1].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		while (temp3 != dispNumbers[2])
        {
			textButtons[2].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }

		topHat.OnInteract();



    }


}





