using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507CGoodsShipmentProviderTest : DataProviderTestCase<CC507CGoodsShipmentProvider>
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
			() => new CC507CGoodsShipmentProvider(null));

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitReport.Consignment";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitReport.Consignment')";
#endif

		var cusExitReport = Factory.New<CusExitReport>();
		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignment", expectedMessage,
			() => new CC507CGoodsShipmentProvider(cusExitReport));
	});

	public void TestAdditionalInformationPl() => CombineAssertions(() =>
	{
		AssertEquals("empty collection", 0, GetProvider().AdditionalInformationPl.Count);

		var inf1 = cusExitReport.AdditionalInfos.AddNew();
		inf1.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		var inf2 = cusExitReport.AdditionalInfos.AddNew();
		inf2.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		var inf3 = cusExitReport.AdditionalInfos.AddNew();
		inf3.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		var traType = cusExitReport.AdditionalInfos.AddNew();
		traType.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var refType = cusExitReport.AdditionalInfos.AddNew();
		refType.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		cusExitReport.CER_Calc_Discrepancies = true;
		var additionalInformationPL = GetProvider().AdditionalInformationPl;
		AssertEquals("merged data", 1, additionalInformationPL.Count);

		inf1.CSI_ReferenceNumber = "asd";
		inf2.CSI_ReferenceNumber = "bcz";
		inf3.CSI_ReferenceNumber = "poi";
		additionalInformationPL = GetProvider().AdditionalInformationPl;
		AssertEquals("not merged data", 3, additionalInformationPL.Count);
	});

	public void TestConsignment() => AssertNotNull(GetProvider().Consignment);

	public void TestGoodsItems() => CombineAssertions(() =>
	{
		AssertEquals("Empty Collection", 0, GetProvider().GoodsItems.Count);

		cusExitConsignment.CusExitConsignmentItems.AddNew();
		cusExitConsignment.CusExitConsignmentItems.AddNew();
		var goodsItems = GetProvider().GoodsItems;
		AssertEquals("2 items in collection", 2, goodsItems.Count);
	});

	protected override CC507CGoodsShipmentProvider GetProvider() => new CC507CGoodsShipmentProvider(cusExitReport);

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
