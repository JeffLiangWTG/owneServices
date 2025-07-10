using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WarehouseOrderAndReceiveDataContextManager<TDocket, TDocketLine> : WarehouseDocketDataContextManager<TDocket>
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		#region GetShipmentDataObjectReader

		protected sealed override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var sourceShipment = UniversalShipment.GetSourceDataObject(universalShipment);
			return GetShipmentDataObjectReaderCore(sourceShipment, logger, factory);
		}

		protected abstract WhsOrderAndReceiveDataObjectReader<TDocket, TDocketLine> GetShipmentDataObjectReaderCore(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory);

		#endregion

		#region Event

		protected sealed override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var isChangeOfInventory = logger?.TopLevelDataContext?.IsWarehouseBondedChangeOfInventory() ?? false;
			return !isChangeOfInventory ? GetEventParentFinderCore(factory, logger) : null;
		}

		protected abstract WhsOrderAndReceiveEventParentFinder<TDocket> GetEventParentFinderCore(BusinessObjectFactory factory, IXmlImportLogger logger);

		// tested in WhsOrderAndReceiveDataObjectReader
		// tested in WhsBondedChangeOfInventoryDataObjectReader
		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			var helper = GetNewCustomsHelper(eventAdded, logger.TopLevelDataContext);
			if (!helper.IsWarehouseBondedChangeOfInventory && (helper.IsDataSourceCustoms && eventAdded.EventType.GetValueOrDefault() == Events.CancelTheWarehouseJobCode)) // customs "asking" for cancellation
			{
				// do this in a separate factory so we can throw away changes if the docket cannot be cancelled.
				var parentInOtherFactory = new BusinessObjectFactory().Load<TDocket>(ParentBO.PK);
				if (parentInOtherFactory.Warehouse.WW_IsVirtualWarehouse)
				{
					helper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(parentInOtherFactory);
				}
				else
				{
					CancelDocket(parentInOtherFactory);

					// This means the job was truly cancelled, rather than just having the customs cancelled event applied.
					parentInOtherFactory.Logs.AddNew(Events.Cancelled);
				}

				// attempt to save.
				ZExceptionReporting.ProcessWithSaveExceptionHandling(parentInOtherFactory.Factory.Save, null, reportErrorsOnly: true);
			}
		}

		protected abstract void CancelDocket(TDocket docket);
		protected abstract CustomsDataSourceHelper<TDocket> GetNewCustomsHelper(UniversalEvent eventAdded, IDataContextDataObject topLevelDataObject);

		#endregion
	}
}
