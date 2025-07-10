using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	sealed class ConsignmentNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Transport -> Land & Port Transport -> Land Transport -> Transport Consignment Number Format"; } // Points to non-localised registry entry
		}

		public ConsignmentNumberGeneratorTarget(DtbConsignment parent)
			: base()
		{
			this.consignment = Argument.NotNull(parent, nameof(parent));
		}

		readonly DtbConsignment consignment;

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var landTransportModeCustomisation = Context.AccessRegistryByServiceLevel(GetRegistryItem());

			return landTransportModeCustomisation.BillOfLadingNumberCustomisations[consignment.LTC_RS_NKServiceLevel]
				?? landTransportModeCustomisation.BillOfLadingNumberCustomisations["ALL"];
		}

		protected override int GetMaxLengthCore()
		{
			return DtbConsignment.Schema.LTC_JobIDMaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("57a6d0aa-bsbs-4325-9935-af18606a1ac0", "consignment number");
		}

		public BillCustomisationByServiceLevelRegistryItem GetRegistryItem()
		{
			return LandTransportRegistry.Instance.TransportConsignmentNumberFormat;
		}
	}
}
