using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class CalculateDutyStrategyTest : TestCaseWithFactory
	{
		public void TestShowPreSaveDialogs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_VatPymntMthd = "DEF";

			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var entryLineFeesAfterReMerge = entryLine.Fees.Cast<CusEntryLineFee>();
			AssertEquals(1, entryLineFeesAfterReMerge.Count());
			var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>();

			var fee = entryLineFees.FirstOrDefault(x => x.CF_ChargeType == "VAT");

			var mergePreSaveDialogStrategy = new CalculateDutyStrategy(declaration);
			mergePreSaveDialogStrategy.ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals(1, entryLineFeesAfterReMerge.Count());

			fee.TW_RateOverride = "ADD";
			mergePreSaveDialogStrategy.ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals(2, entryLineFeesAfterReMerge.Count());

			entryLine.Fees.RemoveAndDeleteAll();
			mergePreSaveDialogStrategy.ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals(1, entryLineFeesAfterReMerge.Count());
		}
	}
}
