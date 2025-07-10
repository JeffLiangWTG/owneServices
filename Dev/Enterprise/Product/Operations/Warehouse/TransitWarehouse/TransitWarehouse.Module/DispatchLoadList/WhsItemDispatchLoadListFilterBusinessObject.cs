using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemDispatchLoadListFilterBusinessObject : WhsTransitFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => WhsItemDispatchLoadListSchema.PK;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddNumberFilters(result);
			AddOrganisationFilters(result);
			return result;
		}

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemDispatchLoadListSchema.WDL_WW_Warehouse;

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.JobID, WhsItemDispatchLoadListSchema.WDL_JobID).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemDispatchLoadListFilterBusinessObject|DispatchLoadListJobID", "Load List ID");
			filters.AddNumberFilter(Schema.ReferenceNumber, WhsItemDispatchLoadListSchema.WDL_ReferenceNumber).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemDispatchLoadListFilterBusinessObject|DispatchLoadListReference", "Reference Number");
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var maxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			var creditorName = filters.AddTextFilter(Schema.Creditor, GetCreditorNameQuery);
			creditorName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemDispatchLoadListFilterBusinessObject|CreditorName", "Creditor Name");
			creditorName.Category = FilterCategories.Organisations;
			creditorName.MaxLength = maxLength;
		}

		ZQuery GetCreditorNameQuery(SQLComparisonOperator comparisonOperator, ZString creditorName)
		{
			return GetAddressQuery<WhsItemDispatchLoadList>(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, creditorName, DocAddressType.Creditor);
		}
	}
}
