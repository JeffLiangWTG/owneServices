using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingBillOfLadingNumberGeneratorTarget : BillOfLadingNumberGeneratorTarget
	{
		public ForwardingBillOfLadingNumberGeneratorTarget(IBillGenerationSupport parent)
			: base()
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}
		readonly IBillGenerationSupport parent;

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			BillOfLadingNumberCustomisationsByServiceLevel transportModeCustomisation = GetNumberCustomisationCoreByTransportMode();
			BillOfLadingNumberCustomisation serviceLevelCustomisation = null;

			if (parent.ServiceLevel != "")
			{
				serviceLevelCustomisation = transportModeCustomisation.BillOfLadingNumberCustomisations[parent.ServiceLevel];
			}

			if (serviceLevelCustomisation == null)
			{
				serviceLevelCustomisation = transportModeCustomisation.BillOfLadingNumberCustomisations["ALL"];
			}

			return serviceLevelCustomisation;
		}

		BillOfLadingNumberCustomisationsByServiceLevel GetNumberCustomisationCoreByTransportMode()
		{
			switch (parent.TransportMode)
			{
				case Constants.TransportModes.Air:
					return Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation_AIR);
				case Constants.TransportModes.Sea:
					return Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation_SEA);
				case Constants.TransportModes.Rail:
					return Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation_RAIL);
				case Constants.TransportModes.Road:
					return Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation_ROAD);
				default:
					return Context.AccessRegistryByServiceLevel(FreightDataRegistry.Instance.HouseBillNumberCustomisation);
			}
		}
	}
}
