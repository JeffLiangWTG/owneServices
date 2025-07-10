using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehouseOrderDataContextManager : WarehouseOrderAndReceiveDataContextManager<WhsOrder, WhsOrderLine>
	{
		#region Context

		public override DataContextType DataContextType => DataContextType.WarehouseOrder;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
			=> recipientRoles.Any(o => o.Code == RecipientRoleType.WAR || o.Code == RecipientRoleType.BWR);

		protected override string DocketTypeCode => DocketType.Codes.Order;

		#endregion

		#region Event

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var whsOrderEventContextReader = new WhsOrderEventExportContextBuilder(ParentBO);
				whsOrderEventContextReader.AddWhsDocketContextValues(result);
			}

			return result;
		}

		protected override WhsOrderAndReceiveEventParentFinder<WhsOrder> GetEventParentFinderCore(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsOrderEventParentFinder(factory, this, logger);
		}

		protected override void CancelDocket(WhsOrder order)
		{
			order.Pick?.CancelPick();
			order.CancelReactivateDocket();
		}

		protected override CustomsDataSourceHelper<WhsOrder> GetNewCustomsHelper(UniversalEvent eventAdded, IDataContextDataObject topLevelDataObject)
		{
			return new CustomsDataSourceHelperForOrder(eventAdded, topLevelDataObject);
		}

		#endregion

		#region Shipment

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsOrderDataObjectWriter(writeManager);
		}

		protected override WhsOrderAndReceiveDataObjectReader<WhsOrder, WhsOrderLine> GetShipmentDataObjectReaderCore(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsOrderDataObjectReader(universalShipment, logger, factory);
		}

		#endregion
	}
}
