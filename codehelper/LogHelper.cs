using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TF_Img_Utils
{
    public class LogHelper
    {
        //在网站根目录下创建日志目录(bin文件夹→debug文件夹→logs文件夹)
        private static string infopath = AppDomain.CurrentDomain.BaseDirectory + "log/info";
        private static string errorpath = AppDomain.CurrentDomain.BaseDirectory + "log/error";
        private static string debugpath = AppDomain.CurrentDomain.BaseDirectory + "log/debug";

        //死锁
        public static object loglock = new object();

        public static void Debug(string content)
        {
            WriteLog(content, LogEnum.Debug);
        }

        public static void Info(string content)
        {
            WriteLog(content, LogEnum.Info);
        }

        public static void Error(string content)
        {
            WriteLog(content, LogEnum.Error);
        }

        public static void WriteSetFile(string settxtpath, string filemsg)
        {
            WriteFileLog(settxtpath, filemsg);
        }
        public static void WriteFileLog(string fileName, string txt)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            CreatePath(path);
            try
            {

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (StreamWriter streamWriter = new StreamWriter(path, true, Encoding.Default))
                {
                    streamWriter.WriteLine(txt);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch
            {
            }
        }

        public static bool CreatePath(string path)
        {
            bool result = true;
            path = path.ToLower();
            string text = AppDomain.CurrentDomain.BaseDirectory.ToLower();
            path = path.Replace(text, "");
            string[] array = path.Split(Path.DirectorySeparatorChar);
            string str = array[0];
            str = text + str;
            int num = array.Length;
            if (array.Length >= 1 && array[array.Length - 1].Contains("."))
            {
                num--;
            }
            try
            {
                for (int i = 1; i < num; i++)
                {
                    if (!string.IsNullOrEmpty(array[i]))
                    {
                        str = str + Path.DirectorySeparatorChar.ToString() + array[i];
                        if (!Directory.Exists(str))
                        {
                            Directory.CreateDirectory(str);
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }


        protected static void WriteLog(string content, LogEnum type = LogEnum.Info)
        {
            lock (loglock)
            {
                string filename = string.Empty;
                switch (type)
                {
                    case LogEnum.Info:
                        if (!Directory.Exists(infopath))//如果日志目录不存在就创建
                        {
                            Directory.CreateDirectory(infopath);
                        }
                        filename = infopath + "/info" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                        break;
                    case LogEnum.Error:
                        if (!Directory.Exists(errorpath))//如果日志目录不存在就创建
                        {
                            Directory.CreateDirectory(errorpath);
                        }
                        filename = errorpath + "/error" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                        break;
                    case LogEnum.Debug:
                        if (!Directory.Exists(debugpath))//如果日志目录不存在就创建
                        {
                            Directory.CreateDirectory(debugpath);
                        }
                        filename = debugpath + "/debug" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                        break;
                }
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                StreamWriter mySw = File.AppendText(filename);
                string write_content = time + " " + type + ": " + content;
                mySw.WriteLine(write_content);
                mySw.Close();
            }
        }
    }

    public enum LogEnum
    {
        Info,
        Error,
        Debug
    }
}
