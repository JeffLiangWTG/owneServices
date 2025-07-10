namespace Enterprise.Customs.US.Business
{
	partial class AESSeverityIndicatorList
	{
		public static USCAESResponseCode.SeverityType GetSeverityType(string code)
		{
			switch (code)
			{
				case Codes.Compliance:
					return USCAESResponseCode.SeverityType.Compliance;
				case Codes.Fatally:
					return USCAESResponseCode.SeverityType.Fatal;
				case Codes.Informational:
					return USCAESResponseCode.SeverityType.Informational;
				case Codes.Verification:
					return USCAESResponseCode.SeverityType.Verify;
				case Codes.Warning:
					return USCAESResponseCode.SeverityType.Warning;
				default:
					return USCAESResponseCode.SeverityType.Notification;
			}
		}
	}
}
