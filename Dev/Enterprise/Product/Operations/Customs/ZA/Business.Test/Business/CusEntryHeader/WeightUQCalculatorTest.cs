using System;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		public void TestWeightForSingleEntryShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_Weight = 500m;
			invoiceHeader.JZ_WeightUQ = "KG";
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 750m;
			invoiceLine.JI_CEI = testInstruction.PK;
			new LineMerger(declaration).DoMerge();
			var calculator = new WeightUQCalculator(declaration.CustomsEntryHeaders[0]);
			AssertEquals(750m, calculator.Weight);
			AssertEquals("KG", calculator.UQ);
		}

		public void TestWeightForMultipleEntryShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "12";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_Weight = 500m;
			invoiceHeader.JZ_WeightUQ = "KG";
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 500m;
			invoiceLine1.JI_Weight = 100m;
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_Weight = 250m;
			invoiceLine2.JI_CEI = testInstruction2.PK;
			new LineMerger(declaration).DoMerge();
			var calculator0 = new WeightUQCalculator(declaration.CustomsEntryHeaders[0]);
			AssertEquals(100m, calculator0.Weight);
			AssertEquals("KG", calculator0.UQ);
			var calculator1 = new WeightUQCalculator(declaration.CustomsEntryHeaders[1]);
			AssertEquals(250m, calculator1.Weight);
			AssertEquals("KG", calculator1.UQ);
		}

		public void TestGrossWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "12";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInstruction.PK;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Weight = 100;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction.PK;
			invoiceLine1.JI_WeightUQ = "LI";
			invoiceLine1.JI_Weight = 100;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_Weight = 200;
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction2.PK;
			invoiceLine3.JI_WeightUQ = "LT";
			invoiceLine3.JI_Weight = 5;
			new LineMerger(declaration).DoMerge();
			var calculator = new WeightUQCalculator(declaration.CustomsEntryHeaders[0]);
			AssertEquals("declaration.TotalGrossWeight.Unit", "", calculator.GrossWeight.Unit);
			AssertEquals("declaration.TotaGrossWeight.Amount", 0m, calculator.GrossWeight.Amount);
			new LineMerger(declaration).DoMerge();
			var calculator1 = new WeightUQCalculator(declaration.CustomsEntryHeaders[1]);
			AssertEquals("declaration.TotalGrossWeight.Unit", "KG", calculator1.GrossWeight.Unit);
			AssertEquals("declaration.TotaGrossWeight.Amount", 201.866209m, calculator1.GrossWeight.Amount);
		}

		protected override Type CusEntryHeaderType => typeof(CusEntryHeader);

		protected override Type JobComInvoiceHeaderType => typeof(JobComInvoiceHeader);
	}
}
