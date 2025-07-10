using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class AWBPrintSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOnErrorMessageError()
		{
			AWBPrintSettings settings = new AWBPrintSettings();

			settings.OnErrorMessageError = "XXX";
			AssertHasError(settings.OnErrorMessageErrorInfo, "Enter a valid selection.");

			settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
			AssertNoErrors(settings.OnErrorMessageErrorInfo);

			settings.OnErrorMessageError = ZString.Empty;
			AssertHasError(settings.OnErrorMessageErrorInfo, "Please enter a value.");
		}
	}
}
