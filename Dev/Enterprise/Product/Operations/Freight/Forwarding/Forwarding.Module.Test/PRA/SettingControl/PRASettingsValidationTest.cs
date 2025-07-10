using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Module;

namespace Enterprise.Freight.Module.Testing
{
	public class PRASettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestErrorBehaviour()
		{
			var factory = new BusinessObjectFactory();
			PRASettings settings = new PRASettings(factory);

			settings.ErrorBehaviour = "XXX";
			AssertHasError(settings.ErrorBehaviourInfo, "Enter a valid selection.");

			settings.ErrorBehaviour = PRASettings.Codes.Abort;
			AssertNoErrors(settings.ErrorBehaviourInfo);

			settings.ErrorBehaviour = ZString.Empty;
			AssertHasError(settings.ErrorBehaviourInfo, "Please enter a value.");
		}
	}
}
