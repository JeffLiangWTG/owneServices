using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvoiceHeaderTypeDeciderStandaloneInvoiceTest : BaseJobComInvoiceHeaderTypeDeciderAbstractTest
	{
		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_Name = "Dummy Company";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "Z1Z";
			Factory.Save();

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_GB = branch.PK;
			return invoice;
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var invoice = bizO as BaseJobComInvoiceHeader;
			if (invoice != null)
			{
				invoice.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}
	}
}
