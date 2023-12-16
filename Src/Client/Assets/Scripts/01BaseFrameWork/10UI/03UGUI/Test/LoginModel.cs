using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginModel : Singleton<LoginModel>
{
    public LoginDataObj loginData;
    public LoginModel()
    {
        if (BinaryDataMgr.Instance.LoadData<LoginDataObj>(typeof(LoginDataObj).ToString()) != null)
            loginData = BinaryDataMgr.Instance.LoadData<LoginDataObj>(typeof(LoginDataObj).ToString());
        else
            loginData = new LoginDataObj();
    }
    public void SaveData()
    {
        BinaryDataMgr.Instance.SaveData(typeof(LoginDataObj).ToString(), loginData);
    }
}
