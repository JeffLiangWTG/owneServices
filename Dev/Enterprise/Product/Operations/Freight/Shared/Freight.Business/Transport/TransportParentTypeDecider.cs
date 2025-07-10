using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.Business
{
	public class TransportParentTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row[JobConsolTransportSchema.JW_ParentType.Name])
			{
				case Constants.TransportParentTypes.TransportBooking:
					return ObjectFactory.GetType<IDtbBookingConsolidation>();

				case Constants.TransportParentTypes.TransitDispatchLoadList:
					return ObjectFactory.GetType<IWhsItemDispatchLoadList>();

				case Constants.TransportParentTypes.TransitReceiveASN:
					return ObjectFactory.GetType<IWhsItemReceiveASN>();

				case Constants.TransportParentTypes.TransitReceiveConsignment:
					return ObjectFactory.GetType<IWhsItemReceiveConsignment>();

				case Constants.TransportParentTypes.ShipmentPreAdvice:
					return ObjectFactory.GetType<IJobShipmentPreplanning>();

				case Constants.TransportParentTypes.Declaration:
					return ObjectFactory.GetType<IBaseJobDeclaration>();

				case Constants.TransportParentTypes.ImporterSecurityFiling:
					return ObjectFactory.GetType<US.ISF.ICusISFHeader>();

				case Constants.TransportParentTypes.AsycudaManifest:
					return ObjectFactory.GetType<ManifestBase.IAsycudaManifestHeader>();

				case Constants.TransportParentTypes.CommercialInvoice:
					return ObjectFactory.GetType<IBaseJobComInvoiceHeader>();

				case Constants.TransportParentTypes.AgencyShipment:
				case Constants.TransportParentTypes.Shipment:
					var shipmentRowFactory = new RowFactory(factory);
					var shipmentRow = shipmentRowFactory.LoadFromPK(JobShipmentSchema.Constants.TableName, (Guid)row[JobConsolTransportSchema.JW_ParentGUID.Name]);
					return new ShipmentTypeDecider().GetTypeForLoad(shipmentRow, factory);

				case Constants.TransportParentTypes.Consol:
					var consolRowFactory = new RowFactory(factory);
					var consolRow = consolRowFactory.LoadFromPK(JobConsolSchema.Constants.TableName, (Guid)row[JobConsolTransportSchema.JW_ParentGUID.Name]);
					return new ConsolTypeDecider().GetTypeForLoad(consolRow, factory);
			}
			return GetTypeForNew();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>();
		}

		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>();
		}
	}
}
