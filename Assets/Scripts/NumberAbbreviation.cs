using System;
using System.Numerics;
using TMPro;
using UnityEngine;

public class NumberAbbreviation : MonoBehaviour
{
    TextMeshPro text;
    public string test;
    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetNumber(string number)
    {
        _SetNumber(BigInteger.Parse(number));
    }

    private void _SetNumber(BigInteger number)
    {
        string[] suffixes = { "", "K", "M", "B", "T", "Q", "Qi", "Sx", "Sp", "O", "N" };
        if (number > 9999)
        {
            text.text = FormatNumber(number, suffixes);
        }
        else
        {
            text.text = number.ToString();
        }
    }

    static string FormatNumber(BigInteger number, string[] suffixes)
    {
        int order = 0;
        double value = (double)number;
        while (value >= 1000 && order < suffixes.Length - 1)
        {
            value /= 1000;
            order++;
        }
        return $"{value:0.##}{suffixes[order]}";
    }
}
