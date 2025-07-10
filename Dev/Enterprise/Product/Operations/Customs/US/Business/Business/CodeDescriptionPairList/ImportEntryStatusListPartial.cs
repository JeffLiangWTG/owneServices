
namespace Enterprise.Customs.US.Business
{
	partial class ImportEntryStatusList
	{
		public static string GetEntryStatusFromErrorCode(string errorCode)
		{
			if (errorCode == "2A5" || errorCode == "2A4")//CERT-RELEASE CERTIFIED VIA SUMMARY, CARGO RELEASE DATA CERTIFIED
			{
				return Codes.CRL;
			}
			else if (errorCode == "2A3")//ENTRY CANNOT BE CERTIFIED
			{
				return Codes.CRF;
			}
			else if (errorCode == "IN4")
			{
				return Codes.CRN;//ENTRY DOES NOT EXIST IN THE SELECTIVITY FILE
			}
			else if (errorCode == "57A")
			{
				return Codes._05;//Paperless
			}
			return "";
		}

		public static bool IsCRLCertified(string errorCode)
		{
			if (errorCode == "2A5" || errorCode == "2A4")
			{
				return true;
			}
			return false;
		}
	}
}
