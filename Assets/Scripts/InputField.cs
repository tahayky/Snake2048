using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snake2048
{
    public class InputField : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
    {
        Image _image;
        public Sprite defaultSprite;
        public Sprite selectedSprite;
        bool selected = false;
        bool inside = false;
        public int maxCharacters = 10;
        [TextArea]
        public string allowedCharacters;
        string Text = string.Empty;
        public TMP_Text tMP_text;
        private void Awake()
        {
            _image=GetComponent<Image>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inside)
            {
                selected = true;
                _image.sprite=selectedSprite;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            inside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inside = false;
        }

        private void Update()
        {
            if(!selected)return;
            else if (Input.GetMouseButtonUp(0))
            {
                if (!inside)
                {
                    selected = false;
                    _image.sprite=defaultSprite;
                }
                return;
            }
            
            var inputString = Input.inputString;
            if (inputString.Length>0)
            {
                if (inputString.Contains('\b')||inputString.Contains('\u007F'))
                {
                    if (Text.Length > 0)
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                        SetText(Text);
                    }

                }
                else if (allowedCharacters.Contains(inputString[0])&&Text.Length<=maxCharacters)
                {
                    Text+=Input.inputString[0];
                    SetText(Text);
                }

            }
            
        }
        
        public void SetText(string text)
        {
            Text = text;
            tMP_text.text = text;
        }
    }
}
