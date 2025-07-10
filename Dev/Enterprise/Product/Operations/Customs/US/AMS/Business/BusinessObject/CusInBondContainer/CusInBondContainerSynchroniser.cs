using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainerSynchroniser : Customs.Business.CusInBondContainerSynchroniser
	{
		public CusInBondContainerSynchroniser(CusInBondContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source, shipmentSource)
		{
		}

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		#region Implementation

		IZType GetTypeOfService()
		{
			var result = ZString.Empty;
			if (Source.JC_DeliveryMode == Core.Constants.DeliveryModes.Codes.CY_CY)
			{
				result = ServiceTypeList.Codes.ContainerYard;
			}
			else if (Source.JC_DeliveryMode == Core.Constants.DeliveryModes.Codes.CFS_CFS)
			{
				result = ServiceTypeList.Codes.ContainerStation;
			}
			else if (shipmentSource.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || shipmentSource.JS_PackingMode == Core.Constants.ContainerModes.Bulk)
			{
				result = ServiceTypeList.Codes.BreakBulk;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingTypeOfService()
		{
			yield return Source.JC_DeliveryModeInfo;
			yield return shipmentSource.JS_PackingModeInfo;
		}

		#endregion

		#region Override

		protected override void GetCusInBondCargoDescCollectionSynchroniser()
		{
			Synchronisers.Add(new CusInBondCargoDescCollectionSynchroniser(shipmentSource, Destination));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_RCInfo, Source.JC_RCInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_IsEmptyInfo, Source.JC_IsEmptyContainerInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_TypeOfServiceInfo, GetTypeOfService, GetInfosAffectingTypeOfService));
		}

		protected override Customs.Business.UNDGDataItemCollectionSynchroniser GetUNDGDataItemCollectionSynchroniser(ForwardingShipment shipmentSource, Customs.Business.CusInBondContainer destination)
		{
			return new UNDGDataItemCollectionSynchroniser(shipmentSource, destination);
		}

		#endregion
	}
}
