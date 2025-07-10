using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(InvoiceCharge))]
class InvoiceChargeTest : EnterpriseBusinessObjectTestCase
{
	public void TestTypeDecider()
	{
		Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.New<JobDeclaration>().Invoices.AddNew().Charges.AddNew();
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetNewBusinessObject();
	}
}
