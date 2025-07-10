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
	public class WhsTransitReceiveConsignmentMatchingHelper : WhsTransitConsignmentMatchingHelper<WhsItemReceiveConsignment>
	{
		public WhsTransitReceiveConsignmentMatchingHelper(UniversalObjectFactory factory, IOrgHeader bookingParty, IColumnIndexer warehouse, IXmlImportLogger logger)
			: base(factory, bookingParty, warehouse, logger)
		{
		}

		protected override SchemaStringColumn JobIDColumn => WhsItemReceiveConsignmentSchema.WRC_JobID;
		protected override SchemaStringColumn ConsignmentIDColumn => WhsItemReceiveConsignmentSchema.WRC_ConsignmentID;
		protected override SchemaStringColumn HouseBillNumberColumn => WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber;
		protected override SchemaDateTimeColumn CreatedTimeColumn => WhsItemReceiveConsignmentSchema.WRC_SystemCreateTimeUtc;
		protected override SchemaGuidColumn WarehouseColumn => WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse;
		protected override SchemaGuidColumn PKSchemaColumn => WhsItemReceiveConsignmentSchema.PK;
		protected override SchemaDateTimeOffsetColumn CompleteTimeColumn => WhsItemReceiveConsignmentSchema.WRC_CompleteTime;
		protected override ZString JobDescription => ResString.GetMultilingualString("57da9760-4e5d-424c-897f-df34f17f5e9f", "Receive Consignment");
	}
}
