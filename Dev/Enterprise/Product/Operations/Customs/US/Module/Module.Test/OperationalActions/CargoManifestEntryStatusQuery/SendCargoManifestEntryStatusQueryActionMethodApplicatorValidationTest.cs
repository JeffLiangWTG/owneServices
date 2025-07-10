using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	sealed class SendCargoManifestEntryStatusQueryActionMethodApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAction()
		{
			var applicator = new SendCargoManifestEntryStatusQueryActionMethodApplicator(new BusinessObjectFactory());
			applicator.Action = CargoManifestStatusQueryActionList.Codes.MAWB;
			applicator.Validation.ValidateAll();
			AssertNoErrorContaining(applicator.ActionInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(applicator.ActionInfo, ListValidation.InvalidCodeError);
			applicator.Action = ZString.Empty;
			applicator.Validation.ValidateAll();
			AssertHasErrorContaining(applicator.ActionInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(applicator.ActionInfo, ListValidation.InvalidCodeError);

			applicator.Action = "XXX";
			AssertHasErrorContaining(applicator.ActionInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateOutputOption()
		{
			var applicator = new SendCargoManifestEntryStatusQueryActionMethodApplicator(new BusinessObjectFactory());
			applicator.Action = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			applicator.UpdateEntryWithResults = true;
			applicator.OutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
			applicator.Validation.ValidateAll();
			AssertHasError(applicator.OutputOptionInfo, SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation.OutputOptionErrorMessage);
			AssertNoErrorContaining(applicator.OutputOptionInfo, ListValidation.InvalidCodeError);

			applicator.UpdateEntryWithResults = false;
			applicator.Validation.ValidateAll();
			AssertNoErrors(applicator.OutputOptionInfo);

			applicator.OutputOption = LimitOutputCodeList.Codes._0MostRecentResults;
			applicator.UpdateEntryWithResults = true;
			applicator.Validation.ValidateAll();
			AssertNoErrors(applicator.OutputOptionInfo);

			applicator.OutputOption = "~";
			AssertHasErrorContaining(applicator.OutputOptionInfo, ListValidation.InvalidCodeError);
		}
	}
}
