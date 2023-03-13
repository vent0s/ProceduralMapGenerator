using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enum = System.Enum;
using septim.core;

namespace septim.ui
{
    public class Console : MonoBehaviour
    {
        [SerializeField] UiManager gameMenu;
        [SerializeField] GameManager gameManager;
        [SerializeField] Text consoleText;
        [SerializeField] Text inputText;
        [SerializeField] InputField inputField;
        private string currentInput;
        private string currentDisplay;
        private string commandHolder1;
        private string commandHolder2;


        public void FuncInputText()
        {
            currentInput = inputText.text;
            FuncConsoleProcess(currentInput);
            currentDisplay = currentDisplay + "\n" + "  " + currentInput;
            consoleText.text = currentDisplay;
            inputText.text = "";
            inputField.text = "";
        }

        public enum GameCommands
        {
            help,
            debug,
            cash,
            //hardpower,
            author,
        }

        private string FuncAnalysingInput_mainPhrase(string inquireString)
        {
            char space = ' ';
            int a = (int)space;
            int b;
            string result = "";

            char insertAlphabet;
            for (int i = 0; i < inquireString.Length; i++)
            {
                b = (int)inquireString[i];
                if (b == a)
                {
                    i = inquireString.Length;
                }
                else
                {
                    insertAlphabet = inquireString[i];
                    result = result + insertAlphabet;
                }
            }
            return result;
        }

        private string FuncAnalysingInput_secondPhrase(string inquireString)
        {
            char space = ' ';
            int a = (int)space;
            int b;
            string result = "";

            char insertAlphabet;
            bool isDetected = false;
            for (int i = 0; i < inquireString.Length; i++)
            {
                b = (int)inquireString[i];
                if (isDetected)
                {
                    insertAlphabet = inquireString[i];
                    result = result + insertAlphabet;
                }
                else if (i == inquireString.Length - 1 && isDetected != true)
                {
                    result = "Nope";
                }
                else
                {
                    if (a == b)
                    {

                        isDetected = true;
                    }
                }
            }
            return result;
        }

        private string FuncAnalysingInput_ThirdPhrase(string inquireString)
        {
            char space = ' ';
            int a = (int)space;
            int b;
            string result = "";

            char insertAlphabet;
            bool isDetected_1 = false;
            bool isDetected_2 = false;
            for (int i = 0; i < inquireString.Length; i++)
            {
                b = (int)inquireString[i];
                if (isDetected_1 && isDetected_2)
                {
                    insertAlphabet = inquireString[i];
                    result = result + insertAlphabet;
                }
                else
                {
                    if (b == a && isDetected_1 == false)
                    {
                        isDetected_1 = true;
                    }
                    else if (b == a && isDetected_1 == true)
                    {
                        isDetected_2 = true;
                    }
                    else if (b == inquireString.Length - 1)
                    {
                        result = "Nope";
                    }
                }
            }
            return result;
        }

        private void FuncConsoleProcess(string inquireString)
        {
            string mainPhrase = FuncAnalysingInput_mainPhrase(inquireString);
            string secondPhrase = FuncAnalysingInput_secondPhrase(inquireString);
            string thirdPhrase = FuncAnalysingInput_ThirdPhrase(inquireString);

            switch (mainPhrase)
            {
                case "help":
                    if (secondPhrase != "Nope")
                    {
                        switch (secondPhrase)
                        {
                            case "help":
                                currentInput = "Why don't you ask the Magic Conch, Squidward?";
                                currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                                consoleText.text = currentDisplay;
                                currentInput = "============";
                                break;

                            case "cash":
                                currentInput = "[cash(number)] to get money";
                                currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                                consoleText.text = currentDisplay;
                                currentInput = "============";
                                break;

                            default:
                                currentInput = "ERROR:INVALID INPUT";
                                currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                                consoleText.text = currentDisplay;
                                currentInput = "============";
                                break;
                        }
                        break;
                    }
                    else
                    {
                        foreach (string name in Enum.GetNames(typeof(GameCommands)))
                        {
                            currentDisplay = currentDisplay + "\n" + "  " + name;
                            consoleText.text = currentDisplay;
                        }
                        currentInput = "============";
                        break;
                    }

                case "cash":
                    if (secondPhrase == "Nope")
                    {
                        currentInput = "ERROR:INVALID PHRASE";
                        currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                        consoleText.text = currentDisplay;
                        currentInput = "[cash (number)] to get money";
                        currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                        consoleText.text = currentDisplay;
                        currentInput = "============";
                        break;
                    }
                    else
                    {
                        int value = int.Parse(secondPhrase);
                        if (secondPhrase != value.ToString())
                        {
                            currentInput = "ERROR:INVALID INPUT";
                            currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                            consoleText.text = currentDisplay;
                            currentInput = "[cash (number)] to get money";
                            currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                            consoleText.text = currentDisplay;
                        }
                        else
                        {
                            currentInput = "input value is" + value;
                            currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                            consoleText.text = currentDisplay;
                        }

                        currentInput = "============";
                        break;
                    }

                case "debug":
                    
                    if (gameManager.gameInteractionState == E_GameInteractionState.debugInteraction)
                    {
                        currentInput = "disable debug mode";
                        gameManager.gameInteractionState = E_GameInteractionState.defaultInteraction;
                        currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                        consoleText.text = currentDisplay;
                    }
                    else
                    {
                        currentInput = "enable debug mode";
                        gameManager.gameInteractionState = E_GameInteractionState.debugInteraction;
                        currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                        consoleText.text = currentDisplay;
                    }

                    currentInput = "============";
                    break;

                case "author":
                    currentInput = "peimingxia.me";
                    currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                    consoleText.text = currentDisplay;
                    currentInput = "============";
                    break;

                default:
                    currentInput = "ERROR:INVALID INPUT";
                    currentDisplay = currentDisplay + "\n" + "  " + currentInput;
                    consoleText.text = currentDisplay;
                    currentInput = "============";
                    break;
            }
        }

        //ARMOURY!!!

    }
}

