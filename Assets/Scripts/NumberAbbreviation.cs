using System;
using System.Numerics;
using TMPro;
using UnityEngine;

namespace Snake2048
{
    public class NumberAbbreviation : MonoBehaviour
    {
        TMP_Text text;
        public string test;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void SetNumber(string number)
        {
            if (BigInteger.TryParse(number, out BigInteger result))
            {
                _SetNumber(result);
            }
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
}