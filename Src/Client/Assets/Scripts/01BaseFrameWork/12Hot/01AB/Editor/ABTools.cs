using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ABTool : EditorWindow
{
    private int nowSelIndex = 0;
    private string[] targetStrings = new string[] { "PC", "IOS", "Android" };
    private string serverIP = "ftp://127.0.0.1";
    private string serverUser = "";
    private string serverPassword = "";

    [MenuItem("CustomTools/ABTool")]
    private static void OpenWindow()
    {
        ABTool windown = EditorWindow.GetWindowWithRect(typeof(ABTool), new Rect(0, 0, 350, 260)) as ABTool;
        windown.Show();
    }
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 15), "平台选择");
        nowSelIndex = GUI.Toolbar(new Rect(10, 30, 330, 20), nowSelIndex, targetStrings);
        GUI.Label(new Rect(10, 60, 90, 20), "资源服务器地址:");
        serverIP = GUI.TextField(new Rect(105, 60, 235, 20), serverIP);
        GUI.Label(new Rect(10, 90, 90, 20), "资源服务器账号:");
        serverUser = GUI.TextField(new Rect(105, 90, 235, 20), serverUser);
        GUI.Label(new Rect(10, 120, 90, 20), "资源服务器密码:");
        serverPassword = GUI.TextField(new Rect(105, 120, 235, 20), serverPassword);
        if (GUI.Button(new Rect(10, 150, 330, 30), "为默认AB包创建对比文件"))
            CreateABCompareFile();
        if (GUI.Button(new Rect(10, 185, 330, 30), "上传默认AB包和对比文件"))
            UploadAllABFile();
        if (GUI.Button(new Rect(10, 220, 330, 30), "将选择的资源复制到StreamingAssets并创建对比文件"))
            MoveABToStreamingAssetsAndCreateABCompareFile();
    }
    private void CreateABCompareFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AssetBundle/" + targetStrings[nowSelIndex]);
        FileInfo[] fileInfos = directory.GetFiles();
        string abCompareInfo = "";

        foreach (FileInfo info in fileInfos)
        {
            if (info.Extension == "")
            {
                abCompareInfo += info.Name + " " + info.Length + " " + GetMD5(info.FullName);
                abCompareInfo += '|';
            }
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);
        File.WriteAllText(Application.dataPath + "/ArtRes/AB/" + targetStrings[nowSelIndex] + "/ABCompareInfo.txt", abCompareInfo);
        AssetDatabase.Refresh();
        Debug.Log("AB包对比文件生成成功");
    }
    private string GetMD5(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5Info.Length; i++)
                sb.Append(md5Info[i].ToString("x2"));
            return sb.ToString();
        }
    }
    private void MoveABToStreamingAssetsAndCreateABCompareFile()
    {
        UnityEngine.Object[] selectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (selectedAsset.Length == 0)
            return;
        string abCompareInfo = "";
        foreach (UnityEngine.Object asset in selectedAsset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string fileName = assetPath.Substring(assetPath.LastIndexOf('/'));
            if (fileName.IndexOf('.') != -1)
                continue;
            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName);
            abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + GetMD5(fileInfo.FullName);
            abCompareInfo += "|";
        }
        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);
        File.WriteAllText(Application.streamingAssetsPath + "/ABCompareInfo.txt", abCompareInfo);
        AssetDatabase.Refresh();
    }
    private void UploadAllABFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AssetBundle/" + targetStrings[nowSelIndex] + "/");
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (FileInfo info in fileInfos)
        {
            if (info.Extension == "" ||
                info.Extension == ".txt")
            {
                FtpUploadFile(info.FullName, info.Name);
            }
        }
    }
    private async void FtpUploadFile(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AssetBundle/" + targetStrings[nowSelIndex] + "/" + fileName)) as FtpWebRequest;
                NetworkCredential n = new NetworkCredential(serverUser, serverPassword);
                req.Credentials = n;
                req.Proxy = null;
                req.KeepAlive = false;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.UseBinary = true;
                Stream upLoadStream = req.GetRequestStream();
                using (FileStream file = File.OpenRead(filePath))
                {
                    byte[] bytes = new byte[2048];
                    int contentLength = file.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0)
                    {
                        upLoadStream.Write(bytes, 0, contentLength);
                        contentLength = file.Read(bytes, 0, bytes.Length);
                    }
                    file.Close();
                    upLoadStream.Close();
                }
                Debug.Log(fileName + "上传成功");
            }
            catch (Exception ex)
            {
                Debug.Log(fileName + "上传失败" + ex.Message);
            }
        });

    }
}
