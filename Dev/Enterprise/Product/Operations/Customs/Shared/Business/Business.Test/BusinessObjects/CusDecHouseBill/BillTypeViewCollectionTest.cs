using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BillTypeViewCollection<Bill, BaseJobDeclaration>))]
	sealed class BillTypeViewCollectionTest : BusinessObjectCollectionViewTestCase<BillTypeViewCollection<Bill, BaseJobDeclaration>>
	{
		public void TestFilterBy()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_CU_ParentBill = bill1.PK;

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill3.CU_CU_ParentBill = bill2.PK;

			declaration.JE_BillsFilterBy = BillFilterByList.Codes.All;
			AssertEquals(3, declaration.FilteredBills.Count);

			declaration.JE_BillsFilterBy = BillFilterByList.Codes.HouseBill;
			AssertEquals(1, declaration.FilteredBills.Count);
			AssertEquals(bill2, declaration.FilteredBills[0]);

			declaration.JE_BillsFilterBy = BillFilterByList.Codes.SubHouseBill;
			AssertEquals(1, declaration.FilteredBills.Count);
			AssertEquals(bill3, declaration.FilteredBills[0]);

			declaration.JE_BillsFilterBy = BillFilterByList.Codes.LowestBills;
			AssertEquals(1, declaration.FilteredBills.Count);
			AssertEquals(true, declaration.FilteredBills.Contains(bill3));
		}

		public void TestSetCollectionRelationship()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_BillsFilterBy = BillFilterByList.Codes.SubHouseBill;
			Bill bill = declaration.FilteredBills.AddNew();
			AssertEquals(declaration, bill.Declaration);
			AssertEquals(BillTypeList.Codes.SubHouseBill, bill.CU_BillType);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Bill result = Factory.New<Bill>();
			result.CU_JE = Declaration.PK;
			result.CU_BillType = BillTypeList.Codes.MasterBill;
			return result;
		}

		protected override BillTypeViewCollection<Bill, BaseJobDeclaration> GetCollectionToTest()
		{
			var result = new BillTypeViewCollection<Bill, BaseJobDeclaration>(Declaration);
			result.FilterBy = BillTypeList.Codes.MasterBill;
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
