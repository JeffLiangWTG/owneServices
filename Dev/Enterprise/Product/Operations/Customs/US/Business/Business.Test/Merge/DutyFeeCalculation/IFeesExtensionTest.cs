using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IFeesExtensionTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestGetGrandTotalFee()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.JI_Tariff = "1211.90.9180";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1m;
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SPI = "AU";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();//To serialise JI_AddInfo which will be cloned

			AssertEquals("HMF Deminimus:nothing to pay", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			reconDeclaration.OriginalEntries[0].US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = new ZDateTime(2017, 11, 17);

			reconDeclaration.InvoiceLines[0].US_R_OrigCV = 250m;
			reconDeclaration.InvoiceLines[0].US_R_OrigTariff = "1211909180";
			reconDeclaration.InvoiceLines[0].US_R_OrigSPI = "AU";

			reconDeclaration.InvoiceLines[1].US_R_OrigCV = 1m;
			reconDeclaration.InvoiceLines[1].US_R_OrigTariff = "4823209000";
			reconDeclaration.InvoiceLines[1].US_R_OrigSPI = "AU";

			reconDeclaration.InvoiceLines[0].US_SPI = "";
			reconDeclaration.InvoiceLines[1].US_SPI = "";

			reconDeclaration.CalculateDutyFeesForChangedEntries();

			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries[0];

			AssertEquals("ReconTotalFeeAmount", 25.31m, new ReconCurrentDutyDataLineHeader(reconOriginalEntry).FeeAndCharges.GetGrandTotalFee());
			AssertEquals("OriginalTotalFeeAmount: Should still be zero", 0m, new ReconOriginalDutyDataLineHeader(reconOriginalEntry).FeeAndCharges.GetGrandTotalFee());
		}

		public void TestUpdateOrAddCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			IDutyDataLineHeader iHeader = entry;

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			AssertEquals("MPF is added", 20m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			iHeader.FeeAndCharges.AdjustAccordingToMinimumAndMaximum(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 21m, 485m);
			AssertEquals("MPF is adjusted", 21m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0m);
			AssertEquals("MPF is updated", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			iHeader.FeeAndCharges.AdjustAccordingToMinimumAndMaximum(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 21m, 485m);
			AssertEquals("Absence of fee does not lift to a minimum amount", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 500m);
			AssertEquals("MPF is updated", 500m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			iHeader.FeeAndCharges.AdjustAccordingToMinimumAndMaximum(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 21m, 485m);
			AssertEquals("max amount", 485m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestUpdateOrAddChargeDoesNotDeleteMandatoryCharges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			List<ZString> mandatoryFees = new List<ZString>();
			mandatoryFees.Add(Core.Constants.USCustoms.FeeCodes.Wines);

			IDutyDataLineHeader iHeader = entry;

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Wines, 20m, mandatoryFees);
			AssertEquals("Set up Wine excise fee", 20m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertEquals("entry.Charges.Count", 1, entry.Charges.Count);

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Honey, 150m);
			AssertEquals("Set up another fee", 150m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals("entry.Charges.Count", 2, entry.Charges.Count);

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Honey, 0m);
			AssertEquals("Change second fee to zero", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals("entry.Charges.Count", 1, entry.Charges.Count);

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Wines, 0m, mandatoryFees);
			AssertEquals("entry.Charges.Count", 1, entry.Charges.Count);
			AssertEquals("Wine excise fee is not removed as mandatory fee", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines));

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Wines, 0m);
			AssertEquals("entry.Charges.Count", 0, entry.Charges.Count);
			AssertEquals("If fee is not in list of mandatory fees it should be removed when no amount is present", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines));
		}

		public void TestUpdateOrAddChargeDoesDeleteMandatoryCottonChargeIfExempt()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			List<ZString> mandatoryFees = new List<ZString>();
			mandatoryFees.Add(Core.Constants.USCustoms.FeeCodes.Cotton);

			IDutyDataLineHeader iHeader = entry;

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Cotton, 20m, mandatoryFees);
			AssertEquals("Set up Cotton fee", 20m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("entry.Charges.Count", 1, entry.Charges.Count);

			iHeader.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Cotton, 0m, mandatoryFees);
			AssertEquals("entry.Charges.Count - cotton fee should be removed", 0, entry.Charges.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
