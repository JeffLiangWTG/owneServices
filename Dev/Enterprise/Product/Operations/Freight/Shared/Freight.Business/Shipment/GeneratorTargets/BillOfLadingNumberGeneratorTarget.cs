using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BillOfLadingNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("dd1f5389-2d26-4bba-8c75-f48026c73878", "Freight -> House Bills -> Number Customizations -> House Bill Number"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			BillOfLadingNumberCustomisationsByServiceLevel svcLevel = Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation);
			return svcLevel.BillOfLadingNumberCustomisations["ALL"];
		}

		protected override int GetMaxLengthCore()
		{
			return JobShipmentSchema.JS_HouseBill.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("f1ab6f99-8c9c-4d73-b599-b54ae8ced32b", "bill of lading");
		}
	}
}
