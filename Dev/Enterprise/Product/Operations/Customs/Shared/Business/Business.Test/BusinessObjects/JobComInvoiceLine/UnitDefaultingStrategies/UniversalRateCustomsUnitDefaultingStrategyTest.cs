using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class UniversalRateCustomsUnitDefaultingStrategyTest : CustomsUnitDefaultingStrategyTest
	{
		public override void TestDefaultUOMs()
		{
			var (tariff, rate, _, _, _, _, rate5, rate6) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "11111111";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM1", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM2", InvoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestDefaultUOMs_AllApplicableRates()
		{
			var (tariff, rate, _, _, _, _, rate5, rate6) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.AllApplicableRates).Returns(new RateView[] { rate5, rate6 });

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>((invoiceLine) => invoiceLine.AllApplicableRates, Enumerable.Empty<ZPropertyInfo>());
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_Tariff = "11111111";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM4", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM5", InvoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestDefaultUOMs_PropertyInfosValueChanged()
		{
			var (tariff, rate, _, _, _, _, rate5, rate6) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.AllApplicableRates).Returns(new RateView[] { rate5, rate6 });

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>((invoiceLine) => invoiceLine.AllApplicableRates, new ZPropertyInfo[] { InvoiceLine.JI_DescriptionInfo });
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Description = "1";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM4", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM5", InvoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestRateUnitOfQuantities()
		{
			var (_, _, tariff2, rate2, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			InvoiceLine.JI_CountryOfOrigin = "FR";
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff2);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate2);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "22222222";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "UX1", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UX2", InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		public void TestRateUnitOfQuantities_DuplicateUQWithTariffUQ()
		{
			var (_, _, tariff2, _, rate3, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();
			InvoiceLine.JI_CountryOfOrigin = "FR";
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff2);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate3);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "23232323";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "UX1", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ (Duplicate)", "", InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		public void TestCountryOfOriginUseTradeGroupDefault()
		{
			var (_, _, tariff2, _, _, rate4, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff2);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate4);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			CombineAssertions("Tariff UOM for TradeGroup should default when COO matches.", () =>
			{
				InvoiceLine.JI_CountryOfOrigin = "CO";
				AssertEquals("CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("CX2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("SX1", InvoiceLine.JI_CustomsThirdUnitQty);
			});

			CombineAssertions("Tariff UOM should be ordered by ZZ8_Type and not ZZ8_UOM.", () =>
			{
				InvoiceLine.JI_CountryOfOrigin = "CU";
				AssertEquals("CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("CA1", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("SX1", InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		public void TestDefaultUOMs_TariffChanges()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			AssertDefaultUOMsWhenValueChange(JobComInvoiceLineSchema.Constants.JI_Tariff, "11111111", "22222222");
		}

		public void TestDefaultUOMs_CountryOfOriginChanges()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			AssertDefaultUOMsWhenValueChange(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, "IS", "ZA");
		}

		public void TestDefaultUOMs_CountryOfOriginIsEmpty()
		{
			var (_, _, tariff2, _, rate3, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();
			InvoiceLine.JI_CountryOfOrigin = "FR";
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff2);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate3);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "23232323";
			CombineAssertions("country is not empty", () =>
			{
				AssertEquals("First UQ", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "UX1", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "", InvoiceLine.JI_CustomsThirdUnitQty);
			});

			InvoiceLine.JI_CountryOfOrigin = "";
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "23232323";
			CombineAssertions("country is empty", () =>
			{
				AssertEquals("First UQ", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CA1", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "CX2", InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		public void TestDefaultUOMs_PrimaryPreferenceChanges()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			AssertDefaultUOMsWhenValueChange(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, "IS", "ZA");
		}

		public void TestDefaultUOMs_ConcessionOrderChanges()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			AssertDefaultUOMsWhenValueChange(JobComInvoiceLineSchema.Constants.JI_ConcessionOrder, "ORD123", "ORD456");
		}

		public void TestUseUniversalTariffAndDiagnoseInfo()
		{
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(false);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			var invoice = declaration.Invoices.AddNew();
			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_JZ = invoice.PK;
			strategy.DefaultUOMs(InvoiceLine);
			AssertEquals("The UniversalRateCustomsUnitDefaultingStrategy cannot be used with a JobComInvoiceLine where UseUniversalTariff is set to False. Initialise: HasInv=False;HasDec=False;DecApp= Exception occur: DecApp=CHF", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestIsConvertableFrom()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);

			var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(
				invoiceLine => invoiceLine.UniversalTariff,
				invoiceLine => new RateView[] { invoiceLine.UniversalDutyRate },
				isConvertibleFrom: (unitQty, list, countryCode, factory) => unitQty == "UM2");
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "11111111";
			CombineAssertions("UM2 is convertable from CU2, so we don't need to default UM2, continue to UM3", () =>
			{
				AssertEquals("CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("UM1", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("UM3", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("", InvoiceLine.JI_CustomAttrib1);
			});
		}

		void AssertDefaultUOMsWhenValueChange(string property, ZString value1, ZString value2)
		{
			CombineAssertions($"Should call DefaultUOMs() when {property} change.", () =>
			{
				var strategy = new UniversalRateCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
				strategy.Initialise(InvoiceLine);
				InvoiceLine.JI_CustomsUnitQty = "";
				InvoiceLine.JI_CustomsSecondUnitQty = "";
				InvoiceLine.JI_CustomsThirdUnitQty = "";
				InvoiceLine[property] = value1;
				AssertEquals("CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("UM1", InvoiceLine.JI_CustomsThirdUnitQty);

				strategy.Deinitialise(InvoiceLine);

				InvoiceLine.JI_CustomsUnitQty = "";
				InvoiceLine.JI_CustomsSecondUnitQty = "";
				InvoiceLine.JI_CustomsThirdUnitQty = "";
				InvoiceLine[property] = value2;
				AssertEquals("", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("", InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}
	}

	public static class UniversalRateCustomsUnitDefaultingStrategyTestingHelper
	{
		public static (TariffView tariff, RateView rate, TariffView tariff2, RateView rate2, RateView rate3, RateView rate4, RateView rate5, RateView rate6) SetupTariffData(string dataGrouping = Core.Constants.CountryCodes.Eritrea, string tariffTypeCode = "HSN")
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, tariffTypeCode);
			newFactory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU1");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "CU2");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.ClassificationUOMType, "RU1");

			var tariff2 = helper.CreateTariff(dataGrouping, tariffType.PK, "22222222", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.StatisticalUOMType, "CX1");

			var tradeGroup = helper.CreateTradeGroup(dataGrouping, Core.Constants.CountryCodes.Colombia, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Colombia);
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.AdditionalUOMType, "CX2", tradeGroup: tradeGroup);

			var tradeGroup2 = helper.CreateTradeGroup(dataGrouping, Core.Constants.CountryCodes.Cuba, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Cuba);
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.AdditionalUOMType, "CA1", tradeGroup: tradeGroup2);

			var rateType = helper.CreateNewOrGetExistingRateType(dataGrouping, "RT1");
			var rateCode = helper.LoadOrCreateNewCusRateCode(newFactory, "RC1", rateType.PK);
			newFactory.Save();

			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateRateUOM(rate.PK, "UM1");
			helper.CreateRateUOM(rate.PK, "UM2");
			helper.CreateRateUOM(rate.PK, "UM3");

			var rate2 = helper.CreateRate(tariff2, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateRateUOM(rate2.PK, "UX1");
			helper.CreateRateUOM(rate2.PK, "UX2");

			var rate3 = helper.CreateRate(tariff2, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateRateUOM(rate3.PK, "UX1");
			helper.CreateRateUOM(rate3.PK, "CX1");

			var rate4 = helper.CreateRate(tariff2, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateRateUOM(rate4.PK, "SX1");
			helper.CreateRateUOM(rate4.PK, "TX1");

			var rate5 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), dataGrouping: Core.Constants.CountryCodes.Latvia);
			helper.CreateRateUOM(rate5.PK, "UM4");
			helper.CreateCusApplicability(rate5.PK, tradeGroup, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "Q038");

			var rate6 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), dataGrouping: Core.Constants.CountryCodes.Latvia);
			helper.CreateRateUOM(rate6.PK, "UM5");
			helper.CreateCusApplicability(rate6.PK, tradeGroup, ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "Q039");

			newFactory.Save();
			return (tariff, rate, tariff2, rate2, rate3, rate4, rate5, rate6);
		}
	}
}
