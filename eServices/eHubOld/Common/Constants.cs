namespace CargoWise.eServices.eHub.Common
{
	public class Constants
	{
		public class InboxEnvelopeStatus
		{
			public const string Receiverd = "REC";
			public const string Processed = "PSD";
			public const string Failure = "FLR";
		}

		public class SendResponseMessageStatus
		{
			public const string OK = "OK!";
			public const string Failure = "Failure!";
			public const string OKWithWarning = "HasWarning";
		}

		public const string InterchangeVersion = "1.0";
		public const string DefaultApplicationVersion = "1.0";

		public const string DefaultDatabaseName = "eHubTransactions";
	}
}
