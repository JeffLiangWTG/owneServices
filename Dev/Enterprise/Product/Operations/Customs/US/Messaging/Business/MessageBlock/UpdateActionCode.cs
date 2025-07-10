namespace Enterprise.Customs.US.Messaging.Business
{
	public enum UpdateActionCode { Add, Delete, Replace, Update, MerchandiseZoneStatusChange }

	public static class UpdateActionCodeConverter
	{
		public const string MerchandiseZoneStatusChangeCode = "S";
		public const string DeleteCode = "D";
		public const string ReplaceCode = "R";
		public const string UpdateCode = "U";
		public const string AddCode = "A";
		public const string AESDeleteCode = "X";

		public static string ConvertToString(this UpdateActionCode code)
		{
			string result = "";
			switch (code)
			{
				case UpdateActionCode.Delete:
					result = DeleteCode;
					break;
				case UpdateActionCode.Replace:
					result = ReplaceCode;
					break;
				case UpdateActionCode.Update:
					result = UpdateCode;
					break;
				case UpdateActionCode.MerchandiseZoneStatusChange:
					result = MerchandiseZoneStatusChangeCode;
					break;
				default:
					result = AddCode;
					break;
			}
			return result;
		}

		public static string ConvertToAESString(this UpdateActionCode code)
		{
			string result = "";
			switch (code)
			{
				case UpdateActionCode.Delete:
					result = AESDeleteCode;
					break;
				case UpdateActionCode.Replace:
					result = ReplaceCode;
					break;
				case UpdateActionCode.Update:
					result = UpdateCode;
					break;
				default:
					result = AddCode;
					break;
			}
			return result;
		}
	}
}
