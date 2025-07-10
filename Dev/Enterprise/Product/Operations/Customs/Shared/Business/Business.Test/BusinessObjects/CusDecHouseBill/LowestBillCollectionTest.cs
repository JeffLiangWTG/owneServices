using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(LowestBillCollection<Bill, BaseJobDeclaration>))]
	sealed class LowestBillCollectionTest : BusinessObjectCollectionViewTestCase<LowestBillCollection<Bill, BaseJobDeclaration>>
	{
		public void TestRebuid()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			AssertEquals("ChildBills", 0, bill.ChildBills.Count);
			AssertEquals("Rebuild result", 1, declaration.LowestBills.Count);

			Bill bill2 = declaration.Bills.AddNew();
			AssertEquals("HouseBill", BillTypeList.Codes.HouseBill, bill2.CU_BillType);
			AssertEquals("ParentMasterBill", bill, bill2.ParentBill);
			AssertEquals("bill.ChildBills", 1, bill.ChildBills.Count);
			AssertEquals("bill2.ChildBills", 0, bill2.ChildBills.Count);
			AssertEquals("Rebuild result", 1, declaration.LowestBills.Count);

			bill2.CU_CU_ParentBill = ZGuid.Empty;
			AssertEquals("ChildBills", 0, bill.ChildBills.Count);
			AssertEquals("Rebuild result", 2, declaration.LowestBills.Count);
		}

		protected override LowestBillCollection<Bill, BaseJobDeclaration> GetCollectionToTest()
		{
			return new LowestBillCollection<Bill, BaseJobDeclaration>(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Bill result = Factory.New<Bill>();
			result.CU_JE = Declaration.PK;
			return result;
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
	}
}
