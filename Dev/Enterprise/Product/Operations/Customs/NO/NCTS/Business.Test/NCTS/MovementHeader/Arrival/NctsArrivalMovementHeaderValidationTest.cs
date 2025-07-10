using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeaderValidation))]
sealed class NctsArrivalMovementHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestGoodsRegistrationNumberMandatoryValidation()
	{
		CombineAssertions(() =>
		{
			arrivalMovementHeader.GoodsRegistrationNumber = "GRN000123";
			AssertNoMessageErrorContaining("When GoodsRegistrationNumber is not empty", arrivalMovementHeader.GoodsRegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			arrivalMovementHeader.GoodsRegistrationNumber = ZString.Empty;
			AssertHasMessageErrorContaining("When GoodsRegistrationNumber is empty", arrivalMovementHeader.GoodsRegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = header.ArrivalMovementHeader;
	}

	NctsHeader header;
	NctsArrivalMovementHeader arrivalMovementHeader;
}
