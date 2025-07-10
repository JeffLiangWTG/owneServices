using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationGroup))]
	sealed class GlbAccreditationGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAccreditationPivotCollection()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			var pivot = group.AccreditationPivotCollection.AddNew();
			pivot.HAP_HAC = accreditation.PK;

			AssertEquals("HAP_HAG should be assigned the parent group's PK", group.PK, pivot.HAP_HAG);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var groupInNewFactory = newFactory.Load<GlbAccreditationGroup>(group.PK);

			AssertEquals("Should have loaded existing child accreditation", 1, groupInNewFactory.AccreditationPivotCollection.Count);
			AssertEquals("Should have loaded existing child accreditation", accreditation.PK, groupInNewFactory.AccreditationPivotCollection[0].HAP_HAC);
		}

		public void TestAccreditations()
		{
			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			var collection = accreditationGroup.Accreditations;
			collection.Load();
			AssertEquals(0, collection.Count);

			var parentAccreditation1 = collection.AddNew();
			parentAccreditation1.HAC_Code = "AAA";

			AssertEquals("Should have added parentAccreditation1", 1, collection.Count);
			AssertEquals("Should have added parentAccreditation1 to requirement pivot collection", 1, accreditationGroup.AccreditationPivotCollection.Count);
			AssertEquals("Pivot should be for the accreditation", accreditationGroup.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAG);
			AssertEquals("Pivot parent should be parentAccreditation1", parentAccreditation1.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAC);

			var parentAccreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			parentAccreditation2.HAC_Code = "BBB";
			collection.Add(parentAccreditation2);

			AssertEquals("Should have added parentAccreditation2", 2, collection.Count);
			AssertEquals("Should have added parentAccreditation2 to requirement pivot collection", 2, accreditationGroup.AccreditationPivotCollection.Count);

			collection.Remove(parentAccreditation2);

			AssertEquals("Should have deleted pivot for parentAccreditation2", 1, collection.Count);
			AssertEquals("Should have deleted pivot for parentAccreditation2", 1, accreditationGroup.AccreditationPivotCollection.Count);
			AssertEquals("Should have deleted pivot for parentAccreditation2", parentAccreditation1.PK, accreditationGroup.AccreditationPivotCollection[0].HAP_HAC);

			Factory.Save();
			AssertEquals(true, parentAccreditation1.IsInDatabase);
			AssertEquals(true, parentAccreditation2.IsInDatabase);

			collection.Load();
			AssertEquals("Filter should find parentAccreditation1", 1, collection.Count);
			AssertEquals("Filter should find parentAccreditation1", parentAccreditation1.PK, collection[0].PK);
		}

		public void TestAccreditationsShouldIncludeTheirRefreshers()
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
			var collection = accreditationGroup.Accreditations;
			AssertEquals("Collection should be loaded. It should include both main accreditation and refresher", 2, collection.Count);
		}
	}
}
