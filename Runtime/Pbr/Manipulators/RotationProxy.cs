using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class RotationProxy: Manipulator 
    {
        RotationManipulator m_RotationManipulator;
        
        public RotationProxy(RotationManipulator rotationManipulator)
        {
            m_RotationManipulator = rotationManipulator;
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

        private void OnMouseMove(MouseMoveEvent evt)
        {
            RotationManipulator.OnMouseMove(evt, false);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            target.ReleasePointer(evt.pointerId);
           m_RotationManipulator.OnPointerUp(evt); 
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            m_RotationManipulator.OnPointerMove(evt, false); 
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if(evt.button != 0)
                return;
            
            target.CapturePointer(evt.pointerId);
            m_RotationManipulator.OnPointerDown(evt, false, false); 
        }
    }
}