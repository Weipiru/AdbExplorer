using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdbExplorerService
{
    public class ExplorerService
    {
        private string host = "127.0.0.1";
        private int port = 5037;

        private AdvancedAdbClient client = null;

        private DeviceData device = null;

        private List<string> currentPathStack = new List<string>();

        private Dictionary<string, List<AndroidFile>> dirCacheDict = new Dictionary<string, List<AndroidFile>>();

        private ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
        private List<DeviceData> cacheDevices = new List<DeviceData>();
        public List<DeviceData> GetDeviceDatas(bool focus = false)
        {
            if (focus || cacheDevices == null || cacheDevices.Count <= 0)
            {
                return client.GetDevices();
            }
            else
            {
                return cacheDevices;
            }
        }


        public void Init()
        {
            client = new AdvancedAdbClient();
            client.Connect(host + ":" + port);
            currentPathStack.Clear();
        }

        public void SetUseDevice(int index)
        {
            this.device = GetDeviceDatas()[index];
        }

        public List<AndroidFile> RefreshCurrentDir(bool focus = false)
        {
            var path = GetCurrentPath();
            List<AndroidFile> entry = null; ;
            if (!focus && dirCacheDict.ContainsKey(path))
            {
                entry = dirCacheDict[path];
                Console.WriteLine("-------------Use cache-------------");
            }
            else
            {
                string cmd = "ls -n " + path;
                Console.WriteLine(cmd);
                receiver = new ConsoleOutputReceiver();
                client.ExecuteRemoteCommand(cmd, device, receiver);
                if (receiver.ParsesErrors)
                {
                    Console.WriteLine(receiver.ToString());
                    entry = null;
                }
                else
                {
                    var responBody = receiver.ToString();
                    entry = ParseLSResponse(path, responBody);
                    entry.Sort();
                    if (dirCacheDict.ContainsKey(path))
                    {
                        dirCacheDict[path] = entry;
                    }
                    else
                    {
                        dirCacheDict.Add(path, entry);
                    }
                }
            }
            return entry;
        }

        public string GetCurrentPath()
        {
            var path = "";
            foreach (var pathName in currentPathStack)
            {
                if (pathName.StartsWith("/"))
                {
                    path += pathName;
                }
                else
                {
                    path += "/" + pathName;
                }
            }
            return path;
        }

        public List<AndroidFile> Go(string name)
        {
            currentPathStack.Add(name);
            return RefreshCurrentDir();
        }

        public List<AndroidFile> Back()
        {
            if (currentPathStack.Count >= 1)
            {
                currentPathStack.RemoveAt(currentPathStack.Count - 1);
            }
            return RefreshCurrentDir();
        }

        public void Open(String path)
        {
            var pathNames = path.Split(new char[] { '/' });
            if (pathNames.Length > 0)
            {
                currentPathStack.Clear();
                currentPathStack.AddRange(pathNames);
                RefreshCurrentDir();
            }
        }

        private List<AndroidFile> ParseLSResponse(string path, string body)
        {
            var split = body.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var entry = new List<AndroidFile>();
            var dir = GetCurrentPath();
            foreach (var name in split)
            {
                var file = AndroidFile.ParseLine(dir, name);
                if (file != null)
                {
                    entry.Add(file);
                    //  Console.WriteLine((file.Type == AndroidFile.FileType.Directory ? "+" : "*") + file.Name + "\t");
                }
            }
            return entry;
        }

        public bool Copy(AndroidFile file, string targetPath)
        {
            string cmd = "cp " + file.Path + "1 " + targetPath;
            receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand(cmd, device, receiver);
            receiver = new ConsoleOutputReceiver();
            return true;
        }

        public void Test()
        {
            client.ExecuteRemoteCommand("ls -n /sdcard", device, receiver);
            Console.WriteLine(receiver.ToString());
        }
    }
}
