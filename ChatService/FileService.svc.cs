using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace ChatService
{
    public class FileService : IFileService
    {
        private string _temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_files");
        private int _timeToDelete = 30 * 60 * 100;

        public FileService()
        {
            if (!Directory.Exists(_temp))
            {
                Directory.CreateDirectory(_temp);
            }
        }

        public Stream DownloadFile(string fileName)
        {
            string filePath = Path.Combine(_temp, fileName);
            if (!File.Exists(filePath)) return null;
            Stream r;
            try
            {
                r = File.OpenRead(filePath);
            }
            catch (Exception e)
            {
                return null;
            }
            return r;
        }

        public string UploadFile(Stream file)
        {
            if (file == null) return null;

            var name = $"{Guid.NewGuid():N}.tmp";
            string filePath = Path.Combine(_temp, name);

            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }

            Timer timer = null;
            timer = new Timer(state =>
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                timer.Dispose();
                //Set timer to delete uploaded file after 30 minutes
            }, null, _timeToDelete, Timeout.Infinite);
            
            return name;
        }
    }
}