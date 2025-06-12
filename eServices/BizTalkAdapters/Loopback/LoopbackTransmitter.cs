using System;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Loopback
{
	public class LoopbackTransmitter : AsyncTransmitter
	{
		public LoopbackTransmitter()
			: base("Loopback Adapter",
				   "1.0",
				   "Loopback Adapter",
				   "loopback",
				   new Guid("91dc6996-1d71-432e-9164-14aa0664b677"),
				   "http://cargowise.com/ehub/biztalkadapters/loopback-properties",
				   typeof(LoopbackTransmitterEndpoint),
				   10)
		{
		}
	}
}
