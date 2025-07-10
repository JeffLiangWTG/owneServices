using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTariffLanguageView))]
	public class CusRefTariffLanguageViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZX7_DataSet()
		{
			var language = Factory.New<CusRefTariffLanguageView>();
			AssertEquals("O", language.ZX7_DataSet);
			Assert(language.ZX7_DataSetInfo.ReadOnly);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewCusTariffLanguage(factory);
		protected override BusinessObject GetNewBusinessObject() => CreateNewCusTariffLanguage(Factory);
		CusRefTariffLanguageView CreateNewCusTariffLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.LoadOrCreateNewCusRateCode(factory, "DJC", rateType.PK);
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var tariffName = ZGuid.NewZGuid().ToString().Replace("-", string.Empty);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, tariffName, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Desc");
			var tariffView = factory.Load<TariffView>(tariff.PK);
			var lanuage = helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "ENG", "Test Description", true);
			return lanuage;
		}

		//ZZRef RefCusTariffLanguage is not supported to delete by CusRefTariffLanguageView, so suspend the delete testing in TestSaveAndDeleteBusinessObject().
		protected override bool CanPersistedObjectBeDeleted => false;
		public override void TestCallsBaseSetDefaultValues() => Assert(true);
	}
}
