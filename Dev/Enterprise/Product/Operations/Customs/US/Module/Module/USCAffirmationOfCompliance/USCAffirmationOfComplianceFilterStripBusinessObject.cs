using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class USCAffirmationOfComplianceFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", USCAffirmationOfComplianceSchema.UL_Code);
			result.AddTextFilter("Description", USCAffirmationOfComplianceSchema.UL_Description);
			result.AddTextFilter("Is Qualifier", GetHasQualifierIndicatorQuery, US_YesNoList);
			result.AddTextFilter("Is Expired", GetIsExpiredIndicatorQuery, US_YesNoList);
			return result;
		}

		public CodeDescriptionPairList US_YesNoList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("US_YesNoList", // 'US_YesNoList' is not a database field
				delegate
				{
					var result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		ZQuery GetHasQualifierIndicatorQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter(USCAffirmationOfComplianceSchema.UL_QualifierIndicator, value == YesNoDefaultList.Codes.Yes);
			}
			return result;
		}

		ZQuery GetIsExpiredIndicatorQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter(USCAffirmationOfComplianceSchema.UL_IsExpired, value == YesNoDefaultList.Codes.Yes);
			}
			return result;
		}
	}
}
