using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.IO;
using System.Diagnostics;
using System.Collections;
using SharpCompress.Archive;

namespace TF_Img_Utils
{
    public class ZipAndRarFileHelper
    {
        public static string exepath = AppDomain.CurrentDomain.BaseDirectory + @"Resource\WinRAR.exe";

        /// <summary>
        /// 压缩包中文件数量
        /// </summary>
        /// <param name="straimpath"></param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public static int FileInZipCount(string straimpath, out string errmsg)
        {
            errmsg = "";
            int iNew;
            ZipEntry zipentity = null;
            FileStream fsfile = null;
            ZipFile zipfile = null;
            try
            {
                fsfile = new FileStream(straimpath, FileMode.OpenOrCreate);
                zipfile = new ZipFile(fsfile);
                long l_new = zipfile.Count;
                iNew = Convert.ToInt32(l_new);
                return iNew;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return 0;
            }
            finally
            {
                if (zipfile != null)
                {
                    if (zipfile.IsUpdating)
                    {
                        zipfile.CommitUpdate();
                    }
                    zipfile.Close();
                }
                if (fsfile != null)
                {
                    fsfile.Close();
                }
                if (zipentity != null)
                {
                    zipentity = null;
                }
            }
        }



        public static string RunCmd(string command)
        {
            //实例一个Process类，启动一个独立进程
            Process p = new Process();

            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c " + command;    //设定程式执行参数
            p.StartInfo.UseShellExecute = false;        //关闭Shell的使用
            p.StartInfo.RedirectStandardInput = true;   //重定向标准输入
            p.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
            p.StartInfo.RedirectStandardError = true;   //重定向错误输出
            p.StartInfo.CreateNoWindow = true;          //设置不显示窗口

            p.Start();   //启动
            string retstr = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
            return retstr;    //从输出流取得命令执行结果
        }

