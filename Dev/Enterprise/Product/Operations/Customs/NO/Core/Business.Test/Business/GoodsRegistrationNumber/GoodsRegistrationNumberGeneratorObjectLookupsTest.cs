using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GoodsRegistrationNumberGeneratorObjectLookups))]
sealed class GoodsRegistrationNumberGeneratorObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestConstructorParams()
	{
		AssertExceptionThrown<ArgumentNullException>("When parent is null", () => new GoodsRegistrationNumberGeneratorObjectLookups(null));
	}

	public void TestAuthorizationsList()
	{
		_ = OrgHeaderTestDataHelper.CreateCusAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);

		var goodsNumberGenerator = new GoodsRegistrationNumberGeneratorObject(Factory.New<CusTempStorageRegHeader>(), Factory);
		var lookups = goodsNumberGenerator.Lookups;
		AssertEquals("List contents", "09123", lookups.AuthorizationsList.CodesAsString);
	}
}
