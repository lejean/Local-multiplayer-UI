using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XInputDotNetPure; // Required in C#

namespace LocalCoop {
    
    /// <summary>
    /// Caches the axis values/x input controller ID from controller analog sticks and triggers
    /// THIS SCRIPT NEEDS TO RUN BEFORE THE DEFAULT TIMING IN SCRIPT EXECUTION ORDER FOR XINPUT
    /// </summary>
    public class PlayerInput : MonoBehaviour {

        public int controllerID;
        //Unitys input manager has some weird bugs with controllers reading other controller's inputs
        //no choice but to convert to xinput
        #region Xinput
        public bool Use_X_Input = false;    //use x input instead if unity's input manager

        PlayerIndex controllerID_X_Input;
        GamePadState state;
        GamePadState prevState;
        #endregion

        #region analog stick values
        public float xAxisLeft { get; set; }   //x axis of left analog stick
        public float yAxisLeft { get; set; }//y axis of left analog stick
        public float xAxisRight { get; set; }   //x axis of right analog stick
        public float yAxisRight { get; set; }   //y axis of right analog stick

        public float leftAxisSum { get; set; }  //left stick y axis + x axis if not 0 means we're moving the joystick
        public float rightAxisSum { get; set; }  //left stick y axis + x axis if not 0 means we're moving the joystick
        #endregion

        #region trigger values
        public float leftTrigger { get; set; }
        public float rightTrigger { get; set; }
        #endregion

        #region input strings
        string LeftStickXaxis;
        string LeftStickYaxis;
        string RightStickXaxis;
        string RightStickYaxis;
        string LeftTrigger;
        string LeftStickButton;
        string RightTrigger;
        string RightStickButton;
        string LeftBumper;
        string RightBumper;
        string DPadXAxis;
        string DPadYAxis;
        string ButtonX;
        string ButtonY;
        string ButtonA;
        string ButtonB;
        string StartBtn;
        string SelectBtn;
        #endregion

        //Returns a bool indicating the left controller stick is tilted or not
        public bool LeftAxisTilted
        {
            get
            {
                CheckLeftAxis();

                if (leftAxisSum == 0) {
                    return false;
                }
                else {
                    return true;
                }                
            }
        }

