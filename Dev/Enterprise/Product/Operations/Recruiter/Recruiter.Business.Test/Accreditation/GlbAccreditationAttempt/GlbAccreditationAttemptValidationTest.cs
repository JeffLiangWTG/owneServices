using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationAttemptValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHAA_CompletionDueDate()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt.HAA_CommencementDate = ZDate.Today.AddDays(-5);
			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(5);
			attempt.HAA_CompletionDate = ZDate.Empty;
			attempt.Accreditation.HAC_Code = "AC1";
			attempt.Accreditation.HAC_Description = "Description(123)";
			Factory.Save();

			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(-10);
			AssertHasError(attempt.HAA_CompletionDueDateInfo, "Completion Due Date Cannot be earlier than Commencement Date.");

			attempt.HAA_CompletionDueDate = ZDate.Today.AddDays(3);
			AssertNoErrors(attempt.HAA_CompletionDueDateInfo);
		}
	}
}
