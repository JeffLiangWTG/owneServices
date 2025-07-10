using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class ProductCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2018, 7, 1)]
		public void TestProductCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "87139000");
			helper.CreateCommodity(tariff1, "HSAIFRSA");
			helper.CreateCommodity(tariff1, "HSAHP");
			helper.CreateCommodity(tariff1, "HSAIPU");
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "11063000");
			helper.CreateCommodity(tariff2, "ZRP0NK0QC00");
			helper.CreateCommodity(tariff2, "ZRP0NF0QC00");
			helper.CreateCommodity(tariff2, "ZRP0CN0QC00");
			helper.CreateCommodity(tariff2, "ZRP0BN0QC00");
			helper.CreateCommodity(tariff2, "ZRP0NA0QC00");
			helper.CreateCommodity(tariff2, "ZDP0WAM0000");
			helper.CreateCommodity(tariff2, "ZRP0DA0QC00");
			helper.CreateCommodity(tariff2, "ZRP0C8AQC00");
			Factory.Save();
			var tariffClass = Factory.New<Classification>();
			tariffClass.CC_TariffNum = "87139000";
			var productCode = Factory.New<ProductCode>();
			productCode.CY_ParentID = tariffClass.PK;
			productCode.CY_ParentTableCode = tariffClass.TablePrefix;
			var productCodeLookups = new ProductCodeLookups(productCode);
			AssertEquals("After reference data upgrade, there should be 3 commodity code records for this tariff", 3, productCodeLookups.ProductCodes.Count);
			tariffClass = Factory.New<Classification>();
			tariffClass.CC_TariffNum = "11063000";
			productCode = Factory.New<ProductCode>();
			productCode.CY_ParentID = tariffClass.PK;
			productCode.CY_ParentTableCode = tariffClass.TablePrefix;
			productCodeLookups = new ProductCodeLookups(productCode);
			AssertEquals("After reference data upgrade, there should be 8 commodity code records for this tariff", 8, productCodeLookups.ProductCodes.Count);
		}
	}
}
