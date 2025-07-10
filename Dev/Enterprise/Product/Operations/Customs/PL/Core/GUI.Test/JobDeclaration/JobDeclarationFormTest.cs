using CargoWise.Common.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		Customs.Business.BaseCusContainer container = declaration.CusContainers.AddNew();
		var invoiceheader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceheader.InvoiceLines.AddNew();
		Customs.Business.Bill bill = declaration.Bills.AddNew();
		EU.Business.Declaration.CusEntryHeader cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.MergedLines.AddNew();
		return declaration;
	}

	[CaptureMemoryDumpForDisposableLeak]
	public override void TestBashingForm() => base.TestBashingForm();
}
