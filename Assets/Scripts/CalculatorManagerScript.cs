using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalculatorManagerScript : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text displayText;  // it is for showing calculation in progress
    [SerializeField] private Text resultText;   // it is for showing Final Result after pressing = button

    private string currentExpression = "";
    private bool showingResult = false;

    private void Start()
    {
        resultText.gameObject.SetActive(false);
        UpdateDisplay();
    }

    // it calls on any number pressing
    public void OnNumberPressed(string number)
    {
        if (showingResult)
        {
            currentExpression = "";
            showingResult = false;
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }

        currentExpression += number;
        UpdateDisplay();
    }

    // it calls on pressing of operators (+, -, *, /)
    public void OnOperatorPressed(string op)
    {
        if (showingResult)
        {
            showingResult = false;
            resultText.gameObject.SetActive(false);
        }

        if (string.IsNullOrEmpty(currentExpression))
            return;

        char lastChar = currentExpression.Replace(" ", "")[^1];
        if (IsOperator(lastChar))
            return;

        currentExpression += " " + op + " ";
        UpdateDisplay();
    }

    // it calls on press of decimalpoint (.)
    public void OnDecimalPressed()
    {
        if (showingResult)
        {
            currentExpression = "";
            showingResult = false;
            resultText.gameObject.SetActive(false);
            resultText.text = "";
        }

        string[] parts = currentExpression.Split(new char[] { '+', '-', '*', '/' });
        string lastPart = parts[^1].Trim();

        if (!lastPart.Contains("."))
        {
            if (string.IsNullOrEmpty(lastPart)) { 
                currentExpression += "0."; 
            } else { 
                currentExpression += "."; 
            }
            UpdateDisplay();
        }
    }

    // it calls on "=" button pressing
    public void OnEqualsPressed()
    {
        if (string.IsNullOrEmpty(currentExpression)) return;

        try
        {
            double result = EvaluateExpression(currentExpression);
            resultText.text = "= " + FormatResult(result);
            showingResult = true;
            resultText.gameObject.SetActive(true);
            currentExpression = result.ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Calculation Error: " + e.Message);
            resultText.text = "Error";
            showingResult = true;
            resultText.gameObject.SetActive(true);
        }
    }

    // it calls on "C" button pressing for doing backspace
    public void OnClearPressed()
    {
        if (!string.IsNullOrEmpty(currentExpression))
        {
            currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);
            if (showingResult)
            {
                showingResult = false;
                resultText.text = "";
                resultText.gameObject.SetActive(false);
            }
            UpdateDisplay();
        }
    }

    // it calls on "AC" button pressing for reset
    public void OnResetPressed()
    {
        currentExpression = "";
        resultText.text = "";
        showingResult = false;
        resultText.gameObject.SetActive(false);
        UpdateDisplay();
    }

    // it calls for updating the Display text
    private void UpdateDisplay()
    {
        displayText.text = string.IsNullOrEmpty(currentExpression) ? "0" : currentExpression;
    }

    // it is for converting result in clean format
    private string FormatResult(double result)
    {
        if (result == (int)result) { 
            return ((int)result).ToString(); 
        }
        return result.ToString("0.##########");
    }

    // it is for checking that a character is a operator on not
    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/';
    }

    // it is for evaluating the Expression according to DMAS rule
    private double EvaluateExpression(string expression)
    {
        expression = expression.Replace(" ", "");

        List<double> numbers = new List<double>();
        List<char> operators = new List<char>();
        string currentNumber = "";
        bool expectingNumber = true;

        // it will break the Expression in numbers and operators
        foreach (char c in expression)
        {
            if (c == '-' && expectingNumber)
            {
                currentNumber += c;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                currentNumber += c;
                expectingNumber = false;
            }
            else if (IsOperator(c))
            {
                if (!string.IsNullOrEmpty(currentNumber))
                {
                    numbers.Add(double.Parse(currentNumber));
                    currentNumber = "";
                }

                operators.Add(c);
                expectingNumber = true;
            }
        }

        if (!string.IsNullOrEmpty(currentNumber))
        {
            numbers.Add(double.Parse(currentNumber));
        }

        // first we will solve to * and / 
        int i = 0;
        while (i < operators.Count)
        {
            if (operators[i] == '*' || operators[i] == '/')
            {
                double result = PerformOperation(numbers[i], numbers[i + 1], operators[i]);
                numbers[i] = result;
                numbers.RemoveAt(i + 1);
                operators.RemoveAt(i);
            }
            else i++;
        }

        // then we wil solve to + and -
        i = 0;
        while (i < operators.Count)
        {
            double result = PerformOperation(numbers[i], numbers[i + 1], operators[i]);
            numbers[i] = result;
            numbers.RemoveAt(i + 1);
            operators.RemoveAt(i);
        }

        return numbers[0];
    }

    // it is for performing Single operation
    private double PerformOperation(double num1, double num2, char op)
    {
        switch (op)
        {
            case '+': return num1 + num2;
            case '-': return num1 - num2;
            case '*': return num1 * num2;
            case '/':
                if (num2 == 0)
                {
                    Debug.LogError("Cannot divide by zero");
                    return 0;
                }
                return num1 / num2;
            default:
                Debug.LogError("Invalid operator: " + op);
                return 0;
        }
    }
}
