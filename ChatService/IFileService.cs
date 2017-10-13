using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ChatService
{
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        Stream DownloadFile(string fileName);

        [OperationContract]
        string UploadFile(Stream file);
    }
}
