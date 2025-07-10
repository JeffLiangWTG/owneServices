using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESSeverityIndicatorListTest : TestCase
	{
		public void TestGetSeverityType()
		{
			AssertEquals(USCAESResponseCode.SeverityType.Compliance, AESSeverityIndicatorList.GetSeverityType(AESSeverityIndicatorList.Codes.Compliance));
			AssertEquals(USCAESResponseCode.SeverityType.Fatal, AESSeverityIndicatorList.GetSeverityType(AESSeverityIndicatorList.Codes.Fatally));
			AssertEquals(USCAESResponseCode.SeverityType.Informational, AESSeverityIndicatorList.GetSeverityType(AESSeverityIndicatorList.Codes.Informational));
			AssertEquals(USCAESResponseCode.SeverityType.Verify, AESSeverityIndicatorList.GetSeverityType(AESSeverityIndicatorList.Codes.Verification));
			AssertEquals(USCAESResponseCode.SeverityType.Warning, AESSeverityIndicatorList.GetSeverityType(AESSeverityIndicatorList.Codes.Warning));
			AssertEquals(USCAESResponseCode.SeverityType.Notification, AESSeverityIndicatorList.GetSeverityType(" "));
			AssertEquals(USCAESResponseCode.SeverityType.Notification, AESSeverityIndicatorList.GetSeverityType("!"));
		}
	}
}
