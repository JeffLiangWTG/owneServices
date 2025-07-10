using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507CGoodsItemProviderTest : DataProviderTestCase<CC507CGoodsItemProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignmentItem",
			() => new CC507CGoodsItemProvider(null, null));
		AssertExceptionThrown<ArgumentNullException>("Null CusExitReport",
			() => new CC507CGoodsItemProvider(cusExitConsignmentItem, null));
	});

	public void TestDeclarationGoodsItemNumber() => CombineAssertions(() =>
	{
		AssertEquals("CCI_LineNumber is empty", ZInt.Zero, GetProvider().DeclarationGoodsItemNumber);

		cusExitConsignmentItem.CCI_LineNumber = 123;
		AssertEquals("CCI_LineNumber is not empty", 123, GetProvider().DeclarationGoodsItemNumber);
	});

	public void TestReferenceNumberUCR() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("empty", GetProvider().ReferenceNumberUCR);

		cusExitConsignmentItem.CCI_UniqueConsignmentReference = "ABC123";
		AssertEquals("not empty", "ABC123", GetProvider().ReferenceNumberUCR);
	});

	public void TestAuthorisations() => AssertEquals(0, Provider.Authorisations.Count);

	public void TestCommodity() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertNull("discrepancies disabled", GetProvider().Commodity);

		cusExitReport.CER_Calc_Discrepancies = true;
		AssertNotNull("discrepancies enabled", GetProvider().Commodity);
	});

	public void TestPackaging() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("empty collection", 0, GetProvider().Packaging.Count);

		var package = Factory.New<CusExitConsignmentPackage>();
		var packagePivot1 = cusExitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		packagePivot1.CNP_CXP_Package = package.PK;
		var packagePivot2 = cusExitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		packagePivot2.CNP_CXP_Package = package.PK;
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertEquals("discrepancies disabled", 0, GetProvider().Packaging.Count);

		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("discrepancies enabled", 2, GetProvider().Packaging.Count);
	});

	public void TestTransportDocuments() => CombineAssertions(() =>
	{
		cusExitReport.CER_Calc_Discrepancies = true;
		AssertEquals("empty collection", 0, GetProvider().TransportDocuments.Count);

		var tra1 = cusExitConsignmentItem.AdditionalInfos.AddNew();
		tra1.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var tra2 = cusExitConsignmentItem.AdditionalInfos.AddNew();
		tra2.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var tra3 = cusExitConsignmentItem.AdditionalInfos.AddNew();
		tra3.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var infType = cusExitConsignmentItem.AdditionalInfos.AddNew();
		infType.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		var refType = cusExitConsignmentItem.AdditionalInfos.AddNew();
		refType.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		cusExitReport.CER_Calc_Discrepancies = false;
		AssertEquals("Discrepancies are disabled", 0, GetProvider().TransportDocuments.Count);

		cusExitReport.CER_Calc_Discrepancies = true;
		var transportDocument = GetProvider().TransportDocuments;
		AssertEquals("discrepancies are enabled", 3, transportDocument.Count);
	});

	public void TestAdditionalReferences() => AssertEquals(0, Provider.AdditionalReferences.Count);

	protected override CC507CGoodsItemProvider GetProvider() => new CC507CGoodsItemProvider(cusExitConsignmentItem, cusExitReport);

	protected override void SetUp()
	{
		base.SetUp();
		cusExitHeader = Factory.New<CusExitHeader>();
		cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
		cusExitConsignmentItem = cusExitConsignment.CusExitConsignmentItems.AddNew();
		cusExitReport = cusExitHeader.CusExitReports.AddNew();
		cusExitReport.CER_CXC_Consignment = cusExitConsignment.PK;
	}

	CusExitHeader cusExitHeader;
	CusExitConsignment cusExitConsignment;
	CusExitConsignmentItem cusExitConsignmentItem;
	CusExitReport cusExitReport;
}
