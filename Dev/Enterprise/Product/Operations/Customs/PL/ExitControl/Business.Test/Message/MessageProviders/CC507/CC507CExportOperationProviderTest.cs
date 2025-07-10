using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507CExportOperationProviderTest : DataProviderTestCase<CC507CExportOperationProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport')";
#endif

		AssertExceptionThrown<ArgumentNullException>("Null CusExitReport", expectedMessage,
			() => new CC507CExportOperationProvider(null));

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport.Consignment";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport.Consignment')";
#endif

		var cusExitReport = Factory.New<CusExitReport>();
		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignment", expectedMessage,
			() => new CC507CExportOperationProvider(cusExitReport));
	});

	public void TestIssuedByAES() => AssertEquals(true, GetProvider().IssuedByAES);

	public void TestMRN() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("MRN is empty", GetProvider().MRN);

		cusExitConsignment.CXC_MovementReference = "ABC";
		AssertEquals("MRN not empty", "ABC", GetProvider().MRN);
	});

	public void TestArrivalNotificationDateAndTime() => CombineAssertions(() =>
	{
		var arrivalNotificationDateAndTime = GetProvider().ArrivalNotificationDateAndTime;
		var currentUtcDateTime = ZDateTimeOffset.Now.ToUtcDateTime();
		var diff = currentUtcDateTime - arrivalNotificationDateAndTime;
		AssertLessThan("date was not set - should be set by default to UTC now date", diff, TimeSpan.FromSeconds(1));

		cusExitReport.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
		AssertEquals("date is set - should contain UTC date", new DateTime(2022, 02, 15, 15, 43, 27), GetProvider().ArrivalNotificationDateAndTime);
	});

	public void TestArrivalNotificationPlace() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("ArrivalNotificationPlace is empty", GetProvider().ArrivalNotificationPlace);

		cusExitReport.CER_Location = "BCA";
		AssertEquals("ArrivalNotificationPlace not empty", "BCA", GetProvider().ArrivalNotificationPlace);
	});

	public void TestStoringFlag() => CombineAssertions(() =>
	{
		AssertNotNull(Provider.StoringFlag);

		cusExitHeader.StoringFlag = true;
		AssertEquals("StoringFlag is true", 1, GetProvider().StoringFlag);

		cusExitHeader.StoringFlag = false;
		AssertEquals("StoringFlag is false", 0, GetProvider().StoringFlag);
	});

	public void TestDiscrepanciesExist() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertEquals("disabled", 0, GetProvider().DiscrepanciesExist);

		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("enabled", 1, GetProvider().DiscrepanciesExist);
	});

	protected override CC507CExportOperationProvider GetProvider() => new CC507CExportOperationProvider(cusExitReport);

	protected override void SetUp()
	{
		base.SetUp();
		cusExitHeader = Factory.New<CusExitHeader>();
		cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
		cusExitReport = cusExitHeader.CusExitReports.AddNew();
		cusExitReport.CER_CXC_Consignment = cusExitConsignment.PK;
	}

	CusExitHeader cusExitHeader;
	CusExitConsignment cusExitConsignment;
	CusExitReport cusExitReport;
}
