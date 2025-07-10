using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class UniversalTariffCustomsUnitDefaultingStrategyTest : CustomsUnitDefaultingStrategyTest
	{
		public override void TestDefaultUOMs()
		{
			var tariff = SetupTariffData();
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);

			var strategy = new UniversalTariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>();
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "1234";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "CU3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "CU4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Fifth UQ", "CU5", InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestDefaultFirstUnitOnly()
		{
			var tariff = SetupTariffData();
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(true);

			var strategy = new UniversalTariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(defaultFirstUnitOnly: true);
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "5678";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Fifth UQ", "", InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestUseUniversalTariff()
		{
			invoiceLineMock.Setup(x => x.UseUniversalTariffCore).Returns(false);
			var strategy = new UniversalTariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => null);
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "5678";
			AssertEquals("The UniversalTariffCustomsUnitDefaultingStrategy cannot be used with a JobComInvoiceLine where UseUniversalTariff is set to False.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		TariffView SetupTariffData()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "HSN");
			newFactory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU1");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "CU2");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.CustomsUOM3Type, "CU3");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.CustomsUOM4Type, "CU4");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.CustomsUOM5Type, "CU5");
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.ClassificationUOMType, "RU1");
			newFactory.Save();

			return tariff;
		}
	}
}
