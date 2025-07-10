using System;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICommunicationModule : IZFilterGridModule, IDisposable
	{
		IOrgHeader Header { get; set; }
	}
}
