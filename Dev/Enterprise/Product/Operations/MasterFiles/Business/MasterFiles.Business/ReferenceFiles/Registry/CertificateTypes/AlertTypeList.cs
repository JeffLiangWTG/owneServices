using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes
{
	public class AlertTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string ErrorAlert = "ERR";
			public const string WarningAlert = "WRN";
			public const string NoAlert = "NON";
		}

		public static class Descriptions
		{
			public static string ErrorAlert => Res.GetString("3ec7fa25-1818-47ca-be2b-4b50c1921635", "Error Alert");
			public static string WarningAlert => Res.GetString("6bc7fc85-92f3-43d3-b74e-ddf67a0d8f3e", "Warning Alert");
			public static string NoAlert => Res.GetString("fccfb120-6b97-4200-a415-52f8f594ba73", "No Alert");
		}

		public AlertTypeList()
		{
			AddPair(Codes.ErrorAlert, Descriptions.ErrorAlert);
			AddPair(Codes.WarningAlert, Descriptions.WarningAlert);
			AddPair(Codes.NoAlert, Descriptions.NoAlert);
		}
	}
}
