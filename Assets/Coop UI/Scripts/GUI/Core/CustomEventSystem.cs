using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LocalCoop;

public class CustomEventSystem : EventSystem {

    protected override void OnEnable() {
        base.OnEnable();
    }

    protected override void Update() {
        
        EventSystem originalCurrent = current;
        current = this;
        base.Update();
        current = originalCurrent;
    }
}


