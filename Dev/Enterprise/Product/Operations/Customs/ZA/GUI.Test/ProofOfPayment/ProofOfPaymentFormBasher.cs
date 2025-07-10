using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(ProofOfPaymentForm))]
	sealed class ProofOfPaymentFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			var bo = Factory.New<CusEntryPayInfo>();
			bo.C9_CH = entryHeader.PK;
			Factory.Save();
			return new ProofOfPaymentForm(bo);
		}
	}
}
