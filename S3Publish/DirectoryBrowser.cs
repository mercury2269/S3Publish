using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace S3Publish
{
    public class DirectoryBrowser
    {
        public List<LocalFile> GetAllFiles(string rootPath)
        {
            return GetAllFilesInPath(rootPath).Select(path => new LocalFile
                                                           {
                                                               Key = GetFileKey(rootPath, path), 
                                                               Path = path,
                                                               ETag = CreateEtag(path)
                                                           })
                .ToList();
        }

        private IEnumerable<string> GetAllFilesInPath(string path)
        {
            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
        }

        private string GetFileKey(string rootPath, string filePath)
        {
            if (rootPath.EndsWith("\\"))
            {
                return filePath.Replace(rootPath, "").Replace(@"\", "/");
            }
            else
            {
                return filePath.Replace(rootPath + "\\", "").Replace(@"\", "/");
            }
            
        }

        private string CreateEtag(string path)
        {
            return "\"" + GetComputedHash(path) + "\"";
        }

        private string GetComputedHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}