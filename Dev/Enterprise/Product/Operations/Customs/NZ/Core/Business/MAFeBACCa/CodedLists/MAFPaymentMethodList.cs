using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class MAFPaymentMethodList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Account = "ACC";
			public const string Cash = "CSH";
			public const string Other = "OTH";
		}

		public static class Descriptions
		{
			public static MultilingualString Account { get { return ResString.GetMultilingualString("MAFPaymentMethodList|Account", "MPI Account"); } }
			public static MultilingualString Cash { get { return ResString.GetMultilingualString("MAFPaymentMethodList|Cash", "Cash"); } }
			public static MultilingualString Other { get { return ResString.GetMultilingualString("MAFPaymentMethodList|Other", "Other"); } }
		}

		public MAFPaymentMethodList()
		{
			AddPair(Codes.Account, Descriptions.Account);
			AddPair(Codes.Cash, Descriptions.Cash);
			AddPair(Codes.Other, Descriptions.Other);
		}
	}
}
