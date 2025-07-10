using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NO.GUI.Testing
{
	internal class RefCusTariffTestHelper
	{
		public void CreateTariffData()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("NO", "HSN");

			var tariff1 = helper.LoadOrCreateNewTariff("NO", tariffType.PK, "22042909", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with rates for MA (paper carton)");
			AddExciseCodeForTariff(tariff1, "MA200", "0.0025 * VFD", "Carton/cardboard");
			var tariff2 = helper.LoadOrCreateNewTariff("NO", tariffType.PK, "22041111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with no rate for beverage packaging");
			AddExciseCodeForTariff(tariff2, "XX200", "0.0025 * VFD", "Not package related");
			factory.Save();

			void AddExciseCodeForTariff(TariffView tariff, string exciseCode, string formula, string description)
			{
				var rateType = helper.CreateNewOrGetExistingRateType("NO", Universal.Constants.RateTypes.Excise);
				var rateCode = helper.LoadOrCreateNewCusRateCode(factory, exciseCode, rateType.PK, description: description);
				_ = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, formula, dataGrouping: "NO");
			}
		}

		public RefCusTariffTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;
	}
}
