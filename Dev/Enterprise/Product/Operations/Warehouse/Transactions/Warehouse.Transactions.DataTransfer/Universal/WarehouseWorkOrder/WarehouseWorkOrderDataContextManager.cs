using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WarehouseWorkOrderDataContextManager : WarehouseDocketDataContextManager<WhsWorkOrder>
	{
		public override DataContextType DataContextType => DataContextType.WarehouseWorkOrder;

		protected override string DocketTypeCode => DocketType.Codes.WorkOrder;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
			=> recipientRoles.Any(o => o.Code == RecipientRoleType.WWO);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddOrderNumber(ParentBO);
				result.AddOrderNumberSplit(ParentBO);
				result.AddDocketReferences(ParentBO);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
			=> new WarehouseWorkOrderEventParentFinder(factory, this, logger);

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			=> new WhsWorkOrderDataObjectReader(Shipment.GetSourceDataObject(universalShipment), logger, factory);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
			=> new WhsWorkOrderDataObjectWriter(writeManager);
	}
}
