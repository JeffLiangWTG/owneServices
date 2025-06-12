using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Renci.SshNet.Sftp;
namespace Renci.SshNet
{
	/// <customization>
	/// Added for testing.
	/// </customization>
	public interface ISftpClient : IBaseClient
    {
        void AppendAllLines(string path, IEnumerable<string> contents);
        void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);
        void AppendAllText(string path, string contents);
        void AppendAllText(string path, string contents, Encoding encoding);
        StreamWriter AppendText(string path);
        StreamWriter AppendText(string path, Encoding encoding);
		IAsyncResult BeginDownloadFile(string path, Stream output);
		IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback);
		IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback, object state, Action<ulong> downloadCallback = null);
		IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state, Action<int> listCallback = null);
        IAsyncResult BeginSynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern, AsyncCallback asyncCallback, object state);
		IAsyncResult BeginUploadFile(Stream input, string path);
		IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback);
		IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null);
		IAsyncResult BeginUploadFile(Stream input, string path, bool canOverride, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null);
        uint BufferSize { get; set; }
        void ChangeDirectory(string path);
        void ChangePermissions(string path, short mode);
        SftpFileStream Create(string path);
        SftpFileStream Create(string path, int bufferSize);
        void CreateDirectory(string path);
        StreamWriter CreateText(string path);
        StreamWriter CreateText(string path, Encoding encoding);
        void Delete(string path);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
		void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null);
        void EndDownloadFile(IAsyncResult asyncResult);
        IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult);
        IEnumerable<FileInfo> EndSynchronizeDirectories(IAsyncResult asyncResult);
        void EndUploadFile(IAsyncResult asyncResult);
        bool Exists(string path);
        SftpFile Get(string path);
        SftpFileAttributes GetAttributes(string path);
        DateTime GetLastAccessTime(string path);
        DateTime GetLastAccessTimeUtc(string path);
        DateTime GetLastWriteTime(string path);
        DateTime GetLastWriteTimeUtc(string path);
        SftpFileSytemInformation GetStatus(string path);
		IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null);
		IEnumerable<ISftpFile> ListDirectoryI(string path);
        SftpFileStream Open(string path, FileMode mode);
        SftpFileStream Open(string path, FileMode mode, FileAccess access);
        SftpFileStream OpenRead(string path);
        StreamReader OpenText(string path);
        SftpFileStream OpenWrite(string path);
        TimeSpan OperationTimeout { get; set; }
        int ProtocolVersion { get; }
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string path);
        string[] ReadAllLines(string path, Encoding encoding);
        string ReadAllText(string path);
        string ReadAllText(string path, Encoding encoding);
        IEnumerable<string> ReadLines(string path);
        IEnumerable<string> ReadLines(string path, Encoding encoding);
        void RenameFile(string oldPath, string newPath);
        void RenameFile(string oldPath, string newPath, bool isPosix);
        void SetAttributes(string path, SftpFileAttributes fileAttributes);
        void SetLastAccessTime(string path, DateTime lastAccessTime);
        void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
        void SetLastWriteTime(string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
        void SymbolicLink(string path, string linkPath);
        IEnumerable<FileInfo> SynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern);
		TraceListener TraceLog { get; set; }
		void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null);
		void UploadFile(Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null);
        string WorkingDirectory { get; }
        void WriteAllBytes(string path, byte[] bytes);
        void WriteAllLines(string path, IEnumerable<string> contents);
        void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);
        void WriteAllLines(string path, string[] contents);
        void WriteAllLines(string path, string[] contents, Encoding encoding);
        void WriteAllText(string path, string contents);
        void WriteAllText(string path, string contents, Encoding encoding);
    }
}
