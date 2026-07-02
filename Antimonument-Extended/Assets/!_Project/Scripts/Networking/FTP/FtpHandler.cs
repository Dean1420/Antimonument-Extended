


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using FileOperations;
using System.Threading.Tasks;
namespace Ftp
{
        public static class FtpHandler{

  public static async Task UploadFile(
    string username,
    string password,
    string url,
    string remoteDirectory,
    string fileName,
    byte[] fileData)
{
    try
    {
        if (fileData == null || fileData.Length == 0)
        {
            Debug.LogError("FTP >>> fileData is null or empty");
            return;
        }

        string path =
            $"{url.TrimEnd('/')}/{remoteDirectory.Trim('/')}/{Path.GetFileName(fileName)}";

        Debug.Log("FTP PATH = " + path);

        var request = (FtpWebRequest)WebRequest.Create(path);
        request.Method = WebRequestMethods.Ftp.UploadFile;

        request.Credentials = new NetworkCredential(username, password);

        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        request.EnableSsl = true; // nur wenn FTPS wirklich aktiv ist

        using (Stream stream = await request.GetRequestStreamAsync())
        {
            await stream.WriteAsync(fileData, 0, fileData.Length);
        }

        using (FtpWebResponse response =
               (FtpWebResponse)await request.GetResponseAsync())
        {
            Debug.Log("FTP >>> server response: " + response.StatusDescription);
        }
    }
    catch (Exception e)
    {
        Debug.LogError("FTP >>> error: " + e);
    }
}
}
}