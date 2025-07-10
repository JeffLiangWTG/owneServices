using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderRefs))]
	sealed class JobComInvoiceHeaderRefsTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				base.TestSaveAndDeleteBusinessObject();
			}
		}

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
				AssertEquals("From CurrentCompany", "ER", (Factory.New<JobComInvoiceHeaderRefs>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceHeaderRefs = invoiceHeader.InvoiceHeaderRefs.AddNew();
				AssertEquals("From InvoiceHeader", "NZ", (invoiceHeaderRefs as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return HeaderRefs;
		}

		JobComInvoiceHeaderRefs HeaderRefs
		{
			get
			{
				if (headerRefs == null)
				{
					BaseJobComInvoiceHeader header = Factory.New<BaseJobComInvoiceHeader>();
					headerRefs = header.InvoiceHeaderRefs.AddNew();
				}
				return headerRefs;
			}
		}
		JobComInvoiceHeaderRefs headerRefs;
	}
}
