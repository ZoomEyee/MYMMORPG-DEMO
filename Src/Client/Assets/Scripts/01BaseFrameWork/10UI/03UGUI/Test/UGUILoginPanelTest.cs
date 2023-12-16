using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UGUILoginPanelTest : UGUIBasePanel
{
    public override void Init()
    {
        foreach (KeyValuePair<string, List<UIBehaviour>> A in controlDic)
        {
            Debug.Log("Key: " + A.Key);
            foreach (var B in A.Value)
            {
                Debug.Log("UIBehaviour: " + B);
            }
            Debug.Log("---------------------------------------");
        }
        Debug.Log(BinaryDataMgr.Instance.BINARY_PER_PATH);
        Debug.Log(GetControl<TextMeshProUGUI>("UserInputField"));
        Debug.Log(GetControl<TextMeshProUGUI>("UserInputField").text);
        if (LoginModel.Instance.loginData.userNameAndPassDic == null)
            Debug.Log(LoginModel.Instance.loginData.userNameAndPassDic + "×Öµä¿Õ");


        UpdateInfo();
        if (LoginModel.Instance.loginData.autoLogin && IsCanLogin())
            UGUIMgr.Instance.HidePanel<UGUILoginPanelTest>();
        GetControl<Button>("LoginButton").onClick.AddListener(() =>
        {
            if (!IsCanLogin())
                return;
            LoginModel.Instance.loginData.rememberPass = GetControl<Toggle>("RememberPassToggle").isOn;
            LoginModel.Instance.loginData.autoLogin = GetControl<Toggle>("AutoLoginToggle").isOn;
            LoginModel.Instance.loginData.nowUserName = GetControl<TextMeshProUGUI>("UserInputField", "Text").text;
            LoginModel.Instance.loginData.nowPass = LoginModel.Instance.loginData.userNameAndPassDic[GetControl<TextMeshProUGUI>("UserInputField", "Text").text];
            LoginModel.Instance.SaveData();
            UGUIMgr.Instance.HidePanel<UGUILoginPanelTest>();
            Debug.Log("µÇÂ¼");
        });
        UGUIMgr.Instance.AddCustomEventListener<Button>("UGUILoginPanelTest", "RegisterButton", EventTriggerType.PointerClick, (baseEventData) =>
        {
            if (GetControl<TextMeshProUGUI>("UserInputField", "Text").text != null && GetControl<TextMeshProUGUI>("PassInputField", "Text").text != null && !LoginModel.Instance.loginData.userNameAndPassDic.ContainsKey(GetControl<TextMeshProUGUI>("UserInputField", "Text").text))
            {
                LoginModel.Instance.loginData.userNameAndPassDic.Add(GetControl<TextMeshProUGUI>("UserInputField", "Text").text, GetControl<TextMeshProUGUI>("PassInputField", "Text").text);
                LoginModel.Instance.SaveData();
            }
            Debug.Log("×¢²á³É¹¦");
        });
    }
    protected override void Update()
    {
        base.Update();
        //if (GetControl<TextMeshProUGUI>("UserInputField", "Text").text != null)
        //    Debug.Log(GetControl<TextMeshProUGUI>("UserInputField", "Text").text);

    }
    public void UpdateInfo()
    {
        GetControl<Toggle>("RememberPassToggle").isOn = LoginModel.Instance.loginData.rememberPass;
        GetControl<Toggle>("AutoLoginToggle").isOn = LoginModel.Instance.loginData.autoLogin;
        GetControl<TextMeshProUGUI>("UserInputField", "Text").text = LoginModel.Instance.loginData.nowUserName;
        if (LoginModel.Instance.loginData.rememberPass)
            GetControl<TextMeshProUGUI>("PassInputField", "Text").text = LoginModel.Instance.loginData.nowPass;
        else
            LoginModel.Instance.loginData.nowPass = null;
    }
    private bool IsCanLogin()
    {
        if (!LoginModel.Instance.loginData.userNameAndPassDic.ContainsKey(GetControl<TextMeshProUGUI>("UserInputField", "Text").text))
        {
            Debug.Log("ÓÃ»§Ãû²»´æÔÚ");
            return false;
        }
        if (GetControl<TextMeshProUGUI>("PassInputField", "Text").text != LoginModel.Instance.loginData.userNameAndPassDic[GetControl<TextMeshProUGUI>("UserInputField", "Text").text])
        {
            Debug.Log("ÃÜÂë´íÎó");
            return false;
        }
        return true;
    }
}