        //Returns a bool indicating the right controller stick is tilted or not
        public bool RightAxisTilted
        {
            get
            {
                CheckRightAxis();

                if (rightAxisSum == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        private void Awake() {

        }

        // Use this for initialization
        void Start() {
            SetControllerIndex_X_Input(controllerID);
            //direct input settings are assigned with 1,2,3,4 but xinput controller IDs are 0,1,2,3 so we add 1
            CacheInputStrings(controllerID+1);
        }

        void SetControllerIndex_X_Input(int controllerID) {
            controllerID_X_Input = (PlayerIndex)controllerID;
        }

        //We need to cache the string after getting the controller Index otherwise they generate too much garbage
        //For direct input only
        void CacheInputStrings(int controllerIndex) {
            LeftStickXaxis = "Left Stick X Axis" + controllerIndex;
            LeftStickYaxis = "Left Stick Y Axis" + controllerIndex;
            RightStickXaxis = "Right Stick X Axis" + controllerIndex;
            RightStickYaxis = "Right Stick Y Axis" + controllerIndex;
            LeftTrigger = "Left Trigger" + controllerIndex;
            LeftStickButton = "Left Stick Click" + controllerIndex;
            RightTrigger = "Right Trigger" + controllerIndex;
            RightStickButton = "Right Stick Click" + controllerIndex;
            LeftBumper = "Left Bumper" + controllerIndex;
            RightBumper = "Right Bumper" + controllerIndex;
            ButtonX = "X" + controllerIndex;
            ButtonY = "Y" + controllerIndex;
            ButtonA = "A" + controllerIndex;
            ButtonB = "B" + controllerIndex;
            DPadXAxis = "D-Pad X Axis" + controllerIndex;
            DPadYAxis = "D-Pad Y Axis" + controllerIndex;
            StartBtn = "Start" + controllerIndex;
            SelectBtn = "Back" + controllerIndex;
        }

        // Update is called once per frame
        void Update() {

            if (Use_X_Input) {
                prevState = state;
                state = GamePad.GetState(controllerID_X_Input);
            }            

            ReadLeftStickInput();
            ReadRightStickInput();

            ReadLeftTrigger();
            ReadRightTrigger();
        }

        #region Sticks
        void ReadLeftStickInput() {
            if (Use_X_Input) {
                xAxisLeft = state.ThumbSticks.Left.X;
                yAxisLeft = state.ThumbSticks.Left.Y;

            }
            else {
                xAxisLeft = Input.GetAxis(LeftStickXaxis);
                yAxisLeft = Input.GetAxis(LeftStickYaxis);
            }

            CheckLeftAxis();
        }

        /// <summary>
        /// Reads the left axis and calculated the absolute value to see if it's 0 or not, meaning it's tilted
        /// </summary>
        void CheckLeftAxis() {
            if (Mathf.Abs(xAxisLeft) + Mathf.Abs(yAxisLeft) > 1) {
                leftAxisSum = 1;
            }
            else {
                leftAxisSum = Mathf.Abs(xAxisLeft) + Mathf.Abs(yAxisLeft);
            }
        }

        public bool LeftStickDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.LeftStick == ButtonState.Released &&
              state.Buttons.LeftStick == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(LeftStickButton);
            }
        }

        void ReadRightStickInput() {
            if (Use_X_Input) {
                xAxisRight = state.ThumbSticks.Right.X;
                yAxisRight = state.ThumbSticks.Right.Y;
            }
            else {
                xAxisRight = Input.GetAxis(RightStickXaxis);
                yAxisRight = Input.GetAxis(RightStickYaxis);
            }

            CheckRightAxis();
        }

        /// <summary>
        /// Reads the right axis and calculated the absolute value to see if it's 0 or not, meaning it's tilted
        /// </summary>
        void CheckRightAxis() {
            if (Mathf.Abs(xAxisRight) + Mathf.Abs(yAxisRight) > 1) {
                rightAxisSum = 1;
            }
            else {
                rightAxisSum = Mathf.Abs(xAxisRight) + Mathf.Abs(yAxisRight);
            }
        }

        public bool RightStickDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.RightStick == ButtonState.Released &&
              state.Buttons.RightStick == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(RightStickButton);
            }
        }
        #endregion

        #region Triggers
        private void ReadLeftTrigger() {
            if (Use_X_Input) {
                leftTrigger = state.Triggers.Left;
            }
            else {
                leftTrigger = Input.GetAxis(LeftTrigger);
            }
        }

        private void ReadRightTrigger() {
            if (Use_X_Input) {
                rightTrigger = state.Triggers.Right;
            }
            else {
                rightTrigger = Input.GetAxis(RightTrigger);
            }            
        }
        #endregion

