using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
 

namespace TF_Img_Utils
{
    public class FileHelper
    {
        public static bool Read(string filename, out string filemsg)
        {
            string filecontent = string.Empty;
            try
            {
                if (File.Exists(filename))
                {
                    StreamReader sreader = new StreamReader(filename);
                    filecontent = sreader.ReadToEnd();
                    sreader.Close();
                    sreader.Dispose();
                }
                filemsg = filecontent;
                return !string.IsNullOrEmpty(filecontent);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                filemsg = filecontent;
                return !string.IsNullOrEmpty(filecontent);
            }
        }


        public static void Write(string filepath,string filecontent)
        {
            //FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                StreamWriter swriter = new StreamWriter(filepath);
                //开始写入
                swriter.Write(filecontent);
                //清空缓冲区
                swriter.Flush();
                //关闭流
                swriter.Close();
                swriter.Dispose();
            }
            catch (Exception ex)
            {
                
            }
            
        }


        public static Bitmap DrawRectangleInPicture(Bitmap bmp, string LabelName, int X0, int Y0, int X1, int Y1)
        {
            if (bmp == null) return null;
            Graphics g = Graphics.FromImage(bmp);
            Brush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(brush, 1);
            pen.DashStyle = DashStyle.Solid;
            g.DrawRectangle(pen, new Rectangle(X0, Y0, Math.Abs(X1 - X0), Math.Abs(Y1 - Y0)));
            Font familyfont = new System.Drawing.Font("宋体", 3F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            Brush brushblue = new SolidBrush(Color.DarkBlue);
            g.DrawString(LabelName.ToNullString(), familyfont, brushblue, X0, (Y0-8F)>0?(Y0-8F):0);
            g.Dispose();
            return bmp;
        }

        public static string DeCompression(string targetFile)
        {
            try
            {
                string detectname = string.Empty;
                using (FileStream fs = new FileStream(targetFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                        string zipfile = Path.GetDirectoryName(targetFile);
                        var achieve = ArchiveFactory.Open(fs);
                        foreach (IArchiveEntry entry in achieve.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(zipfile, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                            }
                            //else
                            //{
                            //    string[] valuearrs = entry.FilePath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                            //    if (valuearrs.Length == 1)
                            //    {
                            //        detectname = valuearrs[0];
                            //    }
                            //}
                        }
                    return detectname;
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }





        public static void UnZip(string zipedFile, string strDirectory="", string password="", bool overWrite=true)
        {
            strDirectory = Path.GetDirectoryName(zipedFile);
            if (string.IsNullOrEmpty(zipedFile))
            {
                throw new ArgumentException(zipedFile);
            }
            if (string.IsNullOrEmpty(strDirectory))
            {
                throw new ArgumentException(strDirectory);
            }
            //if (string.IsNullOrEmpty(password))
            //{
            //    throw new ArgumentException(password);
            //}
            if (strDirectory == "")
            {
                strDirectory = Directory.GetCurrentDirectory();
            }
            if (!strDirectory.EndsWith("\\"))
            {
                strDirectory = strDirectory + "\\";
            }
            try
            {
                using (var s = new ZipInputStream(File.OpenRead(zipedFile)))
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        s.Password = password;
                    }
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        var directoryName = string.Empty;
                        var pathToZip = theEntry.Name;
                        if (pathToZip != "")
                        {
                            directoryName = Path.GetDirectoryName(pathToZip) + "\\";
                        }
                        var fileName = Path.GetFileName(pathToZip);
                        Directory.CreateDirectory(strDirectory + directoryName);
                        if (fileName == "") continue;
                        if ((!File.Exists(strDirectory + directoryName + fileName) || !overWrite) &&
                            (File.Exists(strDirectory + directoryName + fileName))) continue;
                        using (var streamWriter = File.Create(strDirectory + directoryName + fileName))
                        {
                            var data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);

                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else
                                    break;
                            }
                            streamWriter.Close();
                        }
                    }

                    s.Close();
                }
            }
            catch (IOException ioex)
            {
                throw new IOException(ioex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
