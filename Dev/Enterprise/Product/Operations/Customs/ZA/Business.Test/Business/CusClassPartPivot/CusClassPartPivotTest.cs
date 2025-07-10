using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestCusLineTariffDetails()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertType<CusLineTariffDetailCollection<CusLineTariffDetail>>(pivot.CusLineTariffDetails);
		}

		public void TestNewPivotGoodsType()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("N", pivot.CI_NewUsed);
		}

		public void TestITariffFormatProvider()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertType<NumberOnlyTariffFormatter>("TariffFormatter", ((Common.ITariffFormatProvider)pivot).TariffFormatter);
		}

		public void TestCI_CC_ReadOnly()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "111.11.1111";
			Assert("Should not able to edit CI_CC", pivot.CI_CCInfo.ReadOnly);
		}
	}
}
