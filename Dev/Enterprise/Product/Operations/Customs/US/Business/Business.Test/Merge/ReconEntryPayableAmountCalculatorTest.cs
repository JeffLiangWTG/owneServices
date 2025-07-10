using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconEntryPayableAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 6000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 3000m;

			invoiceLine2.US_R_OrigCV = invoiceLine2.JI_LinePrice;
			invoiceLine2.US_R_OrigTariff = invoiceLine2.JI_Tariff;
			invoiceLine2.US_R_OrigFirstQty = invoiceLine2.JI_CustomsQuantity;

			declaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("Nothing more to pay", 0m, declaration.ReconEntry.CH_TotalPaid);

			invoiceLine.JI_LinePrice = 4000m;
			invoiceLine2.JI_LinePrice = 4000m;

			declaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("More Duty to pay", 30m, declaration.ReconEntry.DutyAmount);
			AssertEquals("Same MPF between recon and original (minimum amount applies)", 0m, declaration.ReconEntry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals("Total payable", 30m, declaration.ReconEntry.CH_TotalPaid);
		}

		public void TestOriginalChargesMatchReconCharges()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_NoLineDetails = false;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 6000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 4000m;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;

			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.DairyFee;
			fee.CY_IsOverridden = true;
			fee.CY_FeeAmount = 12.20m;
			declaration.CalculateDutyFeesForChangedEntries();
			AssertEquals(0m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
			AssertEquals(12.20m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		public void TestOriginalChargesMatchReconCharges_HeaderLevel()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 10.43m);

			new ReconEntryPayableAmountCalculator().Calculate(declaration);

			AssertNull(originalEntry.OriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals(10.43m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
		}

		public void TestOriginalChargesMatchReconCharges_NoLineDetails()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			AssertEquals("No Original Charges", 0, originalEntry.OriginalCharges.Count);
			AssertEquals("No Recon Charges", 0, originalEntry.ReconCharges.Count);

			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.OtherExcise, 2300.84m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Original OtherExcise tax should be there", 2300.84m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertNotEquals("ReconCharges should not have matching tax", null, originalEntry.ReconCharges.GetChargeWithThisCode(Core.Constants.USCustoms.FeeCodes.OtherExcise));

			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Blueberry, 10.23m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("Recon Blueberry Fee", 10.23m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry));
			AssertNotEquals("Matching Original Blueberry Fee", null, originalEntry.OriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.Blueberry));
		}

		public void TestOriginalChargesMatchReconChargeMPC()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_ChangedLinesOnly = true;
			AssertEquals("Pre-Condition: 1 Original Charge exists", 1, originalEntry.OriginalCharges.Count);
			AssertEquals("Pre-Condition: MPC exists", Core.Constants.USCustoms.FeeCodes.MPC, originalEntry.OriginalCharges[0].CY_Code);
			AssertEquals("Pre-Condition: No Recon Charges", 0, originalEntry.ReconCharges.Count);

			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 519.76m);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 2300.84m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("1 Recon Charges added", 1, originalEntry.ReconCharges.Count);
			AssertNotNull("Recon MPF", originalEntry.ReconCharges.GetChargeWithThisCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertNull("Recon MPC", originalEntry.ReconCharges.GetChargeWithThisCode(Core.Constants.USCustoms.FeeCodes.MPC));
		}
	}
}
