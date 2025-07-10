using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsBill))]
sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
{
	public void TestAdditionalDocuments() => AssertType<NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>>(nctsBill.AdditionalDocuments);

	public void TestArrivalGoodsItems() => AssertType<NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>>(nctsBill.ArrivalGoodsItems);

	public void TestCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes();
		CombineAssertions(() =>
		{
			AssertEquals("AdditionalInfo", typeof(NctsBillAdditionalDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("PreviousDocument", typeof(CommonPreviousDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals("SupportingDocument", typeof(NctsSupportingDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
		});
	}

	public void TestB0_Weight_ReadOnly() => Assert(!nctsBill.B0_WeightInfo.ReadOnly);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => nctsBill;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

	protected override BusinessObject GetNewBusinessObject() => nctsBill;

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsBill = nctsHeader.Bills.AddNew();
	}

	NctsHeader nctsHeader;
	NctsBill nctsBill;
}
