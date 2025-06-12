using System;
namespace FluentFTP
{
    public interface IFtpClient
    {
        System.Collections.Generic.IEnumerable<int> ActivePorts { get; set; }
        Func<string> AddressResolver { get; set; }
        IAsyncResult BeginConnect(AsyncCallback callback, object state);
        IAsyncResult BeginCreateDirectory(string path, AsyncCallback callback, object state);
        IAsyncResult BeginCreateDirectory(string path, bool force, AsyncCallback callback, object state);
        IAsyncResult BeginDeleteDirectory(string path, AsyncCallback callback, object state, bool fastMode = false);
        IAsyncResult BeginDeleteDirectory(string path, bool force, FtpListOption options, bool fastMode, AsyncCallback callback, object state);
        IAsyncResult BeginDeleteDirectory(string path, bool force, AsyncCallback callback, object state, bool fastMode = false);
        IAsyncResult BeginDeleteFile(string path, AsyncCallback callback, object state);
        IAsyncResult BeginDereferenceLink(FtpListItem item, AsyncCallback callback, object state);
        IAsyncResult BeginDereferenceLink(FtpListItem item, int recMax, AsyncCallback callback, object state);
        IAsyncResult BeginDirectoryExists(string path, AsyncCallback callback, object state);
        IAsyncResult BeginDisconnect(AsyncCallback callback, object state);
        IAsyncResult BeginExecute(string command, AsyncCallback callback, object state);
        IAsyncResult BeginFileExists(string path, FtpListOption options, AsyncCallback callback, object state);
        IAsyncResult BeginFileExists(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetFileSize(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetHash(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetHashAlgorithm(AsyncCallback callback, object state);
        IAsyncResult BeginGetListing(AsyncCallback callback, object state);
        IAsyncResult BeginGetListing(string path, FtpListOption options, AsyncCallback callback, object state);
        IAsyncResult BeginGetListing(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetModifiedTime(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetNameListing(AsyncCallback callback, object state);
        IAsyncResult BeginGetNameListing(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetObjectInfo(string path, AsyncCallback callback, object state);
        IAsyncResult BeginGetWorkingDirectory(AsyncCallback callback, object state);
        IAsyncResult BeginOpenAppend(string path, FtpDataType type, AsyncCallback callback, object state);
        IAsyncResult BeginOpenAppend(string path, AsyncCallback callback, object state);
        IAsyncResult BeginOpenRead(string path, FtpDataType type, AsyncCallback callback, object state);
        IAsyncResult BeginOpenRead(string path, FtpDataType type, long restart, AsyncCallback callback, object state);
        IAsyncResult BeginOpenRead(string path, AsyncCallback callback, object state);
        IAsyncResult BeginOpenRead(string path, long restart, AsyncCallback callback, object state);
        IAsyncResult BeginOpenWrite(string path, FtpDataType type, AsyncCallback callback, object state);
        IAsyncResult BeginOpenWrite(string path, AsyncCallback callback, object state);
        IAsyncResult BeginRename(string path, string dest, AsyncCallback callback, object state);
        IAsyncResult BeginSetHashAlgorithm(FtpHashAlgorithm type, AsyncCallback callback, object state);
        IAsyncResult BeginSetWorkingDirectory(string path, AsyncCallback callback, object state);
        FtpCapability Capabilities { get; }
        void Chmod(string path, FtpPermission owner, FtpPermission group, FtpPermission other);
        void Chmod(string path, int permissions);
        System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates { get; }
        void Connect();
        string ConnectionType { get; }
        int ConnectTimeout { get; set; }
        System.Diagnostics.TraceListener ControlDialogTrace { get; set; }
        void CreateDirectory(string path);
        void CreateDirectory(string path, bool force);
        System.Net.NetworkCredential Credentials { get; set; }
        int DataConnectionConnectTimeout { get; set; }
        bool DataConnectionEncryption { get; set; }
        int DataConnectionReadTimeout { get; set; }
        FtpDataConnectionType DataConnectionType { get; set; }
        void DeleteDirectory(string path, bool fastMode = false);
        void DeleteDirectory(string path, bool force, FtpListOption options, bool fastMode = false);
        void DeleteDirectory(string path, bool force, bool fastMode = false);
        void DeleteFile(string path);
        FtpListItem DereferenceLink(FtpListItem item);
        FtpListItem DereferenceLink(FtpListItem item, int recMax);
        bool DirectoryExists(string path);
        void DisableUTF8();
        void Disconnect();
        void Dispose();
        bool DownloadFile(out byte[] outBytes, string remotePath);
        bool DownloadFile(System.IO.Stream outStream, string remotePath);
        bool DownloadFile(string localPath, string remotePath, bool overwrite = true);
        int DownloadFiles(string localDir, System.Collections.Generic.List<string> remotePaths, bool overwrite = true);
        int DownloadFiles(string localDir, string[] remotePaths, bool overwrite = true);
        bool EnableThreadSafeDataConnections { get; set; }
        System.Text.Encoding Encoding { get; set; }
        FtpEncryptionMode EncryptionMode { get; set; }
        void EndConnect(IAsyncResult ar);
        void EndCreateDirectory(IAsyncResult ar);
        void EndDeleteDirectory(IAsyncResult ar);
        void EndDeleteFile(IAsyncResult ar);
        FtpListItem EndDereferenceLink(IAsyncResult ar);
        bool EndDirectoryExists(IAsyncResult ar);
        void EndDisconnect(IAsyncResult ar);
        FtpReply EndExecute(IAsyncResult ar);
        bool EndFileExists(IAsyncResult ar);
        long EndGetFileSize(IAsyncResult ar);
        void EndGetHash(IAsyncResult ar);
        FtpHashAlgorithm EndGetHashAlgorithm(IAsyncResult ar);
        FtpListItem[] EndGetListing(IAsyncResult ar);
        DateTime EndGetModifiedTime(IAsyncResult ar);
        string[] EndGetNameListing(IAsyncResult ar);
        FtpListItem EndGetObjectInfo(IAsyncResult ar);
        string EndGetWorkingDirectory(IAsyncResult ar);
        System.IO.Stream EndOpenAppend(IAsyncResult ar);
        System.IO.Stream EndOpenRead(IAsyncResult ar);
        System.IO.Stream EndOpenWrite(IAsyncResult ar);
        void EndRename(IAsyncResult ar);
        void EndSetHashAlgorithm(IAsyncResult ar);
        void EndSetWorkingDirectory(IAsyncResult ar);
        FtpReply Execute(string command);
        FtpReply Execute(string command, params object[] args);
        bool FileExists(string path);
        bool FileExists(string path, FtpListOption options);
        int GetChmod(string path);
        FtpListItem GetFilePermissions(string path);
        long GetFileSize(string path);
        FtpHash GetHash(string path);
        FtpHashAlgorithm GetHashAlgorithm();
        FtpListItem[] GetListing();
        FtpListItem[] GetListing(string path);
        FtpListItem[] GetListing(string path, FtpListOption options);
        DateTime GetModifiedTime(string path);
        string[] GetNameListing();
        string[] GetNameListing(string path);
        FtpListItem GetObjectInfo(string path);
        string GetWorkingDirectory();
        bool HasFeature(FtpCapability cap);
        FtpHashAlgorithm HashAlgorithms { get; }
        string Host { get; set; }
        FtpIpVersion InternetProtocolVersions { get; set; }
        bool IsConnected { get; }
        bool IsDisposed { get; }
        int MaximumDereferenceCount { get; set; }
        System.IO.Stream OpenAppend(string path);
        System.IO.Stream OpenAppend(string path, FtpDataType type);
        System.IO.Stream OpenRead(string path);
        System.IO.Stream OpenRead(string path, FtpDataType type);
        System.IO.Stream OpenRead(string path, FtpDataType type, long restart);
        System.IO.Stream OpenRead(string path, long restart);
        System.IO.Stream OpenWrite(string path);
        System.IO.Stream OpenWrite(string path, FtpDataType type);
        int Port { get; set; }
        int ReadTimeout { get; set; }
        void Rename(string path, string dest);
        void SetFilePermissions(string path, FtpPermission owner, FtpPermission group, FtpPermission other);
        void SetFilePermissions(string path, int permissions);
        void SetHashAlgorithm(FtpHashAlgorithm type);
        void SetWorkingDirectory(string path);
        bool SocketKeepAlive { get; set; }
        int SocketPollInterval { get; set; }
        System.Security.Authentication.SslProtocols SslProtocols { get; set; }
        bool StaleDataCheck { get; set; }
        string SystemType { get; }
        int TransferChunkSize { get; set; }
        bool UngracefullDisconnection { get; set; }
        bool UploadFile(byte[] fileData, string remotePath, bool overwrite = true, bool createRemoteDir = false, bool checkFileExistance = true);
        bool UploadFile(System.IO.Stream fileStream, string remotePath, bool overwrite = true, bool createRemoteDir = false, bool checkFileExistance = true);
        bool UploadFile(string localPath, string remotePath, bool overwrite = true, bool createRemoteDir = false, bool checkFileExistance = true);
        int UploadFiles(System.Collections.Generic.List<string> localPaths, string remoteDir, bool overwrite = true, bool createRemoteDir = true);
        int UploadFiles(string[] localPaths, string remoteDir, bool overwrite = true, bool createRemoteDir = true);
        event FtpSslValidation ValidateCertificate;
    }
}
