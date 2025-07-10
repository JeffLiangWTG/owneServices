using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehouseBondedChangeOfInventoryDataContextManager : ShipmentDataContextManager<WhsBondedChangeOfInventory>
	{
		#region Context

		public override ZString DataContextKey => ((IJobNumber)ParentBO).JobNumber;

		public override DataContextType DataContextType => DataContextType.WarehouseBondedChangeOfInventory;

		public override string DefaultOutputDirectory => null;

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var isChangeOfInventory = logger?.TopLevelDataContext?.IsWarehouseBondedChangeOfInventory() ?? false;
			return isChangeOfInventory ? new WarehouseBondedChangeOfInventoryEventParentFinder(factory, this, logger) : null;
		}

		// tested in WhsBondedChangeOfInventoryDataObjectReader
		class WarehouseBondedChangeOfInventoryEventParentFinder : EventParentFinder
		{
			internal WarehouseBondedChangeOfInventoryEventParentFinder(BusinessObjectFactory factory, WarehouseBondedChangeOfInventoryDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				var results = new List<BusinessObject>();

				var order = (WhsOrder)new WhsOrderEventParentFinder(factory, (WarehouseBondedChangeOfInventoryDataContextManager)manager, logger).GetLogParentsForEvent(xmlEvent)?.SingleOrDefault();
				var receive = (WhsReceive)new WhsReceiveEventParentFinder(factory, (WarehouseBondedChangeOfInventoryDataContextManager)manager, logger).GetLogParentsForEvent(xmlEvent)?.SingleOrDefault();

				// WhsChangeOfInventory does not support persistent Logs - Add Order/Receive so events are added and persisted - Order/Reveice DataContextManagers ignore Change of Inventory events (We don't want to add the event twice)
				results.AddSafe(order);
				results.AddSafe(receive);

				// Order/Receive OnUniversalEventAdded each saves their own factory for cancelling, but we need to roll back all changes if receive fails, so they ignore Events added for Change of Inventory.
				// And WhsBondedChangeOfInventory.OnUniversalEventAdded will do the cancelling of the Order and Receive in a single factory. 
				results.AddSafe(order != null ? new WhsBondedChangeOfInventory(factory) { Order = order, Receive = receive } : null);

				return results.ToArray();
			}
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			var orderHelper = new CustomsDataSourceHelperForOrder(eventAdded, logger.TopLevelDataContext);
			if (orderHelper.IsWarehouseBondedChangeOfInventory && eventAdded.EventType.GetValueOrDefault() == ZArchitecture.Business.Events.CancelTheWarehouseJobCode)
			{
				// do this in a separate factory so we can throw away changes if the docket cannot be cancelled.
				var newFactory = new BusinessObjectFactory();

				var orderInOtherFactory = newFactory.Load<WhsOrder>(ParentBO.Order.PK);
				using (orderInOtherFactory.MarkAsImportingForChangeOfInventory())
				{
					orderHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(orderInOtherFactory);
				}

				if (ParentBO.Receive != null)
				{
					var receiveHelper = new CustomsDataSourceHelperForReceive(eventAdded, logger.TopLevelDataContext);
					var receiveInOtherFactory = newFactory.Load<WhsReceive>(ParentBO.Receive.PK);
					using (receiveInOtherFactory.MarkAsImportingForChangeOfInventory())
					{
						receiveHelper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(receiveInOtherFactory);
					}
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(newFactory.Save, null, reportErrorsOnly: true);
			}
		}

		#endregion

		#region Matching

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments => true;

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsBondedChangeOfInventoryDataObjectReader(shipment, logger, factory);
		}

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(r => r.Code == RecipientRoleType.BCO || r.Code == RecipientRoleType.BCR);
		}

		#endregion
	}
}
