using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CusExitReportValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCER_TransportID_RuleR0049E() => CombineAssertions(() =>
	{
		const string message = "[R0049E] A Transport ID is required for a transport type other than one starting '5' or '7'.";

		report.CER_TransportID = ZString.Empty;
		foreach(var (transportType, messageIsExpected) in ((string transportType, bool messageIsExpected)[])[
			("10", true), ("23", true), ("30", true), ("63", true), ("87", true),
			("50", false), ("53", false), ("70", false), ("79", false)])
		{
			AssertMessage(transportType, messageIsExpected);
		}

		report.CER_TransportID = "12";
		foreach(var transportType in (string[])["10", "23", "30", "63", "87"])
		{
			AssertMessage(transportType, messageIsExpected: false);
		}
		return;

		void AssertMessage(string transportType, bool messageIsExpected)
		{
			report.CER_TransportType = transportType;
			report.Validation.ValidateCER_TransportID();
			if (messageIsExpected)
			{
				AssertHasMessageError($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, message is expected", report.CER_TransportIDInfo, message);
			}
			else
			{
				AssertNoMessageError($"Transport ID: {report.CER_TransportID}, Transport type: {transportType}, message is not expected", report.CER_TransportIDInfo, message);
			}
		}
	});

	public void TestCheckCER_RN_NKTransportNationality_RuleR0050E() => CombineAssertions(() =>
	{
		const string message = "[R0050E] A Transport Nationality is required for a transport type other than one starting with '2', '5' or '7'.";

		report.CER_RN_NKTransportNationality = ZString.Empty;
		foreach(var (transportType, messageIsExpected) in ((string transportType, bool messageIsExpected)[])[
			("10", true), ("30", true), ("63", true), ("87", true),
			("23", false), ("50", false), ("53", false), ("70", false), ("79", false)])
		{
			AssertMessage(transportType, messageIsExpected);
		}

		report.CER_RN_NKTransportNationality = "BR";
		foreach(var transportType in (string[])["10", "30", "63", "87"])
		{
			AssertMessage(transportType, messageIsExpected: false);
		}
		return;

		void AssertMessage(string transportType, bool messageIsExpected)
		{
			report.CER_TransportType = transportType;
			report.Validation.ValidateCER_RN_NKTransportNationality();
			if (messageIsExpected)
			{
				AssertHasMessageError($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, message is expected", report.CER_RN_NKTransportNationalityInfo, message);
			}
			else
			{
				AssertNoMessageError($"Transport Nationality: {report.CER_RN_NKTransportNationality}, Transport type: {transportType}, message is not expected", report.CER_RN_NKTransportNationalityInfo, message);
			}
		}
	});

	protected override void SetUp()
	{
		base.SetUp();
		(report, _, _) = CusExitReportTest.GetNewBusinessObject(Factory);
	}
	CusExitReport report;
}