        /// <summary>
        /// 获得RAR信息
        /// </summary>
        /// <param name="rar_path">包文件路径</param>
        /// <returns></returns>
        public static Hashtable CheckRar(string rar_path)
        {
            string strcmd1 = string.Format("rar lb {0} ", rar_path);
            string outcmd_string1 = RunCmd(strcmd1).Replace("\r\n", "|");
            outcmd_string1 = outcmd_string1.Remove(outcmd_string1.LastIndexOf('|'));

            string strcmd2 = string.Format(" l {0} ", rar_path);
            string outcmd_string2 = RunCmd(strcmd2);

            string[] strfilenames = outcmd_string1.Split('|');
            int filecount = strfilenames.Length;
            string[] strfilesizes = new string[filecount];

            for (int i = 0; i < filecount; i++)
            {
                string filesize = outcmd_string2.Substring(outcmd_string2.IndexOf(strfilenames[i]) + strfilenames[i].Length).Trim();
                filesize = filesize.Substring(0, filesize.IndexOf(" "));
                strfilesizes[i] = filesize;
            }

            Hashtable ht_rar = new Hashtable();
            for (int i = 0; i < filecount; i++)
            {
                ht_rar.Add(strfilenames[i], strfilesizes[i]);
            }
            return ht_rar;
        }





        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static bool CreateZip(string sourceFilePath, string destinationZipFilePath, out string errmsg)
        {
            errmsg = "";
            try
            {
                if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                    sourceFilePath += System.IO.Path.DirectorySeparatorChar;
                if (!File.Exists(destinationZipFilePath))
                {
                    File.Create(destinationZipFilePath).Close();
                }
                else
                {
                    File.Delete(destinationZipFilePath);
                    File.Create(destinationZipFilePath).Close();
                }
                using (ZipOutputStream zipStream = new ZipOutputStream(new FileStream(destinationZipFilePath, FileMode.Create, FileAccess.Write, FileShare.Write)))
                {
                    zipStream.SetLevel(6);  // 压缩级别 0-9
                    CreateZipFiles(sourceFilePath, zipStream, sourceFilePath);
                    zipStream.Finish();
                    zipStream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名</param>
        /// <param name="staticFile"></param>
        private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream, string staticFile)
        {
            Crc32 crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    CreateZipFiles(file, zipStream, staticFile);
                }

                else                                            //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);

                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(staticFile.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(tempFile);

                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);

                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }








        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="dirToZip"></param>
        /// <param name="zipedFileName"></param>
        /// <param name="compressionLevel">压缩率0（无压缩）9（压缩率最高）</param>
        public static bool ZipDir(string dirToZip, string zipedFileName, out string errmsg, int compressionLevel = 9)
        {
            errmsg = "";
            try
            {
                if (!Directory.Exists(dirToZip))
                {
                    errmsg = "压缩路径不存在";
                    return false;
                }
                FileInfo fiinfo = new FileInfo(zipedFileName);
                if (!Directory.Exists(fiinfo.DirectoryName))
                {
                    Directory.CreateDirectory(fiinfo.DirectoryName);
                }
                if (Path.GetExtension(zipedFileName) != ".zip")
                {
                    zipedFileName = zipedFileName + ".zip";
                }
                using (FileStream fstream = File.Create(zipedFileName))
                {
                    using (var zipoutputstream = new ZipOutputStream(fstream))
                    {
                        zipoutputstream.SetLevel(compressionLevel);
                        Crc32 crc = new Crc32();
                        Hashtable fileList = GetAllFies(dirToZip);
                        foreach (DictionaryEntry item in fileList)
                        {
                            FileStream fs = new FileStream(item.Key.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Length);
                            ZipEntry entry = new ZipEntry(Path.GetFileName(item.Key.ToString()))
                            {
                                DateTime = (DateTime)item.Value,
                                Size = fs.Length
                            };
                            fs.Close();
                            crc.Reset();
                            crc.Update(buffer);
                            entry.Crc = crc.Value;
                            zipoutputstream.PutNextEntry(entry);
                            zipoutputstream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }

        }

        /// <summary> 
        /// 获取所有文件 
        /// </summary> 
        /// <returns></returns> 
        public static Hashtable GetAllFies(string dir)
        {
            Hashtable filesList = new Hashtable();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }

            GetAllDirFiles(fileDire, filesList);
            GetAllDirsFiles(fileDire.GetDirectories(), filesList);
            return filesList;
        }

        /// <summary> 
        /// 获取一个文件夹下的所有文件夹里的文件 
        /// </summary> 
        /// <param name="dirs"></param> 
        /// <param name="filesList"></param> 
        public static void GetAllDirsFiles(IEnumerable<DirectoryInfo> dirs, Hashtable filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("*.*"))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }

        /// <summary> 
        /// 获取一个文件夹下的文件 
        /// </summary> 
        /// <param name="dir">目录名称</param>
        /// <param name="filesList">文件列表HastTable</param> 
        public static void GetAllDirFiles(DirectoryInfo dir, Hashtable filesList)
        {
            foreach (FileInfo file in dir.GetFiles("*.*"))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }

        /// <summary> 
        /// 功能：解压zip格式的文件。 
        /// </summary> 
        /// <param name="zipFilePath">压缩文件路径</param> 
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param> 
        /// <returns>解压是否成功</returns> 
        public static void UnZip(string zipFilePath, string unZipDir)
        {
            if (zipFilePath == string.Empty)
            {
                throw new Exception("压缩文件不能为空！");
            }
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("压缩文件不存在！");
            }
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹 
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("/"))
                unZipDir += "/";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }
                    if (directoryName != null && !directoryName.EndsWith("/"))
                    {
                    }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {

                            int size;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 压缩单个文件
        /// </summary>
        /// <param name="filePath">被压缩的文件名称(包含文件路径)，文件的全路径</param>
        /// <param name="zipedFileName">压缩后的文件名称(包含文件路径)，保存的文件名称</param>
        /// <param name="compressionLevel">压缩率0（无压缩）到 9（压缩率最高）</param>
        public static void ZipFile(string filePath, string zipedFileName, int compressionLevel = 9)
        {
            // 如果文件没有找到，则报错 
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("文件：" + filePath + "没有找到！");
            }
            // 如果压缩后名字为空就默认使用源文件名称作为压缩文件名称
            if (string.IsNullOrEmpty(zipedFileName))
            {
                string oldValue = Path.GetFileName(filePath);
                if (oldValue != null)
                {
                    zipedFileName = filePath.Replace(oldValue, "") + Path.GetFileNameWithoutExtension(filePath) + ".zip";
                }
            }
            // 如果压缩后的文件名称后缀名不是zip，就是加上zip，防止是一个乱码文件
            if (Path.GetExtension(zipedFileName) != ".zip")
            {
                zipedFileName = zipedFileName + ".zip";
            }
            // 如果指定位置目录不存在，创建该目录 C:\Users\yhl\Desktop\大汉三通
            string zipedDir = zipedFileName.Substring(0, zipedFileName.LastIndexOf("\\", StringComparison.Ordinal));
            if (!Directory.Exists(zipedDir))
            {
                Directory.CreateDirectory(zipedDir);
            }
            // 被压缩文件名称
            string filename = filePath.Substring(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            var streamToZip = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var zipFile = File.Create(zipedFileName);
            var zipStream = new ZipOutputStream(zipFile);
            var zipEntry = new ZipEntry(filename);
            zipStream.PutNextEntry(zipEntry);
            zipStream.SetLevel(compressionLevel);
            var buffer = new byte[2048];
            Int32 size = streamToZip.Read(buffer, 0, buffer.Length);
            zipStream.Write(buffer, 0, size);
            try
            {
                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
            finally
            {
                zipStream.Finish();
                zipStream.Close();
                streamToZip.Close();
            }
        }

        /// <summary> 
        /// 压缩单个文件 
        /// </summary> 
        /// <param name="fileToZip">要进行压缩的文件名，全路径</param> 
        /// <param name="zipedFile">压缩后生成的压缩文件名,全路径</param> 
        public static void ZipFile(string fileToZip, string zipedFile)
        {
            // 如果文件没有找到，则报错 
            if (!File.Exists(fileToZip))
            {
                throw new FileNotFoundException("指定要压缩的文件: " + fileToZip + " 不存在!");
            }
            using (FileStream fileStream = File.OpenRead(fileToZip))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();
                using (FileStream zipFile = File.Create(zipedFile))
                {
                    using (ZipOutputStream zipOutputStream = new ZipOutputStream(zipFile))
                    {
                        // string fileName = fileToZip.Substring(fileToZip.LastIndexOf("\\") + 1);
                        string fileName = Path.GetFileName(fileToZip);
                        var zipEntry = new ZipEntry(fileName)
                        {
                            DateTime = DateTime.Now,
                            IsUnicodeText = true
                        };
                        zipOutputStream.PutNextEntry(zipEntry);
                        zipOutputStream.SetLevel(5);
                        zipOutputStream.Write(buffer, 0, buffer.Length);
                        zipOutputStream.Finish();
                        zipOutputStream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 压缩多个目录或文件
        /// </summary>
        /// <param name="folderOrFileList">待压缩的文件夹或者文件，全路径格式,是一个集合</param>
        /// <param name="zipedFile">压缩后的文件名，全路径格式</param>
        /// <param name="password">压宿密码</param>
        /// <returns></returns>
        public static bool ZipManyFilesOrDictorys(IEnumerable<string> folderOrFileList, string zipedFile, string password)
        {
            bool res = true;
            using (var s = new ZipOutputStream(File.Create(zipedFile)))
            {
                s.SetLevel(6);
                if (!string.IsNullOrEmpty(password))
                {
                    s.Password = password;
                }
                foreach (string fileOrDir in folderOrFileList)
                {
                    //是文件夹
                    if (Directory.Exists(fileOrDir))
                    {
                        res = ZipFileDictory(fileOrDir, s, "");
                    }
                    else
                    {
                        //文件
                        res = ZipFileWithStream(fileOrDir, s);
                    }
                }
                s.Finish();
                s.Close();
                return res;
            }
        }

        /// <summary>
        /// 带压缩流压缩单个文件
        /// </summary>
        /// <param name="fileToZip">要进行压缩的文件名</param>
        /// <param name="zipStream"></param>
        /// <returns></returns>
        private static bool ZipFileWithStream(string fileToZip, ZipOutputStream zipStream)
        {
            //如果文件没有找到，则报错
            if (!File.Exists(fileToZip))
            {
                throw new FileNotFoundException("指定要压缩的文件: " + fileToZip + " 不存在!");
            }
            //FileStream fs = null;
            FileStream zipFile = null;
            ZipEntry zipEntry = null;
            bool res = true;
            try
            {
                zipFile = File.OpenRead(fileToZip);
                byte[] buffer = new byte[zipFile.Length];
                zipFile.Read(buffer, 0, buffer.Length);
                zipFile.Close();
                zipEntry = new ZipEntry(Path.GetFileName(fileToZip));
                zipStream.PutNextEntry(zipEntry);
                zipStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (zipEntry != null)
                {
                }

                if (zipFile != null)
                {
                    zipFile.Close();
                }
                GC.Collect();
                GC.Collect(1);
            }
            return res;

        }

        /// <summary>
        /// 递归压缩文件夹方法
        /// </summary>
        /// <param name="folderToZip"></param>
        /// <param name="s"></param>
        /// <param name="parentFolderName"></param>
        private static bool ZipFileDictory(string folderToZip, ZipOutputStream s, string parentFolderName)
        {
            bool res = true;
            ZipEntry entry = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();
            try
            {
                //创建当前文件夹
                entry = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/")); //加上 “/” 才会当成是文件夹创建
                s.PutNextEntry(entry);
                s.Flush();
                //先压缩文件，再递归压缩文件夹
                var filenames = Directory.GetFiles(folderToZip);
                foreach (string file in filenames)
                {
                    //打开压缩文件
                    fs = File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file)));
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
                if (entry != null)
                {
                }
                GC.Collect();
                GC.Collect(1);
            }
            var folders = Directory.GetDirectories(folderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(folder, s, Path.Combine(parentFolderName, Path.GetFileName(folderToZip))))
                {
                    return false;
                }
            }
            return res;
        }

        /// <summary>
        /// 获取压缩包内文件
        /// </summary>
        /// <param name="targetFile"></param>
        /// <param name="dirname"></param>
        /// <returns></returns>
        public static List<string> GetZipFiles(string targetFile, out string dirname)
        {
            dirname = "";
            List<string> lstfilenames = new List<string>();
            string zipfiledirpath = Path.GetDirectoryName(targetFile);
            using (var achieve = ArchiveFactory.Open(targetFile))
            {

                string detectname = string.Empty;
                foreach (IArchiveEntry entry in achieve.Entries)
                {
                    if (!entry.IsDirectory)//文件
                    {
                        string filename = zipfiledirpath+"\\" + entry.FilePath;
                        lstfilenames.Add(filename);
                    }
                    else//目录
                    {
                        dirname = entry.FilePath;
                    }
                }
                return lstfilenames;
            }

        }


        /// <summary>
        /// 获取压缩包内文件
        /// </summary>
        /// <param name="targetFile"></param>
        /// <param name="dirname"></param>
        /// <returns></returns>
        public static List<string> GetZipFilesTest(string targetFile, out string dirname,out List<string> lstfiles)
        {
            dirname = "";
            lstfiles = new List<string>();
            List<string> lstfilenames = new List<string>();
            string zipfiledirpath = Path.GetDirectoryName(targetFile);
            using (var achieve = ArchiveFactory.Open(targetFile, SharpCompress.Common.Options.KeepStreamsOpen))
            {

                string detectname = string.Empty;
                foreach (IArchiveEntry entry in achieve.Entries)
                {
                    if (!entry.IsDirectory)//文件
                    {
                        string filename = zipfiledirpath + "\\" + entry.FilePath;
                        lstfilenames.Add(filename);
                        lstfiles = lstfilenames;
                    }
                    else//目录
                    {
                        dirname = entry.FilePath;
                    }
                }
                return lstfilenames;
            }

        }


        /// <summary>
        /// 读取zip文件夹中的文件名称
        /// </summary>
        /// <param name="tempPath"></param>
        /// <param name="lstimgs"></param>
        /// <param name="lstnames"></param>
        /// <param name="lstlabelfiles"></param>
        public static void GetFilesFromZipFile(string tempPath, out IList<ICSharpCode.SharpZipLib.Zip.ZipEntry> lstimgs, out IList<ICSharpCode.SharpZipLib.Zip.ZipEntry> lstnames, out IList<ICSharpCode.SharpZipLib.Zip.ZipEntry> lstlabelfiles)
        {
            using (ZipFile TemplateZip = new ICSharpCode.SharpZipLib.Zip.ZipFile(tempPath))
            {
                IEnumerable<ICSharpCode.SharpZipLib.Zip.ZipEntry> FileCheck = TemplateZip.Cast<ICSharpCode.SharpZipLib.Zip.ZipEntry>();

                lstimgs = FileCheck.Where(p => p.Name.ToLower().EndsWith(".png") || p.Name.ToLower().EndsWith(".jpg") || p.Name.ToLower().EndsWith(".jpeg") || p.Name.ToLower().EndsWith(".bmp") || p.Name.ToLower().EndsWith(".gif")).ToList();
                lstnames = FileCheck.Where(p => p.Name.ToLower().EndsWith(".names")).ToList();
                lstlabelfiles = FileCheck.Where(p => p.Name.ToLower().EndsWith(".objdettest") || p.Name.ToLower().EndsWith(".objdettrain")).ToList();
            }

        }


    }

}
