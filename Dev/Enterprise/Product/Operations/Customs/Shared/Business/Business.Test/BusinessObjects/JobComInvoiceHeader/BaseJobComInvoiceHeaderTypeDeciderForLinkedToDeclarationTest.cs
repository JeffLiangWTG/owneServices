using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvoiceHeaderTypeDeciderForLinkedToDeclarationTest : BaseJobComInvoiceHeaderTypeDeciderAbstractTest
	{
		public void TestLoadTypeForEMCS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var expectedType = ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceHeader>();

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
				declaration.FillWithValidTestData();

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.FillWithValidTestData();

				AssertEquals(expectedType, invoiceHeader.GetType());

				Factory.Save();

				var newInvoiceHeader = NewFactory().Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
				AssertEquals(expectedType, newInvoiceHeader.GetType());
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var invoiceHeader = bizO as BaseJobComInvoiceHeader;
			if (invoiceHeader != null)
			{
				invoiceHeader.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			return declaration.Invoices.AddNew();
		}
	}
}
