using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESAdditionalCodeProviderTest : Customs.Business.Testing.DataProviderTestCase<AESAdditionalCodeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusCode", "Value cannot be null.\r\nParameter name: cusCode", () => new AESAdditionalCodeProvider(null, 1));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(4, Provider.SequenceNumber);
	}

	public void TestCode()
	{
		cusCode.CY_Code = "123";
		AssertEquals("123", Provider.Code);
	}

	protected override AESAdditionalCodeProvider GetProvider() => new AESAdditionalCodeProvider(cusCode, 4);

	protected override void SetUp()
	{
		base.SetUp();
		cusCode = Factory.New<SupplementaryCode>();
	}

	CusCodeData cusCode;
}
