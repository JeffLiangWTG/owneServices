using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class YesNoDefaultList
	{
		public static CodeDescriptionPairList GetCachedYesOnlyList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("US_YesOnlyList", // 'US_YesNoList' is not a database field
			delegate
			{
				var result = new YesNoDefaultList();
				result.RemoveCode(YesNoDefaultList.Codes.No);
				result.RemoveCode(YesNoDefaultList.Codes.Default);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedYesNoList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("US_YesNoList", // 'US_YesNoList' is not a database field
			delegate
			{
				CodeDescriptionPairList result = new YesNoDefaultList();
				result.RemoveCode(YesNoDefaultList.Codes.Default);
				return result;
			});
		}
	}
}
