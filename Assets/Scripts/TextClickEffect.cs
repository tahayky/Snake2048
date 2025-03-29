using System;
using UnityEngine;

namespace Snake2048
{
    public class TextClickEffect : MonoBehaviour
    {
        RectTransform rectTransform;
        Vector2 startPosition;
        public float factor;
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

        }

        private void Start()
        {
            startPosition = rectTransform.anchoredPosition;
        }

        public void Press()
        {
            rectTransform.anchoredPosition = startPosition - Vector2.up * factor;
        }

        public void Release()
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
