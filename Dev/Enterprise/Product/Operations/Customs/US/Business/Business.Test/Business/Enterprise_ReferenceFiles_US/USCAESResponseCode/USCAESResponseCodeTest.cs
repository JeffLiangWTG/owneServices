using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCAESResponseCode))]
	sealed class USCAESResponseCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSeverity()
		{
			USCAESResponseCode code = Factory.New<USCAESResponseCode>();
			AssertEquals(USCAESResponseCode.SeverityType.NotApplicable, code.Severity);
			code.UY_Severity = "fatal";
			AssertEquals(USCAESResponseCode.SeverityType.Fatal, code.Severity);
			code.UY_Severity = "Warning";
			AssertEquals(USCAESResponseCode.SeverityType.Warning, code.Severity);
			code.UY_Severity = "VerIFY";
			AssertEquals(USCAESResponseCode.SeverityType.Verify, code.Severity);
			code.UY_Severity = " Compliance ";
			AssertEquals(USCAESResponseCode.SeverityType.Compliance, code.Severity);
			code.UY_Severity = "Informational ";
			AssertEquals(USCAESResponseCode.SeverityType.Informational, code.Severity);
			code.UY_Severity = "Notification";
			AssertEquals(USCAESResponseCode.SeverityType.Notification, code.Severity);
		}
	}
}
