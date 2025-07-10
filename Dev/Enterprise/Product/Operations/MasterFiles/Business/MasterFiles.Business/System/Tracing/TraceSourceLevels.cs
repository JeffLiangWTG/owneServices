using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class TraceSourceLevels : CodeDescriptionPairList
	{
		public TraceSourceLevels()
			: base()
		{
			AddPair(Codes.ActivityTracing, Descriptions.ActivityTracing);
			AddPair(Codes.All, Descriptions.All);
			AddPair(Codes.Critical, Descriptions.Critical);
			AddPair(Codes.Error, Descriptions.Error);
			AddPair(Codes.Information, Descriptions.Information);
			AddPair(Codes.Off, Descriptions.Off);
			AddPair(Codes.Verbose, Descriptions.Verbose);
			AddPair(Codes.Warning, Descriptions.Warning);
		}

		public static class Codes
		{
			public const string ActivityTracing = "ACT";
			public const string All = "ALL";
			public const string Critical = "CRT";
			public const string Error = "ERR";
			public const string Information = "INF";
			public const string Off = "OFF";
			public const string Verbose = "VRB";
			public const string Warning = "WRN";
		}

		public static class Descriptions
		{
			public static MultilingualString ActivityTracing { get { return ResString.GetMultilingualString("22599250-6df0-4876-886b-0764c738fa59", "Allows the Stop, Start, Suspend, Transfer, Resume events through."); } }
			public static MultilingualString All { get { return ResString.GetMultilingualString("914ab8ad-ac89-4717-ad5a-d28c03607cd7", "Allows all events through."); } }
			public static MultilingualString Critical { get { return ResString.GetMultilingualString("415bd548-3794-4a79-a472-fa75e97f9887", "Allows only Critical events through."); } }
			public static MultilingualString Error { get { return ResString.GetMultilingualString("aaac8d18-2935-4dda-a3ea-394b9447785f", "Allows Critical and Error events through."); } }
			public static MultilingualString Information { get { return ResString.GetMultilingualString("09c5e169-47a7-4464-baeb-9df18f742ef2", "Allows Critical, Error, Warning, and Information events through."); } }
			public static MultilingualString Off { get { return ResString.GetMultilingualString("5d9e0694-badc-41bc-a4c9-a1c225a5ba77", "Does not allow any events through."); } }
			public static MultilingualString Verbose { get { return ResString.GetMultilingualString("c2c5cb83-0aa9-4c80-a60f-64596a8df4ff", "Allows Critical, Error, Warning, Information and Verbose events through."); } }
			public static MultilingualString Warning { get { return ResString.GetMultilingualString("a7f2682d-0996-4066-9161-4573395a5199", "Allows Critical, Error and Warning events through."); } }
		}
	}
}
