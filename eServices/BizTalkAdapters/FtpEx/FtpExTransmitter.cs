using System;
using CargoWise.eHub.BizTalkAdapters.Common;

namespace CargoWise.eHub.BizTalkAdapters.FtpEx
{
	public class FtpExTransmitter : TransferrerTransmitter
	{
		public FtpExTransmitter() : base(
			"FTPEx Transmitter",
			"1.0",
			"Custom FTP adapter",
			"FTPEx",
			new Guid("4d501d97-ab97-47dc-b35d-eb45567691a0"),
			"http://cargowise.com/ehub/biztalkadapters/ftpex-properties",
			typeof(FtpExTransmitterEndpoint),
			20)
		{
		}
	}
}
