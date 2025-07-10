using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OnSavingCriticalCheckExceptionHandlingTest : ExceptionHandledWithPopupTest
	{
		protected override ExceptionHandledWithPopup GetExceptionInstance()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessEntityForCriticalCheckException bizO = factory.New<DummyBusinessEntityForCriticalCheckException>();
			return new OnSavingCriticalCheckException<DummyBusinessEntityForCriticalCheckException>(bizO, CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2, "Error Has Occurred On Saving", "E=MC2");
		}
	}

	class OnSavingCriticalCheckExceptionTest : TransactionedTestCase
	{
		public void TestConstruction()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessEntityForCriticalCheckException bizO = factory.New<DummyBusinessEntityForCriticalCheckException>();
			AssertNull(((IHaveConstructorStackTrace)bizO).ConstructorStackTrace);
			OnSavingCriticalCheckException<DummyBusinessEntityForCriticalCheckException> ex = new OnSavingCriticalCheckException<DummyBusinessEntityForCriticalCheckException>(bizO, CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2, "Error Has Occurred On Saving", "E=MC2");

			AssertEquals("Message(User Message)", "Error Has Occurred On Saving", ex.Message);
			AssertEquals("Message(User Message)", "E=MC2", ex.DeveloperErrorMessage);
			AssertNotNull("BusinessObject", ex.BusinessEntity);
			AssertEquals("BusinessObject", bizO, ex.BusinessEntity);
			AssertEquals("ErrorType", nameof(CriticalValidationErrorType.AutoGLJournalWithoutDueDate_2), ex.ErrorType);
		}
	}
}
