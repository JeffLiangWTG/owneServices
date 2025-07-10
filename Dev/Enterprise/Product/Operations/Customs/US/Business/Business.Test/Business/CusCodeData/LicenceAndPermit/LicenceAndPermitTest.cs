using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LicenceAndPermit))]
	sealed class LicenceAndPermitTest : Customs.Business.Testing.CusCodeDataTest<LicenceAndPermit>
	{
		public void TestMaxLengthOfData()
		{
			var licence = InvoiceLine.LicenceAndPermits.AddNew();
			AssertEquals("Messages allow only 10", 10, licence.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => InvoiceLine.LicenceAndPermits.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<LicenceAndPermit>();

		protected override IEnumerable<LicenceAndPermit> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<LicenceAndPermit>();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.LicenceAndPermits.Add(result);
			yield return result;
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.Invoices.AddNew();
					invoiceLine = declaration.InvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
