using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	[ExcludeFromCodeCoverage]
	public class HttpExTransmitter : Transmitter
	{
		public HttpExTransmitter()
			: base(
				"HTTPEx Transmitter",
				"1.0",
				"Custom HTTP adapter",
				"HTTPEx",
				new Guid("525c9cd4-19a2-4de2-b8ef-661b35eb3d31"),
				HttpExPropertyNamespace,
				typeof(HttpExTransmitterEndpoint),
				1)
		{
		}

		internal const string HttpExPropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/httpex-properties";
	}
}
