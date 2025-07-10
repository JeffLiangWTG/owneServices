using CargoWise.EntityFramework;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class JobDeclarationModulePerformanceTest : ZModulePerformanceTest
	{
		protected override ModuleIdentifier GetModuleID() => CustomsModuleIDs.JobDeclaration;
		protected override int MaximumDBHitsForPerformSearch => 200;
		protected override void PrepareModuleForPerformanceTest(ZFilterGridModule module)
		{
			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 100; i++)
			{
				var declaration = factory.New<JobDeclaration>();
				var importer = OrgHeader.New(factory);
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				var supplier = OrgHeader.New(factory);
				supplier.FillWithValidTestData();
				declaration.JE_OH_Supplier = supplier.PK;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			}

			factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}
	}
}
