using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507CConsignmentProviderTest : DataProviderTestCase<CC507CConsignmentProvider>
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
			() => new CC507CConsignmentProvider(null));

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport.Consignment";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport.Consignment')";
#endif

		var cusExitReport = Factory.New<CusExitReport>();
		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignment", expectedMessage,
			() => new CC507CConsignmentProvider(cusExitReport));

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport.Header";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport.Header')";
#endif

		var cusExitConsignment = Factory.New<CusExitConsignment>();
		cusExitReport.CER_CXC_Consignment = cusExitConsignment.PK;
		AssertExceptionThrown<ArgumentNullException>("Null CusExitHeader", expectedMessage,
			() => new CC507CConsignmentProvider(cusExitReport));
	});

	public void TestModeOfTransportAtTheBorder() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CER_TransportType is not set", GetProvider().ModeOfTransportAtTheBorder);

		cusExitReport.CER_TransportType = "A";
		AssertEquals("CER_TransportType is A", "A", GetProvider().ModeOfTransportAtTheBorder);
	});

	public void TestReferenceNumberUcr() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("CXC_UniqueConsignmentReference is not set", GetProvider().ReferenceNumberUcr);

		cusExitConsignment.CXC_UniqueConsignmentReference = "ABC123";
		AssertEquals("CXC_UniqueConsignmentReference is A", "ABC123", GetProvider().ReferenceNumberUcr);
	});

	public void TestExitCarrier() => CombineAssertions(() =>
	{
		AssertNull("Carrier is not set", GetProvider().ExitCarrier);

		var orgCarrier = Factory.New<OrgHeader>();
		var orgAddress = orgCarrier.Addresses.AddNew();
		cusExitHeader.CXH_OA_Carrier = orgAddress.PK;
		AssertNotNull("Carrier is present", GetProvider().ExitCarrier);
	});

	public void TestTransportEquipments() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("empty collection", 0, GetProvider().TransportEquipments.Count);

		cusExitHeader.CusExitContainers.AddNew();
		cusExitHeader.CusExitContainers.AddNew();
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertEquals("Discrepancies are disabled", 0, GetProvider().TransportEquipments.Count);

		cusExitReport.CER_Calc_Discrepancies = true;
		var transportEquipments = GetProvider().TransportEquipments;
		AssertEquals("discrepancies are enabled", 2, transportEquipments.Count);
		AssertEquals("1st item should have sequence number 1", 1, transportEquipments.First().SequenceNumber);
		AssertEquals("2nd item should have sequence number 2", 2, transportEquipments.Last().SequenceNumber);
	});

	public void TestLocationOfGoods() => AssertNotNull("PL Uses UCC6 thus this should never be null", Provider.LocationOfGoods);

	public void TestActiveBorderTransportMeans() => AssertNotNull(Provider.ActiveBorderTransportMeans);

	public void TestTransportDocument() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("empty collection", 0, GetProvider().TransportDocument.Count);

		var tra1 = cusExitReport.AdditionalInfos.AddNew();
		tra1.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var tra2 = cusExitReport.AdditionalInfos.AddNew();
		tra2.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var tra3 = cusExitReport.AdditionalInfos.AddNew();
		tra3.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var infType = cusExitReport.AdditionalInfos.AddNew();
		infType.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		var refType = cusExitReport.AdditionalInfos.AddNew();
		refType.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertEquals("Discrepancies are disabled", 0, GetProvider().TransportDocument.Count);

		cusExitReport.CER_Calc_Discrepancies = true;
		var transportDocument = GetProvider().TransportDocument;
		AssertEquals("discrepancies are enabled", 3, transportDocument.Count);
	});

	protected override CC507CConsignmentProvider GetProvider() => new CC507CConsignmentProvider(cusExitReport);

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
