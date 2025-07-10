using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public sealed class AviationSecuritySchemeMembershipEx : AviationSecuritySchemeMembership
	{
		public AviationSecuritySchemeMembershipEx()
		{
			AddPair(Codes.Yes, Descriptions.Yes);
			AddPair(Codes.No, Descriptions.No);
		}

		public static new class Codes
		{
			public const string Yes = "YES";
			public const string No = "NO";
		}

		public static new class Descriptions
		{
			public static MultilingualString Yes
			{
				get { return ResString.GetMultilingualString("AviationSecuritySchemeMembershipEx|Yes", "Participates in an Aviation Security Scheme"); }
			}

			public static MultilingualString No
			{
				get { return ResString.GetMultilingualString("AviationSecuritySchemeMembershipEx|No", "Does not participate in an Aviation Security Scheme"); }
			}
		}
	}
}