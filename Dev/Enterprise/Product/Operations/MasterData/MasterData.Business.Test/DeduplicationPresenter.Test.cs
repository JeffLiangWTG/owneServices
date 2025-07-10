using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationNonPersistentBusinessObjectProviderTest : TestCaseWithFactory
	{
		public void TestComputeResult_ForOrganisation()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var glowCandidate1 = new DeduplicationOrgHeader(org);
			var glowCandidate2 = new DeduplicationOrgHeader(org2);
			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels();

			AssertEquals("Result is flattened based on child elements in scoringResult", 11, result.Count());
		}

		public void TestComputeResult_CorrectlyProcessTheMultiSourceForChildScoringResults()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var glowCandidate1 = new DeduplicationOrgHeader(org);
			var glowCandidate2 = new DeduplicationOrgHeader(org2);
			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			Assert("Precondition", scorer.ChildResults.Any());

			var childResultContainsComparisionResults = scorer.ChildResults.FirstOrDefault(u => u.ComparisonResults.Any() && u.MasterType == typeof(MultiSourcePhoneNumber));
			AssertNotNull(childResultContainsComparisionResults);

			var childResultsFirst = scorer.ChildResults.First();
			(childResultsFirst.ChildResults as List<ScoringResult>).Add(childResultContainsComparisionResults);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels();
			AssertEquals("Correctly generate the child scoring results", 12, result.Count());
		}

		public void TestPresenterModelHasChildPKsWhenItIsAnAddress()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var glowCandidate1 = new DeduplicationOrgHeader(org);
			var glowCandidate2 = new DeduplicationOrgHeader(org2);
			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels();

			var address = result.First(x => x.ChildGroupNameForType == "Addresses");
			AssertNotEquals(Guid.Empty, address.ChildMasterID);
		}

		public void TestGetHeadingWithNullMasterAndTargetTypeDoesNotThrowException()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var glowCandidate1 = new DeduplicationOrgHeader(org);
			var glowCandidate2 = new DeduplicationOrgHeader(org2);
			var scoringResult = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);

			scoringResult.TargetType = null;
			scoringResult.MasterType = null;

			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scoringResult });

			AssertNoExceptionThrown(() => provider.GeneratePresenterModels());
		}

		public void TestGetOrgNamesFromGlbPerson()
		{
			AssertGetOrgNamesFromGlbPerson(true);
			AssertGetOrgNamesFromGlbPerson(false);
		}

		public void TestPopulateResultModelsForAssociations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TESTORG1";
			org1.MainAddress.OA_RN_NKCountryCode = "AU";
			var orgContact1 = org1.Contacts.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TESTORG2";
			org2.MainAddress.OA_RN_NKCountryCode = "AU";
			var orgContact2 = org2.Contacts.AddNew();
			var orgContact3 = org2.Contacts.AddNew();

			orgContact1.OC_ContactName = orgContact2.OC_ContactName = "Peter Lewis";
			orgContact3.OC_ContactName = "Dummy";

			var glbPerson1 = GlbPerson.CreateFromContact(Factory, orgContact1);
			var glbPerson2 = GlbPerson.CreateFromContact(Factory, orgContact2);
			var glbPerson3 = GlbPerson.CreateFromContact(Factory, orgContact3);
			var glbStaff = glbPerson1.StaffCollection.AddNew();
			glbStaff.GS_PER = glbPerson1.PK;
			var applicant = glbPerson1.ApplicantCollection.AddNew() as Integration.Recruiter.IHRJobApplicant;
			applicant.HA_PER = glbPerson1.PK;

			var primary = glbPerson1.PrimaryRelationship;
			primary.PPR_PER = glbPerson1.PK;
			primary.PPR_PrimaryId = glbStaff.PK;
			primary.PPR_PrimaryTableCode = GlbStaffSchema.Constants.Prefix;

			Factory.Save();

			var glowCandidate1 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson1) as DeduplicationGlbPerson;
			var glowCandidate2 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson2) as DeduplicationGlbPerson;
			var glowCandidate3 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson3) as DeduplicationGlbPerson;

			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2, glowCandidate3 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels().ToList();
			result = result.Where(u => u.GroupNameForType == DeduplicationProvider.Constants.ActiveAssociations).ToList();
			var countryInfo = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			CombineAssertions(() =>
			{
				AssertEquals(false, result.Any(u => u.TargetID == glowCandidate3.PK));
				AssertEquals(27, result.Count);
				Assert(result.All(u => u.ChildGroupNameForType == DeduplicationProvider.Constants.ActiveAssociations && u.ChildDisplayModeForType == DeduplicationDisplayMode.Detailed && u.ChildGroupNameForType == DeduplicationProvider.Constants.ActiveAssociations));
				AssertModelsResult(result.Where(u => u.ChildMasterID == orgContact1.PK).ToList(), orgContact1.OC_ContactName, countryInfo.Description);
				AssertModelsResult(result.Where(u => u.ChildMasterID == glbStaff.PK).ToList(), glbStaff.GS_FullName, string.Empty, true);
				AssertModelsResult(result.Where(u => u.ChildMasterID == applicant.PK).ToList(), applicant.HA_FullName, string.Empty, true);
			});
		}

		public void TestPopulateResultModelsForAssociations_TargetHasMoreAssociations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TESTORG1";
			org1.MainAddress.OA_RN_NKCountryCode = "AU";
			var orgContact1 = org1.Contacts.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TESTORG2";
			org2.MainAddress.OA_RN_NKCountryCode = "AU";
			var orgContact2 = org2.Contacts.AddNew();

			orgContact1.OC_ContactName = orgContact2.OC_ContactName = "Peter Lewis";

			var glbPerson1 = GlbPerson.CreateFromContact(Factory, orgContact1);
			var glbPerson2 = GlbPerson.CreateFromContact(Factory, orgContact2);

			var glbStaff = glbPerson2.StaffCollection.AddNew();
			glbStaff.GS_PER = glbPerson2.PK;

			var applicant = glbPerson2.ApplicantCollection.AddNew() as Integration.Recruiter.IHRJobApplicant;
			applicant.HA_PER = glbPerson2.PK;

			Factory.Save();

			var glowCandidate1 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson1) as DeduplicationGlbPerson;
			var glowCandidate2 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson2) as DeduplicationGlbPerson;

			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels().ToList();
			result = result.Where(u => u.GroupNameForType == DeduplicationProvider.Constants.ActiveAssociations).ToList();

			CombineAssertions(() =>
			{
				AssertEquals(27, result.Count);
				Assert(result.All(u => u.ChildGroupNameForType == DeduplicationProvider.Constants.ActiveAssociations && u.ChildDisplayModeForType == DeduplicationDisplayMode.Detailed && u.ChildGroupNameForType == DeduplicationProvider.Constants.ActiveAssociations));
			});
		}

		public void TestGetApplicantInfo()
		{
			var countryInfo = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var applicant = Factory.New<Integration.Recruiter.IHRJobApplicant>();
			var applicantBizO = applicant as BusinessObject;
			applicantBizO[HRJobApplicantSchema.Constants.HA_EmailAddress] = "D@D.com";
			applicant.HA_FullName = "A";
			applicant.HA_City = "B";
			applicant.HA_State = "C";
			applicant.HA_RN_NKCountry = "AU";

			var provider = new DeduplicationPresenterForTest(null, null, null);
			var info = provider.GetApplicantInfoExposed("AAA", applicant, Guid.NewGuid());

			CombineAssertions(() =>
			{
				AssertEquals("AAA", info.Type);
				AssertEquals("A", info.Name);
				AssertEquals("", info.UNLOCO);
				AssertEquals("B", info.City);
				AssertEquals("C", info.State);
				AssertEquals(countryInfo.Description, info.Country);
				AssertEquals("True", info.Active);
				AssertEquals("D@D.com", info.Email);
				AssertEquals(applicant.PK, info.PK);
				AssertEquals("False", info.IsPrimary);
			});

			info = provider.GetApplicantInfoExposed("AAA", applicant, applicant.PK.ToGuid());
			AssertEquals("True", info.IsPrimary);
		}

		public void TestGetStaffInfo()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "A";
			staff.GS_City = "B";
			staff.GS_State = "C";
			staff.GS_RN_NKCountryCode = "";
			staff.GS_IsActive = true;
			staff.GS_EmailAddress = "D@D.com";
			var provider = new DeduplicationPresenterForTest(null, null, null);
			var info = provider.GetStaffInfoExposed("AAA", staff, Guid.NewGuid());

			CombineAssertions(() =>
			{
				AssertEquals("AAA", info.Type);
				AssertEquals("A", info.Name);
				AssertEquals("", info.UNLOCO);
				AssertEquals("B", info.City);
				AssertEquals("C", info.State);
				AssertEquals(null, info.Country);
				AssertEquals("True", info.Active);
				AssertEquals("D@D.com", info.Email);
				AssertEquals(staff.PK, info.PK);
				AssertEquals("False", info.IsPrimary);
			});

			staff.GS_IsActive = false;
			info = provider.GetStaffInfoExposed("AAA", staff, staff.PK.ToGuid());
			AssertEquals("False", info.Active);
			AssertEquals("True", info.IsPrimary);
		}

		public void TestGetContactInfo()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "A";
			orgHeader.MainAddress.OA_Address1 = "Dummy";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.OA_City = "B";
			orgHeader.MainAddress.OA_State = "C";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "";
			contact.OC_IsActive = true;
			contact.OC_Email = "D@D.com";
			contact.OC_OH = orgHeader.PK;
			var provider = new DeduplicationPresenterForTest(null, null, null);
			var info = provider.GetContactInfoExposed("AAA", contact, Guid.NewGuid());

			CombineAssertions(() =>
			{
				AssertEquals("AAA", info.Type);
				AssertEquals("A", info.Name);
				AssertEquals("AUSYD", info.UNLOCO);
				AssertEquals("B", info.City);
				AssertEquals("C", info.State);
				AssertEquals(null, info.Country);
				AssertEquals("True", info.Active);
				AssertEquals("D@D.com", info.Email);
				AssertEquals(contact.PK, info.PK);
				AssertEquals("False", info.IsPrimary);
			});

			contact.OC_IsActive = false;
			info = provider.GetContactInfoExposed("AAA", contact, contact.PK.ToGuid());
			AssertEquals("False", info.Active);
			AssertEquals("True", info.IsPrimary);
		}

		void AssertModelsResult(List<DeduplicationPresenterModel> models, string name, string countryDesc, bool targetIsEmpty = false)
		{
			AssertColumns(models);

			var nameInfo = models.First(u => u.MasterValue.ToString() == DeduplicationProvider.Constants.Name);
			AssertEquals(name, nameInfo.ChildMasterValue);
			AssertEquals(targetIsEmpty ? string.Empty : name, nameInfo.ChildTargetValue ?? string.Empty);

			var countryInfo = models.First(u => u.MasterValue.ToString() == DeduplicationProvider.Constants.Country);
			AssertEquals(countryDesc, countryInfo.ChildMasterValue ?? string.Empty);
			AssertEquals(targetIsEmpty ? string.Empty : countryDesc, countryInfo.ChildTargetValue ?? string.Empty);
		}

		void AssertColumns(List<DeduplicationPresenterModel> models)
		{
			AssertContainsExactElementsInAnyOrder(new[]
				{
					DeduplicationProvider.Constants.Type,
					DeduplicationProvider.Constants.Name,
					DeduplicationProvider.Constants.PrimaryWorkplace,
					DeduplicationProvider.Constants.UNLOCO,
					DeduplicationProvider.Constants.City,
					DeduplicationProvider.Constants.State,
					DeduplicationProvider.Constants.Country,
					DeduplicationProvider.Constants.Active,
					DeduplicationProvider.Constants.Email
				},
			models.Select(u => u.MasterValue.ToString()));
		}

		class DeduplicationPresenterForTest : DeduplicationPresenter
		{
			public DeduplicationPresenterForTest(IDeduplicationGlowObject master, IEnumerable<IDeduplicationGlowObject> targetGlows, IEnumerable<ScoringResult> scoringResults) : base(master, targetGlows, scoringResults)
			{
			}

			public AssociationInfo GetApplicantInfoExposed(string applicantStr, Integration.Recruiter.IHRJobApplicant applicant, Guid primaryPK) => GetApplicantInfo(applicantStr, applicant, primaryPK);

			public AssociationInfo GetStaffInfoExposed(string staffStr, GlbStaff staff, Guid primaryPK) => GetStaffInfo(staffStr, staff, primaryPK);

			public AssociationInfo GetContactInfoExposed(string contactStr, OrgContact contact, Guid primaryPK) => GetContactInfo(contactStr, contact, primaryPK);
		}

		#region Implementation

		void AssertGetOrgNamesFromGlbPerson(bool hasRelatedName)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TESTORG1";
			var orgContact1 = org1.Contacts.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TESTORG2";
			var orgContact2 = org2.Contacts.AddNew();

			if (hasRelatedName)
			{
				var relatedName1 = org1.BrandsOrRelatedNames.AddNew();
				var relatedName2 = org2.BrandsOrRelatedNames.AddNew();
				relatedName1.P1_RelatedName = relatedName2.P1_RelatedName = "NewName";
			}

			orgContact1.OC_ContactName = orgContact2.OC_ContactName = "Peter Lewis";

			var glbPerson1 = GlbPerson.CreateFromContact(Factory, orgContact1);
			var glbPerson2 = GlbPerson.CreateFromContact(Factory, orgContact2);

			var glowCandidate1 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson1) as DeduplicationGlbPerson;
			var glowCandidate2 = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson2) as DeduplicationGlbPerson;

			var scorer = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var provider = new DeduplicationPresenter(glowCandidate1, new List<IDeduplicationGlowObject>() { glowCandidate2 }, new List<ScoringResult> { scorer });
			var result = provider.GeneratePresenterModels().ToList();

			AssertEquals("Result is flattened based on child elements in scoringResult", 2, result.Count);
			AssertEquals(DeduplicationProvider.Constants.PersonNames, result[0].ChildGroupNameForType);
			AssertEquals("Peter Lewis", result[0].ChildMasterValue);
			AssertEquals("Peter Lewis", result[0].ChildTargetValue);
			AssertEquals(DeduplicationProvider.Constants.OrganisationNames, result[1].ChildGroupNameForType);

			if (hasRelatedName)
			{
				AssertEquals(org1.BrandsOrRelatedNames.First().PK, result[1].ChildMasterID);
				AssertEquals(org2.BrandsOrRelatedNames.First().PK, result[1].ChildTargetID);
				AssertEquals("NewName", result[1].ChildMasterValue);
				AssertEquals("NewName", result[1].ChildTargetValue);
			}
			else
			{
				AssertEquals(org1.PK, result[1].ChildMasterID);
				AssertEquals(org2.PK, result[1].ChildTargetID);
				AssertEquals("TESTORG1", result[1].ChildMasterValue);
				AssertEquals("TESTORG2", result[1].ChildTargetValue);
			}
		}

		#endregion
	}
}
