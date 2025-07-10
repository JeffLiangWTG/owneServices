using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EIDOSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestErrorBehaviour()
		{
			ReleaseImportOrderSettings settings = new ReleaseImportOrderSettings();

			settings.ErrorBehaviour = "XXX";
			AssertHasError(settings.ErrorBehaviourInfo, "Enter a valid On Error / Message Error.");

			settings.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Skip;
			AssertNoErrors(settings.ErrorBehaviourInfo);

			settings.ErrorBehaviour = "";
			AssertHasError(settings.ErrorBehaviourInfo, "Please enter an On Error / Message Error.");
		}
	}
}
