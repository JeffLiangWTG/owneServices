using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class TransportNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected TransportNumberGeneratorTarget(DtbTransport parent)
			: base()
		{
			this.parent = Argument.NotNull(parent, "parent");
		}

		readonly DtbTransport parent;

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var transportModeCustomisation = Context.AccessRegistryByServiceLevel(GetRegistryItemCore());

			return transportModeCustomisation.BillOfLadingNumberCustomisations[parent.KM_RS_NKServiceLevel]
				?? transportModeCustomisation.BillOfLadingNumberCustomisations["ALL"];
		}

		protected override int GetMaxLengthCore()
		{
			return DtbBookingSchema.KM_JobID.MaxLength;
		}

		public abstract BillCustomisationByServiceLevelRegistryItem GetRegistryItemCore();

		protected override ZString GetNameCore()
		{
			return Res.GetString("57a6d0aa-d3d3-4325-9935-af18606a1ac0", "Transport number");
		}
	}
}
