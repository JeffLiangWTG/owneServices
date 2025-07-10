using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(SecondaryNotifyPartyCollection))]
	class SecondaryNotifyPartyCollectionTest : CargoWise.EntityFramework.Testing.ActiveBusinessObjectCollectionTestCase<SecondaryNotifyPartyCollection>
	{
		public void TestSetRelationshipDefaults()
		{
			var snp = SecondaryNotifyParties.AddNew();
			AssertEquals(Bill.PK, snp.CY_ParentID);
			AssertEquals(CusInBondBillSchema.Constants.Prefix, snp.CY_ParentTableCode);
		}

		public void TestGetNonEmptySNPInSortOrder()
		{
			var snp1 = SecondaryNotifyParties.AddNew();
			snp1.CY_Data = "OOT5";
			var snp2 = SecondaryNotifyParties.AddNew();
			var snp3 = SecondaryNotifyParties.AddNew();
			snp3.CY_Data = "OOT3";
			var snp4 = SecondaryNotifyParties.AddNew();
			snp4.CY_Data = "OOT4";
			snp3.CY_Order = 1;
			snp2.CY_Order = 2;
			snp1.CY_Order = 3;
			snp4.CY_Order = 4;

			var snp5 = new BusinessObjectFactory().New<SecondaryNotifyParty>();
			snp5.CY_Order = 3;
			snp5.CY_Data = "OOT1";
			snp5.CY_ParentID = Bill.PK;
			snp5.CY_ParentTableCode = Bill.TablePrefix;
			snp5.Factory.Save();
			snp5 = Factory.Load<SecondaryNotifyParty>(snp5.PK);
			AssertEquals("Precondition", 5, SecondaryNotifyParties.Count);
			AssertEquals((short)1, snp3.CY_Order);
			AssertEquals((short)2, snp2.CY_Order);
			AssertEquals((short)3, snp1.CY_Order);
			AssertEquals((short)3, snp5.CY_Order);
			AssertEquals((short)4, snp4.CY_Order);

			var snps = SecondaryNotifyParties.GetNonEmptySNPInSortOrder();
			AssertEquals(4, snps.Length);
			AssertEquals("SNP3", snp3, snps[0]);
			AssertEquals("SNP1", snp1, snps[2]);
			AssertEquals("SNP5", snp5, snps[1]);
			AssertEquals("SNP4", snp4, snps[3]);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SecondaryNotifyParty>();
			result.CY_ParentID = Bill.PK;
			result.CY_ParentTableCode = Bill.TablePrefix;
			return result;
		}

		CusInBondBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.New<CusInBondHeader>();
					AssertNotNull(header.MovementHeader);
					bill = header.Bills.AddNew();
					AssertNotNull(bill.MovementDetail);
				}
				return bill;
			}
		}
		CusInBondBill bill;

		SecondaryNotifyPartyCollection SecondaryNotifyParties
		{
			get { return Bill.SecondaryNotifyParties; }
		}

		protected override SecondaryNotifyPartyCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertNotNull(header.MovementHeader);
			bill = header.Bills.AddNew();
			AssertNotNull(bill.MovementDetail);

			return new SecondaryNotifyPartyCollection(bill);
		}

		#endregion
	}
}
