using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOriginalEntryHeaderValidationTest : TestCaseWithFactory
	{
		public void TestValidateMPC()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_ChangedLinesOnly = true;
			entry.Validation.ValidateMPC();
			AssertNoMessageErrorContaining(entry.MPCInfo, ReconOriginalEntryHeaderValidation.EnterNumberGreaterThanZero);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 10m);
			entry.Validation.ValidateMPC();
			AssertHasMessageErrorContaining(entry.MPCInfo, ReconOriginalEntryHeaderValidation.EnterNumberGreaterThanZero);
			entry.MPC = 10m;
			AssertNoMessageErrorContaining(entry.MPCInfo, ReconOriginalEntryHeaderValidation.EnterNumberGreaterThanZero);
		}
	}
}
