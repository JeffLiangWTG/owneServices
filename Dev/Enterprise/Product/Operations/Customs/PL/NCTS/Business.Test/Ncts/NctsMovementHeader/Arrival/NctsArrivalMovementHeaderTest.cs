using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;
using NCTSTestHelper = Enterprise.Customs.EU.NCTS.Business.Testing.NCTSTestHelper;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
sealed class NctsArrivalMovementHeaderTest : EU.NCTS.Business.Testing.NctsArrivalMovementHeaderAbstractTest
{
	public void TestHeaderType() => AssertType<NctsHeader>(GetNewBusinessObject(Factory).Header);

	public void TestGoodsLocationType() => AssertType<CusGoodsLocation>(GetNewBusinessObject(Factory).GoodsLocation);

	public void TestValidation() => AssertType<NctsArrivalMovementHeaderValidation>(GetNewBusinessObject(Factory).Validation);

	public void TestLookups() => AssertType<NctsArrivalMovementHeaderLookups>(GetNewBusinessObject(Factory).Lookups);

	public void TestAuthorizationLocationAttribute()
	{
		var authorizationLocationInfo = GetNewBusinessObject(Factory).AuthorizationLocationInfo;

		NCTSTestHelper.AssertCaptions(authorizationLocationInfo, "Goods Location from Authorization", "", "Goods Loc. from Auth.");
		AssertEquals(nameof(NctsHeader.Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationRuleList), authorizationLocationInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public void TestRepresentativeTraderAttribute()
	{
		var arrivalMovementHeader = GetNewBusinessObject(Factory);
		var representativeTraderInfo = arrivalMovementHeader.RepresentativeTraderInfo;

		NCTSTestHelper.AssertCaptions(arrivalMovementHeader.RepresentativeTraderInfo, "Trader Representative for Communication", "Trader Rep. For Comm.", "Trader Rep.");
		AssertEquals("Lookups.Organisations", representativeTraderInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	public void TestAdditionalDocuments() => AssertType<EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>>(GetNewBusinessObject(Factory).AdditionalDocuments);

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
			&& info.Name != "DestinationCustomsOfficeCodeForArrival")
		{
			base.TestBizObjectField(info);
		}
	}

	NctsArrivalMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.BH_HeaderType = "A";
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		return arrivalMovementHeader;
	}
}
