using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyCargoTypeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public AgencyCargoTypeCodeDescriptionPairList()
		{
			AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
			AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
			AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
			AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
			AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
		}

		public static IEnumerable<ZString> TopLevelPackCargoTypes
		{
			get
			{
				yield return Constants.ContainerModes.RollOnRollOff;
				yield return Constants.ContainerModes.BreakBulk;
				yield return Constants.ContainerModes.Bulk;
				yield return Constants.ContainerModes.Liquid;
			}
		}
	}
}
