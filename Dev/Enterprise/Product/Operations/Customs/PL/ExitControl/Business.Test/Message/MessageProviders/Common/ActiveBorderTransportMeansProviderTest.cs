using System;
using Enterprise.Customs.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class ActiveBorderTransportMeansProviderTest : DataProviderTestCase<ActiveBorderTransportMeansProvider>
{
	public void TestConstructor()
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport')";
#endif
		AssertExceptionThrown<ArgumentNullException>("Null CusExitReport", expectedMessage,
			() => new ActiveBorderTransportMeansProvider(null));
	}

	public void TestTypeOfIdentification() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CER_TransportType is not set", GetProvider().TypeOfIdentification);

		cusExitReport.CER_TransportType = "A";
		AssertEquals("CER_TransportType is A", "A", GetProvider().TypeOfIdentification);
	});

	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CER_TransportID is not set", GetProvider().IdentificationNumber);

		cusExitReport.CER_TransportID = "B";
		AssertEquals("CER_TransportID is B", "B", GetProvider().IdentificationNumber);
	});

	public void TestIdentificationNumber_R0049E() => CombineAssertions(() =>
	{
		const string testTransportID = "B";
		cusExitReport.CER_TransportID = testTransportID;

		foreach (var (transportType, valueExpected) in ((string TransportType, bool ValueExpected)[])[
			("10", true), ("23", true), ("30", true), ("63", true), ("87", true),
			("50", false), ("53", false), ("70", false), ("79", false)])
		{
			AssertValue(transportType, valueExpected);
		}
		return;

		void AssertValue(string transportType, bool valueIsExpected)
		{
			cusExitReport.CER_TransportType = transportType;
			if (valueIsExpected)
			{
				AssertEquals($"Transport Type is {transportType}, value expected", testTransportID, GetProvider().IdentificationNumber);
			}
			else
			{
				AssertNullOrEmpty($"Transport Type is {transportType}, value not expected", GetProvider().IdentificationNumber);
			}
		}
	});

	public void TestNationality() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CER_RN_NKTransportNationality is not set", GetProvider().Nationality);

		cusExitReport.CER_RN_NKTransportNationality = CountryCodes.Poland;
		AssertEquals("CER_RN_NKTransportNationality PL", CountryCodes.Poland, GetProvider().Nationality);
	});

	public void TestNationality_R0050E() => CombineAssertions(() =>
	{
		cusExitReport.CER_RN_NKTransportNationality = CountryCodes.Poland;

		foreach (var (transportType, valueExpected) in ((string TransportType, bool ValueExpected)[])[
			("10", true), ("30", true), ("63", true), ("87", true),
			("23", false), ("50", false), ("53", false), ("70", false), ("79", false)])
		{
			AssertValue(transportType, valueExpected);
		}
		return;

		void AssertValue(string transportType, bool valueIsExpected)
		{
			cusExitReport.CER_TransportType = transportType;
			if (valueIsExpected)
			{
				AssertEquals($"Transport Type is {transportType}, value expected", CountryCodes.Poland, GetProvider().Nationality);
			}
			else
			{
				AssertNullOrEmpty($"Transport Type is {transportType}, value not expected", GetProvider().Nationality);
			}
		}
	});

	protected override ActiveBorderTransportMeansProvider GetProvider() => new ActiveBorderTransportMeansProvider(cusExitReport);

	protected override void SetUp()
	{
		base.SetUp();
		cusExitReport = Factory.New<CusExitReport>();
	}

	CusExitReport cusExitReport;
}
