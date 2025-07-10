
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PopulateOwnerRefList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Yes = "YES";
			public const string No = "NO";
			public const string Default = "DEF";
		}

		public static class Description
		{
			public static string Yes
			{
				get { return Res.GetString("1e1375a1-a52b-4c96-bc28-391d99e4d4fa", "Populates the Owner's Ref field with Order Numbers"); }
			}
			public static string No
			{
				get { return Res.GetString("e4119ce8-7ea3-4d5f-83c4-3fd36bc10866", "Does not populate Owner's Ref with Order Numbers"); }
			}
			public static string Default
			{
				get { return Res.GetString("aac5aa43-55de-4388-8684-bf4dffa9aa8b", "Functions based on the system registry set-up"); }
			}
		}

		public PopulateOwnerRefList()
		{
			AddPair(Codes.Yes, Description.Yes);
			AddPair(Codes.No, Description.No);
			AddPair(Codes.Default, Description.Default);
		}
	}
}
