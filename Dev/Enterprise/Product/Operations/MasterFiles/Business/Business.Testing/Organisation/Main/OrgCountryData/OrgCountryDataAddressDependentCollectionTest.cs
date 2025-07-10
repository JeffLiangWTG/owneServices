using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataAddressDependentCollection))]
	sealed class OrgCountryDataAddressDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCountryDataAddressDependentCollection>
	{
		public void TestRelationshipFilter()
		{
			var org = Factory.New<OrgHeader>();

			var data1 = Factory.New<OrgCountryData>();
			data1.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Brazil;
			data1.OV_EXApprovalNumber = "data1";

			var data2 = Factory.New<OrgCountryData>();
			data2.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data2.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Australia;
			data1.OV_EXApprovalNumber = "data2";

			var data3 = Factory.New<OrgCountryData>();
			data3.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data3.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			data1.OV_EXApprovalNumber = "data3";

			var data4 = Factory.New<OrgCountryData>();
			data4.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data4.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.EuropeanUnion;
			data1.OV_EXApprovalNumber = "data4";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var coll = new OrgCountryDataAddressDependentCollection(org.MainAddress);
				AssertEquals(1, coll.Count);
				AssertEquals(data2.OV_EXApprovalNumber, coll[0].OV_EXApprovalNumber);
				AssertEquals(true, coll[0].AddedThroughCollection);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var coll = new OrgCountryDataAddressDependentCollection(org.MainAddress);
				AssertEquals(1, coll.Count);
				AssertEquals(data4.OV_EXApprovalNumber, coll[0].OV_EXApprovalNumber);
				AssertEquals(true, coll[0].AddedThroughCollection);
			}
		}

		public void TestDefaultsForNewChild()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgCountryDataAddressDependentCollection coll = new OrgCountryDataAddressDependentCollection(org.MainAddress);

			OrgCountryData data = coll.AddNew();
			AssertEquals(true, data.AddedThroughCollection);
		}

		protected override OrgCountryDataAddressDependentCollection GetCollectionToTest()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgCountryDataAddressDependentCollection(testHeader.MainAddress);
		}
	}
}
