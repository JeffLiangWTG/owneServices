using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationCartageIntegrationPostCMR : BaseJobDeclarationCartageIntegrationTest
	{
		protected override ZString GetCMROrLegacy()
		{
			return "CMR";
		}

		protected override ZString SetContainerMode(ZString currentValue)
		{
			ZString result = currentValue;
			if (currentValue == Constants.ContainerModes.Containerised)
			{
				result = BaseCusContainer.ContainerModes.FullContainerLoad;
			}
			else if (currentValue == Constants.ContainerModes.BuyersConsol)
			{
				result = BaseCusContainer.ContainerModes.FCX;
			}
			return result;
		}
	}
}
