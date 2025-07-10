using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	internal class DA63DocumentWrapperTest : TestCaseWithFactory
	{
		public void TestDA63EntryLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, entryHeader.MergedLines.Count);
				var tester = new DA63DocumentWrapper(entryHeader);
				AssertEquals(1, tester.DA63EntryLines.Count);
				AssertEquals(entryLine1, tester.DA63EntryLines[0].EntryLine);
				AssertEquals(true, tester.HasDA63EntryLines);
				invoiceLine3.JI_Procedure = "00YY";
				tester = new DA63DocumentWrapper(entryHeader);
				AssertEquals(2, tester.DA63EntryLines.Count);
				AssertContainsExactElementsInAnyOrder(new CusEntryLine[] { entryLine1, entryLine3 }, tester.DA63EntryLines.OfType<DA63LineDetailWrapper>().Select(x => x.EntryLine));
				AssertEquals(true, tester.HasDA63EntryLines);
				invoiceLine1.JI_Procedure = "00ZZ";
				invoiceLine3.JI_Procedure = "00ZZ";
				tester = new DA63DocumentWrapper(entryHeader);
				AssertEquals(0, tester.DA63EntryLines.Count);
				AssertEquals(false, tester.HasDA63EntryLines);
			});
		}

		public void TestTotalAmountClaimed()
		{
			invoiceLine3.JI_Procedure = "00YY";
			invoiceLine1.JI_ImportDutyPaid = 1.01m;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 1.02m);
			invoiceLine1.JI_ImportVATPaid = 1.03m;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 1.04m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 1.05m);
			invoiceLine2.JI_ImportDutyPaid = 2.01m;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 2.02m);
			invoiceLine2.JI_ImportVATPaid = 2.03m;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 2.04m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 2.05m);
			invoiceLine3.JI_ImportDutyPaid = 3.01m;
			invoiceLine3.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 3.02m);
			invoiceLine3.JI_ImportVATPaid = 3.03m;
			invoiceLine3.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 3.04m);
			invoiceLine3.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 3.05m);
			var tester = new DA63DocumentWrapper(entryHeader);
			AssertEquals(28.48m, tester.TotalAmountClaimed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var newFactory = new BusinessObjectFactory();
			var testHelper = new ZAUniversalReferenceTestDataHelper(newFactory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "XX", "YY", "5", "XXYY5", "EXP");
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "XX", "ZZ", "4", "XXYY5", "EXP");
			newFactory.Save();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "YY";
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "ZZ";
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_Procedure = "";
			declaration.DoMerge();
			entryHeader = declaration.ActiveEntryHeaders[0];
			entryLine1 = invoiceLine1.CusEntryLine;
			entryLine2 = invoiceLine2.CusEntryLine;
			entryLine3 = invoiceLine3.CusEntryLine;
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine1;
		protected JobComInvoiceLine invoiceLine2;
		protected JobComInvoiceLine invoiceLine3;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine1;
		protected CusEntryLine entryLine2;
		protected CusEntryLine entryLine3;
	}
}
