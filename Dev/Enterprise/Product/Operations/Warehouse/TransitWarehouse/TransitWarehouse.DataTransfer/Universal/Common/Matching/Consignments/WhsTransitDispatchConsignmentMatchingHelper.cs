using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsignmentMatchingHelper : WhsTransitConsignmentMatchingHelper<WhsItemDispatchConsignment>
	{
		public WhsTransitDispatchConsignmentMatchingHelper(UniversalObjectFactory factory, IOrgHeader bookingParty, IColumnIndexer warehouse, IXmlImportLogger logger)
			: base(factory, bookingParty, warehouse, logger)
		{
		}

		protected override SchemaStringColumn JobIDColumn => WhsItemDispatchConsignmentSchema.WDC_JobID;
		protected override SchemaStringColumn ConsignmentIDColumn => WhsItemDispatchConsignmentSchema.WDC_ConsignmentID;
		protected override SchemaStringColumn HouseBillNumberColumn => WhsItemDispatchConsignmentSchema.WDC_HouseBillNumber;
		protected override SchemaDateTimeColumn CreatedTimeColumn => WhsItemDispatchConsignmentSchema.WDC_SystemCreateTimeUtc;
		protected override SchemaGuidColumn WarehouseColumn => WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse;
		protected override SchemaGuidColumn PKSchemaColumn => WhsItemDispatchConsignmentSchema.PK;
		protected override SchemaDateTimeOffsetColumn CompleteTimeColumn => WhsItemDispatchConsignmentSchema.WDC_CompleteTime;
		protected override ZString JobDescription => ResString.GetMultilingualString("38c1532a-3aa1-4e5d-9c49-7aec169991df", "Dispatch Consignment");
	}
}
