using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("16fcd8cb-ffc4-451e-b9fc-d76a305b6a17", "Freight -> House Bills -> Number Customizations -> Shipment Number"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return JobShipmentSchema.JS_UniqueConsignRef.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("7fac54ae-5b3f-491b-ac37-2a914a9279ce", "shipment number");
		}
	}
}
