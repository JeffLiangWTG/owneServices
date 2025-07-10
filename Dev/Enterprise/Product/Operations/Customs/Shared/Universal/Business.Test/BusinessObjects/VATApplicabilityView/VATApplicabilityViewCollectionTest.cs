using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(VATApplicabilityViewCollection))]
	public class VATApplicabilityViewCollectionTest : ActiveBusinessObjectCollectionTestCase<VATApplicabilityViewCollection>
	{
		public void TestMatchesFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("ES", parent: eunId);
			helper.CreateNewOrGetExistingDataGrouping("CDS");
			helper.CreateNewOrGetExistingDataGrouping("GB");
			var tariffType = helper.CreateTariffType("EUN", "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff("EUN", tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vatApplicability1 = helper.CreateVATApplicabilityView(tariff, "ES", "IV1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vatApplicability2 = helper.CreateVATApplicabilityView(tariff, "CDS", "666", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vatApplicability3 = helper.CreateVATApplicabilityView(tariff, "GB", "RT1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vatApplicability4 = helper.CreateVATApplicabilityView(tariff, "GB", "RT2", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var vATApplicabilityViewCollection = new VATApplicabilityViewCollection(tariffReloaded, true, true);
			AssertNotNull(vATApplicabilityViewCollection.FindByPK(vatApplicability1.PK));
			AssertNotNull(vATApplicabilityViewCollection.FindByPK(vatApplicability2.PK));
			AssertNotNull(vATApplicabilityViewCollection.FindByPK(vatApplicability3.PK));
			AssertNull(vATApplicabilityViewCollection.FindByPK(vatApplicability4.PK));
		}

		public void TestTariffNationalCodeRelatedData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var vatApplicability1 = helper.CreateVATApplicabilityView(tariff1, Core.Constants.CountryCodes.Eritrea, "RT1", startDate: new ZDateTime(2011, 1, 1), endDate: new ZDateTime(2012, 12, 31));
			var vatApplicability2 = helper.CreateVATApplicabilityView(tariff1, Core.Constants.CountryCodes.SouthAfrica, "RT2", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var tariff2Rate1 = helper.CreateNewOrGetExistingVATApplicability(tariff2, Core.Constants.CountryCodes.Eritrea, "RT1", startDate: new ZDateTime(2011, 1, 1), endDate: new ZDateTime(2012, 12, 31));
			var tariff2Rate2 = helper.CreateNewOrGetExistingVATApplicability(tariff2, Core.Constants.CountryCodes.SouthAfrica, "RT2", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			var tariffNationalCode = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, tariff1.PK, "TNC", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), new ZDate(2010, 12, 9), "dummy Description 0", ensureDataGroupingExists: false);
			var vatApplicability3 = helper.CreateNewOrGetExistingVATApplicability(tariffNationalCode, Core.Constants.CountryCodes.Eritrea, "RT1", startDate: new ZDateTime(2011, 1, 1), endDate: new ZDateTime(2012, 12, 31));
			var vatApplicability4 = helper.CreateNewOrGetExistingVATApplicability(tariffNationalCode, Core.Constants.CountryCodes.SouthAfrica, "RT2", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			var vatApplicabilities = tariffNationalCode.VATApplicabilities;
			AssertEquals(4, vatApplicabilities.Count);
			AssertCollectionContains(vatApplicability1, vatApplicabilities);
			AssertCollectionContains(vatApplicability2, vatApplicabilities);
			AssertCollectionContains(vatApplicability3, vatApplicabilities);
			AssertCollectionContains(vatApplicability4, vatApplicabilities);
		}

		public void TestMatchesDataGroupingFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentGroupCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(currentGroupCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada, "Canada", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China, "China", parentDataGrouping);

			var tariffType = helper.CreateTariffType(currentGroupCode, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			var dutyRateType = helper.CreateCusRateType(currentGroupCode, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			Factory.Save();

			var tariff = helper.CreateTariff(currentGroupCode, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var vatFR = helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "FR", startDate: startDate, endDate: endDate);
			var vatIT = helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, "IT", startDate: startDate, endDate: endDate);
			var vatCA = helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Canada, "CA", startDate: startDate, endDate: endDate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariffReloaded = newFactory.Load<TariffView>(tariff.PK);
			var filteredRateCollection = new VATApplicabilityViewCollection(tariffReloaded, true, false);

			CombineAssertions("EffectiveDataGrouping filter inactive", () =>
			{
				tariffReloaded.Wrapper.EffectiveDataGrouping = ZString.Empty;
				AssertEquals("Expected count", 3, filteredRateCollection.Count);
				AssertNotNull($"{nameof(vatFR)} not null", filteredRateCollection.FindByPK(vatFR.PK));
				AssertNotNull($"{nameof(vatIT)} not null", filteredRateCollection.FindByPK(vatIT.PK));
				AssertNotNull($"{nameof(vatCA)} not null", filteredRateCollection.FindByPK(vatCA.PK));
			}

			);
			CombineAssertions("EffectiveDataGrouping = FR", () =>
			{
				tariffReloaded.Wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.France;
				AssertEquals("Expected count", 1, filteredRateCollection.Count);
				AssertNotNull($"{nameof(vatFR)} not null", filteredRateCollection.FindByPK(vatFR.PK));
			}

			);
			CombineAssertions("EffectiveDataGrouping = IT", () =>
			{
				tariffReloaded.Wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected count", 1, filteredRateCollection.Count);
				AssertNotNull($"{nameof(vatIT)} not null", filteredRateCollection.FindByPK(vatIT.PK));
			}

			);
			CombineAssertions("EffectiveDataGrouping = CA", () =>
			{
				tariffReloaded.Wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Canada;
				AssertEquals("Expected count", 1, filteredRateCollection.Count);
				AssertNotNull($"{nameof(vatCA)} not null", filteredRateCollection.FindByPK(vatCA.PK));
			}

			);
			CombineAssertions("Setting again EffectiveDataGrouping = CN", () =>
			{
				tariffReloaded.Wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.China;
				AssertEquals("Expected count", 0, filteredRateCollection.Count);
			}

			);
		}

		protected override VATApplicabilityViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new VATApplicabilityViewCollection(cusTariff);
		}
	}
}
