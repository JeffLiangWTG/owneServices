using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		declaration.CusContainers.AddNew();
		declaration.AdditionalInfos.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceHeader.AdditionalInfos.AddNew();
		invoiceLine.AdditionalInfos.AddNew();
		declaration.Bills.AddNew();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.MergedLines.AddNew();
		return declaration;
	}
}
