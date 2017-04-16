﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOneDrive
{
    public class SimpleClient
    {

        private static object syncRoot = new Object();

        private SimpleClient()
        {

        }

        public static SimpleClient Instance
        {
            get
            {
              
                if (_Instance == null)
                    lock (syncRoot)
                    {
                        if (_Instance == null)
                            _Instance = new SimpleClient();
                    }

                return _Instance;
            }
        }

        private static SimpleClient _Instance = null;

        public string AccessToken { get { return AuthStore.Instance.GetToken(); } }

        // ファイル一覧表示
        public async Task<MyFiles> GetRootFiles()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri("https://graph.microsoft.com/v1.0/me/drive/root/children?$select=name,weburl,createdDateTime,lastModifiedDateTime")
                );
                var response = await httpClient.SendAsync(request);
                MyFiles files = JsonConvert.DeserializeObject<MyFiles>(response.Content.ReadAsStringAsync().Result);

                return files;
            }

        }

        // ファイル一覧表示
        public async Task<MyFiles> GetSubFiles(string path)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}:/children?$select=name,weburl,createdDateTime,lastModifiedDateTime", path))
                );
                var response = await httpClient.SendAsync(request);
                MyFiles files = JsonConvert.DeserializeObject<MyFiles>(response.Content.ReadAsStringAsync().Result);

                return files;
            }

        }

        // ファイル アップロード処理
        public async Task uploadFileFromUrl(string url, string name, string remotePath)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Prefer", "respond-async");
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}:/children", remotePath))
                );
                var content = new Dictionary<string, object>();
                content.Add("@microsoft.graph.sourceUrl", url);
                content.Add("name", name);
                content.Add("file", new List<string>());
                request.Content = new StringContent("{\"@microsoft.graph.sourceUrl\": \""+ url + "\",\"name\": \""+ name + "\",\"file\": { }}", 
                                                       Encoding.UTF8, "application/json");
                var response = await httpClient.SendAsync(request);

                Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
        }

        // ファイル アップロード処理
        private async Task uploadFile(string localPath, string remotePath)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "octet-stream");
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Put,
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}:/{1}:/content", remotePath, new FileInfo(localPath).Name))
                );
                request.Content = new ByteArrayContent(ReadFileContent(localPath));
                var response = await httpClient.SendAsync(request);

            }
        }

        // ローカル ファイルの読み取り処理
        private byte[] ReadFileContent(string filePath)
        {
            byte[] buf = new byte[2048];
            using (FileStream inStrm = new FileStream(filePath, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int readBytes = inStrm.Read(buf, 0, buf.Length);
                while (readBytes > 0)
                {
                    memoryStream.Write(buf, 0, readBytes);
                    readBytes = inStrm.Read(buf, 0, buf.Length);
                }
                return memoryStream.ToArray();
            }

        }

        // ファイル名の変更処理
        private async Task renameFile(string remotePath, string name, string newname)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpRequestMessage request = new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}:/{1}", remotePath, name))
                );

                MyFileModify filemod = new MyFileModify();
                filemod.name = newname;
                request.Content = new StringContent(JsonConvert.SerializeObject(filemod), Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);

            }

        }

        // ファイル削除処理
        public async Task deleteFile(string remotePath, string name)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}", name))
                );
                var response = await httpClient.SendAsync(request);

            }

        }


        // フォルダの新規作成
        public async Task CreateFolder(string remotePath, string pathname)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(string.Format("https://graph.microsoft.com/v1.0/me/drive/root:/{0}:/children", remotePath))
                );


                request.Content = new StringContent("{\"name\": \"" + pathname + "\",\"folder\": { }}", Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);

                Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

        }
    }
    public class MyFile
    {
        public string name { get; set; }
        // 以下のプロパティは今回使用しませんが、デバッグ時に値を見ることをお勧めします。
        public string webUrl { get; set; }
        public string createdDateTime { get; set; }
        public string lastModifiedDateTime { get; set; }
    }

    public class MyFiles
    {
        public List<MyFile> value;
    }

    // ファイル移動時に使います。
    public class MyParentFolder
    {
        public string path { get; set; }
    }

    public class MyFileModify
    {
        public string name { get; set; }
        // ファイル移動時に使います。
        public MyParentFolder parentReference { get; set; }
    }

}