using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineRefs))]
	sealed class JobComInvLineRefsBaseOnlyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<JobComInvLineRefs>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var invoiceLineRefs = invoiceLine.InvoiceLineRefs.AddNew();
				AssertEquals("From InvoiceLine", "NZ", (invoiceLineRefs as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return LineRefs;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return LineRefs;
		}

		JobComInvLineRefs LineRefs
		{
			get
			{
				if (lineRefs == null)
				{
					var invoice = Factory.New<BaseJobComInvoiceHeader>();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					lineRefs = invoiceLine.InvoiceLineRefs.AddNew();
				}
				return lineRefs;
			}
		}
		JobComInvLineRefs lineRefs;
	}
}
