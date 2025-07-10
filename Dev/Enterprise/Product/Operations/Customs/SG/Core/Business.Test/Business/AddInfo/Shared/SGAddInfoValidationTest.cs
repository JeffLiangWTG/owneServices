using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class SGAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			AddInfo parent = GetNewAddInfo();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected abstract AddInfo GetNewAddInfo();
		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "24011010");
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.KGM);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff1);
			Factory.Save();
		}
	}
}
