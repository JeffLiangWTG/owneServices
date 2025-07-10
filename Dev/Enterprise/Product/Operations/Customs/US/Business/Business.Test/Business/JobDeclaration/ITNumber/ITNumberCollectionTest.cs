using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ITNumberCollection))]
	public class ITNumberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ITNumberCollection>
	{
		public void TestPopulate()
		{
			ITNumberCollection collection = GetCollectionToTest();
			AssertEquals(4, collection.Count);
		}

		protected override ITNumberCollection GetCollectionToTest()
		{
			return new ITNumberCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ITNumber(Declaration.Bills[0].ITAndSplitDetails.AddNew());
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					Bill bill1 = Declaration.Bills.AddNew();
					bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
					bill1.CU_BillNum = "M1";

					bill1 = Declaration.Bills.AddNew();
					bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
					bill1.CU_BillNum = "H1";
					bill1.ITNumber = "5678";

					bill1 = Declaration.Bills.AddNew();
					bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
					bill1.CU_BillNum = "H2";
					bill1.ITNumber = "1234";
					bill1.ITAndSplitDetails.AddNew().US_ITNumber = "V12456789";
					bill1.ITAndSplitDetails.AddNew().US_ITNumber = "987654321";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
