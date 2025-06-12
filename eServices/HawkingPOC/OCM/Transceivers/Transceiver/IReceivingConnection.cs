using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OcmPoc.Transceivers
{
	public interface IReceivingConnection : IConnection
	{
		TimeSpan PollInterval { get; }
		Task<IEnumerable<string>> ListAsync();
		Task<Stream> ReceiveAsync(string name);
		Task AcknowledgeAsync(string name);
	}
}