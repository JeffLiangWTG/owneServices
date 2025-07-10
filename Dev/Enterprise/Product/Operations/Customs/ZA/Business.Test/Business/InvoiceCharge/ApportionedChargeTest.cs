using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ApportionedCharge))]
	sealed class ApportionedChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			return invoice.GroupCharges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
