using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GeneralCriticalValidationTest : TestCaseWithFactory
	{
		public void TestGeneralDeveloperMessageAndUserMessage()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessEntityForCriticalCheckException>();
			StackTrace trace = new StackTrace();
			((IHaveConstructorStackTrace)bizO).ConstructorStackTrace = trace;

			var criticalValidation = new DummyCriticalValidation(bizO);

			Assert("Testing for interactive user", Globals.IsUserInteractive);
			try
			{
				criticalValidation.RunOnSavingCheck();
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertEquals("ErrorType", nameof(CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2), ex.ErrorType);
				AssertContains("User Message", @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: User Message", ex.Message);
				AssertContains("Tech Details", @"E=MC2
Mass To Energy", ex.DeveloperErrorMessage);
				AssertContains("StackTrace", string.Format(@"The Enterprise.MasterFiles.Business.Testing.DummyBusinessEntityForCriticalCheckException was constructed at the following trace:
{0}", trace.ToString()), ex.DeveloperErrorMessage);
				AssertNotNull("BusinessObject", ex.BusinessEntity);
				AssertEquals("BusinessObject", bizO, ex.BusinessEntity);
			}

			Globals.IsUserInteractive = false; //batch processor
			Assert("Testing for non-interactive user", !Globals.IsUserInteractive);
			try
			{
				criticalValidation.RunOnSavingCheck();
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertEquals("ErrorType", nameof(CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2), ex.ErrorType);
				AssertContains("User Message", @"An error has occurred. Service task failed to process completely.
Please wait for the next cycle of service task.
Error Message: User Message", ex.Message);
				AssertContains("Tech Details", @"E=MC2
Mass To Energy", ex.DeveloperErrorMessage);
				AssertContains("StackTrace", string.Format(@"The Enterprise.MasterFiles.Business.Testing.DummyBusinessEntityForCriticalCheckException was constructed at the following trace:
{0}", trace.ToString()), ex.DeveloperErrorMessage);
				AssertNotNull("BusinessObject", ex.BusinessEntity);
				AssertEquals("BusinessObject", bizO, ex.BusinessEntity);
			}
		}

		internal class DummyCriticalValidation : CriticalValidation<DummyBusinessEntityForCriticalCheckException>
		{
			public DummyCriticalValidation(DummyBusinessEntityForCriticalCheckException parent) : base(parent) { }

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2, ResString.GetMultilingualString("a1454259-0e16-4e69-b2d0-fec11dc7c1f2", "User Message"), "E=MC2", string.Empty, "Mass To Energy");
			}
		}
	}
}
