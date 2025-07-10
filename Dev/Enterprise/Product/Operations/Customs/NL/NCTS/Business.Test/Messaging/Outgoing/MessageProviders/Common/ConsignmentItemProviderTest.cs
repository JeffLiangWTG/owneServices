using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(ConsignmentItemProvider))]
sealed class ConsignmentItemProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentItemProvider>
{
	public void TestGoodsItemNumber()
	{
		item.BY_LineNo = 3;
		AssertEquals(3, Provider.GoodsItemNumber);
	}

	public void TestDeclarationGoodsItemNumber()
	{
		item.BY_DeclarationGoodsItemNumber = 4;
		AssertEquals(4, Provider.DeclarationGoodsItemNumber);
	}

	public void TestDeclarationType()
	{
		item.BY_Type = "D";
		AssertEquals("D", Provider.DeclarationType);
	}

	public void TestCountryOfDispatch()
	{
		item.BY_RN_NKCountryOfDispatch = "BE";
		AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
	}

	public void TestCountryOfDestination()
	{
		item.BY_RN_NKCountryOfDestination = "DE";
		AssertEquals(Core.Constants.CountryCodes.Germany, Provider.CountryOfDestination);
	}

	public void TestReferenceNumberUCR()
	{
		item.BY_CommercialReferenceNumber = "ref";
		AssertEquals("ref", Provider.ReferenceNumberUCR);
	}

	public void TestConsignee()
	{
		Factory.CreateJobDocAddress("CEA", parent: item);
		AssertNotNull(Provider.Consignee);
	}

	public void TestAdditionalSupplyChainActors()
	{
		Factory.CreateCusReference<CusSupplyChainActorReference>("", "SCA", parent: item);
		AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
	}

	public void TestCommodity()
	{
		AssertNotNull(Provider.Commodity);
	}

	public void TestPreviousDocuments()
	{
		item.PreviousDocuments.AddNew();
		AssertEquals("Count", 1, Provider.PreviousDocuments.Count);
	}

	public void TestSupportingDocuments()
	{
		item.SupportingDocuments.AddNew();
		AssertEquals(1, Provider.SupportingDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalInfo = item.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		AssertEquals(1, Provider.AdditionalReferences.Count);
	}

	public void TestAdditionalInformation()
	{
		var additionalInfo = item.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AssertEquals(1, Provider.AdditionalInformation.Count);
	}

	public void TestPackagings()
	{
		var package = item.Packages.AddNew();
		AssertEquals(1, Provider.Packagings.Count);
	}

	public void TestTransportDocuments()
	{
		var additionalInfo = item.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		AssertEquals(1, Provider.TransportDocuments.Count);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		item.BY_TransportChargesMethodOfPayment = "C";
		AssertEquals("C", Provider.TransportChargesMethodOfPayment);
	}

	protected override ConsignmentItemProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		item = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		provider = new ConsignmentItemProvider(item);
	}

	NctsDepartureCargoDesc item;
	ConsignmentItemProvider provider;
}
