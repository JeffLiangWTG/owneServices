using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsTransitDispatchConsignmentFilterBusinessObject : WhsTransitConsignmentFilterBusinessObject<WhsItemDispatchConsignment>
	{
		#region WhsTransitConsignmentFilterBusinessObject Members

		public override SchemaGuidColumn PKSchemaColumn => WhsItemDispatchConsignmentSchema.PK;

		protected override SchemaGuidColumn PackageStateFKSchemaColumn => WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse;

		protected override SchemaStringColumn ConsignmentDirectionSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_Direction;

		protected override bool IsRCN => false;

		protected override void AddFiltersCore(ModuleFilterCollection result)
		{
			AddNumberFilters(result);
			AddTextFilters(result);
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCheckpoint() => Env.Security.WhsItemDispatchConsignmentJobInvoicing;

		#endregion

		#region Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFountainFilter(Schema.JobID, WhsItemDispatchConsignmentSchema.WDC_JobID, "DC").MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitDispatchConsignmentFilterBusinessObject|DCNID", "DCN ID");
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.ReferenceNumber, WhsItemDispatchConsignmentSchema.WDC_ConsignmentID).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitDispatchConsignmentFilterBusinessObject|DispatchConsignmentReference", "Reference Number");
		}

		protected override ZQuery GetCTOFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return base.GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.DepartureCTOAddress);
		}

		#endregion
	}
}
