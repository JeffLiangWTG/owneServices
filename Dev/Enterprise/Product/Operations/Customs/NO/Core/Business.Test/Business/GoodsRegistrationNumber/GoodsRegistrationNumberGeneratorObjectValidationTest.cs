using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GoodsRegistrationNumberGeneratorObjectValidation))]
sealed class GoodsRegistrationNumberGeneratorObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructorParams()
	{
		AssertExceptionThrown<ArgumentNullException>("When parent is null", () => new GoodsRegistrationNumberGeneratorObjectValidation(null));
	}

	public void TestCheckGoodsRegistrationDate() =>
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CreateGoodsNumberGeneratorObject().GoodsRegistrationDateInfo);

	public void TestCheckWarehouseAuthorisationId()
	{
		_ = OrgHeaderTestDataHelper.CreateCusAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(CreateGoodsNumberGeneratorObject().WarehouseAuthorisationIdInfo, "09321", "09123");
	}

	GoodsRegistrationNumberGeneratorObject CreateGoodsNumberGeneratorObject() =>
		new(Factory.New<CusTempStorageRegHeader>(), Factory);
}
