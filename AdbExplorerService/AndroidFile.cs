using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdbExplorerService
{
    public class AndroidFile : IComparable<AndroidFile>
    {
        private readonly static Regex regex
            = new Regex(@"([\w].+)\s+(\d)\s+(\w+)\s+(\w+)\s+(\d+)\s(\d{4}-\d{1,2}-\d{1,2}\s\d{1,2}:\d{1,2})\s(.+)", RegexOptions.Singleline);
        public enum FileType
        {
            Directory,
            File,
            Link
        }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public string Permission { get; private set; }

        public FileType Type { get; private set; }

        public string CreateDataTime { get; private set; }

        public string Source { get; private set; }

        public static AndroidFile ParseLine(string dir, string line)
        {
            //"drwx--x--x 4 root sdcard_rw 4096 2010-01-01 08:01 emulated"
            //"lrwxrwxrwx 1 root root         7 2022-04-02 08:48 sdcard0 -> /sdcard"
            var matches = regex.Matches(line);

            if (matches.Count > 0)
            {
                var groups = matches[0].Groups;
                if (groups.Count >= 8)
                {
                    var file = new AndroidFile();
                    file.Permission = groups[1].Value.Substring(1);
                    var temp = groups[1].Value;
                    if (temp.StartsWith("d"))
                    {
                        file.Type = FileType.Directory;
                    }
                    else if (temp.StartsWith("l"))
                    {
                        file.Type = FileType.Link;
                    }
                    else
                    {
                        file.Type = FileType.File;
                    }

                    file.CreateDataTime = groups[6].Value;
                    file.Source = line;
                    switch (file.Type)
                    {
                        case FileType.Directory:
                        case FileType.File:
                            file.Name = groups[7].Value;
                            file.Path = dir + "/" + file.Name;
                            break;
                        case FileType.Link:
                            var tempArr = groups[7].Value.Split(new char[] { ' ' });
                            file.Name = tempArr[0];
                            file.Path = dir + "/" + tempArr[2];
                            break;
                        default:
                            break;
                    }

                    return file;
                }

            }
            else
            {
                return null;
            }
            return null;
        }

        public override string ToString()
        {
            return Name + ":" + Type.ToString();
        }

        public int CompareTo(AndroidFile other)
        {
            if (this.Type == other.Type)
            {
                return this.Name.CompareTo(other.Name);
            }
            else
            {
                if (this.Type == FileType.Directory)
                {
                    return -1;
                }
                else if (this.Type == FileType.File || other.Type == FileType.Link)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static AndroidFile BackAndroidFile()
        {
            var file = new AndroidFile();
            file.Name = "..";
            file.Type= FileType.Directory;
            return file;
        }
    }
}
