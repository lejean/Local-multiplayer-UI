using LocalCoop;
using System;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems {
    /// <summary>
    /// A BaseInputModule converted to catch controller input with xInput/directInput.
    /// This module isn't optimized, it's possible alot of code isn't needed, I just made enough changes to make it work with X_input.
    /// </summary>
    /// <remarks>
    /// Input module for working with a controller.
    /// </remarks>
    public class X_InputModule : PointerInputModule {

        private float m_PrevActionTime;
        private Vector2 m_LastMoveVector;
        private int m_ConsecutiveMoveCount = 0;

        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;

        private GameObject m_CurrentFocusedGameObject;

        private PointerEventData m_InputPointerEvent;

        public PlayerInput playerInput;

        protected override void Awake() {

        }

        protected X_InputModule() {
        }

        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public enum InputMode {
            Mouse,
            Buttons
        }

        public enum ActionButtons {
            X,
            Y,
            A,
            B,
            R1,
            L1
        }

        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public InputMode inputMode {
            get { return InputMode.Mouse; }
        }

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private ActionButtons m_SubmitButton = ActionButtons.A;

        /// <summary>
        /// Name of the cancel button.
        /// </summary>
        [SerializeField]
        private ActionButtons m_CancelButton = ActionButtons.B;

        [SerializeField]
        private float m_InputActionsPerSecond = 10;

        [SerializeField]
        private float m_RepeatDelay = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
        private bool m_ForceModuleActive;

        [Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
        public bool allowActivationOnMobileDevice {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        /// <summary>
        /// Force this module to be active.
        /// </summary>
        /// <remarks>
        /// If there is no module active with higher priority (ordered in the inspector) this module will be forced active even if valid enabling conditions are not met.
        /// </remarks>
        public bool forceModuleActive {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        /// <summary>
        /// Number of keyboard / controller inputs allowed per second.
        /// </summary>
        public float inputActionsPerSecond {
            get { return m_InputActionsPerSecond; }
            set { m_InputActionsPerSecond = value; }
        }

        protected override void Start() {

        }

        void AssignPlayerInput() {
  
        }

        /// <summary>
        /// Delay in seconds before the input actions per second repeat rate takes effect.
        /// </summary>
        /// <remarks>
        /// If the same direction is sustained, the inputActionsPerSecond property can be used to control the rate at which events are fired. However, it can be desirable that the first repetition is delayed, so the user doesn't get repeated actions by accident.
        /// </remarks>
        public float repeatDelay {
            get { return m_RepeatDelay; }
            set { m_RepeatDelay = value; }
        }

        private bool ShouldIgnoreEventsOnNoFocus() {
            switch (SystemInfo.operatingSystemFamily) {
                case OperatingSystemFamily.Windows:
                case OperatingSystemFamily.Linux:
                case OperatingSystemFamily.MacOSX:
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isRemoteConnected)
                        return false;
#endif
                    return true;
                default:
                    return false;
            }
        }

        public override void UpdateModule() {
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus()) {
                if (m_InputPointerEvent != null && m_InputPointerEvent.pointerDrag != null && m_InputPointerEvent.dragging)
                    ExecuteEvents.Execute(m_InputPointerEvent.pointerDrag, m_InputPointerEvent, ExecuteEvents.endDragHandler);

                m_InputPointerEvent = null;

                return;
            }

            m_LastMousePosition = m_MousePosition;
            m_MousePosition = input.mousePosition;
        }

        public override bool IsModuleSupported() {
            return m_ForceModuleActive || input.mousePresent || input.touchSupported;
        }

        public override bool ShouldActivateModule() {
            if (!base.ShouldActivateModule())
                return false;

            var shouldActivate = m_ForceModuleActive;

            if (playerInput) {
                shouldActivate |= IsButtonDown(m_SubmitButton);
                shouldActivate |= IsButtonDown(m_CancelButton);
                shouldActivate |= !Mathf.Approximately(playerInput.xAxisLeft, 0.0f);
                shouldActivate |= !Mathf.Approximately(playerInput.yAxisLeft, 0.0f);
            }

            shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
            shouldActivate |= input.GetMouseButtonDown(0);

            if (input.touchCount > 0)
                shouldActivate = true;

            return shouldActivate;
        }

        /// <summary>
        /// See BaseInputModule.
        /// </summary>
        public override void ActivateModule() {
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            base.ActivateModule();
            m_MousePosition = input.mousePosition;
            m_LastMousePosition = input.mousePosition;

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        /// <summary>
        /// See BaseInputModule.
        /// </summary>
        public override void DeactivateModule() {
            base.DeactivateModule();
            ClearSelection();
        }

        public override void Process() {
            //print("process custominput");
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus()) {
                //print("not focused");
                return;
            }

            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents) {
                //print("sendNavigationEvents");
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }
        }

        /// <summary>
        /// Calculate and send a submit event to the current selected object.
        /// </summary>
        /// <returns>If the submit event was used by the selected object.</returns>
        protected bool SendSubmitEventToSelectedObject() {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            if (!playerInput) {
                return false;
            }

            var data = GetBaseEventData();

            if (IsButtonDown(m_SubmitButton)) {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
            }

            if (IsButtonDown(m_CancelButton)) {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            }
            return data.used;
        }

        private Vector2 GetRawMoveVector() {
            Vector2 move = Vector2.zero;
            move.x = playerInput.xAxisLeft;
            move.y = playerInput.yAxisLeft;

            if (playerInput.xAxisLeft != 0) {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (playerInput.yAxisLeft != 0) {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }

        /// <summary>
        /// Calculate and send a move event to the current selected object.
        /// </summary>
        /// <returns>If the move event was used by the selected object.</returns>
        protected bool SendMoveEventToSelectedObject() {
            float time = Time.unscaledTime;

            if (!playerInput) {
                //print("assign playerinput");
                return false;
            }

            Vector2 movement = GetRawMoveVector();
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f)) {
                m_ConsecutiveMoveCount = 0;
                return false;
            }

            // If user pressed key again, always allow event
            //bool allow = input.GetButtonDown(m_HorizontalAxis) || input.GetButtonDown(m_VerticalAxis);
            bool allow = playerInput.LeftAxisTilted;

            bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);
            if (allow) {
                // Otherwise, user held down key or axis.
                // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
                if (similarDir && m_ConsecutiveMoveCount == 1)
                    allow = (time > m_PrevActionTime + m_RepeatDelay);
                // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
                else
                    allow = (time > m_PrevActionTime + 1f / m_InputActionsPerSecond);

                //print(allow);
            }
            if (!allow)
                return false;

            // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);

            if (axisEventData.moveDir != MoveDirection.None) {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
                if (!similarDir)
                    m_ConsecutiveMoveCount = 0;
                m_ConsecutiveMoveCount++;
                m_PrevActionTime = time;
                m_LastMoveVector = movement;
            }
            else {
                m_ConsecutiveMoveCount = 0;
            }

            return axisEventData.used;
        }

        protected bool SendUpdateEventToSelectedObject() {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        protected GameObject GetCurrentFocusedGameObject() {
            return m_CurrentFocusedGameObject;
        }

        bool IsButtonDown(ActionButtons button) {
            switch (button) {
                case ActionButtons.X:
                    return playerInput.ButtonXDown();
                case ActionButtons.Y:
                    return playerInput.ButtonYDown();
                case ActionButtons.A:
                    return playerInput.ButtonADown();
                case ActionButtons.B:
                    return playerInput.ButtonBDown();
                case ActionButtons.R1:
                    return playerInput.RightBumperDown();
                case ActionButtons.L1:
                    return playerInput.LeftBumperDown();
                default:
                    Debug.Log("implement " + button);
                    break;
            }

            return false;
        }
    }
}