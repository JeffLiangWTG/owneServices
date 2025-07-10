using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
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
	public class USCCarrierAndFIRMSFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CodeDescriptionPairList US_YesNoList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("US_YesNoList", // 'US_YesNoList' is not a database field
				delegate
				{
					var result = new Business.YesNoDefaultList();
					result.RemoveCode(Business.YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public CodeDescriptionPairList TypeList
		{
			get
			{
				return Factory.GetCachedValue("USCCarrierAndFIRMSTypeList",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("CARRIER");
					result.AddPair("FIRMS");
					return result;
				});
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(null, false);

			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", USCCarrierAndFIRMSSchema.US_Code);
			result.AddTextFilter("Name", USCCarrierAndFIRMSSchema.US_Name);

			result.AddTextFilter("Facility Type", USCCarrierAndFIRMSSchema.US_FacilityType);
			result.AddTextFilter("District Port", USCCarrierAndFIRMSSchema.US_DistrictPortCode);
			result.AddTextFilter("Address", USCCarrierAndFIRMSSchema.US_Address);
			result.AddTextFilter("IsActive", GetIsActiveQuery, US_YesNoList);
			result.AddTextFilter("Transportation Mode", USCCarrierAndFIRMSSchema.US_TransportationMode, TransportModeCodes);

			var filter = result.AddTextFilter("Type", USCCarrierAndFIRMSSchema.US_Type, TypeList);
			var list = filter.ComparisonOperator_List;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			list.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			list.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			return result;
		}

		ZQuery GetIsActiveQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter(USCCarrierAndFIRMSSchema.US_IsActive, value == Business.YesNoDefaultList.Codes.Yes);
			}
			return result;
		}

		TransportModeCodes TransportModeCodes => Factory.GetCachedValue<TransportModeCodes>();
	}
}
