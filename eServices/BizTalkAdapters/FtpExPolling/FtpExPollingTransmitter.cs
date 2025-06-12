using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;

namespace CargoWise.eHub.BizTalkAdapters.FtpExPolling
{
	public class FtpExPollingTransmitter : Transmitter
	{
		const string NAME = "FTPEx-Polling";

		public FtpExPollingTransmitter()
			: base(
				String.Format("{0} Transmitter", NAME),
				"1.0",
				"Custom FTP polling adapter",
				NAME,
				new Guid("525c9cd4-19a2-4de2-b8ef-661b35eb3d31"),
				"http://cargowise.com/ehub/biztalkadapters/ftpex-polling-properties",
				typeof(FtpExPollingTransmitterEndpoint),
				1)
		{
		}
	}
}
