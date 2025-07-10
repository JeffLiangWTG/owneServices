namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public enum ApplicationType { AES, AIS, AISUCC5, NCTS }

	public static class ApplicationTypeExtensions
	{
		public static string ToDataGrouping(this ApplicationType applicationType) =>
			applicationType == ApplicationType.AISUCC5 ? Constants.DataGroupings.IEUCC5 : Constants.IECountryCode;

		public static string ToPageHeaderText(this ApplicationType applicationType) =>
			applicationType == ApplicationType.AISUCC5 ? ApplicationType.AIS.ToString() : applicationType.ToString();
	}
}
