using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	abstract class JobDeclarationFormTest : BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestShowPreSaveDialogsContainsCalculateDutyStrategy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeForFormBashing;
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

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLineFeesAfterReMerge = entryHeader.MergedLines[0].Fees.Cast<CusEntryLineFee>();
				var fee = entryLineFeesAfterReMerge.FirstOrDefault(x => x.CF_ChargeType == "VAT");
				Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(1, entryLineFeesAfterReMerge.Count());

				fee.TW_RateOverride = "ADD";
				Assert(declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

				var iShowPreSaveDialog = testForm as IShowPreSaveDialog;
				iShowPreSaveDialog.ShowPreSaveDialogs();
				Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(2, entryLineFeesAfterReMerge.Count());

				fee.Delete();
				Assert(declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

				iShowPreSaveDialog.ShowPreSaveDialogs();
				Assert(!declaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(1, entryLineFeesAfterReMerge.Count());
			}
		}

		protected override Dictionary<string, string[]> GetControlsForLock()
		{
			var result = base.GetControlsForLock();
			result.Add(Core.Constants.Customs.DeclarationTabPages.Codes.EntryDetails, new[] { nameof(JobDeclarationUserControl.EntryDetailsTabPage) });
			result.Add(Core.Constants.Customs.DeclarationTabPages.Codes.BondedDetails, new[] { nameof(JobDeclarationUserControl.BondedDetailsTabPage) });
			return result;
		}
	}
}