        #region Bumpers
        public bool LeftBumperDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.LeftShoulder == ButtonState.Released &&
              state.Buttons.LeftShoulder == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(LeftBumper);
            }            
        }

        public bool LeftBumperPressed() {
            if (Use_X_Input) {
                return (state.Buttons.LeftShoulder == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(LeftBumper);
            }            
        }

        public bool LeftBumperUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.LeftShoulder == ButtonState.Pressed &&
              state.Buttons.LeftShoulder == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(LeftBumper);
            }            
        }

        public bool RightBumperDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.RightShoulder == ButtonState.Released &&
              state.Buttons.RightShoulder == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(RightBumper);
            }            
        }

        public bool RightBumperPressed() {
            if (Use_X_Input) {
                return (state.Buttons.RightShoulder == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(RightBumper);
            }            
        }

        public bool RightBumperUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.RightShoulder == ButtonState.Pressed &&
              state.Buttons.RightShoulder == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(RightBumper);
            }            
        }
        #endregion

        #region Dpad
        public bool D_Pad_Up_Pressed() {
            if (Use_X_Input) {
                return (prevState.DPad.Up == ButtonState.Released &&
              state.DPad.Up == ButtonState.Pressed) ? true : false;
            }
            else {
                return (Input.GetAxis(DPadYAxis) == 1) ? true : false;
            }
        }

        public bool D_Pad_Left_Pressed() {
            if (Use_X_Input) {
                return (prevState.DPad.Left == ButtonState.Released &&
              state.DPad.Left == ButtonState.Pressed) ? true : false;
            }
            else {
                return (Input.GetAxis(DPadXAxis) == -1) ? true : false;
            }
        }

        public bool D_Pad_Down_Pressed() {
            if (Use_X_Input) {
                return (prevState.DPad.Down == ButtonState.Released &&
              state.DPad.Down == ButtonState.Pressed) ? true : false;
            }
            else {
                return (Input.GetAxis(DPadYAxis) == -1) ? true : false;
            }
        }

        public bool D_Pad_Right_Pressed() {
            if (Use_X_Input) {
                return (prevState.DPad.Right == ButtonState.Released &&
              state.DPad.Right == ButtonState.Pressed) ? true : false;
            }
            else {
                return (Input.GetAxis(DPadXAxis) == 1) ? true : false;
            }
        }

        public bool D_Pad_Right_Released() {
            if (Use_X_Input) {
                return (prevState.DPad.Right == ButtonState.Pressed &&
              state.DPad.Right == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(ButtonA);
            }
        }

        public float ReadDPadXaxis() {
            return Input.GetAxis(DPadXAxis);
        }

        public float ReadDPadYaxis() {
            return Input.GetAxis(DPadYAxis);
        }
        #endregion

        #region start, select, xbox
        public bool StartDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.Start == ButtonState.Released &&
              state.Buttons.Start == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(StartBtn);
            }            
        }

        public bool SelectDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.Back == ButtonState.Released &&
              state.Buttons.Back == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(SelectBtn);
            }
        }

        public bool XboxDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.Guide == ButtonState.Released &&
              state.Buttons.Guide == ButtonState.Pressed) ? true : false;
            }
            else {
                print("no xbox button implemented");
                return false;
            }
        }
        #endregion

        #region Button X
        public bool ButtonXDown() {

            if (Use_X_Input) {
                return (prevState.Buttons.X == ButtonState.Released &&
              state.Buttons.X == ButtonState.Pressed) ? true : false;
                
            }
            else {
                return Input.GetButtonDown(ButtonX);
            }

        }

        public bool ButtonXPressed() {
            if (Use_X_Input) {
                return (state.Buttons.X == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(ButtonX);
            }
        }

        public bool ButtonXUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.X == ButtonState.Pressed &&
              state.Buttons.X == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(ButtonX);
            }
        }
        #endregion

        #region Button Y
        public bool ButtonYDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.Y == ButtonState.Released &&
              state.Buttons.Y == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(ButtonY);
            }            
        }

        public bool ButtonYPressed() {
            if (Use_X_Input) {
                return (state.Buttons.Y == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(ButtonY);
            }            
        }

        public bool ButtonYUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.Y == ButtonState.Pressed &&
              state.Buttons.Y == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(ButtonY);
            }            
        }
        #endregion

        #region Button A
        public bool ButtonADown() {
            if (Use_X_Input) {
                return (prevState.Buttons.A == ButtonState.Released &&
              state.Buttons.A == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(ButtonA);
            }
            
        }

        public bool ButtonAPressed() {
            if (Use_X_Input) {
                return (state.Buttons.A == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(ButtonA);
            }            
        }

        public bool ButtonAUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.A == ButtonState.Pressed &&
              state.Buttons.A == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(ButtonA);
            }            
        }
        #endregion

        #region Button B
        public bool ButtonBDown() {
            if (Use_X_Input) {
                return (prevState.Buttons.B == ButtonState.Released &&
              state.Buttons.B == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButtonDown(ButtonB);
            }
            
        }

        public bool ButtonBPressed() {
            if (Use_X_Input) {
                return (state.Buttons.B == ButtonState.Pressed) ? true : false;
            }
            else {
                return Input.GetButton(ButtonB);
            }            
        }

        public bool ButtonBUp() {
            if (Use_X_Input) {
                return (prevState.Buttons.B == ButtonState.Pressed &&
              state.Buttons.B == ButtonState.Released) ? true : false;
            }
            else {
                return Input.GetButtonUp(ButtonB);
            }            
        }
        #endregion

    }
}