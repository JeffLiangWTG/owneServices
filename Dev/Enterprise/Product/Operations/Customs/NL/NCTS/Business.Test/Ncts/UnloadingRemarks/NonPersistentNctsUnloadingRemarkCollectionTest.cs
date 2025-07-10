using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NonPersistentNctsUnloadingRemarkCollection))]
sealed class NonPersistentNctsUnloadingRemarkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentNctsUnloadingRemarkCollection>
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew", true, GetCollectionToTest().AllowNew);
	}

	public void TestPopulateNonPersistentNctsUnloadingRemarkCollection() => CombineAssertions(() =>
	{
		arrivalMovement.BM_UnloadingRemarks = "";
		AssertEquals(0, GetCollectionToTest().Count);
		arrivalMovement.BM_UnloadingRemarks = "Free Text";
		AssertEquals(0, GetCollectionToTest().Count);
		arrivalMovement.BM_UnloadingRemarks = "<00;NL123456789;0;123>";
		AssertEquals(1, GetCollectionToTest().Count);
		arrivalMovement.BM_UnloadingRemarks = "<00;NL123456789;0;123>\r\n<1;NL987654321;1;456>\r\nFree Text";
		AssertEquals(2, GetCollectionToTest().Count);
	});

	protected override NonPersistentNctsUnloadingRemarkCollection GetCollectionToTest() => new NonPersistentNctsUnloadingRemarkCollection(arrivalMovement);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new NonPersistentNctsUnloadingRemark(arrivalMovement);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovement = nctsHeader.ArrivalMovementHeader;
	}

	NctsArrivalMovementHeader arrivalMovement;
}
