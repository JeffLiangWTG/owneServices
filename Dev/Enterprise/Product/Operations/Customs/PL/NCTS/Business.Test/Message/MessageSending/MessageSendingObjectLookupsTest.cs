using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class MessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAmendmentTypeList_Cached()
	{
		var lookups = messageSendingObject.Lookups;
		var amendmentTypeList = lookups.AmendmentTypeList;

		AssertSame(amendmentTypeList, lookups.AmendmentTypeList);
	}

	public void TestAmendmentTypeList()
	{
		AssertContainsExactElementsInAnyOrder(new string[] { AmendmentTypeList.Codes._0DeclarationAmendment, AmendmentTypeList.Codes._1GuaranteeAmendment }, messageSendingObject.Lookups.AmendmentTypeList.GetAllCodes());
	}

	public void TestTirPageNumberTypeList()
	{
		var lookups = messageSendingObject.Lookups;
		var list = lookups.TirPageNumberTypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder(new string[]
				{
					TirPageNumberTypeList.Codes._02,
					TirPageNumberTypeList.Codes._04,
					TirPageNumberTypeList.Codes._06,
					TirPageNumberTypeList.Codes._08,
					TirPageNumberTypeList.Codes._10,
					TirPageNumberTypeList.Codes._12,
					TirPageNumberTypeList.Codes._14,
					TirPageNumberTypeList.Codes._16,
					TirPageNumberTypeList.Codes._18,
					TirPageNumberTypeList.Codes._20
				}
				, list.GetAllCodes());
			AssertSame(list, lookups.TirPageNumberTypeList);
		});
	}

	public void TestTirUnloadingNumberTypeList()
	{
		var lookups = messageSendingObject.Lookups;
		var list = lookups.TirUnloadingNumberTypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder(new string[]
				{
					TirUnloadingNumberTypeList.Codes.D1,
					TirUnloadingNumberTypeList.Codes.D2,
					TirUnloadingNumberTypeList.Codes.D3
				}
				, list.GetAllCodes());
			AssertSame(list, lookups.TirUnloadingNumberTypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}
	MessageSendingObject messageSendingObject;
}
