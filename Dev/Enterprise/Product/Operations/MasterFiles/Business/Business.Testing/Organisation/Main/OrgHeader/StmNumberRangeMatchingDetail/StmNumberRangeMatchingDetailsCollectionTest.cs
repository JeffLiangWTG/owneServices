using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmNumberRangeMatchingDetailsCollection))]
	sealed class StmNumberRangeMatchingDetailsCollectionTest : ActiveBusinessObjectCollectionTestCase<StmNumberRangeMatchingDetailsCollection>
	{
		#region TestRelationshipFilter

		public void TestRelationshipFilter()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var sameOrg1 = Factory.New<StmNumberRangeMatchingDetail>();
			sameOrg1.NRM_OwnerId = header.PK;
			var sameOrg2 = Factory.New<StmNumberRangeMatchingDetail>();
			sameOrg2.NRM_OwnerId = header.PK;
			var differentOrg = Factory.New<StmNumberRangeMatchingDetail>();
			differentOrg.NRM_OwnerId = ZGuid.NewZGuid();

			var collection = new StmNumberRangeMatchingDetailsCollection(header);

			AssertContainsExactElementsInAnyOrder(new StmNumberRangeMatchingDetail[] { sameOrg1, sameOrg2 }, collection);

			var sameOrg4 = Factory.New<StmNumberRangeMatchingDetail>();
			sameOrg4.NRM_OwnerId = header.PK;

			AssertContainsExactElementsInAnyOrder(new StmNumberRangeMatchingDetail[] { sameOrg1, sameOrg2, sameOrg4 }, collection);
		}

		#endregion

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new StmNumberRangeMatchingDetailsCollection(header);
			var item = collection.AddNew();

			AssertEquals(item.NRM_OwnerId, header.PK);
			AssertEquals(item.NRM_OwnerTableCode, OrgHeaderSchema.Constants.Prefix);
			AssertEquals(item.NRM_RangeType, OrgConstants.NumberFountains.Code.TransportReferenceNumbers);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var certificate = staff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
			certificate.XZ_RefNumber = "1234";

			collection = new StmNumberRangeMatchingDetailsCollection(staff);
			item = collection.AddNew();

			AssertEquals(item.NRM_OwnerId, staff.PK);
			AssertEquals(item.NRM_OwnerTableCode, GlbStaffSchema.Constants.Prefix);
			AssertEquals(item.NRM_RangeType, OrgConstants.NumberFountains.Code.PatentNumber);
			AssertEquals(item.PatentNumber, certificate.XZ_RefNumber);

			collection.DeleteAll();
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.ACE;

			item = collection.AddNew();
			AssertNotEquals(item.PatentNumber, certificate.XZ_RefNumber);

			collection.DeleteAll();
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Brazil;

			item = collection.AddNew();
			AssertNotEquals(item.PatentNumber, certificate.XZ_RefNumber);
		}

		#endregion

		#region Implementation

		protected override StmNumberRangeMatchingDetailsCollection GetCollectionToTest()
		{
			return new StmNumberRangeMatchingDetailsCollection(Factory.NewWithValidTestData<OrgHeader>());
		}

		#endregion
	}
}
