using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using fbchat_sharp;
using fbchat_sharp.API;
using System.IO;
using Newtonsoft.Json;

namespace BroadcastMessage
{
    class Custom_Client : MessengerClient
    {
        private static Custom_Client _client { get;set; }
        public bool useCookies = false;
        public static Custom_Client getInstance()
        {
                if (_client == null)
                {
                    _client = new Custom_Client();
                }
                return _client;
        }
        private string path = Setting.Default["CookiePath"].ToString();
        private string name = Setting.Default["Filename"].ToString();
        private string json = string.Empty;
        private Dictionary<String, List<Cookie>> jar;
        private Custom_Client()
        {
            path = path + name;
            if (File.Exists(path))
            {
                try
                {
                    json = File.ReadAllText(path);
                    jar = JsonConvert.DeserializeObject<Dictionary<String, List<Cookie>>>(json);
                    useCookies = true;
                }
                catch(FileLoadException ex)
                {
                    throw ex;
                }
            }
            else
            {
                useCookies = false;
                jar = new Dictionary<string, List<Cookie>>();
            }
        }
        public async Task fromSavedSession()
        {
            await fromSession(jar);
        }
        protected override async Task DeleteCookiesAsync()
        {
            jar.Clear();
            File.Delete(path);
            await Task.Yield();
        }

        protected override async Task<Dictionary<string, List<Cookie>>> ReadCookiesFromDiskAsync()
        {
            return jar;
        }

        protected override async Task WriteCookiesToDiskAsync(Dictionary<string, List<Cookie>> cookieJar)
        {
            jar = cookieJar;
            json = JsonConvert.SerializeObject(jar);
            File.WriteAllText(path,json);
            await Task.Yield();
        }
    }
}
