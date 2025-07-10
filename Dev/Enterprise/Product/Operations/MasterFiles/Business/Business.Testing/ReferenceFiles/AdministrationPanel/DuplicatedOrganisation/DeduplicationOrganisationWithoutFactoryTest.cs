using System;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DeduplicationOrganisationWithoutFactoryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_FindsLowConfidenceMatches_RegardlessOfRegistryThreshold()
		{
			var factory = new BusinessObjectFactory();

			var masterOrg = factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_FullName = "COSTCO PTY";
			masterOrg.MainAddress.OA_Address1 = "72 O'Riordan Street";
			masterOrg.MainAddress.OA_City = "SYDNEY";
			masterOrg.MainAddress.OA_PostCode = "2015";
			masterOrg.MainAddress.OA_State = "NSW";
			var contact = masterOrg.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";
			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(masterOrg.OH_FullName, "AU"));

			var mediumMatchOrg = factory.NewWithValidTestData<OrgHeader>();
			mediumMatchOrg.OH_FullName = "COSTCO";
			mediumMatchOrg.MainAddress.OA_Address1 = "70 O'Riordan Street";
			mediumMatchOrg.MainAddress.OA_City = "SYDNEY";
			mediumMatchOrg.MainAddress.OA_PostCode = "2015";
			mediumMatchOrg.MainAddress.OA_State = "NSW";
			var mediumMatchContact = mediumMatchOrg.Contacts.AddNew();
			mediumMatchContact.OC_ContactName = "John Masden";
			mediumMatchContact.OC_Phone = "+61449742654";
			CreatePatternMatchingName(factory, mediumMatchOrg, hashedMasterName);

			var lowMatchOrg = factory.NewWithValidTestData<OrgHeader>();
			lowMatchOrg.OH_FullName = "SINTCO";
			lowMatchOrg.MainAddress.OA_Address1 = "Anderson Street";
			lowMatchOrg.MainAddress.OA_City = "SYDNEY";
			lowMatchOrg.MainAddress.OA_PostCode = "2032";
			lowMatchOrg.MainAddress.OA_State = "NSW";
			var lowMatchContact = lowMatchOrg.Contacts.AddNew();
			lowMatchContact.OC_ContactName = "James Smith";
			lowMatchContact.OC_Phone = "+6141008249";
			CreatePatternMatchingName(factory, lowMatchOrg, hashedMasterName);

			var noneMatchOrg = factory.NewWithValidTestData<OrgHeader>();
			noneMatchOrg.OH_FullName = "SCP FOUNDATION AU";
			noneMatchOrg.MainAddress.OA_Address1 = "HYDE PARK";
			noneMatchOrg.MainAddress.OA_City = "SYDNEY";
			noneMatchOrg.MainAddress.OA_PostCode = "2601";
			noneMatchOrg.MainAddress.OA_State = "ACT";
			var nonMatchContact = noneMatchOrg.Contacts.AddNew();
			nonMatchContact.OC_ContactName = "Alto Clef";
			nonMatchContact.OC_Phone = "+6165248524";
			CreatePatternMatchingName(factory, noneMatchOrg, hashedMasterName);

			factory.Save();

			var registryEnableDuplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var registryMinimumConfidence = OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value;

			try
			{
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var dedupePerson = factory.Load<DeduplicationOrganisationForTest>(masterOrg.PK);
				var duplicates = dedupePerson.FindPotentialDuplicatesCore_ForTest();

				AssertContainsExactElementsInAnyOrder("Should return results for medium and low matches, even though registry threshold is set to > medium",
					new[] { mediumMatchOrg.PK, lowMatchOrg.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Low, duplicates.Single(match => match.TargetPK == lowMatchOrg.PK).ConfidenceRating);
				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchOrg.PK).ConfidenceRating);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryMinimumConfidence);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnableDuplicationFinder);
			}
		}

		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_IsNotAffectedByMaximumPotentialTargets_SetInRegistry()
		{
			var registryTimeout = OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.Value;
			var registryEnableDuplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var registryMaxPotentialTargets = OrganisationsDataRegistry.Instance.MaximumPotentialTargets.Value;

			try
			{
				OrganisationsDataRegistry.Instance.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
				OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 180);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var factory = new BusinessObjectFactory();
				var org = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader[] orgTargets = new OrgHeader[12];

				for (int i = 0; i < orgTargets.Length; i++)
				{
					orgTargets[i] = factory.NewWithValidTestData<OrgHeader>();

					orgTargets[i].OH_Code = "ABVA" + i.ToString();
					orgTargets[i].OH_FullName = "COSTCO PTY";
					orgTargets[i].OH_RL_NKClosestPort = "AUSYD";
					orgTargets[i].MainAddress.OA_Address1 = "72 O'Riordan Street";
					orgTargets[i].MainAddress.OA_City = "SYDNEY";
					orgTargets[i].MainAddress.OA_PostCode = "2015";
					orgTargets[i].MainAddress.OA_State = "NSW";

					var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
					patternMatchingName.PMN_OH = orgTargets[i].PK;
					patternMatchingName.PMN_ParentId = orgTargets[i].PK;
					patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(orgTargets[i].OH_FullName, "AU"));
					patternMatchingName.PMN_RN_NKCountryCode = "AU";
					patternMatchingName.PMN_ParentTableCode = "OH";
					patternMatchingName.PMN_IsActive = true;
				}

				org.OH_Code = "ABVZA";
				org.OH_FullName = "COSTCO PTY";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.OA_Address1 = "72 O'Riordan Street";
				org.MainAddress.OA_City = "SYDNEY";
				org.MainAddress.OA_PostCode = "2015";
				org.MainAddress.OA_State = "NSW";
				org.MainAddress.OA_ValidationStatus = "MAN";

				factory.Save();

				var deDupOrg = factory.Load<DeduplicationOrganisationForTest>(org.PK);
				var result = deDupOrg.FindPotentialDuplicatesCore_ForTest();

				AssertEquals(12, result.Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryMaxPotentialTargets);
				OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryTimeout);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnableDuplicationFinder);
			}
		}

		[UseSnapshotProtection]
		public void TestPropagateForcedReloadRequiredInvokesReloadRequired()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTDUP1";
			factory.Save();

			var dedupeOrg = factory.Load<DeduplicationOrganisationForTest>(org.PK);
			var actionInvoked = DeduplicationAction.None;
			org.DeduplicationActionOccurred += (o, e) => actionInvoked = e.InvokedAction;

			dedupeOrg.PropagateForcedReloadRequired();

			AssertEquals("ReloadRequired Deduplication Action was invoked", DeduplicationAction.ReloadRequired, actionInvoked);
		}

		#region Implementation

		PatternMatchingName CreatePatternMatchingName(BusinessObjectFactory factory, OrgHeader orgheader, int hashValue)
		{
			var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = orgheader.PK;
			patternMatchingName.PMN_ParentId = orgheader.PK;
			patternMatchingName.PMN_HashedValue = hashValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		#endregion
	}
}
