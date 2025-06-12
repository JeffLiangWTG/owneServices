using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public interface IWinScpClient : IDisposable
	{
		void Close();
		Task CloseAsync(CancellationToken cancelToken);
		IEnumerable<WinScpFileInfo> EnumerateRemoteFiles(WinScpLocation location);
		Task<IEnumerable<WinScpFileInfo>> EnumerateRemoteFilesAsync(WinScpLocation location, CancellationToken cancelToken);
		bool FileExists(WinScpLocation location);
		Task<bool> FileExistsAsync(WinScpLocation location, CancellationToken cancelToken);
		Stream GetFile(WinScpLocation location);
		Task<Stream> GetFileAsync(WinScpLocation location, CancellationToken cancelToken);
		void MoveFile(WinScpLocation fromPath, WinScpLocation toPath);
		Task MoveFileAsync(WinScpLocation fromPath, WinScpLocation toPath, CancellationToken cancelToken);
		void Open();
		Task OpenAsync(CancellationToken cancelToken);
		void PutFile(Stream stream, WinScpLocation location);
		Task PutFileAsync(Stream stream, WinScpLocation location, CancellationToken cancelToken);
		void RemoveFile(WinScpLocation downloadLocn);
		Task RemoveFileAsync(WinScpLocation downloadLocn, CancellationToken cancelToken);
	}

	public interface IWinScpClientFactory
	{
		IWinScpClient CreateClient(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId);
		Task<IWinScpClient> CreateClientAsync(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId, CancellationToken cancellationToken);
	}
}
