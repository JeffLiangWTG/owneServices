using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AdValoremFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestAdValoremFee_Sorghum()
		{
			CreateTariff("1007900000", Core.Constants.USCustoms.FeeCodes.Sorghum);
			CreateTariff("99038803", ZString.Empty);

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine1.JI_Tariff = "1007900000";
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2.JI_Tariff = "1007900000";
			invoiceLine2.US_SupTariff = "99038803";
			invoiceLine2.JI_LinePrice = 10000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Sorghum fee for invoice line 1, no sup tariff", 60m, invoiceLine1.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Sorghum));
			AssertEquals("Sorghum fee for invoice line 2, with sup tariff", 60m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Sorghum));
		}

		public void TestAdValoremFee_Pork()
		{
			CreateTariff("0103920091", Core.Constants.USCustoms.FeeCodes.Pork);
			CreateTariff("99038815", ZString.Empty);

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine1.JI_Tariff = "0103920091";
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2.JI_Tariff = "0103920091";
			invoiceLine2.US_SupTariff = "99038815";
			invoiceLine2.JI_LinePrice = 10000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Pork fee for invoice line 1, no sup tariff", 60m, invoiceLine1.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Pork));
			AssertEquals("Pork fee for invoice line 2, with sup tariff", 60m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Pork));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		JobDeclaration Declaration
		{
			get
			{
				if (_declaration == null)
				{
					_declaration = Factory.New<JobDeclaration>();
					_declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					_declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					_declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					_declaration.US_EnableENS = true;
				}

				return _declaration;
			}
		}
		JobDeclaration _declaration;

		void CreateTariff(ZString tariffNumber, ZString feeCode)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = new ZDateTime(2009, 4, 1);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);

			if (!feeCode.IsEmpty)
			{
				var dutyRate = tariff.DutyRates.AddNew();
				dutyRate.UD_DutyElement = "5";
				dutyRate.UD_TaxFeeClassCode = feeCode;
				dutyRate.UD_TaxFeeComputationCode = "7";
				dutyRate.UD_TaxFeeFlag = "1";
				dutyRate.UD_TaxFeeAdvalorem = 0.00600000m;
			}
		}
	}
}
