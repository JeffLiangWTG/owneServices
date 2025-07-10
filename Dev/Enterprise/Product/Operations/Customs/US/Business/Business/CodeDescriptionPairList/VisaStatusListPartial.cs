namespace Enterprise.Customs.US.Business
{
	partial class VisaStatusList
	{
		public static bool IsOnFile(string code)
		{
			return code == Codes.OnFileCancelled ||
				code == Codes.OnFileUsed ||
				code == Codes.OnFileNotUsed;
		}

		public static bool IsStatusDateLastStateUpdateChangeDate(string code)
		{
			return code == Codes.OnFileCancelled ||
				code == Codes.OnFileUsed;
		}
	}
}
