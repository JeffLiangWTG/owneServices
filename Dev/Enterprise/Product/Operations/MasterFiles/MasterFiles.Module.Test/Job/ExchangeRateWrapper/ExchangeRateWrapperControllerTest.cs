using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExchangeRateWrapperController))]
	public class ExchangeRateWrapperControllerTest : ZSingletonControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new BulkExchangeRateUpdater(Factory);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ExchangeRateWrapper;
		}
	}
}
