using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class HttpNetMgr : Singleton<HttpNetMgr>
{
    public string HTTP_PATH = "http://192.168.50.109:8000/Http_Server/";
    public string USER_NAME = "Czk";
    public string PASS_WORD = "czk.123";
    public async void DownLoadFile(string fileName, string loacFilePath, UnityAction<HttpStatusCode> action)
    {
        HttpStatusCode result = HttpStatusCode.OK;
        await Task.Run(() =>
        {
            try
            {
                HttpWebRequest req = HttpWebRequest.Create(HTTP_PATH + fileName) as HttpWebRequest;
                req.Method = WebRequestMethods.Http.Head;
                req.Timeout = 2000;
                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    res.Close();
                    req = HttpWebRequest.Create(HTTP_PATH + fileName) as HttpWebRequest;
                    req.Method = WebRequestMethods.Http.Get;
                    req.Timeout = 2000;
                    res = req.GetResponse() as HttpWebResponse;
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        using (FileStream fileStream = File.Create(loacFilePath))
                        {
                            Stream stream = res.GetResponseStream();
                            byte[] bytes = new byte[4096];
                            int contentLength = stream.Read(bytes, 0, bytes.Length);

                            while (contentLength != 0)
                            {
                                fileStream.Write(bytes, 0, contentLength);
                                contentLength = stream.Read(bytes, 0, bytes.Length);
                            }
                            fileStream.Close();
                            stream.Close();
                        }
                        result = HttpStatusCode.OK;
                    }
                    else
                    {
                        result = res.StatusCode;
                    }
                }
                else
                {
                    result = res.StatusCode;
                }
                res.Close();
            }
            catch (WebException w)
            {
                result = HttpStatusCode.InternalServerError;
                Debug.Log("下载出错" + w.Message + w.Status);
            }
        });
        action?.Invoke(result);
    }
    public async void UpLoadFile(string fileName, string loacalFilePath, UnityAction<HttpStatusCode> action)
    {
        HttpStatusCode result = HttpStatusCode.BadRequest;
        await Task.Run(() =>
        {
            try
            {
                HttpWebRequest req = HttpWebRequest.Create(HTTP_PATH) as HttpWebRequest;
                req.Method = WebRequestMethods.Http.Post;
                req.ContentType = "multipart/form-data;boundary=Czk";
                req.Timeout = 500000;
                req.Credentials = new NetworkCredential(USER_NAME, PASS_WORD);
                req.PreAuthenticate = true;

                string head = "--Czk\r\n" +
                "Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\n" +
                "Content-Type:application/octet-stream\r\n\r\n";
                head = string.Format(head, fileName);
                byte[] headBytes = Encoding.UTF8.GetBytes(head);
                byte[] endBytes = Encoding.UTF8.GetBytes("\r\n--Czk--\r\n");
                using (FileStream localStream = File.OpenRead(loacalFilePath))
                {
                    req.ContentLength = headBytes.Length + localStream.Length + endBytes.Length;
                    Stream upLoadStream = req.GetRequestStream();
                    upLoadStream.Write(headBytes, 0, headBytes.Length);
                    byte[] bytes = new byte[4096];
                    int contentLenght = localStream.Read(bytes, 0, bytes.Length);
                    while (contentLenght != 0)
                    {
                        upLoadStream.Write(bytes, 0, contentLenght);
                        contentLenght = localStream.Read(bytes, 0, bytes.Length);
                    }
                    upLoadStream.Write(endBytes, 0, endBytes.Length);
                    upLoadStream.Close();
                    loacalFilePath.Clone();
                }
                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                result = res.StatusCode;
                res.Close();
            }
            catch (WebException w)
            {
                Debug.Log("上传出错" + w.Status + w.Message);
            }
        });
        action?.Invoke(result);
    }
}
