using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(HouseBillRefNo))]
	sealed class HouseBillRefNoTest : Customs.Business.Testing.CusCodeDataTest<HouseBillRefNo>
	{
		public void TestIInBondBillReferenceNumberMembers()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			IInBondBillReferenceNumber billRef = refNo;
			refNo.CY_Code = ReferenceQualifierList.Codes.BM;
			refNo.CY_Data = "HELLO234";
			AssertEquals(ReferenceQualifierList.Codes.BM, billRef.Qualifier);
			AssertEquals("HELLO234", billRef.ReferenceIdentifier);
		}

		public void TestValidation()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			AssertEquals("Validation", typeof(HouseBillRefNoValidation), refNo.Validation.GetType());
		}

		public void TestLookups()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			AssertEquals("Validation", typeof(HouseBillRefNoLookups), refNo.Lookups.GetType());
		}

		public void TestDescription()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			refNo.CY_Code = ReferenceQualifierList.Codes.BN;
			AssertEquals(ReferenceQualifierList.Descriptions.BN, refNo.Description);
		}

		public void TestSetDefaultValues()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			AssertEquals(CusCodeDataTypeList.Codes.HouseBillRefNo, refNo.CY_Type);
		}

		public void TestParent()
		{
			HouseBillRefNo refNo = Factory.New<HouseBillRefNo>();
			refNo.CY_ParentID = Bill.PK;
			refNo.CY_ParentTableCode = "CU";
			AssertEquals(Bill, refNo.Parent);
		}

		protected override BusinessObject GetNewBusinessObject() => Bill.ReferenceNos.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().Bills.AddNew().ReferenceNos.AddNew();

		Bill bill;
		Bill Bill
		{
			get
			{
				if (bill == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					bill = declaration.Bills.AddNew();
				}

				return bill;
			}
		}
	}
}
