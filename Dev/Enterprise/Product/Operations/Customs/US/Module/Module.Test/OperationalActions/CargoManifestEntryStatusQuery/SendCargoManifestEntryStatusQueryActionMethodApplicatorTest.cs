using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendCargoManifestEntryStatusQueryActionMethodApplicator))]
	sealed class SendCargoManifestEntryStatusQueryActionMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestValidationMode()
		{
			AssertEquals(Applicator.ValidationMode, ValidationModes.None);
		}

		public void TestSetValue()
		{
			var applicator = Applicator;
			applicator.Action = CargoManifestStatusQueryActionList.Codes.MAWB;
			AssertEquals(applicator.RequestForReleatedBOLInfo.ReadOnly, false);
			applicator.RequestForReleatedBOL = true;
			AssertEquals(applicator.UpdateEntryWithResultsInfo.ReadOnly, false);
			applicator.UpdateEntryWithResults = true;
			applicator.OutputOption = LimitOutputCodeList.Codes._1Last5Results;

			applicator.Action = CargoManifestStatusQueryActionList.Codes.HAWB;
			applicator.OutputOption = LimitOutputCodeList.Codes._1Last5Results;
			AssertEquals(applicator.RequestForReleatedBOLInfo.ReadOnly, true);
			AssertEquals(applicator.RequestForReleatedBOL, false);
			AssertEquals(applicator.UpdateEntryWithResultsInfo.ReadOnly, false);
			applicator.Validation.ValidateAll();
			AssertHasError(applicator.OutputOptionInfo, SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation.OutputOptionErrorMessage);

			applicator.Action = CargoManifestStatusQueryActionList.Codes.InBond;
			AssertEquals(applicator.RequestForReleatedBOLInfo.ReadOnly, true);
			AssertEquals(applicator.RequestForReleatedBOL, false);
			AssertEquals(applicator.UpdateEntryWithResultsInfo.ReadOnly, true);
			AssertEquals(applicator.UpdateEntryWithResults, false);
			AssertNoError(applicator.OutputOptionInfo, SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation.OutputOptionErrorMessage);
		}

		public void TestDefaultLimitOutputOptionIfRequired()
		{
			var applicator = Applicator;
			AssertEquals(ZString.Empty, applicator.OutputOption);

			applicator.Action = CargoManifestStatusQueryActionList.Codes.Entry;
			AssertEquals(LimitOutputCodeList.Codes._0MostRecentResults, applicator.OutputOption);

			applicator.Action = CargoManifestStatusQueryActionList.Codes.MAWB;
			AssertEquals(LimitOutputCodeList.Codes._2AllAvailableResults, applicator.OutputOption);

			applicator.UpdateEntryWithResults = true;
			AssertEquals(LimitOutputCodeList.Codes._0MostRecentResults, applicator.OutputOption);

			applicator.UpdateEntryWithResults = false;
			AssertEquals(LimitOutputCodeList.Codes._2AllAvailableResults, applicator.OutputOption);
		}

		new SendCargoManifestEntryStatusQueryActionMethodApplicator Applicator => (SendCargoManifestEntryStatusQueryActionMethodApplicator)base.Applicator;
	}
}
