using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.Business
{
	public class CarrierShipmentGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => OceanCarrierDataRegistry.Instance.OceanCarrierShipmentReferenceNumberFormat.Location();

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(OceanCarrierDataRegistry.Instance.OceanCarrierShipmentReferenceNumberFormat);
		}

		protected override int GetMaxLengthCore()
		{
			return CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("87ca5b85-4c49-4b40-8145-aef2efeb9103", "carrier shipment reference number");
		}
	}
}
