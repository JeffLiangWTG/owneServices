using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, guaranteeHeader.CPH_UnitOfMeasure);
		}

		public void TestResetUOMToTRY()
		{
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.BANKA;
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, guaranteeHeader.CPH_UnitOfMeasure);

			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.DAC;
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, guaranteeHeader.CPH_UnitOfMeasure);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		}
		CusGuaranteeHeader guaranteeHeader;
	}
}
