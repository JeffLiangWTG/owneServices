using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	[TestedType(typeof(DeclarationBasherForm))]
	sealed class ImportDeclarationBasherFormTest : DeclarationBasherFormTest
	{
		public override ZString MessageTypeForFormBashing
		{
			get
			{
				return JobMessageTypeList.Codes.Import;
			}
		}

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}
	}
}
