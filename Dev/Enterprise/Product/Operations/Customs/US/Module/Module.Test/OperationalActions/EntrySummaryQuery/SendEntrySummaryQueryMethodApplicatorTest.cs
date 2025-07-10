using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendEntrySummaryQueryMethodApplicator))]
	sealed class SendEntrySummaryQueryMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestValidationMode()
		{
			AssertEquals(((SendEntrySummaryQueryMethodApplicator)Applicator).ValidationMode, ValidationModes.None);
		}
	}
}
