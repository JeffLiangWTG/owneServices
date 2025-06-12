using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public interface ITransferrer : IDisposable
	{
		void ReadLocationConfiguration(XmlDocument configDOM);
		void Open();
		void Close();
		IEnumerable<TransferrerFileInfo> ListFiles (TransferrerProperties.Receive.Location location, TransferrerProperties.Receive receive, CancellationTokenSource cancellationTokenSource);
		Stream GetFile(string path);
		void PutFile(string path, Stream source);
		void RenameFile(string path, string dest);
		void DeleteFile(string path);
		bool FileExists(string path);

		string Server { get; set; }
		int Port { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		int Timeout { get; set; }
		ILog Logger { get; set; }
		string ConfigDom { get; set; }
		CancellationToken CancelToken { get; set; }
		bool HighPriority { get; set; }
	}
}
