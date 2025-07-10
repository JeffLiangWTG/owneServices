using CargoWise.Common;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class WarehouseTypeProvider : IWarehouseTypeProvider
	{
		public WarehouseTypeProvider(Shipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
		}
		readonly Shipment shipment;

		#region IWarehouseTypeProvider Members

		WarehouseType IWarehouseTypeProvider.GetWarehouseType()
		{
			var result = WarehouseType.Default;
			if (shipment.MessageType.GetCodeAsUpperCase() == JobMessageTypeList.Codes.FTZ)
			{
				result = WarehouseType.FreeTradeZone;
			}
			return result;
		}

		#endregion
	}
}
