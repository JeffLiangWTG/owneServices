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
	sealed class USCFIRMSFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CodeDescriptionPairList US_YesNoList
		{
			get
			{
				return Factory.GetCachedValue("US_YesNoList", // 'US_YesNoList' is not a database field
				delegate
				{
					CodeDescriptionPairList result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public FacilityTypeList FacilityTypeList => new FacilityTypeList();

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(USCFIRMS.Constants.Code, USCFIRMSSchema.US_Code).MultilingualDescription = ResString.GetMultilingualString("9d3e9575-61df-4610-9ba1-5185a8be28a1", USCFIRMS.Constants.Code);
			result.AddTextFilter(USCFIRMS.Constants.Name, USCFIRMSSchema.US_Name).MultilingualDescription = ResString.GetMultilingualString("77438e1b-5afd-4713-9d4c-78067e0df19b", USCFIRMS.Constants.Name);
			result.AddTextFilter(USCFIRMS.Constants.DistrictPortCode, USCFIRMSSchema.US_DistrictPortCode).MultilingualDescription = ResString.GetMultilingualString("c4da685c-37f3-4eec-aa82-c927c7edafd7", USCFIRMS.Constants.DistrictPortCode);
			result.AddTextFilter(USCFIRMS.Constants.Address, USCFIRMSSchema.US_Address).MultilingualDescription = ResString.GetMultilingualString("e8b9cb7d-2052-43a0-9b95-87dc76803ebf", USCFIRMS.Constants.Address);
			result.AddTextFilter(USCFIRMS.Constants.City, USCFIRMSSchema.US_City).MultilingualDescription = ResString.GetMultilingualString("8c149e31-8ecd-4e1b-b9ff-d4455d2b6f63", USCFIRMS.Constants.City);
			result.AddTextFilter(USCFIRMS.Constants.State, USCFIRMSSchema.US_State).MultilingualDescription = ResString.GetMultilingualString("74d9d57c-0827-4282-a977-41d44b9f2378", USCFIRMS.Constants.State);
			result.AddTextFilter(USCFIRMS.Constants.ZipCode, USCFIRMSSchema.US_ZipCode).MultilingualDescription = ResString.GetMultilingualString("10d0a7ab-36ca-4070-92fc-050b878d7afe", USCFIRMS.Constants.ZipCode);
			result.AddTextFilter(USCFIRMS.Constants.Country, USCFIRMSSchema.US_Country).MultilingualDescription = ResString.GetMultilingualString("3078c4db-eab2-491c-8b25-05571ede0520", USCFIRMS.Constants.Country);
			result.AddTextFilter(USCFIRMS.Constants.FacilityType, USCFIRMSSchema.US_FacilityType, FacilityTypeList).MultilingualDescription = ResString.GetMultilingualString("1fee8015-5d7a-4e8c-ba91-9fb211f61d8e", USCFIRMS.Constants.FacilityType);
			result.AddTextFilter(USCFIRMS.Constants.IsActive, GetIsActiveQuery, US_YesNoList).MultilingualDescription = ResString.GetMultilingualString("7acfef82-7648-4cdf-89b1-c988cc8c3245", USCFIRMS.Constants.IsActive);
			result.AddDateFilter(USCFIRMS.Constants.LastUpdate, USCFIRMSSchema.US_LastUpdate).MultilingualDescription = ResString.GetMultilingualString("547351bd-e9ee-4d2b-8024-2579d1db98c1", USCFIRMS.Constants.LastUpdate);

			return result;
		}

		ZQuery GetIsActiveQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter(USCFIRMSSchema.US_IsActive, value == YesNoDefaultList.Codes.Yes);
			}
			return result;
		}
	}
}
