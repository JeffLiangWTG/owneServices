using CargoWise.Types;
using Enterprise.Customs.Universal;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	class TariffCustomsUnitDefaultingStrategyTest : CustomsUnitDefaultingStrategyTest
	{
		public override void TestDefaultUOMs()
		{
			var tariff = CreateTariff("CU1", "CU2", "CU3", "CU4", "CU5");
			var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => tariff.Object);
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
			var tariff = CreateTariff("CU1", "CU2", "CU3", "CU4", "CU5");
			var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => tariff.Object, defaultFirstUnitOnly: true);
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "1234";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Fifth UQ", "", InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestDefaultUnits()
		{
			AssertDefaultUnits((false, false, false, false, false), ("", "", "", "", ""));
			AssertDefaultUnits((true, false, false, false, false), ("CU1", "", "", "", ""));
			AssertDefaultUnits((false, true, false, false, false), ("", "CU2", "", "", ""));
			AssertDefaultUnits((false, false, true, false, false), ("", "", "CU3", "", ""));
			AssertDefaultUnits((false, false, false, true, false), ("", "", "", "CU4", ""));
			AssertDefaultUnits((false, false, false, false, true), ("", "", "", "", "CU5"));
			AssertDefaultUnits((true, false, false, false, true), ("CU1", "", "", "", "CU5"));
		}

		void AssertDefaultUnits((bool defaultFirstUnit, bool defaultSecondUnit, bool defaultThirdUnit, bool defaultFourthUnit, bool defaultFifthUnit) defaultUnits, (ZString UQ1, ZString UQ2, ZString UQ3, ZString UQ4, ZString UQ5) expectUQs)
		{
			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			InvoiceLine.JI_CustomsFourthUnitQty = ZString.Empty;
			InvoiceLine.JI_CustomsFifthUnitQty = ZString.Empty;
			var tariff = CreateTariff("CU1", "CU2", "CU3", "CU4", "CU5");
			var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => tariff.Object, defaultUnits);
			strategy.DefaultUOMs(InvoiceLine);
			InvoiceLine.JI_Tariff = "1234";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", expectUQs.UQ1, InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", expectUQs.UQ2, InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", expectUQs.UQ3, InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", expectUQs.UQ4, InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Fifth UQ", expectUQs.UQ5, InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestDefaultsUpdateOnTariffChange()
		{
			var tariff = CreateTariff("CU1", "CU2", "CU3", "CU4", "CU5");
			var tariff2 = CreateTariff("CX1", "CX2", "CX3", "CX4", "CX5");
			CombineAssertions(() =>
			{
				var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => tariff.Object);
				strategy.Initialise(InvoiceLine);
				InvoiceLine.JI_Tariff = "1234";
				AssertEquals("Tariff 1 First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Tariff 1 Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Tariff 1 Third UQ", "CU3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Tariff 1 Fourth UQ", "CU4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Tariff 1 Fifth UQ", "CU5", InvoiceLine.JI_CustomsFifthUnitQty);
				strategy.Deinitialise(InvoiceLine);

				strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => tariff2.Object);
				strategy.Initialise(InvoiceLine);
				InvoiceLine.JI_Tariff = "5678";
				AssertEquals("Tariff 2 First UQ", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Tariff 2 Second UQ", "CX2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Tariff 2 Third UQ", "CX3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Tariff 2 Fourth UQ", "CX4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Tariff 2 Fifth UQ", "CX5", InvoiceLine.JI_CustomsFifthUnitQty);
				strategy.Deinitialise(InvoiceLine);
			});
		}

		public void TestInvalidTariffChangeLeavesUQValues()
		{
			InvoiceLine.JI_Tariff = "1234";
			InvoiceLine.JI_CustomsUnitQty = "CU1";
			InvoiceLine.JI_CustomsSecondUnitQty = "CU2";
			InvoiceLine.JI_CustomsThirdUnitQty = "CU3";
			InvoiceLine.JI_CustomsFourthUnitQty = "CU4";
			InvoiceLine.JI_CustomsFifthUnitQty = "CU5";

			var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine => null);
			strategy.Initialise(InvoiceLine);
			InvoiceLine.JI_Tariff = "5678";
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "CU3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "CU4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Fifth UQ", "CU5", InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestDefaultUOMs_ChangingMessageType_ShouldDefault()
		{
			var tariff = CreateTariff("CU1", "CU2", "CU3", "CU4", "CU5");
			var tariff2 = CreateTariff("CX1", "CX2", "CX3", "CX4", "CX5");
			CombineAssertions(() =>
			{
				var strategy = new TariffCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(invoiceLine =>
				{
					if (invoiceLine.IsImport)
					{
						return tariff.Object;
					}
					return tariff2.Object;
				});
				strategy.Initialise(InvoiceLine);
				InvoiceLine.JI_Tariff = "5678";
				AssertEquals("Not Import UQ1", "CX1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Not Import UQ2", "CX2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Not Import UQ3", "CX3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Not Import UQ4", "CX4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Not Import UQ5", "CX5", InvoiceLine.JI_CustomsFifthUnitQty);

				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				AssertEquals("Precondition: JI_JZ should be empty. This proves that we can attach to JE_MessageType after the InvoiceLine has been created.", ZGuid.Empty, InvoiceLine.JI_JZ);
				InvoiceLine.JI_JZ = invoice.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import UQ1", "CU1", InvoiceLine.JI_CustomsUnitQty);
				AssertEquals("Import UQ2", "CU2", InvoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Import UQ3", "CU3", InvoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Import UQ4", "CU4", InvoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("Import UQ5", "CU5", InvoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		Mock<ITariff> CreateTariff(string uq1, string uq2, string uq3, string uq4, string uq5)
		{
			var tariffMock = new Mock<ITariff>();
			tariffMock.Setup(t => t.UQ1).Returns(uq1);
			tariffMock.Setup(t => t.UQ2).Returns(uq2);
			tariffMock.Setup(t => t.UQ3).Returns(uq3);
			tariffMock.Setup(t => t.UQ4).Returns(uq4);
			tariffMock.Setup(t => t.UQ5).Returns(uq5);
			return tariffMock;
		}
	}
}
