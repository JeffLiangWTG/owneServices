using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	public interface ITransferrer : IDisposable
	{
		Task OpenAsync(XmlDocument configXml, ILog log, CancellationToken cancelToken);
		Task CloseAsync(ILog log, CancellationToken cancelToken);
		Task<List<TransferrerFileInfo>> ListFilesAsync(string folder, string fileMask, ILog log, CancellationToken cancelToken);
		Task<Stream> GetFileAsync(string path, ILog log, CancellationToken cancelToken);
		Task PutFileAsync(string path, Stream source, ILog log, CancellationToken cancelToken);
		Task RenameFileAsync(string path, string dest, ILog log, CancellationToken cancelToken);
		Task DeleteFileAsync(string path, ILog log, CancellationToken cancelToken);
		Task<bool> FileExistsAsync(string path, ILog log, CancellationToken cancelToken);
	}
}
