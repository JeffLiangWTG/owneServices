using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class SendReportSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestErrorAction()
		{
			var settings = new DocDataObjectSendingMessageSettings();

			settings.ErrorAction = "XXX";
			AssertHasError(settings.ErrorActionInfo, "Enter a valid selection.");

			settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Abort;
			AssertNoErrors(settings.ErrorActionInfo);

			settings.ErrorAction = ZString.Empty;
			AssertHasError(settings.ErrorActionInfo, "Please enter a value.");
		}
	}
}
