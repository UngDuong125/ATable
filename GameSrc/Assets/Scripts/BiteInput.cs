using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MotMam
{
    public enum ScreenSide
    {
        Any,
        Left,
        Right
    }

    [DefaultExecutionOrder(-100)]
    public class BiteInput : MonoBehaviour
    {
        public event Action<ScreenSide> Pressed;
        public event Action<ScreenSide> Released;
        public event Action Cancelled;

        bool tracking;
        bool split;
        bool sideTag;
        ScreenSide side;
        readonly List<RaycastResult> uiHits = new();

        public void UseSplit(bool value)
        {
            split = value;
        }

        // Báo bên màn hình lúc nhấn nhưng không hủy khi kéo sang bên kia.
        public void UseSideTag(bool value)
        {
            sideTag = value;
        }

        void Update()
        {
            bool held = TryPointer(out Vector2 pos);
            if (held && !tracking)
            {
                if (OverUi(pos)) return;
                tracking = true;
                side = split || sideTag ? SideOf(pos) : ScreenSide.Any;
                Pressed?.Invoke(side);
            }
            else if (held && tracking)
            {
                if (split && SideOf(pos) != side)
                {
                    tracking = false;
                    Cancelled?.Invoke();
                }
            }
            else if (!held && tracking)
            {
                tracking = false;
                Released?.Invoke(side);
            }
        }

        bool OverUi(Vector2 pos)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = pos };
            uiHits.Clear();
            EventSystem.current.RaycastAll(data, uiHits);
            if (uiHits.Count == 0) return false;
            var top = uiHits[0].gameObject;
            return top == null || top.GetComponentInParent<UiPassThrough>() == null;
        }

        static ScreenSide SideOf(Vector2 pos)
        {
            return pos.x < Screen.width * 0.5f ? ScreenSide.Left : ScreenSide.Right;
        }

        static bool TryPointer(out Vector2 pos)
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                pos = touch.primaryTouch.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                pos = mouse.position.ReadValue();
                return true;
            }

            pos = default;
            return false;
        }
    }
}
