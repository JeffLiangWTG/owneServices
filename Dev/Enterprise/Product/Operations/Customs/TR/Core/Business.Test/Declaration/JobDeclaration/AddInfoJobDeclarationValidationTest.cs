using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_ShippingCountry()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_ShippingCountryInfo);
		}

		public void TestCheckZG_CountryOfSupply()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_CountryOfSupplyInfo);
		}

		public void TestCheckZG_BankCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_BankCodeInfo);
		}

		public void TestCheckZG_TradeType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_TradeTypeInfo, "ER", TradeTypeList.Codes.ETD);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
