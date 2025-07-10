using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AuditClassificationLinesMethod))]
	sealed class AuditClassificationLinesMethodTest : OperationalActionMethodTest<AuditClassificationLinesMethod>
	{
		protected override AuditClassificationLinesMethod NewMethod()
		{
			return new AuditClassificationLinesMethod();
		}

		public void TestTestNameAndDescription()
		{
			AssertEquals("Audit Classification Lines", Method.Name);
			AssertEquals("Audit Classification Lines", Method.Description);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(AuditClassificationLinesMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}
	}
}
