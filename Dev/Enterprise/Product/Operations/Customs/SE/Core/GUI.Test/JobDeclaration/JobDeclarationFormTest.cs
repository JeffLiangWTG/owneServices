using Enterprise.Customs.SE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.SE.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.CusContainers.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			declaration.Bills.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			return declaration;
		}
	}
}
