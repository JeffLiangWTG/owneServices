using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentRouteLeg : AutoCarrierShipmentRouteLeg
	{
		public CarrierShipmentRouteLeg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CarrierShipmentHeader CarrierShipmentHeader => Factory.Load<CarrierShipmentHeader>(CRG_CSH_CarrierShipment);

		[RelatedBusinessObject(nameof(CarrierShipmentHeader))]
		public override ZGuid CRG_CSH_CarrierShipment
		{
			get => base.CRG_CSH_CarrierShipment;
			set => base.CRG_CSH_CarrierShipment = value;
		}
	}
}
