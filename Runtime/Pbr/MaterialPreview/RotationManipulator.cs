using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class RotationManipulator : Manipulator
    {
        public event Action<Vector2> OnDrag;

        public Vector2 TotalRotation { get; private set; } = new();

        const float k_XSpeed = 20.0f;
        const float k_YSpeed = 20.0f;
        
        bool m_ModifierRotation;

        public RotationManipulator(bool modifierRotation)
        {
            m_ModifierRotation = modifierRotation;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public void OnPointerDown(PointerDownEvent evt)
        {
            var isShiftPressed = (evt.modifiers & EventModifiers.Shift) != 0 || !m_ModifierRotation;

            if (!isShiftPressed)
                return;

            target.CapturePointer(evt.pointerId);
        }

        public void OnPointerMove(PointerMoveEvent evt)
        {
            var isShiftPressed = (evt.modifiers & EventModifiers.Shift) != 0 || !m_ModifierRotation; 

            if (!isShiftPressed)
                return;
            if (!target.HasPointerCapture(evt.pointerId))
                return;

            var rotX = TotalRotation.x + evt.deltaPosition.x * k_XSpeed * 0.02f;
            var rotY =  TotalRotation.y + evt.deltaPosition.y * k_YSpeed * 0.02f;
            TotalRotation = new Vector2(rotX, rotY);

            OnDrag?.Invoke(TotalRotation);
            evt.StopPropagation();
            evt.StopImmediatePropagation();
        }

        public void OnPointerUp(PointerUpEvent evt)
        {
            target.ReleasePointer(evt.pointerId);
        }

        public static void OnMouseMove(MouseMoveEvent evt)
        {
            var isShiftPressed = (evt.modifiers & EventModifiers.Shift) != 0;
            if (!isShiftPressed)
                return;
            evt.StopPropagation();
        }
    }
}
