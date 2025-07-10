using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public static class IApportionmentMethodOverrideHelper
	{
		public static int GetSpecificityScore(this IApportionmentMethodOverride apportionmentMethod)
		{
			var score = 0;
			if (apportionmentMethod.Module != ApportionmentMethod.AllCode && apportionmentMethod.Module != ZString.Empty)
			{
				score += 16;
			}

			if (apportionmentMethod.ConsolType != ApportionmentMethod.AllCode && apportionmentMethod.ConsolType != ZString.Empty)
			{
				score += 8;
			}

			if (apportionmentMethod.TransportMode != ApportionmentMethod.AllCode && apportionmentMethod.TransportMode != ZString.Empty)
			{
				score += 4;
			}

			if (apportionmentMethod.Direction != ApportionmentMethod.AllCode && apportionmentMethod.Direction != ZString.Empty)
			{
				score += 2;
			}

			if (apportionmentMethod.ContainerMode != ApportionmentMethod.AllCode && apportionmentMethod.ContainerMode != ZString.Empty)
			{
				score += 1;
			}

			return score;
		}
	}
}
