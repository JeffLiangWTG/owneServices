using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitDataObjectReaderHandlerManager;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public abstract class WhsTransitDataObjectReader<T> : ShipmentDataObjectReader<T>
		where T : BusinessObject
	{
		#region Constructor

		protected WhsTransitDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#endregion

		public TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		public TransitStoredProcedureHandler StoredProcedureHandler => (TransitStoredProcedureHandler)HandlerManager.GetHandler<TransitStoredProcedureHandler>();

		#region GetHouseBill

		protected ZString GetHouseBill()
		{
			var sourceDataObject = GetSourceDataObject();
			return sourceDataObject.WayBillNumber.GetValueOrDefault();
		}

		#endregion

		#region GetConsignmentID

		protected ZString GetConsignmentID()
		{
			var sourceDataObject = GetSourceDataObject();
			var shipmentDataSource = sourceDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			var consignmentDataSource = sourceDataObject.GetMatchingDataSource(DataContextType.LandTransportConsignment);
			return (shipmentDataSource?.Key.GetValueOrDefault() ?? consignmentDataSource?.Key.GetValueOrDefault()) ?? ZString.Empty;
		}

		#endregion

		#region GetSourceDataObject

		protected UniversalShipment GetSourceDataObject()
		{
			return SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE ? dataObject : UniversalShipment.GetSourceDataObject(dataObject);
		}

		#endregion

		#region ExecuteStoreProcedure

		protected void ExecuteStoreProcedure()
		{
			if (StoredProcedureHandler != null)
			{
				StoredProcedureHandler.UpdateParent(this);
				StoredProcedureHandler.Execute(factory, dataObject, logger);
			}
		}

		#endregion

		#region RegisterHandler

		protected virtual void RegisterHandler()
		{
			if (StoredProcedureHandler == null)
			{
				var handler = HandlerManager.BuildHandler(HandlerType.StoredProcedure, dataObject);
				HandlerManager.RegisterHandler(handler);
			}

			StoredProcedureHandler.Initialize(dataObject, logger, this);
		}

		#endregion

		#region GetEarliestShipmentETD

		static protected ZDateTime? GetEarliestShipmentETD(UniversalShipment consolDO, UniversalShipment sourceDO)
		{
			var result = new ZDateTime?();
			if (consolDO != null && consolDO.SubShipmentCollection != null)
			{
				var dateCollection = consolDO.SubShipmentCollection.Where(s => getDepartureDateByDataObject(s) != null).Select(s => getDepartureDateByDataObject(s));
				if (dateCollection.Any())
				{
					result = dateCollection.OrderBy(date => date.Value).FirstOrDefault().Value;
				}
			}
			else
			{
				result = GetShipmentETDBySouceDO(sourceDO);
			}

			return result;
		}

		static protected ZDateTime? GetShipmentETDBySouceDO(UniversalShipment sourceDO)
		{
			var result = new ZDateTime?();

			var date = getDepartureDateByDataObject(sourceDO);
			if (date != null)
			{
				result = date.Value;
			}
			return result;
		}

		static Date getDepartureDateByDataObject(UniversalShipment shipment)
		{
			return getDateByDataObject(shipment, DateType.Departure);
		}

		static Date getDateByDataObject(UniversalShipment shipment, DateType dateType)
		{
			if (shipment.DateCollection != null && shipment.DateCollection.Count > 0)
			{
				return shipment.DateCollection.FirstOrDefault(d => d.Type == dateType);
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}
