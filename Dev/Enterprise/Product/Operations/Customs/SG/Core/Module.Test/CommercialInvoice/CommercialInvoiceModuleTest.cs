using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceModuleForTest))]
	sealed class CommercialInvoiceModuleTest : Customs.Module.Testing.CommercialInvoiceModuleTest
	{
		public void TestGetNewController()
		{
			using (var module = new CommercialInvoiceModuleForTest())
			{
				var header = Factory.New<JobComInvoiceHeader>();
				var controller = module.GetNewController(header);
				AssertType<CommercialInvoiceController>(controller);
			}
		}

		sealed class CommercialInvoiceModuleForTest : CommercialInvoiceModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject) => base.GetNewController(selectedBusinessObject);
		}
	}
}
