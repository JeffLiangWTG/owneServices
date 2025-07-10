using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
{
	public void TestArrivalMovementHeaderType()
	{
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		AssertType<NctsArrivalMovementHeader>(header.ArrivalMovementHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<NctsHeader>();
	}

	NctsHeader header;
}
