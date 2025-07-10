using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	public class CashFlowActivityConfiguratonLookups : ZLookups
	{
		public CashFlowActivityConfiguratonLookups(CashFlowActivityConfiguration parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CodeList
		{
			get
			{
				if (codeList == null)
				{
					codeList = CashFlowCodeLists.CashFlowTypeList;
				}
				return codeList;
			}
		}
		CodeDescriptionPairList codeList;

		public CodeDescriptionPairList ActivityTypeList
		{
			get
			{
				if (activityTypeList == null)
				{
					activityTypeList = new CodeDescriptionPairList();
					activityTypeList.AddPair(ActivityTypeCodes.Undefined, ActivityTypeDescriptions.Undefined);
					activityTypeList.AddPair(ActivityTypeCodes.NonCash, ActivityTypeDescriptions.NonCash);
					activityTypeList.AddPair(ActivityTypeCodes.Cash, ActivityTypeDescriptions.Cash);
					activityTypeList.AddPair(ActivityTypeCodes.Exchange, ActivityTypeDescriptions.Exchange);
					activityTypeList.AddPair(ActivityTypeCodes.Operating, ActivityTypeDescriptions.Operating);
					activityTypeList.AddPair(ActivityTypeCodes.Investing, ActivityTypeDescriptions.Investing);
					activityTypeList.AddPair(ActivityTypeCodes.Financing, ActivityTypeDescriptions.Financing);
				}
				return activityTypeList;
			}
		}
		CodeDescriptionPairList activityTypeList;

		public static class ActivityTypeCodes
		{
			public const string Undefined = "X";
			public const string NonCash = "N";
			public const string Cash = "C";
			public const string Exchange = "E";
			public const string Operating = "O";
			public const string Investing = "I";
			public const string Financing = "F";
		}

		public static class ActivityTypeDescriptions
		{
			public static MultilingualString Undefined => ResString.GetMultilingualString("6E488969-6824-439E-AC4D-922D8F24684F", "Undefined Activities");
			public static MultilingualString NonCash => ResString.GetMultilingualString("846B66AC-A539-420B-96C8-F37CC1D1A2C3", "Non Cash");
			public static MultilingualString Cash => ResString.GetMultilingualString("F8CA02D2-85DA-4DAA-9108-EE7552E77058", "Cash or Cash Equivalent");
			public static MultilingualString Exchange => ResString.GetMultilingualString("5DD2433D-FEF4-466D-9229-907E1C889A29", "Effects of Exchange Rate Change");
			public static MultilingualString Operating => ResString.GetMultilingualString("E62E852A-48AD-4A6D-BAC2-7216A25B1960", "Operating Activities");
			public static MultilingualString Investing => ResString.GetMultilingualString("19B0D5F6-A480-44F4-9E46-5BDB518BD62D", "Investing Activities");
			public static MultilingualString Financing => ResString.GetMultilingualString("AEC59D1E-BF43-4E12-91B5-8EB5D5732331", "Financing Activities");
		}
	}
}
