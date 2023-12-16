using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class LoginDataObj
{
    public Dictionary<string, string> userNameAndPassDic = new Dictionary<string, string>();
    public string nowUserName;
    public string nowPass;
    public bool rememberPass;
    public bool autoLogin;
}
