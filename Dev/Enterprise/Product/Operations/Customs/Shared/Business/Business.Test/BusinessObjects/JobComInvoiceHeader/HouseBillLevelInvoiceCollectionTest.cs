
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class HouseBillLevelInvoiceCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderActiveCollection>
	{
		public void TestRemoveReferenceFromInvoicesWhenBillIsDeleted()
		{
			BaseJobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = HouseBill.PK;
			AssertEquals(true, HouseBill.Invoices.Contains(invoice));

			HouseBill.Delete();

			AssertEquals(ZGuid.Empty, invoice.JZ_CU_RelatedHouseBill);
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return HouseBill.Invoices;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = Declaration.PK;
			invoice.JZ_CU_RelatedHouseBill = HouseBill.PK;
			return invoice;
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		Bill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = Declaration.Bills.AddNew();
				}
				return fHouseBill;
			}
		}
		Bill fHouseBill;
	}
}
