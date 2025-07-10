
namespace Enterprise.Customs.SG.V4.MHUB
{
	public static class MHUBConstants
	{
		public const string RequestStatusOK = "0";
		public const string RequestStatusOKNoData = "1";
		public const string RequestStatusFail = "-1";
		public const string JSessionIDCookieName = "JSESSIONID";
		public const string MHubDateFormat = "ddMMyyyy";
		public const string MHXWIN = "MHXWIN";
		public const string CARGOWISE = "CargoWise";

		public enum CommandType { Login, Submit, Retrieve, Delete, Logout, None, getRTkey, getParam }

		public enum ResponseStatusCodes { Unknown, CompleteSuccess, PartialSuccess, MailboxEmpty, Failed }

		public enum DownloadAndDecryptOptions { None, DownloadOnly, DownloadAndDecrypt, }

		public static class Parameters
		{
			public const string ErrorCode = "ErrorCode";
			public const string ErrorMsg = "ErrorMsg";
			public const string recip_id = "recip_id";
			public const string msg_id = "msg_id";
			public const string msgId = "msgId";
			public const string path = "path";
			public const string requestStatus = "requestStatus";
			public const string attachment = "attachment";
			public const string loc = "loc";
			public const string filename = "filename";
			public const string cont_type = "cont_type";
			public const string destroy = "destroy";
			public const string notifn = "notifn";
			public const string Userid = "Userid";
			public const string Password = "Password";
			public const string AppId = "AppId";
			public const string NewPassword = "NewPassword";
			public const string Encrypted = "Encrypted";
			public const string zipfile = "zipfile";
			public const string Split = "Split";
			public const string cont_id = "cont_id";
			public const string recipientID = "recip_id";
			public const string mail_type = "mail_type";
			public const string MessageMailType = "1";
			public const string Digest = "DigestForLibs";
			public const string ClientID = "ClientId";
			public const string CurrentVersion = "CurrentVersion";
			public const string VndId = "VndId";
		}
	}
}
