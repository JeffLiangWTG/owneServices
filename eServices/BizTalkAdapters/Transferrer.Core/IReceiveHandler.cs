using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	public interface IReceiveHandler : IDisposable
	{
		Task OpenAsync(XmlDocument configXml, ILog log, CancellationToken cancelToken);
		Task<List<TransferrerFileInfo>> ListServerFilesAsync(ILog log, CancellationToken cancelToken);
		Task<Stream> DownloadAsync(TransferrerFileInfo file, ILog log, CancellationToken cancelToken);
		Task PostDownloadProcessingAsync(TransferrerFileInfo file, ILog log);
		Task CloseAsync(ILog log, CancellationToken cancelToken);
	}
}