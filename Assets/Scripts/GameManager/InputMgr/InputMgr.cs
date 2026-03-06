using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class InputMgr : SingletonLocal<InputMgr>
{
    public bool isStart = false;
    public PlayerInput playerInput;
    protected override void Awake()
    {
        base.Awake();
        //专用性update事件订阅
        //MonoController.Instance.AddUpdateListener (MyUpdate);
       
        playerInput = GetComponent<PlayerInput> ();
        if(playerInput == null )
        {
            playerInput = gameObject.AddComponent<PlayerInput> ();
        }
        playerInput.actions = Resources.Load<InputActionAsset> ("Actions");
        
        //Keyboard.current.onTextInput += CheckKeyCode;
        
    }

    private void Start()
    {
        if(isStart)
        {
            CheckMouse ();
            CheckKeyCode ();
        }
        //CheckMouse ();
        //CheckKeyCode ();
    }

    private void CheckMouse()
    {
        playerInput.actions["PlayerInput/MouseLeftClick"].performed += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键按下", KeyCode.Mouse0);
        };
        playerInput.actions["PlayerInput/MouseRightClick"].performed += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键按下", KeyCode.Mouse1);
        };
        playerInput.actions["PlayerInput/MouseMiddleClick"].performed += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键按下", KeyCode.Mouse2);
        };
        playerInput.actions["PlayerInput/MouseLeftClick"].canceled += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键抬起", KeyCode.Mouse0);
        };
        playerInput.actions["PlayerInput/MouseRightClick"].canceled += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键抬起", KeyCode.Mouse1);
        };
        playerInput.actions["PlayerInput/MouseMiddleClick"].canceled += ctx =>
        {
            EventCenter.Instance.EventTrigger ("鼠标某键抬起", KeyCode.Mouse2);
        };
    }

    private void CheckKeyCode()
    {
        playerInput.actions["PlayerInput/KeyboardAnyKey"].performed += ctx =>
        {
            KeyCode keyCode = KeyNameToKeyCode (ctx.control.name);
            EventCenter.Instance.EventTrigger ("某键按下", keyCode);
        };
        playerInput.actions["PlayerInput/KeyboardAnyKey"].canceled += ctx =>
        {
            KeyCode keyCode = KeyNameToKeyCode (ctx.control.name);
            EventCenter.Instance.EventTrigger ("某键抬起", keyCode);
        };
        //KeyCode keyCode = CharToKeyCode (_keyCode);
        //if(Input.GetKeyDown (keyCode))
        //{
        //    EventCenter.Instance.EventTrigger ("某键按下", keyCode);
        //}
        //if(Input.GetKeyUp (keyCode))
        //{
        //    EventCenter.Instance.EventTrigger ("某键抬起", keyCode);
        //}
    }
    //private void MyUpdate()
    //{
    //    if(isStart == false) return;
    //    CheckKeyCode (KeyCode.W);
    //    CheckKeyCode (KeyCode.A);
    //    CheckKeyCode (KeyCode.S);
    //    CheckKeyCode (KeyCode.D);
    //}
    private KeyCode CharToKeyCode(char ch)//?
    {
        if(char.IsLetter (ch))
        {
            return (KeyCode)System.Enum.Parse (typeof (KeyCode), ch.ToString ().ToUpper ());
        }
        else if(char.IsDigit (ch))
        {
            return (KeyCode)System.Enum.Parse (typeof (KeyCode), "Alpha" + ch);
        }
        // 可以继续加符号映射
        return KeyCode.None;
    }

    KeyCode KeyNameToKeyCode(string keyName)
    {
        // 新输入系统的 key.name 与旧的 KeyCode 大部分一致，例如：
        // "w" -> KeyCode.W
        // "space" -> KeyCode.Space
        // "leftShift" -> KeyCode.LeftShift
        string pureKeyName = keyName
        .Replace (" [Keyboard]", "")  // 去掉键盘后缀
        .Replace (" [Mouse]", "")     // 去掉鼠标后缀
        .Replace (" ", "");
        if(Enum.TryParse<KeyCode> (pureKeyName, true, out KeyCode keyCode))
        {
            return keyCode;
        }
        return KeyCode.None;
    }

    public void StartOrEndCheck(bool isOpen)
    {
        isStart = isOpen;
    }
}
