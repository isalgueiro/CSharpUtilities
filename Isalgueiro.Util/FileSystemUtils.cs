namespace Isalgueiro.Util
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Utilities for working with directories and files
    /// </summary>
    class FileSystemUtils
    {

        private static readonly String[] TEXT_EXTS = { ".bat", ".classpath", ".conf", ".css", ".cvsignore", ".dtd", ".html", ".java", ".js", ".jsp", ".jspf", ".log", ".properties", ".sh", ".sql", ".tld", ".txt", ".wsdd", ".wsdl", ".xml", ".xsd", ".xsl" };
        private static readonly String[] BINARY_EXTS = { ".class", ".doc", ".exe", ".gif", ".gz", ".jar", ".jpg", ".mdb", ".pdf", ".rar", ".sar", ".swf", ".swp", ".vsd", ".war", ".xls", ".zip" };
        private static readonly List<String> textExts = new List<string>(TEXT_EXTS);
        private static readonly List<String> bianryExts = new List<string>(BINARY_EXTS);

        /// <summary>
        /// Checks if given file is binary data (returns true) or text (false)
        /// </summary>
        /// <seealso>http://www.entechsolutions.com/how-to-detect-if-file-is-text-or-binary-using-c</seealso>
        /// <param name="filePath">Full path to file</param>
        /// <param name="sampleSize">Optional. Ammount of bytes that will read from file (default 10KB) to find out if this is a text file or binary data. Lower sample size runs faster, higher sample size gives more accurate results</param>
        /// <returns><code>true</code> if it is a binary file</returns>
        public static bool IsBinaryFile(string filePath, int sampleSize = 10240)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("filePath");
            }

            FileInfo fi = new FileInfo(filePath);
            String ext = fi.Extension.ToLower();
            fi = null;
            bool hasExtension = !String.IsNullOrEmpty(ext);
            if (hasExtension)
            {
                if (textExts.Contains(ext))
                {
                    return false;
                }
                if (bianryExts.Contains(ext))
                {
                    return true;
                }
            }


            char[] buffer = new char[sampleSize];
            string sampleContent;

            using (var sr = new StreamReader(filePath))
            {
                int length = sr.Read(buffer, 0, sampleSize);
                sampleContent = new string(buffer, 0, length);
            }

            if (sampleContent.Contains("\0\0\0"))
            {
                if (hasExtension)
                {
                    bianryExts.Add(ext);
                }
                return true;
            }
            if (hasExtension)
            {
                textExts.Add(ext);
            }
            return false;
        }

        /// <summary>
        /// Recursively <b>CLONES</b> a directory; deletes directories and files that exists in destination folder but doesn't in source
        /// </summary>
        /// <param name="SourcePath">Directory to be cloned</param>
        /// <param name="DestinationPath">Path where SourcePath will be cloned</param>
        /// <param name="overwriteexisting">If true will overwrite existing files in destination path</param>
        /// <param name="ignore">Folder name to ignore</param>
        public static void CloneDirectory(string SourcePath, string DestinationPath, bool overwriteexisting, String ignore)
        {
            SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
            DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

            if (Directory.Exists(DestinationPath) == false)
                Directory.CreateDirectory(DestinationPath);

            List<String> sourceFiles = new List<string>();
            foreach (string fls in Directory.GetFiles(SourcePath))
            {
                FileInfo flinfo = new FileInfo(fls);
                flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                sourceFiles.Add(flinfo.Name);
            }
            foreach (string fls in Directory.GetFiles(DestinationPath))
            {
                FileInfo flinfo = new FileInfo(fls);
                if (!sourceFiles.Contains(flinfo.Name))
                {
                    flinfo.Delete();
                }
            }
            sourceFiles = new List<string>();
            foreach (string drs in Directory.GetDirectories(SourcePath))
            {
                DirectoryInfo drinfo = new DirectoryInfo(drs);
                if (!drinfo.Name.Equals(ignore))
                {
                    CloneDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting, ignore);
                    sourceFiles.Add(drinfo.Name);
                }
            }
            foreach (string drs in Directory.GetDirectories(DestinationPath))
            {
                DirectoryInfo drinfo = new DirectoryInfo(drs);
                if (!drinfo.Name.Equals(ignore) && !sourceFiles.Contains(drinfo.Name))
                {
                    drinfo.Delete(true);
                }
            }
        }

        /// <summary>
        /// $ mkdir -p
        /// </summary>
        /// <param name="dirPath"></param>
        public static void CreateDirectory(String dirPath)
        {
            CreateDirectory(new DirectoryInfo(dirPath));
        }

        /// <summary>
        /// $ mkdir -p
        /// </summary>
        /// <param name="dirinfo"></param>
        public static void CreateDirectory(DirectoryInfo dirinfo)
        {
            if (!dirinfo.Exists)
            {
                if (!dirinfo.Parent.Exists)
                {
                    CreateDirectory(dirinfo.Parent);
                }
                dirinfo.Create();
            }
        }

        /// <summary>
        /// It seems that when your filesystem is using something different than UTF things like File.Exists fail. This method will convert the path to UTF8, returning an String you can use to refer to this path in your code so things like File.Exists will work.
        /// </summary>
        /// <param name="defaultEncodingPath">String as read with the default encoding</param>
        /// <returns>UTF8 and windows-friendly path</returns>
        public static String GetUtf8ValdPath(String defaultEncodingPath)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(defaultEncodingPath)).Replace("/", @"\");
        }
    }
}