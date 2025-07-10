using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	internal sealed class StorageNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("fb8fda01-5fe9-43d7-8723-9a9d076376d5", "Freight -> Shipment -> Storage Number Customization"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(FreightDataRegistry.Instance.StorageNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return StorageMainSchema.SM_PhysicalLocation.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("57a6d0aa-b543-4325-9935-af18606a1ac0", "storage number");
		}
	}
}
