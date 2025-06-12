using System;
using CargoWise.eHub.BizTalkAdapters.Common;
using System.Threading;
using System.Collections.Concurrent;

namespace CargoWise.eHub.BizTalkAdapters.FtpEx
{
	public class FtpExReceiver : TransferrerReceiver 
	{
		public FtpExReceiver() : base(
			"FTPEx Adapter",
			"1.0",
			"Custom FTP adapter",
			"FTPEx",
			new Guid("077b332e-933a-4526-9327-9581ab6a3f01"),
			"http://cargowise.com/ehub/biztalkadapters/ftpex-properties",
			typeof(FtpExReceiverEndpoint))
		{
		}
	}
}