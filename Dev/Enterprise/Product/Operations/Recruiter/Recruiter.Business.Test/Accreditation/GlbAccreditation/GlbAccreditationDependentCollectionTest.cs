using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationDependentCollection))]
	sealed class GlbAccreditationDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbAccreditationDependentCollection(Factory.New<GlbAccreditation>(), new ZQuery());
		}

		public void TestAccreditationDependentCollection()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();

			var query = new ZDBOnlyQuery(typeof(GlbAccreditation));

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationRequirementPivot), GlbAccreditationRequirementPivotSchema.HAR_HAC_Parent);
			pivotSubQuery.AddToFilter(GlbAccreditationRequirementPivotSchema.HAR_HAC, accreditation.PK);

			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var collection = new GlbAccreditationDependentCollection(accreditation, query);
			collection.Load();
			AssertEquals(0, collection.Count);

			var parentAccreditation1 = collection.AddNew();
			parentAccreditation1.HAC_Code = "AAA";

			AssertEquals("Should have added parentAccreditation1", 1, collection.Count);
			AssertEquals("Should have added parentAccreditation1 to requirement pivot collection", 1, accreditation.RequirementPivotCollection.Count);
			AssertEquals("Pivot should be for the accreditation", accreditation.PK, accreditation.RequirementPivotCollection[0].HAR_HAC);
			AssertEquals("Pivot parent should be parentAccreditation1", parentAccreditation1.PK, accreditation.RequirementPivotCollection[0].HAR_HAC_Parent);

			var parentAccreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			parentAccreditation2.HAC_Code = "BBB";
			collection.Add(parentAccreditation2);

			AssertEquals("Should have added parentAccreditation2", 2, collection.Count);
			AssertEquals("Should have added parentAccreditation2 to requirement pivot collection", 2, accreditation.RequirementPivotCollection.Count);

			collection.Remove(parentAccreditation2);

			AssertEquals("Should have deleted pivot for parentAccreditation2", 1, collection.Count);
			AssertEquals("Should have deleted pivot for parentAccreditation2", 1, accreditation.RequirementPivotCollection.Count);
			AssertEquals("Should have deleted pivot for parentAccreditation2", parentAccreditation1.PK, accreditation.RequirementPivotCollection[0].HAR_HAC_Parent);

			Factory.Save();
			AssertEquals(true, parentAccreditation1.IsInDatabase);
			AssertEquals(true, parentAccreditation2.IsInDatabase);

			collection.Load();
			AssertEquals("Filter should find parentAccreditation1", 1, collection.Count);
			AssertEquals("Filter should find parentAccreditation1", parentAccreditation1.PK, collection[0].PK);
		}

		public void TestAccreditationGroupDependentCollection()
		{
			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();

			var query = new ZDBOnlyQuery(typeof(GlbAccreditation));

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationGroupPivot), GlbAccreditationGroupPivotSchema.HAP_HAC);
			pivotSubQuery.AddToFilter(GlbAccreditationGroupPivotSchema.HAP_HAG, accreditationGroup.PK);

			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var collection = new GlbAccreditationDependentCollection(accreditationGroup, query);
			collection.Load();
			AssertEquals(0, collection.Count);

			var memberAccreditation1 = collection.AddNew();
			memberAccreditation1.HAC_Code = "AAA";

			AssertEquals("Should have added memberAccreditation1", 1, collection.Count);
			AssertEquals("Should have added memberAccreditation1 to pivot collection", 1, accreditationGroup.AccreditationPivotCollection.Count);
			AssertEquals("Pivot should be for the group", accreditationGroup.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAG);
			AssertEquals("Pivot parent should be memberAccreditation1", memberAccreditation1.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAC);

			var memberAccreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			memberAccreditation2.HAC_Code = "BBB";
			collection.Add(memberAccreditation2);

			AssertEquals("Should have added memberAccreditation2", 2, collection.Count);
			AssertEquals("Should have added memberAccreditation2 to pivot collection", 2, accreditationGroup.AccreditationPivotCollection.Count);

			collection.Remove(memberAccreditation2);

			AssertEquals("Should have deleted pivot for memberAccreditation2", 1, collection.Count);
			AssertEquals("Should have deleted pivot for memberAccreditation2", 1, accreditationGroup.AccreditationPivotCollection.Count);
			AssertEquals("Should have deleted pivot for memberAccreditation2", memberAccreditation1.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAC);

			Factory.Save();
			AssertEquals(true, memberAccreditation1.IsInDatabase);
			AssertEquals(true, memberAccreditation2.IsInDatabase);

			collection.Load();
			AssertEquals("Filter should find memberAccreditation1", 1, collection.Count);
			AssertEquals("Filter should find memberAccreditation1", memberAccreditation1.PK, collection[0].PK);
		}

		public void TestAccreditationGroupRefresher()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("BBC", (NoResString)"BBC");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();

			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "BBB";
			accreditation.HAC_CertificateCode = "BBC";
			var accreditationRefresher = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditationRefresher.HAC_Code = "RBB";
			accreditationRefresher.HAC_CertificateCode = "BBC";
			accreditationRefresher.HAC_IsRefresher = true;
			AssertEquals("Precondition", accreditationRefresher, accreditation.RefresherAccreditation);

			var query = new ZDBOnlyQuery(typeof(GlbAccreditation));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationGroupPivot), GlbAccreditationGroupPivotSchema.HAP_HAC);
			pivotSubQuery.AddToFilter(GlbAccreditationGroupPivotSchema.HAP_HAG, accreditationGroup.PK);
			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var collection = new GlbAccreditationDependentCollection(accreditationGroup, query);
			collection.Load();
			AssertEquals(0, collection.Count);

			collection.Add(accreditation);

			AssertEquals("Should have added accreditation as well as its refresher", 2, collection.Count);
			AssertEquals("Should only have added the base accreditation to the pivot collection", 1, accreditationGroup.AccreditationPivotCollection.Count);
			AssertEquals("Pivot should be for the group", accreditationGroup.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAG);
			AssertEquals("Pivot subject should be accreditation", accreditation.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAC);

			collection.Remove(accreditation);
			AssertEquals("Should have removed both accreditation and its refresher", 0, collection.Count);
			AssertEquals("Should have removed accreditation from the pivot collection", 0, accreditationGroup.AccreditationPivotCollection.Count);
		}

		public void TestLoadShouldLoadRefresher()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("BBC", (NoResString)"BBC");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();

			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "BBB";
			accreditation.HAC_CertificateCode = "BBC";
			var accreditationRefresher = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditationRefresher.HAC_Code = "RBB";
			accreditationRefresher.HAC_CertificateCode = "BBC";
			accreditationRefresher.HAC_IsRefresher = true;
			accreditationRefresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			Factory.Save();
			AssertEquals("Precondition", true, accreditation.IsInDatabase);
			AssertEquals("Precondition", true, accreditationRefresher.IsInDatabase);
			AssertEquals("Precondition", accreditationRefresher, accreditation.RefresherAccreditation);

			var pivot = accreditationGroup.AccreditationPivotCollection.AddNew();
			pivot.HAP_HAC = accreditation.PK;
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(GlbAccreditation));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationGroupPivot), GlbAccreditationGroupPivotSchema.HAP_HAC);
			pivotSubQuery.AddToFilter(GlbAccreditationGroupPivotSchema.HAP_HAG, accreditationGroup.PK);
			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var collection = new GlbAccreditationDependentCollection(accreditationGroup, query);
			collection.Load();
			AssertEquals("Load should also load refresher", 2, collection.Count);

			collection.Load(new ZQuery());
			AssertEquals("Load should also load refresher", 2, collection.Count);

			collection.Reload(true);
			AssertEquals("Load should also load refresher", 2, collection.Count);
		}
	}
}
