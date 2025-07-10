using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class ChildBillCollectionTest<T> : BusinessObjectCollectionViewTestCase<T> where T : ChildBillCollection<Bill, BaseJobDeclaration>
	{
		public void TestBillTypeForChildBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MB1234567";
			Bill masterBill = declaration.PrimaryMasterBill;
			AssertEquals(BillTypeList.Codes.MasterBill, masterBill.CU_BillType);
			Bill houseBill = masterBill.ChildBills.AddNew();
			AssertEquals(BillTypeList.Codes.HouseBill, houseBill.CU_BillType);
			Bill subHouseBill = houseBill.ChildBills.AddNew();
			AssertEquals(BillTypeList.Codes.SubHouseBill, subHouseBill.CU_BillType);
		}

		public void TestRemoveReferenceFromChildren()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();

			Bill bill2 = declaration.Bills.AddNew();
			AssertEquals("Bill2 linked to MB bill1", bill1.PK, bill2.CU_CU_ParentBill);

			Bill bill3 = declaration.Bills.AddNew();
			AssertEquals("Bill3 linked to MB bill1", bill1.PK, bill3.CU_CU_ParentBill);
			AssertEquals("MB bill1 has 2 child bills", 2, bill1.ChildBills.Count);

			bill1.ChildBills.RemoveReferenceFromChildren();
			AssertEquals("No more children", 0, bill1.ChildBills.Count);

			declaration.Bills.RemoveAndDelete(bill2);
			AssertEquals("bill3 does not point to bill2 any more", ZGuid.Empty, bill3.CU_CU_ParentBill);
		}

		public void TestHasAGUIPresentationChild()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = bill1.PK;

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_CU_ParentBill = bill1.PK;

			AssertEquals(true, bill1.ChildBills.HasAGUIPresentationChild);

			bill2.CU_GUIPresentationRecord = false;
			AssertEquals(false, bill1.ChildBills.HasAGUIPresentationChild);
		}

		public void TestSetCollectionRelationships()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = bill1.ChildBills.AddNew();
			AssertNotNull(bill2.Declaration);
			AssertEquals(bill1, bill2.ParentBill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Bill result = Factory.New<Bill>();
			result.CU_CU_ParentBill = Bill.PK;
			result.CU_JE = Bill.Declaration.PK;
			return result;
		}

		protected Bill Bill
		{
			get
			{
				if (fBill == null)
				{
					BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
					fBill = declaration.Bills.AddNew();
				}
				return fBill;
			}
		}
		Bill fBill;
	}
}
