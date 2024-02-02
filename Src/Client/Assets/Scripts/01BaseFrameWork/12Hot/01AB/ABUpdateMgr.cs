using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ABUpdateMgr : MonoSingleton<ABUpdateMgr>
{
    private Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();
    private Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();
    private List<string> downLoadList = new List<string>();
    private string serverIP = "ftp://127.0.0.1";
    public void CheckUpdate(UnityAction<bool> overCallBack = null, UnityAction<string> updateInfoCallBack =null)
    {
        remoteABInfo.Clear();
        localABInfo.Clear();
        downLoadList.Clear();
        DownLoadRemoteABCompareFile((isOver) =>
        {
            updateInfoCallBack?.Invoke("开始更新资源");
            if (isOver)
            {
                updateInfoCallBack?.Invoke("对比文件下载结束");
                string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
                updateInfoCallBack?.Invoke("解析远端对比文件");
                GetABCompareFileInfo(remoteInfo, remoteABInfo);
                updateInfoCallBack?.Invoke("解析远端对比文件完成");

                GetLocalABCompareFileInfo((isOver)=> {
                    if (isOver)
                    {
                        updateInfoCallBack?.Invoke("解析本地对比文件完成");
                        updateInfoCallBack?.Invoke("开始对比");
                        foreach (string abName in remoteABInfo.Keys)
                        {
                            if (!localABInfo.ContainsKey(abName))
                                downLoadList.Add(abName);
                            else
                            {
                                if (localABInfo[abName].md5 != remoteABInfo[abName].md5)
                                    downLoadList.Add(abName);
                                localABInfo.Remove(abName);
                            }
                        }
                        updateInfoCallBack?.Invoke("对比完成");
                        updateInfoCallBack?.Invoke("删除无用的AB包文件");
                        foreach (string abName in localABInfo.Keys)
                        {
                            if (File.Exists(Application.persistentDataPath + "/" + abName))
                                File.Delete(Application.persistentDataPath + "/" + abName);
                        }
                        updateInfoCallBack?.Invoke("下载和更新AB包文件");
                        DownLoadABFile((isOver) =>
                        {
                            if (isOver)
                            {
                                updateInfoCallBack?.Invoke("更新本地AB包对比文件为最新");
                                File.WriteAllText(Application.persistentDataPath + "/ABCompareInfo.txt", remoteInfo);
                            }
                            overCallBack?.Invoke(isOver);
                        }, updateInfoCallBack);
                    }
                    else
                        overCallBack?.Invoke(false);
                });
            }
            else
                overCallBack?.Invoke(false);
        });
    }
    private bool DownLoadFile(string fileName, string localPath)
    {
        try
        {
            string pInfo =
#if UNITY_IOS
            "IOS";
#elif UNITY_ANDROID
            "Android";
#else
            "PC";
#endif
            FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AssetBundle/" + pInfo + "/" + fileName)) as FtpWebRequest;
            NetworkCredential n = new NetworkCredential("MrTang", "MrTang123");
            req.Credentials = n;
            req.Proxy = null;
            req.KeepAlive = false;
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.UseBinary = true;
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downLoadStream = res.GetResponseStream();
            using (FileStream file = File.Create(localPath))
            {
                byte[] bytes = new byte[2048];
                int contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                while (contentLength != 0)
                {
                    file.Write(bytes, 0, contentLength);
                    contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                }
                file.Close();
                downLoadStream.Close();
                return true;
            }
        }
        catch (Exception ex)
        {
            print(fileName + "下载失败" + ex.Message);
            return false;
        }

    }
    public async void DownLoadRemoteABCompareFile(UnityAction<bool> overCallBack = null)
    {
        bool isOver = false;
        int reDownLoadMaxNum = 5;
        string localPath = Application.persistentDataPath;
        while (!isOver && reDownLoadMaxNum > 0)
        {
            await Task.Run(() => {
                isOver = DownLoadFile("ABCompareInfo.txt", localPath + "/ABCompareInfo_TMP.txt");
            });
            --reDownLoadMaxNum;
        }
        overCallBack?.Invoke(isOver);
    }
    public void GetABCompareFileInfo(string info, Dictionary<string, ABInfo> ABInfo)
    {
        string[] strs = info.Split('|');
        string[] infos = null;
        for (int i = 0; i < strs.Length; i++)
        {
            infos = strs[i].Split(' ');
            ABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
        }
    }
    public void GetLocalABCompareFileInfo(UnityAction<bool> overCallBack = null)
    {
        if (File.Exists(Application.persistentDataPath + "/ABCompareInfo.txt"))
            StartCoroutine(GetLocalABCOmpareFileInfo("file:///" + Application.persistentDataPath + "/ABCompareInfo.txt", overCallBack));
        else if (File.Exists(Application.streamingAssetsPath + "/ABCompareInfo.txt"))
        {
            string path =
                #if UNITY_ANDROID
                Application.streamingAssetsPath;
                #else
                "file:///" + Application.streamingAssetsPath;
                #endif
            StartCoroutine(GetLocalABCOmpareFileInfo(path + "/ABCompareInfo.txt", overCallBack));
        }
        else
            overCallBack?.Invoke(true);
    }
    private IEnumerator GetLocalABCOmpareFileInfo(string filePath, UnityAction<bool> overCallBack = null)
    {
        UnityWebRequest req = UnityWebRequest.Get(filePath);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            GetABCompareFileInfo(req.downloadHandler.text, localABInfo);
            overCallBack?.Invoke(true);
        }
        else
            overCallBack?.Invoke(false);
    }
    public async void DownLoadABFile(UnityAction<bool> overCallBack = null, UnityAction<string> updatePro = null)
    {
        string localPath = Application.persistentDataPath + "/";
        bool isOver = false;
        List<string> tempList = new List<string>();
        int reDownLoadMaxNum = 5;
        int downLoadOverNum = 0;
        int downLoadMaxNum = downLoadList.Count;
        while (downLoadList.Count > 0 && reDownLoadMaxNum > 0)
        {
            for (int i = 0; i < downLoadList.Count; i++)
            {
                isOver = false;
                await Task.Run(() => {
                    isOver = DownLoadFile(downLoadList[i], localPath + downLoadList[i]);
                });
                if (isOver)
                {
                    updatePro?.Invoke(++downLoadOverNum + "/" +downLoadMaxNum);
                    tempList.Add(downLoadList[i]);
                }
            }
            for (int i = 0; i < tempList.Count; i++)
                downLoadList.Remove(tempList[i]);
            --reDownLoadMaxNum;
        }
        overCallBack?.Invoke(downLoadList.Count == 0);
    }
    public class ABInfo
    {
        public string name;
        public long size;
        public string md5;

        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
    }
}


