using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class ErrorCodeList : CodeDescriptionPairList
	{
		public ErrorCodeList()
		{
			AddErrorCode("4", "1", "DETAIN FOR PORT HEALTH");
			AddErrorCode("4", "2", "DETAIN FOR STATE VET");
			AddErrorCode("4", "3", "DETAIN FOR PLANT INSPECTION");
			AddErrorCode("4", "4", "DETAIN FOR SANAB");
			AddErrorCode("4", "15", "DETAIN FOR SAPS");
			AddErrorCode("4", "16", "DETAIN FOR DTI FOR PERMIT AND CERTIFICATE OF ORIGIN");
			AddErrorCode("4", "17", "DETAIN FOR QUARANTINE MASTER");
			AddErrorCode("4", "18", "DETAIN FOR REGIONAL ASSIZER");
			AddErrorCode("4", "19", "DETAIN FOR RESERVE BANK");
			AddErrorCode("4", "20", "DETAIN FOR SABS");
			AddErrorCode("2", "21", "STOP /DETAIN FULL CONSIGNMENT");
			AddErrorCode("2", "22", "STOP /DETAIN CONTAINER NUMBER");
			AddErrorCode("2", "23", "STOP /DETAIN SPECIFIC PACKAGE");
			AddErrorCode("2", "24", "STOP/DETAIN NUMBER OF PACKAGES");
			AddErrorCode("13", "99", "BRING IN ALL SUPPORTING DOCUMENTATION");
			AddErrorCode("13", "100", "FREE TEXT FOR QUERIES");
		}

		protected void AddErrorCode(string statusCode, string errorCode, string description)
		{
			Add(new ErrorCode(statusCode, errorCode, description));
		}
	}

	public class ErrorCode : CodeDescriptionPair
	{
		public ErrorCode(string statusCode, string errorCode, string description)
			: base(errorCode, description)
		{
			this.StatusCode = statusCode;
		}
		public readonly string StatusCode;
	}
}
