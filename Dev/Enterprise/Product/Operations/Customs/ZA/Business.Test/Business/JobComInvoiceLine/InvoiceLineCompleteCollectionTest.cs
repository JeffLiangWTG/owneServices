using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestSetDefaultValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_PaymentNo = "123";
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "20";
			var testInvLine = invHeader.InvoiceLines.AddNew();
			AssertEquals("11", testInvLine.ProcedureCode);
			AssertEquals("1120", testInvLine.JI_Procedure);
			AssertEquals("JI_AdvancePaymentNo should default to the value of JZ_PaymentNo, in this case 123.", "123", testInvLine.JI_AdvancePaymentNo);
		}

		public void TestUpdateMaxCountValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(-1, declaration.InvoiceLines.MaxCount);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals(-1, declaration.InvoiceLines.MaxCount);
			using (ZACustomsRegistry.Instance.ExbondMaxNumberJobInvoiceLines.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2))
			{
				declaration.InvoiceLines.UpdateMaxCountValidation();
				AssertEquals(2, declaration.InvoiceLines.MaxCount);
			}
			declaration.InvoiceLines.UpdateMaxCountValidation();
			AssertEquals(-1, declaration.InvoiceLines.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection((JobDeclaration)Declaration);
	}
}
