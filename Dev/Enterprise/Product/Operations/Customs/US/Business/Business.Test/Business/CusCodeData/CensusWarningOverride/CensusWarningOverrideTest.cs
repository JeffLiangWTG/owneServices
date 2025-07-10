using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CensusWarningOverride))]
	sealed class CensusWarningOverrideTest : Customs.Business.Testing.CusCodeDataTest<CensusWarningOverride>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			return invoiceLine.CensusWarningOverrides.AddNew();
		}

		protected override IEnumerable<CensusWarningOverride> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.CensusWarningOverrides.AddNew();
			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			yield return product.PivotsForBinding.AddNew().CensusWarningOverrides.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.CensusWarningOverrides.AddNew();
		}
	}
}
