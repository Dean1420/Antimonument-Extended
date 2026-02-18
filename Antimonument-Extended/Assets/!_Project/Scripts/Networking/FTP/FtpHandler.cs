


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using FileOperations;
namespace Ftp
{
    public static class FtpHandler
    {
        public static async void uploadFile(string username, string password, string url, string remoteDirectory, string filenName, byte[] fileData)
        {
            try
            {
                // create ftp-request
                string path = url + "/" + remoteDirectory + "/" + Path.GetFileName(filenName);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);

                request.Method = WebRequestMethods.Ftp.UploadFile;

                // create ftp credential
                NetworkCredential credentials = new NetworkCredential(username, password);
                request.Credentials = credentials;

                // ftp flags
                request.EnableSsl = true;
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                // upload data
                using (Stream ftpStream = request.GetRequestStream())
                {
                    await ftpStream.WriteAsync(fileData, 0, fileData.Length);
                    Debug.Log("FTP >>> bytes written: " + fileData.Length);
                }

                // server response
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Debug.Log("FTP >>> server response: " + response.StatusDescription);
                }
            }
            catch (Exception e)
            {
                Debug.Log("FTP >>> error: " + e.Message);
            }
        }
    }
}