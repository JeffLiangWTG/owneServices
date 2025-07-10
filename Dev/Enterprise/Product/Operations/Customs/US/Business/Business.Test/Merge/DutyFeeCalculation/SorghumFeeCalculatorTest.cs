using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SorghumFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2009, 4, 4)]
		public void TestSorghumFee_Tariff1()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1007000020";
			invoiceLine.JI_LinePrice = 1431.34m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8.59m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Sorghum).CY_FeeAmount);
		}

		[TestDate(2009, 4, 4)]
		public void TestNoSorghumFee_Tariff1WhenTIB()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1007000020";
			invoiceLine.JI_LinePrice = 1431.34m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Sorghum));
		}

		[TestDate(2009, 4, 4)]
		public void TestSorghumFee_Tariff2()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1007000040";
			invoiceLine.JI_LinePrice = 1431.34m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8.59m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Sorghum).CY_FeeAmount);
		}

		[TestDate(2009, 4, 4)]
		public void TestSorghumFee_TariffThatIsNotSorghum()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DOTIsApplicable;
			invoiceLine.JI_LinePrice = 1431.34m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Sorghum));
		}

		[TestDate(2009, 3, 31)]
		public void TestSorghumFee_NotCalculatedInMarch2009()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1007000020";
			invoiceLine.JI_LinePrice = 1431.34m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Sorghum));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			CreateTariff("1007000020");
			CreateTariff("1007000040");
		}

		JobDeclaration Declaration
		{
			get
			{
				if (_declaration == null)
				{
					_declaration = Factory.New<JobDeclaration>();
					_declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					_declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					_declaration.US_EnableENS = true;
				}

				return _declaration;
			}
		}
		JobDeclaration _declaration;

		void CreateTariff(ZString tariffNumber)
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = new ZDateTime(2009, 4, 1);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_DutyElement = "5";
			dutyRate.UD_TaxFeeClassCode = "109";
			dutyRate.UD_TaxFeeComputationCode = "7";
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeAdvalorem = 0.00600000m;
		}
	}
}
