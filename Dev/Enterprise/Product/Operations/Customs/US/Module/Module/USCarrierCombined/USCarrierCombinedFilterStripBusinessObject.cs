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
	sealed class USCarrierCombinedFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", USCarrierCombinedSchema.UI_Code).MultilingualDescription = ResString.GetMultilingualString("3586591f-3ee2-40c8-a792-57ee30d46778", "Code");
			result.AddTextFilter("Name", USCarrierCombinedSchema.UI_Name).MultilingualDescription = ResString.GetMultilingualString("c583e48f-d940-419b-afdc-f15ce0b8fc8f", "Name");
			result.AddTextFilter("Mode Of Transportation", USCarrierCombinedSchema.UI_ModeOfTransportation, TransportModeCodes).MultilingualDescription = ResString.GetMultilingualString("a9a9857a-086f-4beb-a984-46f0ca80b950", "Mode Of Transportation");
			result.AddTextFilter("Address", USCarrierCombinedSchema.UI_Address).MultilingualDescription = ResString.GetMultilingualString("2cdd0439-8b5a-44d4-8eec-91a6a6adac5b", "Address");
			result.AddTextFilter("Airway Bill Prefix", USCarrierCombinedSchema.UI_AirwayBillPrefix).MultilingualDescription = ResString.GetMultilingualString("5ddb4217-a661-4af5-aba5-b0e96046c803", "Airway Bill Prefix");

			return result;
		}

		TransportModeCodes TransportModeCodes => Factory.GetCachedValue<TransportModeCodes>();
	}
}
