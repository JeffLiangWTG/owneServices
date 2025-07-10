using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgTimetableType
	{
		public static class Codes
		{
			public const string Pickup = "PIC";
			public const string Deliver = "DLV";
		}

		public static class Descriptions
		{
			public static MultilingualString Pickup
			{
				get { return ResString.GetMultilingualString("32883cfe-034b-48f9-8e77-6ef5a8781f06", "Pickup"); }
			}

			public static MultilingualString Deliver
			{
				get { return ResString.GetMultilingualString("9240dfe4-5bab-4fe9-8bfb-d8fe351e02a6", "Deliver"); }
			}
		}
	}
}
