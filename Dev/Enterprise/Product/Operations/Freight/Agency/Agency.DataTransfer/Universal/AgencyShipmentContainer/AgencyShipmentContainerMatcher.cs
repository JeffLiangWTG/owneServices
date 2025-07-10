namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Business;

	class AgencyShipmentContainerMatcher
	{
		public AgencyShipmentContainerMatcher(AgencyShipment shipment, AgencyShipmentContainerReferences refrerences)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
			this.references = Argument.NotNull(refrerences, "refrerences");
		}

		readonly AgencyShipment shipment;
		readonly AgencyShipmentContainerReferences references;

		public AgencyShipmentContainer GetBestMatch()
		{
			var containers = shipment.ShippingContainers.Cast<AgencyShipmentContainer>();
			return GetBestMatch(containers);
		}

		public AgencyShipmentContainer GetBestMatch(IEnumerable<AgencyShipmentContainer> containers)
		{
			var applicableContainers = containers.Where(c => !c.IsTopLevelPack);

			return GetBestMatchByContainerNumberOnly(applicableContainers) ??
				GetBestMatchByReleaseNumberOnly(applicableContainers) ??
				GetBestMatchByContainerAndReleaseNumbers(applicableContainers);
		}

		AgencyShipmentContainer GetBestMatchByContainerNumberOnly(IEnumerable<AgencyShipmentContainer> containers)
		{
			AgencyShipmentContainer result = null;
			if (references.ContainerNumber != ZString.Empty)
			{
				var containersByNumber = containers
					.Where(c => c.JC_ContainerNum == references.ContainerNumber)
					.ToArray();

				if (containersByNumber.Length == 1)
				{
					result = containersByNumber[0];
				}
			}

			return result;
		}

		AgencyShipmentContainer GetBestMatchByReleaseNumberOnly(IEnumerable<AgencyShipmentContainer> containers)
		{
			AgencyShipmentContainer result = null;
			if (references.ContainerReleaseNumber != ZString.Empty)
			{
				var containersByReleaseNumber = containers
					.Where(c => c.JC_ReleaseNum == references.ContainerReleaseNumber)
					.ToArray();

				if (containersByReleaseNumber.Length == 1)
				{
					result = containersByReleaseNumber[0];
				}
			}

			return result;
		}

		AgencyShipmentContainer GetBestMatchByContainerAndReleaseNumbers(IEnumerable<AgencyShipmentContainer> containers)
		{
			AgencyShipmentContainer result = null;
			var containersByNumberAndReleaseNumberAndISO = containers
				.Where(c => c.JC_ContainerNum == references.ContainerNumber
							&& c.JC_ReleaseNum == references.ContainerReleaseNumber
							&& c.RefContainer != null && c.RefContainer.ISOType.ISOCode == references.ContainerISOCode)
				.ToArray();

			if (containersByNumberAndReleaseNumberAndISO.Length == 1)
			{
				result = containersByNumberAndReleaseNumberAndISO[0];
			}

			return result;
		}
	}
}
