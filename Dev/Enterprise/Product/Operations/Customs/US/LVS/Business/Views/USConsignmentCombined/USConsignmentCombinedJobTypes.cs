
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class USConsignmentCombinedJobTypes : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Consignment = "ULB";
			public const string Declaration = "JE";
		}

		public static class Descriptions
		{
			public static MultilingualString Consignment { get { return ResString.GetMultilingualString("b7b33c8f-a1ca-4d35-808a-310b6f769ed7", "Consignment"); } }
			public static MultilingualString Declaration { get { return ResString.GetMultilingualString("10d716f8-3bea-47fc-ac2e-32a0fc542eb7", "Declaration"); } }
		}

		public USConsignmentCombinedJobTypes()
		{
			AddPair(Codes.Consignment, Descriptions.Consignment);
			AddPair(Codes.Declaration, Descriptions.Declaration);
		}
	}
}
