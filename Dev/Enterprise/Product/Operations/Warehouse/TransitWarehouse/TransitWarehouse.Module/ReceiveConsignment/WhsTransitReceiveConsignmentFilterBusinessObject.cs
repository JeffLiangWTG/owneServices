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
	public class WhsTransitReceiveConsignmentFilterBusinessObject : WhsTransitConsignmentFilterBusinessObject<WhsItemReceiveConsignment>
	{
		#region WhsTransitConsignmentFilterBusinessObject Members

		public override SchemaGuidColumn PKSchemaColumn => WhsItemReceiveConsignmentSchema.PK;

		protected override SchemaGuidColumn PackageStateFKSchemaColumn => WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse;

		protected override SchemaStringColumn ConsignmentDirectionSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_Direction;

		protected override bool IsRCN => true;

		protected override void AddFiltersCore(ModuleFilterCollection result)
		{
			AddNumberFilters(result);
			AddTextFilters(result);
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCheckpoint() => Env.Security.WhsItemReceiveConsignmentJobInvoicing;

		#endregion

		#region Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFountainFilter(Schema.JobID, WhsItemReceiveConsignmentSchema.WRC_JobID, "RC").MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitReceiveConsignmentFilterBusinessObject|RCNID", "RCN ID");
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.ReferenceNumber, WhsItemReceiveConsignmentSchema.WRC_ConsignmentID).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitReceiveConsignmentFilterBusinessObject|ReceiveConsignmentReference", "Reference Number");
		}

		protected override ZQuery GetCTOFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return base.GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ArrivalCTOAddress);
		}

		#endregion
	}
}
