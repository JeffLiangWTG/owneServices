using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseInvoiceLineApportionedCharge))]
	public class BaseInvoiceLineApportionedChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDeciderForInvoiceLineApportionedCharge()
		{
			AssertEquals("Type decider type", typeof(InvoiceLineApportionedChargeTypeDecider), BaseInvoiceLineApportionedCharge.TypeDecider.GetType());
		}

		public void TestSetDefaultValues()
		{
			BaseInvoiceLineApportionedCharge apportionedCharge = Factory.New<BaseInvoiceLineApportionedCharge>();
			AssertEquals("IsApportioned", true, apportionedCharge.J7_IsApportionedCharge);

			apportionedCharge.Parent = invoiceLine;
			AssertEquals("ParentTableCode", "JI", apportionedCharge.J7_ParentTableCode);
		}

		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;
		BaseInvoiceLineApportionedCharge testCharge;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetupAllTestObjects();
			return testCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return base.Factory.New(GetExpectedBusinessObjectType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupAllTestObjects();
		}

		protected virtual void SetupAllTestObjects()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			testCharge = invoiceLine.ApportionedCharges.AddNew();
		}
	}
}
