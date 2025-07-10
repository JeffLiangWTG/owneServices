using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	class USInvoiceLineFSISLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLotsInformationIsMissing()
		{
			USInvoiceLineFSISLine line = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCLeCERT, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				var msg1 = "All certificates issued in countries where eCert is not available require at least one line. eCert is only available in Australia and New Zealand";
				line.Validation.ValidateAll();
				AssertHasRowMessageError(line, msg1);
				line.Lots.AddNew();
				line.Validation.ValidateAll();
				AssertNoRowMessageError(line, msg1);
			}
			line.Lots.RemoveAndDeleteAll();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCLeCERT, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var msg2 = "All certificates issued in countries where eCert is not available require at least one line. eCert is only available in Australia, New Zealand and Chile.";
				line.Validation.ValidateAll();
				AssertHasRowMessageError(line, msg2);
				line.Lots.AddNew();
				line.Validation.ValidateAll();
				AssertNoRowMessageError(line, msg2);
			}
		}
	}
}
