using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snake2048
{
    [RequireComponent(typeof(ButtonExtendedEvents))]
    public class ButtonExtended : Button
    {
        bool _clicked = false;
        ButtonExtendedEvents _buttonExtendedEvents;
        protected override void Awake()
        {
            base.Awake();
            _buttonExtendedEvents=GetComponent<ButtonExtendedEvents>();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            if (!IsActive() || !IsInteractable())
                return;
            _clicked = true;
            _buttonExtendedEvents.OnPressed?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (!_clicked) return;
            _buttonExtendedEvents.OnReleased?.Invoke();
            _clicked = false;
        }
    }
}
