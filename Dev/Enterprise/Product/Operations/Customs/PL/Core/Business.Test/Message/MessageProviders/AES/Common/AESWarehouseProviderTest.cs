using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESWarehouseProviderTest : Customs.Business.Testing.DataProviderTestCase<AESWarehouseProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusCode", "Value cannot be null.\r\nParameter name: cusCode",
				() => new AESWarehouseProvider(null));
		});
	}

	public void TestType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Many signs", "1", Provider.Type);
			cusCode.OK_CustomsRegNo = "1";
			AssertEquals("One sign", "1", Provider.Type);
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertEquals("Empty", string.Empty, Provider.Type);
		});
	}

	public void TestIdentifier()
	{
		AssertEquals("123", Provider.Identifier);
	}

	protected override AESWarehouseProvider GetProvider() => new AESWarehouseProvider(cusCode);

	protected override void SetUp()
	{
		base.SetUp();
		cusCode = Factory.New<OrgCusCode>();
		cusCode.OK_CustomsRegNo = "123";
	}

	OrgCusCode cusCode;
}
