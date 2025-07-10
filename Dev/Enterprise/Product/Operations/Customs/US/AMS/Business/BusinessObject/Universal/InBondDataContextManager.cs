using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public sealed class InBondDataContextManager : InBondDataContextManager<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.USAMS; }
		}

		protected override ZString CusInBondApplicationCode
		{
			get { return Common.CusInBondApplicationCodeList.Codes.AMS; }
		}

		protected override List<DataContextType> ParentDataContextTypes
		{
			get { return new List<DataContextType> { }; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CusInBondHeaderDataObjectWriter(writeManager);
		}

		protected override CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> GetShipmentDataObjectReaderCore(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
		{
			if (TryGetHVLVShipmentDataObject(universalShipment, out var shipmentDataObject))
			{
				var parentShipment = GetParentShipment(shipmentDataObject, factory.BOFactory);
				if (parentShipment != null)
				{
					return new HVLVCusInBondHeaderDataObjectReader(universalShipment, shipmentDataObject, parentShipment, logger, factory);
				}
			}

			return new CusInBondHeaderDataObjectReader(universalShipment, logger, factory, parentBO);
		}

		protected override ZBool IsSupportedTransportMode(ZString transportCode)
		{
			switch (transportCode)
			{
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
					return true;
				default:
					return false;
			}
		}

		bool TryGetHVLVShipmentDataObject(Shipment universalShipment, out Shipment shipmentDataObject)
		{
			var result = false;
			shipmentDataObject = null;

			if (universalShipment.SubShipmentCollection != null && universalShipment.MessageType.Code.Equals(DirectionTypeList.Codes.NVOCC))
			{
				shipmentDataObject = universalShipment.SubShipmentCollection.First(subShipment => HasDataSourceFromShipment(subShipment) && subShipment.ShipmentType.Code.Value == Core.Constants.ShipmentTypes.HighVolumeLowValue);
				result = shipmentDataObject != null;
			}

			return result;
		}

		bool HasDataSourceFromShipment(Shipment universalShipment)
		{
			var result = false;
			if (universalShipment.DataContext != null
				&& universalShipment.DataContext.DataSourceCollection != null)
			{
				result = universalShipment.DataContext.DataSourceCollection.Any(dataSource => dataSource.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment));
			}

			return result;
		}

		ForwardingShipment GetParentShipment(Shipment shipmentDataObject, BusinessObjectFactory factory)
		{
			var result = default(ForwardingShipment);
			if (shipmentDataObject.DataContext != null
				&& shipmentDataObject.DataContext.DataSourceCollection != null
				&& shipmentDataObject.DataContext.DataSourceCollection.Any())
			{
				var shipmentJobNumber = shipmentDataObject.DataContext.DataSourceCollection.First(dataSource => dataSource.Type.Equals(nameof(DataContextType.ForwardingShipment))).Key;
				result = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentJobNumber));
			}

			return result;
		}
	}
}
