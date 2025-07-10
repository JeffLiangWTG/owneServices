using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerModeList : CodeDescriptionPairList
	{
		public AgencyShipmentContainerModeList(string cargoType)
		{
			foreach (var containerMode in GetContainerModes(cargoType).Where(c => !string.IsNullOrEmpty(c)))
			{
				AddPair(containerMode, Constants.ContainerModeDescriptions.GetDescription(containerMode));
			}
		}

		public static IEnumerable<string> GetContainerModes(string cargoType)
		{
			if (string.IsNullOrWhiteSpace(cargoType) || cargoType == Constants.ContainerModes.FCL)
			{
				yield return Constants.ContainerModes.BuyersConsol;
				yield return Constants.ContainerModes.FCL;
				yield return Constants.ContainerModes.Groupage;
				yield return Constants.ContainerModes.LCL;
				yield return string.Empty;
			}

			if (string.IsNullOrWhiteSpace(cargoType) || cargoType == Constants.ContainerModes.RollOnRollOff)
			{
				yield return Constants.ContainerModes.RollOnRollOff;
			}

			if (string.IsNullOrWhiteSpace(cargoType) || cargoType == Constants.ContainerModes.BreakBulk)
			{
				yield return Constants.ContainerModes.BreakBulk;
			}

			if (string.IsNullOrWhiteSpace(cargoType) || cargoType == Constants.ContainerModes.Bulk)
			{
				yield return Constants.ContainerModes.Bulk;
			}

			if (string.IsNullOrWhiteSpace(cargoType) || cargoType == Constants.ContainerModes.Liquid)
			{
				yield return Constants.ContainerModes.Liquid;
			}
		}
	}
}
