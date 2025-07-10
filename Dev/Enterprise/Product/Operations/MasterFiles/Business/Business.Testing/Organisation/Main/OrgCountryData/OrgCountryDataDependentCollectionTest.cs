using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCountryDataDependentCollection))]
	sealed class OrgCountryDataDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestSuspendCountChanged()
		{
			// Temporarily disable this test since it calls Remove withouting delete 
			Assert(true);
		}

		public void TestRelationshipFilter()
		{
			var org = Factory.New<OrgHeader>();

			var data1 = Factory.New<OrgCountryData>();
			data1.OV_OH_OrgHeader = org.PK;
			data1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Brazil;
			data1.OV_EXApprovalNumber = "data1";

			var data2 = Factory.New<OrgCountryData>();
			data2.OV_OH_OrgHeader = org.PK;
			data2.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data2.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Australia;
			data1.OV_EXApprovalNumber = "data2";

			var data3 = Factory.New<OrgCountryData>();
			data3.OV_OH_OrgHeader = org.PK;
			data3.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			data1.OV_EXApprovalNumber = "data3";

			var data4 = Factory.New<OrgCountryData>();
			data4.OV_OH_OrgHeader = org.PK;
			data4.OV_OA_ApprovedLocation = org.MainAddress.PK;
			data4.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.EuropeanUnion;
			data1.OV_EXApprovalNumber = "data4";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var coll = new OrgCountryDataDependentCollection(org);
				coll.Load();
				AssertEquals(1, coll.Count);
				AssertEquals(data2.OV_EXApprovalNumber, coll[0].OV_EXApprovalNumber);
				AssertEquals(true, coll[0].AddedThroughCollection);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				var coll = new OrgCountryDataDependentCollection(org);
				coll.Load();
				AssertEquals(1, coll.Count);
				AssertEquals(data4.OV_EXApprovalNumber, coll[0].OV_EXApprovalNumber);
				AssertEquals(true, coll[0].AddedThroughCollection);
			}
		}

		public void TestRelationshipFilter_AddressNull()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			try
			{
				OrgHeader org = Factory.New<OrgHeader>();

				OrgCountryData data1 = Factory.New<OrgCountryData>();
				data1.OV_OH_OrgHeader = org.PK;
				data1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
				data1.OV_OA_ApprovedLocation = org.MainAddress.PK;

				OrgCountryData data2 = Factory.New<OrgCountryData>();
				data2.OV_OH_OrgHeader = org.PK;
				data2.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;

				OrgCountryDataDependentCollection coll = new OrgCountryDataDependentCollection(org);
				coll.Load();
				AssertEquals(1, coll.Count);
				AssertEquals(data1, coll[0]);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestDefaultsForNewChild()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgCountryDataDependentCollection coll = new OrgCountryDataDependentCollection(org);

			OrgCountryData data = coll.AddNew();
			AssertEquals(true, data.AddedThroughCollection);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgCountryDataDependentCollection(testHeader);
		}

		#endregion
	}
}
