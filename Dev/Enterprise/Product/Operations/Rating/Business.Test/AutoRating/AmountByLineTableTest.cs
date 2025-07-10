using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class AmountByLineTableTest : TestCaseWithFactory
	{
		public void TestBaseLinesStillCanBeRetrieved()
		{
			var tariff1 = Factory.NewWithValidTestData<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("DST", "AIR", "AU", "US");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine = tariffEntry.AddRateLine("DDOC");
			tariffLine.TL_RateCalculator = UnitCalculator.Code;
			tariffLine.TL_WeightVolume = "KG";
			tariffLine.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)5m;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var rate = Factory.NewWithValidTestData<ClientRate>();
			org1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			rate.TH_OH = org1.PK;

			var rateEntry = rate.AddRateEntry("DST", "AIR", "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("DDOC");
			rateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -25m;

			Factory.Save();

			RatingCriteria criteria = new TestRatingCriteria();
			criteria.ValuesCanBeSet = true;
			criteria.FreightMode = FreightMode.AIR;
			criteria.Origin = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			criteria.Destination = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);

			var measures = criteria.RateableMeasures;
			measures.SetWeightWithCommodity(2, Core.Constants.Weight.Kilograms, "GEN");
			var part = measures.GetPartList(MeasureType.Weight)[0];

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			var amountByLineTable = new AmountByLineTable(parameters);

			// Add match for derived rate line
			amountByLineTable.AddAmount(MeasureType.Weight, rateLine, part);

			var partsForBaseLine = amountByLineTable.GetPartsForLineMeasure(MeasureType.Weight, tariffLine);

			AssertEquals(1, partsForBaseLine.Count);
			AssertEquals(part, partsForBaseLine[0].Part);
		}
	}
}
