
namespace Enterprise.Customs.SG.V4.Business
{
	public static class SGBatchProcessorConstants
	{
		public static class Directories
		{
			public const string OuputDirectory = "SG4Outbox";
			public const string InputDirectory = "SG4Inbox";
		}

		public static class Test
		{
			public const string SecureFtpID = "cwise";
			public const string RecipientMailBox = "DCST401";
			public const string HomeDirectory = @"/fshome/sftp/cwise";
		}
	}
}
