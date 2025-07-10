using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(License))]
	public class LicenseTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<License>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return License;
		}

		protected override IEnumerable<License> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var laceyActLine = invoiceLine.LaceyActLines.AddNew();
			yield return laceyActLine.Licenses.AddNew();
		}

		License License
		{
			get
			{
				if (license == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var laceyActLine = invoiceLine.LaceyActLines.AddNew();
					license = laceyActLine.Licenses.AddNew();
				}
				return license;
			}
		}
		License license;
	}
}
