using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendAESTIRActionMethodApplicator))]
	sealed class SendAESTIRActionMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestValidationMode()
		{
			AssertEquals(((SendAESTIRActionMethodApplicator)Applicator).ValidationMode, ValidationModes.None);
		}
	}
}
