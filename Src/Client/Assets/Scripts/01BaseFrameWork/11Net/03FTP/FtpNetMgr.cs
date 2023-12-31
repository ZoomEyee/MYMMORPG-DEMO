using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FtpNetMgr : Singleton<FtpNetMgr>
{
    public string FTP_PATH = "ftp://127.0.0.1/";
    public string USER_NAME = "Czk";
    public string PASSWORD = "czk.123";

    public async void UpLoadFile(string fileName, string localPath, UnityAction action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Proxy = null;
                Stream upLoadStream = req.GetRequestStream();
                using (FileStream fileStream = File.OpenRead(localPath))
                {
                    byte[] bytes = new byte[1024];
                    int contentLength = fileStream.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0)
                    {
                        upLoadStream.Write(bytes, 0, contentLength);
                        contentLength = fileStream.Read(bytes, 0, bytes.Length);
                    }
                    fileStream.Close();
                    upLoadStream.Close();
                }
                Debug.Log("上传成功");
            }
            catch (Exception e)
            {
                Debug.Log("上传文件出错" + e.Message);
            }
        });
        action?.Invoke();
    }
    public async void DownLoadFile(string fileName, string localPath, UnityAction action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.Proxy = null;
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                Stream downLoadStream = res.GetResponseStream();
                using (FileStream fileStream = File.Create(localPath))
                {
                    byte[] bytes = new byte[1024];
                    int contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0)
                    {
                        fileStream.Write(bytes, 0, contentLength);
                        contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                    }
                    fileStream.Close();
                    downLoadStream.Close();
                }
                res.Close();
                Debug.Log("下载成功");
            }
            catch (Exception e)
            {
                Debug.Log("下载失败" + e.Message);
            }
        });
        action?.Invoke();
    }
    public async void DeleteFile(string fileName, UnityAction<bool> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.DeleteFile;
                req.Proxy = null;
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                res.Close();
                action?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.Log("移除失败" + e.Message);
                action?.Invoke(false);
            }
        });
    }
    public async void GetFileSize(string fileName, UnityAction<long> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.GetFileSize;
                req.Proxy = null;
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                action?.Invoke(res.ContentLength);
                res.Close();
            }
            catch (Exception e)
            {
                Debug.Log("获取大小失败" + e.Message);
                action?.Invoke(0);
            }
        });
    }
    public async void CreateDirectory(string directoryName, UnityAction<bool> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + directoryName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.MakeDirectory;
                req.Proxy = null;
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                res.Close();
                action?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.Log("创建文件夹失败" + e.Message);
                action?.Invoke(false);
            }
        });
    }
    public async void GetFileList(string directoryName, UnityAction<List<string>> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + directoryName)) as FtpWebRequest;
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                req.KeepAlive = false;
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.ListDirectory;
                req.Proxy = null;
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                StreamReader streamReader = new StreamReader(res.GetResponseStream());
                List<string> nameStrs = new List<string>();
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    nameStrs.Add(line);
                    line = streamReader.ReadLine();
                }
                res.Close();
                action?.Invoke(nameStrs);
            }
            catch (Exception e)
            {
                Debug.Log("获取文件列表失败" + e.Message);
                action?.Invoke(null);
            }
        });
    }
}
