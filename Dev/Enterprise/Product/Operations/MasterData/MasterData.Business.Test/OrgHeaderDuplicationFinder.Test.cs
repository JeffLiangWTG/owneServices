using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderDuplicationFinderWithFactoryTest : TestCaseWithFactory
	{
		public void TestSettingRegistryTimeoutShouldSetDeuplcateDetectionTimeout()
		{
			//Arrange
			var factory = new BusinessObjectFactory();
			var testTimeout1 = 123;
			var testTimeout2 = 300;
			var org = factory.NewWithValidTestData<OrgHeader>();
			var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);

			//Act
			OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTimeout1);

			//Assert
			AssertEquals(testTimeout1, duplicationFinder.DuplicateDetectionTimeoutExposed);

			//Act
			OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTimeout2);

			//Assert
			AssertEquals(testTimeout2, duplicationFinder.DuplicateDetectionTimeoutExposed);
		}

		public void TestFindPotentialDuplicates_ShouldStopProcessingResultByBatchWhenTimeOut()
		{
			InitOrgValue(out var org, "ABVZA");
			InitOrgValue(out _, "ABVZA1");
			InitOrgValue(out _, "ABVZA2");
			InitOrgValue(out _, "ABVZA3");
			InitOrgValue(out _, "ABVZA4");
			InitOrgValue(out _, "ABVZA5");

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);
				duplicationFinder.LoadTargetBizOsMockTimeOutAtSecondTimes = true;
				AssertEquals("Precondition", false, duplicationFinder.TokenSourceForTest.IsCancellationRequested);
				AssertEquals("Precondition", false, duplicationFinder.ShouldStopProcessing);

				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();
				AssertEquals("Should only finished scoring one batch duplicates", 4, duplicates.Count);
				AssertEquals("Cancellation is requested after timeout", true, duplicationFinder.TokenSourceForTest.IsCancellationRequested);
				AssertEquals("ShouldStopProcessing is true", true, duplicationFinder.ShouldStopProcessing);
			}
		}

		public void TestFindPotentialDuplicates_ShouldProcessingResultByBatchCorrectly()
		{
			InitOrgValue(out var org, "ABVZA");
			InitOrgValue(out _, "ABVZA1");
			InitOrgValue(out _, "ABVZA2");
			InitOrgValue(out _, "ABVZA3");
			InitOrgValue(out _, "ABVZA4");
			InitOrgValue(out _, "ABVZA5");
			InitOrgValue(out _, "ABVZA6");

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();
				AssertEquals("Should finished scoring two batches duplicates", 6, duplicates.Count);
			}
		}

		public void TestFindPotentialDuplicates_ScoringResultsShouldNotGreatThanMaxScoringResult()
		{
			InitOrgValue(out var org, "ABVZA");
			InitOrgValue(out _, "ABVZA1");
			InitOrgValue(out _, "ABVZA2");
			InitOrgValue(out _, "ABVZA3");
			InitOrgValue(out _, "ABVZA4");
			InitOrgValue(out _, "ABVZA5");

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, false);
				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();
				AssertEquals("Should finished scoring with 4 results", 4, duplicates.Count);
			}
		}

		public void TestGetPatternMatchingResultModelParentPK()
		{
			var org = Factory.New<OrgHeader>();
			var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);
			var parentPK = duplicationFinder.GetPatternMatchingResultModelParentPKForTest(new PatternMatchingResultModel()
			{
				OrgPK = org.PK.ToGuid(),
				PersonPK = Guid.Empty
			});

			AssertEquals(org.PK.ToGuid(), parentPK);
		}

		#region Implementation

		void InitOrgValue(out OrgHeader orgHeader, string code)
		{
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = "COSTCO PTY";
			orgHeader.MainAddress.OA_Address1 = "72 O'Riordan Street";
			orgHeader.MainAddress.OA_City = "SYDNEY";
			orgHeader.MainAddress.OA_PostCode = "2015";
			orgHeader.MainAddress.OA_State = "NSW";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Masden";
			contact1.OC_Phone = "+61449743938";

			CreatePatternMatchingName(orgHeader.PK, orgHeader.OH_FullName);
		}

		void CreatePatternMatchingName(ZGuid pk, ZString fullName)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = pk;
			patternMatchingName.PMN_ParentId = pk;
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(fullName, "AU"));
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;
		}

		#endregion
	}

	public class OrgHeaderDuplicationFinderWithoutFactoryTest : DuplicationFinderWithoutFactoryBaseTest
	{
		[UseSnapshotProtection]
		public override void TestWithoutPlaceholders()
		{
			AssertFindResults(false);
		}

		[UseSnapshotProtection]
		public override void TestWithPlaceholders()
		{
			AssertFindResults(true);
		}

		[UseSnapshotProtection]
		public override void TestWithoutPlaceholders_WithoutStandardizationRules()
		{
			AssertFindResults(false, false);
		}

		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_IntegrationAsync()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "ABVZA";
			org.OH_FullName = "COSTCO PTY";
			org.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "John Masden";
			contact1.OC_Phone = "+61449743938";

			org2.OH_Code = "ABVZA1";
			org2.OH_FullName = "COSTCO PTY";
			org2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org2.MainAddress.OA_City = "SYDNEY";
			org2.MainAddress.OA_PostCode = "2015";
			org2.MainAddress.OA_State = "NSW";

			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "John Masden";
			contact2.OC_Phone = "+61449743938";

			var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org2.PK;
			patternMatchingName.PMN_ParentId = org2.PK;
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(org2.OH_FullName, "AU"));
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			var patternMatchingAddress = factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = org2.PK;
			patternMatchingAddress.PMA_ParentId = org2.MainAddress.PK;
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org2.MainAddress.OA_Address1 + org2.MainAddress.OA_Address2 + org2.MainAddress.OA_City + org2.MainAddress.OA_PostCode + org2.MainAddress.OA_State);
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";
			patternMatchingAddress.PMA_ParentTableCode = "OA";
			patternMatchingAddress.PMA_IsActive = true;

			var patternMatchingPhone = factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.PMP_OH = org2.PK;
			patternMatchingPhone.PMP_ParentId = contact2.PK;
			patternMatchingPhone.PMP_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePhone(contact2.OC_Phone));
			patternMatchingPhone.PMP_RN_NKCountryCode = "AU";
			patternMatchingPhone.PMP_ParentTableCode = "OC";
			patternMatchingPhone.PMP_IsActive = true;

			factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var dedupeStarted = false;

				org.DeduplicationStarted += (o, e) =>
				{
					dedupeStarted = true;
				};

				var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);

				duplicationFinder.SetDuplicationTimeout(180);
				var result = duplicationFinder.GetPotentialTargetsThreadSafeAsync(GlbStaff.CurrentUser.GS_Code).Result;

				var result2 = result.ToList();

				AssertEquals(1, result2.Count);

				org.OH_FullName = "COSTCO PTY 1";
				AssertEquals(true, dedupeStarted);

				((IDeduplicatable)org).ShouldRunDeduplication = false;
				dedupeStarted = false;

				org.OH_FullName = "COSTCO PTY";
				AssertEquals(false, dedupeStarted);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_IntegrationNonAsync()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "ABVZA";
			org.OH_FullName = "COSTCO PTY";
			org.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "John Masden";
			contact1.OC_Phone = "+61449743938";

			org2.OH_Code = "ABVZA1";
			org2.OH_FullName = "COSTCO PTY";
			org2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org2.MainAddress.OA_City = "SYDNEY";
			org2.MainAddress.OA_PostCode = "2015";
			org2.MainAddress.OA_State = "NSW";

			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "John Masden";
			contact2.OC_Phone = "+61449743938";

			var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org2.PK;
			patternMatchingName.PMN_ParentId = org2.PK;
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(org2.OH_FullName, "AU"));
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			var patternMatchingAddress = factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = org2.PK;
			patternMatchingAddress.PMA_ParentId = org2.MainAddress.PK;
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org2.MainAddress.OA_Address1 + org2.MainAddress.OA_Address2 + org2.MainAddress.OA_City + org2.MainAddress.OA_PostCode + org2.MainAddress.OA_State);
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";
			patternMatchingAddress.PMA_ParentTableCode = "OA";
			patternMatchingAddress.PMA_IsActive = true;

			var patternMatchingPhone = factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.PMP_OH = org2.PK;
			patternMatchingPhone.PMP_ParentId = contact2.PK;
			patternMatchingPhone.PMP_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePhone(contact2.OC_Phone));
			patternMatchingPhone.PMP_RN_NKCountryCode = "AU";
			patternMatchingPhone.PMP_ParentTableCode = "OC";
			patternMatchingPhone.PMP_IsActive = true;

			factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var dedupeStarted = false;

				org.DeduplicationStarted += (o, e) =>
				{
					dedupeStarted = true;
				};

				IDuplicationFinder<OrgHeader, OrgHeader> duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);
				var result = Enumerable.Empty<ScoringResult>();

				((OrgHeaderDuplicationFinderForIntegrationTest)duplicationFinder).SetDuplicationTimeout(180);
				duplicationFinder.GetPotentialTargets(GlbStaff.CurrentUser.GS_Code);

				result = ((OrgHeaderDuplicationFinderForIntegrationTest)duplicationFinder).DuplicationScoringResults;

				var result2 = result.ToList();

				AssertEquals(1, result2.Count);

				org.OH_FullName = "COSTCO PTY 1";
				AssertEquals(true, dedupeStarted);

				((IDeduplicatable)org).ShouldRunDeduplication = false;
				dedupeStarted = false;

				org.OH_FullName = "COSTCO PTY";
				AssertEquals(false, dedupeStarted);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargetsByEmailDomainAsync()
		{
			var factory = new BusinessObjectFactory();
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(factory);
			var org = list[0];
			var org2 = list[1];
			var mail = "Anyone@wisetechglobal.com";
			var hash = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(mail));
			var pmd = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd.PMD_OH = org.PK;
			pmd.PMD_HashedValue = hash;
			var pmd1 = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd1.PMD_OH = org2.PK;
			pmd1.PMD_HashedValue = hash;
			factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(null, false);

				var result = duplicationFinder.GetPotentialTargetsByEmailThreadSafeAsync(mail);
				result.Wait();

				var result2 = result.Result;
				AssertEquals(2, result2.Count());
				AssertContainsExactElementsInAnyOrder(new[] { org.PK, org2.PK }, result2.Select(x => x.PK));
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargetsAsync()
		{
			var pk = ZGuid.Empty;
			var instance = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var address1 = "PADDINGTON NSW";
					var email = "ABCD@TEST.COM";
					var factory = new BusinessObjectFactory();
					var masterOrg = factory.NewWithValidTestData<OrgHeader>();
					masterOrg.OH_Code = "VARIOBAIO";
					masterOrg.OH_FullName = "ORGANISATION";
					var masterAddress = masterOrg.MainAddress;
					masterAddress.Address1 = address1;
					masterAddress.OA_Email = email;
					pk = masterOrg.PK;

					var targetOrg = factory.NewWithValidTestData<OrgHeader>();
					targetOrg.OH_Code = "VARIOBAIO2";
					targetOrg.OH_FullName = "ORGANISATION";
					var targetAddress = targetOrg.MainAddress;
					targetAddress.Address1 = address1;
					targetAddress.OA_Email = email;

					var pmt = factory.NewWithValidTestData<PatternMatchingResult>();
					pmt.PMT_MasterPK = masterOrg.PK;
					pmt.PMT_TargetPK = targetOrg.PK;
					pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "OH";
					pmt.PMT_Status = "TIG";
					pmt.PMT_GS_NKExcludeBy = "STD";

					var patternMatchingName = factory.New<PatternMatchingName>();
					patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
					patternMatchingName.PMN_OH = targetOrg.PK;
					patternMatchingName.PMN_ParentTableCode = "OH";
					patternMatchingName.PMN_ParentId = targetOrg.PK;
					patternMatchingName.PMN_RN_NKCountryCode = "AU";

					var patternMatchingAddress = factory.New<PatternMatchingAddress>();
					patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
					patternMatchingAddress.PMA_OH = targetOrg.PK;
					patternMatchingAddress.PMA_ParentTableCode = "OA";
					patternMatchingAddress.PMA_ParentId = targetAddress.PK;
					patternMatchingAddress.PMA_RN_NKCountryCode = "AU";

					var patternMatchingEmail = factory.New<PatternMatchingEmail>();
					patternMatchingEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
					patternMatchingEmail.PME_OH = targetOrg.PK;
					patternMatchingEmail.PME_ParentTableCode = "OA";
					patternMatchingEmail.PME_ParentId = targetAddress.PK;
					patternMatchingEmail.PME_RN_NKCountryCode = "AU";

					factory.Save();

					var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
					try
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;

						IDuplicationFinder<OrgHeader, OrgHeader> duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(masterOrg, false);

						return duplicationFinder.GetPotentialDuplicatesAsync("STD").Result;
					}
					finally
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			var result2 = instance.Result.ToList();

			AssertEquals(0, result2.Count);

			instance.Dispose();

			instance = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var masterOrg = new BusinessObjectFactory().Load<OrgHeader>(pk);
					var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
					try
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;

						IDuplicationFinder<OrgHeader, OrgHeader> duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(masterOrg, false);

						return duplicationFinder.GetPotentialDuplicatesAsync("OST").Result;
					}
					finally
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			result2 = instance.Result.ToList();

			AssertEquals(1, result2.Count);
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargets()
		{
			var address1 = "PADDINGTON NSW";
			var email = "ABCD@TEST.COM";
			var factory = new BusinessObjectFactory();
			var masterOrg = factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_Code = "VARIOBAIO";
			masterOrg.OH_FullName = "ORGANISATION";
			var masterAddress = masterOrg.MainAddress;
			masterAddress.Address1 = address1;
			masterAddress.OA_Email = email;

			var targetOrg = factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "VARIOBAIO2";
			targetOrg.OH_FullName = "ORGANISATION";
			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = address1;
			targetAddress.OA_Email = email;

			var pmt = factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = masterOrg.PK;
			pmt.PMT_TargetPK = targetOrg.PK;
			pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "OH";
			pmt.PMT_Status = "TIG";
			pmt.PMT_GS_NKExcludeBy = "USR";

			var patternMatchingName = factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			var patternMatchingAddress = factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
			patternMatchingAddress.PMA_OH = targetOrg.PK;
			patternMatchingAddress.PMA_ParentTableCode = "OA";
			patternMatchingAddress.PMA_ParentId = targetAddress.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var patternMatchingEmail = factory.New<PatternMatchingEmail>();
			patternMatchingEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
			patternMatchingEmail.PME_OH = targetOrg.PK;
			patternMatchingEmail.PME_ParentTableCode = "OA";
			patternMatchingEmail.PME_ParentId = targetAddress.PK;
			patternMatchingEmail.PME_RN_NKCountryCode = "AU";

			factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;
				IDuplicationFinder<OrgHeader, OrgHeader> duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(masterOrg, false);

				AssertEquals("Size of potential targets", 0, duplicationFinder.GetPotentialTargets("USR").Count());
				duplicationFinder.ScoringResults.Clear();
				AssertEquals("Size of potential targets", 1, duplicationFinder.GetPotentialTargets("OST").Count());
				duplicationFinder.ScoringResults.Clear();
				AssertEquals("Size of potential targets", 1, duplicationFinder.GetPotentialTargets(GlbStaff.CurrentUser.GS_Code).Count());

				pmt = factory.NewWithValidTestData<PatternMatchingResult>();
				pmt.PMT_MasterPK = masterOrg.PK;
				pmt.PMT_TargetPK = targetOrg.PK;
				pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "OH";
				pmt.PMT_Status = "TIG";
				pmt.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
				factory.Save();

				duplicationFinder.ScoringResults.Clear();
				AssertEquals("Size of potential targets", 0, duplicationFinder.GetPotentialTargets(GlbStaff.CurrentUser.GS_Code).Count());

				duplicationFinder.ScoringResults.Clear();
				AssertEquals("Remove", DuplicationResponseMessages.Success, duplicationFinder.RemoveTemporaryIgnore(targetOrg).Message);
				AssertEquals("Size of potential targets", 1, duplicationFinder.GetPotentialTargets(GlbStaff.CurrentUser.GS_Code).Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_ShouldStopProcessingAfterTimeout()
		{
			var instance = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					var org = factory.NewWithValidTestData<OrgHeader>();
					var org2 = factory.NewWithValidTestData<OrgHeader>();

					org.OH_Code = "ABVZA";
					org.OH_FullName = "COSTCO PTY";
					org.MainAddress.OA_Address1 = "72 O'Riordan Street";
					org.MainAddress.OA_City = "SYDNEY";
					org.MainAddress.OA_PostCode = "2015";
					org.MainAddress.OA_State = "NSW";

					var contact1 = org.Contacts.AddNew();
					contact1.OC_ContactName = "John Masden";
					contact1.OC_Phone = "+61449743938";

					org2.OH_Code = "ABVZA1";
					org2.OH_FullName = "COSTCO PTY";
					org2.MainAddress.OA_Address1 = "72 O'Riordan Street";
					org2.MainAddress.OA_City = "SYDNEY";
					org2.MainAddress.OA_PostCode = "2015";
					org2.MainAddress.OA_State = "NSW";

					var contact2 = org2.Contacts.AddNew();
					contact2.OC_ContactName = "John Masden";
					contact2.OC_Phone = "+61449743938";

					var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
					patternMatchingName.PMN_OH = org2.PK;
					patternMatchingName.PMN_ParentId = org2.PK;
					patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(org2.OH_FullName, "AU"));
					patternMatchingName.PMN_RN_NKCountryCode = "AU";
					patternMatchingName.PMN_ParentTableCode = "OH";
					patternMatchingName.PMN_IsActive = true;

					var patternMatchingAddress = factory.NewWithValidTestData<PatternMatchingAddress>();
					patternMatchingAddress.PMA_OH = org2.PK;
					patternMatchingAddress.PMA_ParentId = org2.MainAddress.PK;
					patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org2.MainAddress.OA_Address1 + org2.MainAddress.OA_Address2 + org2.MainAddress.OA_City + org2.MainAddress.OA_PostCode + org2.MainAddress.OA_State);
					patternMatchingAddress.PMA_RN_NKCountryCode = "AU";
					patternMatchingAddress.PMA_ParentTableCode = "OA";
					patternMatchingAddress.PMA_IsActive = true;

					var patternMatchingPhone = factory.NewWithValidTestData<PatternMatchingPhone>();
					patternMatchingPhone.PMP_OH = org2.PK;
					patternMatchingPhone.PMP_ParentId = contact2.PK;
					patternMatchingPhone.PMP_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePhone(contact2.OC_Phone));
					patternMatchingPhone.PMP_RN_NKCountryCode = "AU";
					patternMatchingPhone.PMP_ParentTableCode = "OC";
					patternMatchingPhone.PMP_IsActive = true;

					factory.Save();

					var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

					try
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)org).ShouldRunDeduplication = true;

						var duplicationFinder = new OrgHeaderDuplicationFinderForIntegrationTest(org, true);

						duplicationFinder.SetDuplicationTimeout(duplicationFinder.timeDelay.Add(TimeSpan.FromSeconds(-1)).Seconds);
						duplicationFinder.StandardizeMasterWithDelay = true;

						var result = ((IDuplicationFinder<OrgHeader, OrgHeader>)duplicationFinder).GetPotentialDuplicatesAsync(GlbStaff.CurrentUser.GS_Code).Result;

						var result2 = result.ToList();

						AssertEquals(0, result2.Count);
						AssertEquals("Cancellation is requested after timeout", true, duplicationFinder.TokenSourceForTest.IsCancellationRequested);
					}
					finally
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);
			instance.Wait();
		}

		void AssertFindResults(bool shouldUsePlaceholder, bool shouldLoadStandardizationRules = true)
		{
			const string placeholder = "N/A";
			var address1 = shouldUsePlaceholder ? placeholder : "PADDINGTON NSW";
			var email = shouldUsePlaceholder ? placeholder : "ABCD@TEST.COM";
			var factory = new BusinessObjectFactory();
			var masterOrg = factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_Code = "TESTYO1";
			masterOrg.OH_FullName = "ORGANISATION";
			var masterAddress = masterOrg.MainAddress;
			masterAddress.Address1 = address1;
			masterAddress.OA_Email = email;

			var targetOrg = factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";
			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = address1;
			targetAddress.OA_Email = email;

			var patternMatchingName = factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			var patternMatchingAddress = factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
			patternMatchingAddress.PMA_OH = targetOrg.PK;
			patternMatchingAddress.PMA_ParentTableCode = "OA";
			patternMatchingAddress.PMA_ParentId = targetAddress.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var patternMatchingEmail = factory.New<PatternMatchingEmail>();
			patternMatchingEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
			patternMatchingEmail.PME_OH = targetOrg.PK;
			patternMatchingEmail.PME_ParentTableCode = "OA";
			patternMatchingEmail.PME_ParentId = targetAddress.PK;
			patternMatchingEmail.PME_RN_NKCountryCode = "AU";

			factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldLoadStandardizationRules);
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;
				var duplicationFinder = new OrgHeaderDuplicationFinder(masterOrg, false, new OrganisationDeduplicationStrategy());
				var duplicates = duplicationFinder.FindPotentialDuplicates(true);

				if (shouldUsePlaceholder)
				{
					AssertEquals(0, duplicates.Count);
				}
				else if (!shouldLoadStandardizationRules)
				{
					AssertEquals(0, duplicates.Count);
				}
				else
				{
					AssertEquals(1, duplicates.Count);
				}
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}
	}

	[TestedType(typeof(OrgHeaderDuplicationFinder))]
	public class OrgHeaderDuplicationFinderTest : DuplicationFinderBaseTest<OrgHeaderDuplicationFinder, OrgHeader, OrgHeader, CargoWise.Glow.Model.Interfaces.IOrgHeader>
	{
		public void TestFindPotentialDuplicates_WithInvalidOperationExceptions_NoExceptionThrown()
		{
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
				var orgheader = list[0];
				((IDeduplicatable)orgheader).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTestWithException(orgheader, true);
				var message = new IOExceptionMessages();

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.ReadWhenNoDataIsPresent) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.ReadWhenReaderIsClosedt) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.TheConnectionIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.NextResultWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.FieldCoiuntWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.InternalConnectionError) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.CheckDataIsReadyWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.OpenAndAvailableConnectionError) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				Assert(ExceptionReporterTestListener.Instance.Count == 0);
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 0);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFindPotentialDuplicates_WithInvalidOperationExceptions_HandledByDeveloperException()
		{
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
				var orgheader = list[0];
				((IDeduplicatable)orgheader).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTestWithException(orgheader, true)
				{
					exceptionFromStandardizeMaster = new InvalidOperationException("Some invalid operation exception has occured.") { Source = "System.Data" }
				};
				duplicationFinder.FindPotentialDuplicates(false);
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertEquals("Some invalid operation exception has occured.", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFindPotentialDuplicates_WithSqlExceptions_HandledByDeveloperException()
		{
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
				var orgheader = list[0];
				((IDeduplicatable)orgheader).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTestWithException(orgheader, true)
				{
					exceptionFromStandardizeMaster = GetNewSqlException()
				};
				duplicationFinder.FindPotentialDuplicates(false);
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestGetPotentialTargetsByEmailDomainAsync_GetRegistryTimeoutFailed()
		{
			var duplicationFinder = new OrgHeaderDuplicationFinderForTestWithException(null, false)
			{
				throwSqlExceptionFromRegistryTimeout = true
			};

			var result = AsyncTaskSynchronizer.Run(() => duplicationFinder.GetPotentialTargetsByEmailDomainAsync(null));

			AssertNotNull(result);
			AssertEquals(0, result.Count());

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region implementation of base test

		public override void TestScoringResultWithNoTarget_ReturnsEmptyList()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(org2, org2, -383906410)
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicationFinder.StandardizeMasterForTest();
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals(0, actual: duplicates.Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestFindPotentialDuplicates()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];
			org3.OH_IsActive = false;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(org, org2, -383906410),
				GetNewPatternMatchingResultModel(org, org3, -383906410, "US")
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.GenerateTargetGlows(new[] { org2, org3 });
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals(2, actual: duplicates.Count());
				AssertContainsExactElementsInAnyOrder(new[] { org2.PK, org3.PK }, duplicates.Select(scoringResult => scoringResult.TargetPK));
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestGetOrderedList()
		{
			var duplicationFinder = new OrgHeaderDuplicationFinderForTest(null, false);
			AssertNoExceptionThrown(() => duplicationFinder.GetOrderedListForTest(null, null));
			AssertNoExceptionThrown(() => duplicationFinder.GetOrderedListForTest(new OrgHeader[1], null));

			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var org4 = Factory.New<OrgHeader>();
			var org5 = Factory.New<OrgHeader>();
			var orderedList = duplicationFinder.GetOrderedListForTest(new[] { org5, org1, org2, org4, org3 }, new HashSet<Guid>(new[] { Guid.NewGuid(), org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid(), Guid.NewGuid(), Guid.NewGuid(), org4.PK.ToGuid(), org5.PK.ToGuid() })).ToArray();

			AssertEquals(5, orderedList.Length);
			AssertEquals(org1.PK, orderedList[0].PK);
			AssertEquals(org2.PK, orderedList[1].PK);
			AssertEquals(org3.PK, orderedList[2].PK);
			AssertEquals(org4.PK, orderedList[3].PK);
			AssertEquals(org5.PK, orderedList[4].PK);
		}

		public override void TestFindPotentialDuplicatesWithMultiLanguage()
		{
			var engData = GetTestDataWithLanguageCode(SharedConstants.Languages.English);
			AssertFindPotentialDuplicatesWithMultiLanguage(engData, ConfidenceRating.High);

			var chsData = GetTestDataWithLanguageCode(SharedConstants.Languages.ChineseSimplified);
			AssertFindPotentialDuplicatesWithMultiLanguage(chsData, ConfidenceRating.High);

			var chtData = GetTestDataWithLanguageCode(SharedConstants.Languages.ChineseTraditional);
			AssertFindPotentialDuplicatesWithMultiLanguage(chtData, ConfidenceRating.High);

			var otherData = GetTestDataWithLanguageCode("Dummy");
			AssertFindPotentialDuplicatesWithMultiLanguage(otherData, ConfidenceRating.Medium);
		}

		#endregion

		#region Implementations

		public void TestExclude()
		{
			var deDupOrg = Factory.New<DeduplicationOrganisationForTest>();
			deDupOrg.DOH_Status = DeduplicationHelper.StatusConstants.ToBeProcessed;
			((INeedRow)deDupOrg).Row.AcceptChanges();
			var org = Factory.NewWithPrimaryKey<OrgHeader>(deDupOrg.PK.ToGuid());
			org.OH_Code = "TESTORG";
			Factory.Save();

			((IDeduplicatable)org).IsExcludedFromDeduplication = true;
			deDupOrg.Reload();
			AssertEquals(DeduplicationHelper.StatusConstants.Excluded, deDupOrg.DOH_Status);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, deDupOrg.DOH_ExcludedBy);
			var excResult = Factory.LoadTop1<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, deDupOrg.PK)
				.AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.Excluded)
				.AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, SQLComparisonOperator.NotEqual, ZString.Empty));
			AssertNotNull("There is an EXC PatternMatchingResult", excResult);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, excResult.PMT_GS_NKExcludeBy);
		}

		public void TestInclude()
		{
			var deDupOrg = Factory.New<DeduplicationOrganisationForTest>();
			deDupOrg.DOH_Status = DeduplicationHelper.StatusConstants.ToBeProcessed;
			((INeedRow)deDupOrg).Row.AcceptChanges();
			var org = Factory.NewWithPrimaryKey<OrgHeader>(deDupOrg.PK.ToGuid());
			org.OH_Code = "TESTORG";
			Factory.Save();

			var excResult = Factory.New<PatternMatchingResult>();
			excResult.PMT_MasterPK = deDupOrg.PK;
			excResult.PMT_MasterTableCode = deDupOrg.MasterOrgHeader.TableCode;
			excResult.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			excResult.PMT_FoundTimeUtc = ZDateTime.UtcToday;
			excResult.PMT_ScorePercent = 0;
			excResult.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			deDupOrg.Reload();

			AssertEquals(DeduplicationHelper.StatusConstants.Excluded, deDupOrg.DOH_Status);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, deDupOrg.DOH_ExcludedBy);

			((IDeduplicatable)org).IsExcludedFromDeduplication = false;
			deDupOrg.Reload();

			AssertEquals(DeduplicationHelper.StatusConstants.ToBeProcessed, deDupOrg.DOH_Status);
			AssertEquals(ZString.Empty, deDupOrg.DOH_ExcludedBy);

			Assert("The EXC PatternMatchingResult is gone", excResult.IsDeleted);
		}

		public void TestFindTargetPKs_MaximumValue_IsInRegistrySettings()
		{
			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			var orgPks = new List<Guid>();

			orgPks.Add(masterOrg.PK.ToGuid());

			for (int i = 0; i < 20; i++)
			{
				orgPks.Add(Guid.NewGuid());
			}

			var patternMatchingResultsModel = new List<PatternMatchingResultModel>();

			for (int i = 0; i < orgPks.Count; i++)
			{
				patternMatchingResultsModel.Add(new PatternMatchingResultModel { OrgPK = orgPks[i] });
			}

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.MaximumPotentialTargets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(masterOrg, false);
				var result = duplicationFinder.FindTargetPKsAndResultModelsTest(patternMatchingResultsModel.ToArray());

				AssertEquals("Maximum TargetPKs to load is got from registry", 10, result.Count());
			}
		}

		void AssertFindPotentialDuplicatesWithMultiLanguage((OrgHeader orgMaster, OrgHeader orgTarget) orgData, ConfidenceRating expectRating)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = orgData.orgMaster.PK;
			patternMatchingName.PMN_ParentId = orgData.orgMaster.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(orgData.orgMaster, orgData.orgTarget, -383906410),
			};

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)orgData.orgMaster).ShouldRunDeduplication = true;
				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgData.orgMaster, false);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.GenerateTargetGlows(new[] { orgData.orgTarget });
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult).ToList();

				AssertEquals(1, duplicates.Count);
				AssertEquals(orgData.orgTarget.PK, duplicates[0].TargetPK);
				AssertEquals(expectRating, duplicates[0].ConfidenceRating);
			}
		}

		(OrgHeader orgMaster, OrgHeader orgTarget) GetTestDataWithLanguageCode(string languageCode)
		{
			var orgMaster = Factory.New<OrgHeader>();
			orgMaster.OH_Language = languageCode;
			orgMaster.OH_Code = "TESTORGA";
			orgMaster.OH_RL_NKClosestPort = "AUSYD";

			var orgTarget = Factory.New<OrgHeader>();
			orgTarget.OH_Language = languageCode;
			orgTarget.OH_Code = "TESTORGB";
			orgTarget.OH_RL_NKClosestPort = "AUSYD";

			var addressMaster = orgMaster.Addresses.MainAddress;
			addressMaster.OA_Language = languageCode;

			var addressTarget = orgTarget.Addresses.MainAddress;
			addressTarget.OA_Language = languageCode;
			addressTarget.OA_Code = "Code";

			switch (languageCode)
			{
				case SharedConstants.Languages.English:

					#region Init Engish Org Data

					orgMaster.OH_FullName = "KITCHAN";
					addressMaster.Address1 = "SMITH";
					addressMaster.Address2 = "SMITH";

					orgTarget.OH_FullName = "KITCHEN";
					addressTarget.Address1 = "SMYTHE";
					addressTarget.Address2 = "SMYTHE";

					#endregion

					break;
				case SharedConstants.Languages.ChineseSimplified:

					#region Init ChineseSimplified Org Data

					orgMaster.OH_FullName = "为所欲为公司名";
					addressMaster.Address1 = "为嬴鱼科技地址信息";
					addressMaster.Address2 = "唯嬴鱼科技地址信息";

					orgTarget.OH_FullName = "唯所与唯公司名";
					addressTarget.Address1 = "为迎鱼科技地址信息";
					addressTarget.Address2 = "唯迎鱼科技地址信息";

					#endregion

					break;
				case SharedConstants.Languages.ChineseTraditional:

					#region Init ChineseTraditional Org Data

					orgMaster.OH_FullName = "為所欲為公司名";
					addressMaster.Address1 = "為嬴魚科技地址信息";
					addressMaster.Address2 = "為赢魚科技地址信息";

					orgTarget.OH_FullName = "唯所與唯公司名";
					addressTarget.Address1 = "唯迎魚科技地址信息";
					addressTarget.Address2 = "唯迎魚科技地址信息";

					#endregion

					break;
				default:

					#region Init Other Org Data

					orgMaster.OH_FullName = "為所欲為公司名";
					addressMaster.Address1 = "SMITH";
					addressMaster.Address2 = "為赢魚科技地址信息";

					orgTarget.OH_FullName = "唯所與唯公司名";
					addressTarget.Address1 = "SMITH";
					addressTarget.Address2 = "唯赢魚科技地址信息";

					#endregion

					break;
			}
			Factory.Save();

			return (orgMaster, orgTarget);
		}

		OrgHeader[] SetConfidenceRatingExclusionOrgs()
		{
			// Master
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "COSTCO PTY";
			org.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";

			// setup the pattern match, forcing targets got the same hash as the Master
			var masterHashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(org.OH_FullName, "AU"));
			void createPatternMatching(OrgHeader orgheader)
			{
				var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
				patternMatchingName.PMN_OH = orgheader.PK;
				patternMatchingName.PMN_ParentId = orgheader.PK;
				patternMatchingName.PMN_HashedValue = masterHashedValue;
				patternMatchingName.PMN_RN_NKCountryCode = "AU";
				patternMatchingName.PMN_ParentTableCode = "OH";
				patternMatchingName.PMN_IsActive = true;
			}

			// Targets
			// Exact
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "COSTCO PTY";
			org1.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org1.MainAddress.OA_City = "SYDNEY";
			org1.MainAddress.OA_PostCode = "2015";
			org1.MainAddress.OA_State = "NSW";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Masden";
			contact1.OC_Phone = "+61449743938";
			createPatternMatching(org1);

			// High
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "COSTCO";
			org2.MainAddress.OA_Address1 = "72 O'Riordan Street";
			org2.MainAddress.OA_City = "SYDNEY";
			org2.MainAddress.OA_PostCode = "2015";
			org2.MainAddress.OA_State = "NSW";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "John Masden";
			contact2.OC_Phone = "+61449743938";
			createPatternMatching(org2);

			// Medium
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "COSTCO";
			org3.MainAddress.OA_Address1 = "70 O'Riordan Street";
			org3.MainAddress.OA_City = "SYDNEY";
			org3.MainAddress.OA_PostCode = "2015";
			org3.MainAddress.OA_State = "NSW";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "John Masden";
			contact3.OC_Phone = "+61449742654";
			createPatternMatching(org3);

			// Low
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "SINTCO";
			org4.MainAddress.OA_Address1 = "Anderson Street";
			org4.MainAddress.OA_City = "SYDNEY";
			org4.MainAddress.OA_PostCode = "2032";
			org4.MainAddress.OA_State = "NSW";
			var contact4 = org4.Contacts.AddNew();
			contact4.OC_ContactName = "James Smith";
			contact4.OC_Phone = "+6141008249";
			createPatternMatching(org4);

			// None
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_FullName = "SCP FOUNDATION AU";
			org5.MainAddress.OA_Address1 = "HYDE PARK";
			org5.MainAddress.OA_City = "SYDNEY";
			org5.MainAddress.OA_PostCode = "2601";
			org5.MainAddress.OA_State = "ACT";
			var contact5 = org5.Contacts.AddNew();
			contact5.OC_ContactName = "Alto Clef";
			contact5.OC_Phone = "+6165248524";
			createPatternMatching(org5);

			Factory.Save();

			return new[] { org, org1, org2, org3, org4, org5 };
		}

		public void TestExcludesWithConfidenceRating_Medium()
		{
			AssertExcludesWithConfidenceRating("MED", ConfidenceRating.Medium, 2);
		}

		public void TestExcludesWithConfidenceRating_Low()
		{
			AssertExcludesWithConfidenceRating("LOW", ConfidenceRating.Low, 3);
		}

		public void TestExcludesWithConfidenceRating_None()
		{
			AssertExcludesWithConfidenceRating("NON", ConfidenceRating.None, 4);
		}

		public void TestExcludesWithConfidenceRating_Undefined()
		{
			AssertExcludesWithConfidenceRating("UND", ConfidenceRating.Undefined, 5);
		}

		[UseSnapshotProtection]
		public void AssertExcludesWithConfidenceRating(string registryConfidenceRating, ConfidenceRating exclusion_rating, int expectedCount)
		{
			var orgs = SetConfidenceRatingExclusionOrgs();
			var orgMaster = orgs[0];
			var orgTargets = orgs.Skip(1).ToArray();

			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryConfidenceRating))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)orgMaster).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgMaster, true);

				var duplicates = duplicationFinder.FindPotentialDuplicates(false).ToList();
				foreach (var dup in duplicates)
				{
					AssertGreaterThan(dup.ConfidenceRating, exclusion_rating);
				}
				AssertEquals(expectedCount, duplicates.Count);
			}
		}

		public enum OverridableFunctions
		{
			Undefined,
			StandardizeMasterTimeout,
			GetPatternMatchingResultModelsThrowSqlException,
			PopulateTargetGlows
		}

		public class OrgHeaderDuplicationFinderForTest : OrgHeaderDuplicationFinder
		{
			public OrgHeaderDuplicationFinderForTest(OrgHeader header, OverridableFunctions functions)
				: base(header, false, new OrganisationDeduplicationStrategy())
			{
				this.functions = functions;
			}

			readonly OverridableFunctions functions;
			public ZGuid tempTargetPK;

			public OrgHeaderDuplicationFinderForTest(OrgHeader header, bool useMaxScoringResult)
				: base(header, useMaxScoringResult, new OrganisationDeduplicationStrategy())
			{
			}

			public OrgHeaderDuplicationFinderForTest(OrgHeader header, bool shouldUseCache, bool useMaxScoringResult)
				: base(header, shouldUseCache, useMaxScoringResult)
			{
			}

			public OrgHeaderDuplicationFinderForTest(DeduplicationOrgHeader deduplicationOrgHeader, bool shouldUseCache, bool useMaxScoringResult)
				: base(deduplicationOrgHeader, shouldUseCache, useMaxScoringResult)
			{
			}

			public void GenerateTargetGlows(OrgHeader targetHeaderBizo)
			{
				var list = new List<CargoWise.Glow.Model.Interfaces.IOrgHeader>
				{
					ConvertMasterToGlowModel(targetHeaderBizo)
				};
				TargetGlows = list;
			}

			public void GenerateTargetGlows(OrgHeader[] targetHeaderBizos)
			{
				var list = new List<CargoWise.Glow.Model.Interfaces.IOrgHeader>();
				foreach (var target in targetHeaderBizos)
				{
					list.Add(ConvertMasterToGlowModel(target));
				}
				TargetGlows = list;
			}

			protected void PopulateTargetGlows(IFactory factory, IEnumerable<PatternMatchingResultModel> patternMatchingResults)
			{
				if (patternMatchingResults != null)
				{
					using (var timer = new DeduplicationPerformanceMonitor())
					{
						var candidatePKs = new HashSet<Guid>(FindTargetPKsAndResultModels(patternMatchingResults.ToArray()).Select(u => u.Key));
						var targetBizos = LoadTargetBizOs(factory, candidatePKs);

						TargetGlows = GetTargetGlowBizos(targetBizos);
						DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, TargetGlows, string.Join("_", nameof(PopulateTargetGlows), nameof(TargetGlows)), timer.ElapsedDuration, MasterGlow);
					}
				}
			}

			public void PopulateTargetGlows(OrgHeader targetHeaderBizo, BusinessObjectFactory factory)
			{
				if (functions == OverridableFunctions.PopulateTargetGlows)
				{
					var org = factory.NewWithValidTestData<OrgHeader>();

					factory.Save();
					tempTargetPK = org.PK;

					PopulateTargetGlows(factory, new List<PatternMatchingResultModel>
					{
						new PatternMatchingResultModel
						{
							 CountryCode = "AU",
							  HashedValue = 13832323,
							   OrgPK = org.PK.ToGuid(),
								ParentID = org.PK.ToGuid(),
								 ParentTablePrefix = OrgHeaderSchema.Constants.Prefix
						}
					});
				}
				else
				{
					var candidatePKs = new HashSet<Guid>() { targetHeaderBizo.PK.ToGuid() };
					var targetBizos = LoadTargetBizOs(factory, candidatePKs);
					TargetGlows = targetBizos.Select(ConvertMasterToGlowModel).ToList();
				}
			}

			public string ComputeResponseMessageForTest(string staffCode, OrgHeader target)
			{
				return ComputeResponseMessage(staffCode, target.PK);
			}

			public UserIgnoreStatus ComputeIgnoreForTest(string message)
			{
				return ComputeIgnoreStatus(message);
			}

			public IEnumerable<OrgHeader> LoadTargetBizosTest(IFactory factory, HashSet<Guid> pkList)
			{
				return LoadTargetBizOs(factory, pkList);
			}

			public bool GetShouldFindDuplications()
			{
				return ShouldFindDuplications;
			}

			public IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> FindTargetPKsAndResultModelsTest(PatternMatchingResultModel[] patternMatchingResults)
			{
				return FindTargetPKsAndResultModels(patternMatchingResults);
			}

			public IEnumerable<OrgHeader> GetOrderedListForTest(OrgHeader[] targetBizos, HashSet<Guid> candidatePKs) => GetOrderedList(targetBizos, candidatePKs);

			public void SetLastRunStatusForTest(DuplicationStatus duplicationStatus)
			{
				((ISupportDuplicationFinder)this).LastRunStatus = duplicationStatus;
			}

			protected override void StandardizeMaster()
			{
				if (functions == OverridableFunctions.StandardizeMasterTimeout)
				{
					Thread.Sleep(TimeSpan.FromSeconds(12));
				}
				else
				{
					base.StandardizeMaster();
				}
			}

			protected override IEnumerable<PatternMatchingResultModel> GetPatternMatchingResultModels()
			{
				if (functions is OverridableFunctions.GetPatternMatchingResultModelsThrowSqlException)
				{
					throw GetNewSqlException();
				}
				else
				{
					return base.GetPatternMatchingResultModels();
				}
			}

			public void StandardizeMasterForTest()
			{
				StandardizeMaster();
			}

			public IEnumerable<ScoringResult> ScoreResultsForTest(IEnumerable<PatternMatchingResultModel> patternMatchingResults)
			{
				var resultScorer = new DuplicationFinderResultsScorer<CargoWise.Glow.Model.Interfaces.IOrgHeader>(shouldUseCache, MasterGlow, MaxScoringResult, DebuggerParticipant, this);
				resultScorer.ScoringResults(TargetGlows, patternMatchingResults, GetGlowPK, GetPatternMatchingResultModelParentPK, GenerateCacheSubkey, ScoreGlowModel, TokenSource.Token);

				return ScoringResults;
			}

			public IEnumerable<CargoWise.Glow.Model.Interfaces.IOrgHeader> TargetGlowsExposed => TargetGlows;

			public DeduplicationExclusionManager<OrgHeader> ExclusionManager => exclusionManager;
		}

		public class OrgHeaderDuplicationFinderForTestWithException : OrgHeaderDuplicationFinder
		{
			internal Exception exceptionFromStandardizeMaster;
			internal bool throwSqlExceptionFromRegistryTimeout;

			public OrgHeaderDuplicationFinderForTestWithException(OrgHeader header, bool useMaxRecords)
				: base(header, useMaxRecords, new OrganisationDeduplicationStrategy())
			{
			}

			public IEnumerable<ScoringResult> GetDuplicationsAsync_Exposed()
			{
				try
				{
					return AsyncTaskSynchronizer.Run(async () => await GetDuplicationsAsync());
				}
				catch (Exception ex)
				{
					throw ex.InnerException;
				}
			}

			protected override bool ShouldFindDuplications => true;

			protected override void StandardizeMaster()
			{
				if (exceptionFromStandardizeMaster != null)
				{
					throw exceptionFromStandardizeMaster;
				}

				base.StandardizeMaster();
			}

			protected override int RegistryTimeout
			{
				get
				{
					if (throwSqlExceptionFromRegistryTimeout)
					{
						throw GetNewSqlException();
					}

					return base.RegistryTimeout;
				}
			}
		}

		sealed class DummyDebuggerWindow : IDeduplicationDebuggerParticipant, IDisposable
		{
			public DummyDebuggerWindow()
			{
				DeduplicationUtils.DebuggerHubInstance.Register(this);
			}

			public string ReceivedMessage;

			public string DebuggerName => DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName;

			public IDeduplicationDebuggerHub DebuggerHub { get; set; }

			public void Receive(object value, string methodName, TimeSpan executionTime)
			{
				ReceivedMessage = string.Join("", "Information has been received from ", methodName);
			}

			public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, object glowBizO)
			{
			}

			public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, string prefix)
			{
			}

			public void Dispose()
			{
				ReceivedMessage = null;
				DebuggerHub.RemoveParticipant(this);
			}
		}

		class DirtyRecordFinderForTest : IDirtyRecordFinder
		{
			public bool IsOrgDirtyForDeduplication() => true;
			public string GetDirtyReason() => "Sample reason\nwith detailed explanations\non multiple lines.";
		}

		class OrgHeaderDuplicationFinderForDirtyRecordTest : OrgHeaderDuplicationFinder
		{
			public OrgHeaderDuplicationFinderForDirtyRecordTest(OrgHeader header, bool useMaxScoringResult)
				: base(header, useMaxScoringResult, new OrganisationDeduplicationStrategy())
			{
			}

			public OrgHeaderDuplicationFinderForDirtyRecordTest(OrgHeader header, IDeduplicationDebuggerParticipant participant, bool useMaxScoringResult)
				: base(header, useMaxScoringResult, new OrganisationDeduplicationStrategy())
			{
				DebuggerParticipant = participant;
			}

			new public bool IsBizoDirty(OrgHeader header, IDirtyRecordFinder finder)
			{
				return base.IsBizoDirty(header, finder);
			}

			public IDeduplicationDebuggerParticipant Participant => DebuggerParticipant;
		}

		#endregion

		#region Further tests

		public void TestPotentialDuplicatesMaxResults()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];
			var org4 = list[3];
			var org5 = list[4];
			var org6 = list[5];

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(org, org2, -383906410),
				GetNewPatternMatchingResultModel(org, org3, -383906410),
				GetNewPatternMatchingResultModel(org, org4, -383906410),
				GetNewPatternMatchingResultModel(org, org5, -383906410),
				GetNewPatternMatchingResultModel(org, org6, -383906410)
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);

				duplicationFinder.GenerateTargetGlows(new[] { org2, org3, org4, org5, org6 });

				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals("Use Max Records is false, so duplicates should be capped at 4", 4, actual: duplicates.Count());

				duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, true);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.GenerateTargetGlows(new[] { org2, org3, org4, org5, org6 });

				duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals("Use Max Records is true, so total duplicates is below the cap and all can be returned", 5, actual: duplicates.Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestLoadTargetBizos()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			org2.OH_IsActive = false;
			var org3 = list[2];
			org3.OH_RL_NKClosestPort = "USSYD";

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				org2.PK.ToGuid(),
				org3.PK.ToGuid(),
			};

			var registrySettingDedup = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var registrySettingExcludeFromOtherCountries = OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.Value;
			var registrySettingExcludeInactive = OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.Value;

			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);

				duplicationFinder.GenerateTargetGlows(new[] { org2, org3 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(2, actual: duplicates.Length);
				AssertEquals(org2.PK, duplicates[0].PK);
				AssertEquals(org3.PK, duplicates[1].PK);
				OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				org.OH_RL_NKClosestPort = "";
				org.MainAddress.OA_RN_NKCountryCode = "";
				duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();
				AssertEquals(0, actual: duplicates.Length);

				org.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
				duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();
				AssertEquals(1, actual: duplicates.Length);

				org.OH_RL_NKClosestPort = Constants.CountryCodes.Australia;
				org.MainAddress.OA_RN_NKCountryCode = "";
				duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();
				AssertEquals(1, actual: duplicates.Length);

				OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();
				AssertEquals(0, actual: duplicates.Length);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettingDedup);
				OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettingExcludeFromOtherCountries);
				OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettingExcludeInactive);
			}
		}

		public override void TestLoadTargetBizosForTIG()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];

			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				org2.PK.ToGuid(),
				org3.PK.ToGuid(),
			};

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);

				duplicationFinder.GenerateTargetGlows(new[] { org2, org3 });
				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(2, actual: duplicates.Length);
				AssertEquals(org2.PK, duplicates[0].PK);
				AssertEquals(org3.PK, duplicates[1].PK);

				var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
				pmt.PMT_MasterPK = org.PK;
				pmt.PMT_TargetPK = org3.PK;
				pmt.PMT_Status = "TIG";
				pmt.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
				pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = org.TablePrefix;
				Factory.Save();

				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				duplicationFinder.ExclusionManager.BuildDisplay();

				var expectedExclusionInfo = $@"
   {org3.OH_Code}: This record has been ignored by you    
 ";

				AssertEquals(1, actual: duplicates.Length);
				AssertEquals(org2.PK, duplicates[0].PK);
				AssertEquals(1, duplicationFinder.ExclusionManager.ItemsCount);
				AssertMultilineASCIIEquals(expectedExclusionInfo, duplicationFinder.ExclusionManager.DisplayInfo);
			}
		}

		public override void TestLoadTargetBizosForPIG()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];

			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_TargetPK = org3.PK;
			pmt.PMT_Status = "PIG";
			pmt.PMT_GS_NKExcludeBy = "BLA";
			pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = org.TablePrefix;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				org2.PK.ToGuid(),
				org3.PK.ToGuid(),
			};

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);

				duplicationFinder.GenerateTargetGlows(new[] { org2, org3 });
				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				duplicationFinder.ExclusionManager.BuildDisplay();

				var expectedExclusionInfo = $@"
   {org3.OH_Code}: This record has been ignored for everyone    
 ";

				AssertEquals(1, actual: duplicates.Length);
				AssertEquals(org2.PK, duplicates[0].PK);
				AssertEquals(1, duplicationFinder.ExclusionManager.ItemsCount);
				AssertMultilineASCIIEquals(expectedExclusionInfo, duplicationFinder.ExclusionManager.DisplayInfo);
			}
		}

		public override void TestLoadTargetBizosForEXC()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];

			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = org3.PK;
			pmt.PMT_TargetPK = ZGuid.Empty;
			pmt.PMT_Status = "EXC";
			pmt.PMT_GS_NKExcludeBy = "BLA";
			pmt.PMT_MasterTableCode = org2.TablePrefix;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				org2.PK.ToGuid(),
				org3.PK.ToGuid(),
			};

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);

				duplicationFinder.GenerateTargetGlows(new[] { org2, org3 });
				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				duplicationFinder.ExclusionManager.BuildDisplay();

				var expectedExclusionInfo = $@"
   {org3.OH_Code}: Excluded from De-duplication    
 ";

				AssertEquals(1, actual: duplicates.Length);
				AssertEquals(org2.PK, duplicates[0].PK);
				AssertEquals(1, duplicationFinder.ExclusionManager.ItemsCount);
				AssertMultilineASCIIEquals(expectedExclusionInfo, duplicationFinder.ExclusionManager.DisplayInfo);
			}
		}

		public void TestExcludesFromMatching_PotentialDuplicate()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.PotentialDuplicate, false);
		}

		public void TestExcludesFromMatching_NotDuplicate()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.NoDuplicates, false);
		}

		public void TestExcludesFromMatching_TemporaryIgnore()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.TemporaryIgnore, false);
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.TemporaryIgnore, true);

			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.TemporaryIgnore, false, true);
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.TemporaryIgnore, true, true);
		}

		public void TestExcludesFromMatching_PermanentIgnore()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.PermanentIgnore, true);
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.PermanentIgnore, true, true);
		}

		public void TestExcludesFromMatching_Excluded()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.Excluded, true);
		}

		public void TestExcludesFromMatching_Error()
		{
			AssertExclusionFromMatchingByStatus(PatternMatchingResult.StatusCodes.Error, false);
		}

		public void TestExcludesRelationshipsFromMatching()
		{
			AssertExclusionFromMatchingByRelationship(false);
		}

		public void TestExcludesReversedRelationshipsFromMatching()
		{
			AssertExclusionFromMatchingByRelationship(true);
		}

		void AssertExclusionFromMatchingByStatus(string statusCode, bool expectExclusion, bool inverseIgnore = false)
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			var patternMatchingResult = Factory.NewWithValidTestData<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			if (new[] { PatternMatchingResult.StatusCodes.Excluded, PatternMatchingResult.StatusCodes.NoDuplicates }.Contains(statusCode))
			{
				patternMatchingResult.PMT_MasterPK = org2.PK;
			}
			else
			{
				if (!inverseIgnore)
				{
					patternMatchingResult.PMT_MasterPK = org.PK;
					patternMatchingResult.PMT_TargetPK = org2.PK;
				}
				else
				{
					patternMatchingResult.PMT_MasterPK = org2.PK;
					patternMatchingResult.PMT_TargetPK = org.PK;
				}

				patternMatchingResult.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;
			}
			patternMatchingResult.PMT_FoundTimeUtc = ZDateTime.Now;
			patternMatchingResult.PMT_Status = statusCode;
			switch (statusCode)
			{
				case PatternMatchingResult.StatusCodes.PotentialDuplicate:
					patternMatchingResult.PMT_GS_NKExcludeBy = "";
					break;
				case PatternMatchingResult.StatusCodes.TemporaryIgnore when expectExclusion:
					patternMatchingResult.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
					break;
				default:
					patternMatchingResult.PMT_GS_NKExcludeBy = "ABC";
					break;
			}
			patternMatchingResult.PMT_ScorePercent = 100;

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(org, org2, -383906410)
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var useMaxScoringResult = false;
				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, useMaxScoringResult);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.PopulateTargetGlows(org2, Factory);
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				duplicationFinder.ExclusionManager.BuildDisplay();

				if (expectExclusion)
				{
					AssertEquals("Should not find a duplicate", 0, duplicates.Count());
					AssertEquals(1, duplicationFinder.ExclusionManager.ItemsCount);
				}
				else
				{
					AssertEquals("Should find 1 duplicate", 1, duplicates.Count());
					AssertEquals(org2.PK, duplicates.Single().TargetPK);
				}
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		void AssertExclusionFromMatchingByRelationship(bool reversed)
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;

			var relatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			if (reversed)
			{
				relatedParty.PR_OH_Parent = org.PK;
				relatedParty.PR_OH_RelatedParty = org2.PK;
			}
			else
			{
				relatedParty.PR_OH_Parent = org2.PK;
				relatedParty.PR_OH_RelatedParty = org.PK;
			}
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				org2.PK.ToGuid()
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(org, false);
				duplicationFinder.StandardizeMasterForTest();
				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals("Should not find a duplicate", 0, duplicates.Length);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestExcludeRelatedHierarchy()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>(); //Main parent           A
			var orgB = Factory.NewWithValidTestData<OrgHeader>(); //					 / \   
			var orgC = Factory.NewWithValidTestData<OrgHeader>(); //					B	C
			var orgD = Factory.NewWithValidTestData<OrgHeader>(); //						|
			var orgE = Factory.NewWithValidTestData<OrgHeader>(); //						D <- test point					
			var orgF = Factory.NewWithValidTestData<OrgHeader>(); //						|
			orgA.OH_Code = "AAA"; //														E
			orgB.OH_Code = "BBB";
			orgC.OH_Code = "CCC";
			orgD.OH_Code = "DDD";
			orgE.OH_Code = "EEE";
			orgF.OH_Code = "FFF";

			Factory.Save();

			var relation_A_B = Factory.New<OrgRelatedParty>();
			relation_A_B.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_A_B.PR_OH_Parent = orgB.PK;
			relation_A_B.PR_OH_RelatedParty = orgA.PK;

			var relation_A_C = Factory.New<OrgRelatedParty>();
			relation_A_C.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_A_C.PR_OH_Parent = orgC.PK;
			relation_A_C.PR_OH_RelatedParty = orgA.PK;

			var relation_C_D = Factory.New<OrgRelatedParty>();
			relation_C_D.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_C_D.PR_OH_Parent = orgD.PK;
			relation_C_D.PR_OH_RelatedParty = orgC.PK;

			var relation_D_E = Factory.New<OrgRelatedParty>();
			relation_D_E.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_D_E.PR_OH_Parent = orgE.PK;
			relation_D_E.PR_OH_RelatedParty = orgD.PK;

			Factory.Save();

			var list = new HashSet<Guid>
			{
				orgA.PK.ToGuid(),
				orgB.PK.ToGuid(),
				orgC.PK.ToGuid(),
				orgD.PK.ToGuid(),
				orgE.PK.ToGuid(),
				orgF.PK.ToGuid()
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)orgD).ShouldRunDeduplication = true;
				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgD, false);
				duplicationFinder.StandardizeMasterForTest();
				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, list).ToArray();

				AssertEquals(1, duplicates.Length);
				var org = duplicates.FirstOrDefault();
				AssertNotNull(org);
				AssertEquals("FFF", org.OH_Code);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestFindTargetPKsSortsByNumberOFHashes()
		{
			var mainOrg = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var notMainOrg = Factory.NewWithValidTestData<OrgHeader>();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(notMainOrg, mainOrg, -383906410),
				GetNewPatternMatchingResultModel(org2, org2, -383906411),
				GetNewPatternMatchingResultModel(org3, org3, -383906412),
				GetNewPatternMatchingResultModel(org3, org3, -383906413),
				GetNewPatternMatchingResultModel(org3, org3, -383906414)
			};

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;

			OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			((IDeduplicatable)mainOrg).ShouldRunDeduplication = true;
			var duplicationFinder = new OrgHeaderDuplicationFinderForTest(mainOrg, false);
			duplicationFinder.StandardizeMasterForTest();
			var result = duplicationFinder.FindTargetPKsAndResultModelsTest(targetResult.ToArray()).ToList();

			OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);

			// Make sure that the order of the orgs is correct
			AssertEquals(org3.PK.ToGuid(), result[0].Key);
			AssertEquals(org2.PK.ToGuid(), result[1].Key);
			AssertContainsExactElementsInAnyOrder(new[] { targetResult[2], targetResult[3], targetResult[4] }, result[0].ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { targetResult[1] }, result[1].ToArray());

			// Make sure that the Master Org is not returned in the list
			AssertEquals(2, result.Count);
		}

		protected override PatternMatchingResultModel GetNewPatternMatchingResultModel(OrgHeader parent, OrgHeader target, int hash, string countryCode = "AU")
		{
			var pmrm = base.GetNewPatternMatchingResultModel(parent, target, hash, countryCode);
			pmrm.OrgPK = target.PK.ToGuid();

			return pmrm;
		}

		public void TestShouldFindDuplications()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var orgMaster = list[0];
			((IDeduplicatable)orgMaster).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgMaster, false);
				var shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry DISABLED; non-web environment; valid master BizO: should NOT search for duplicates", !shouldRun);
			}

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgMaster, false);
				var shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; valid master BizO: should search for duplicates", shouldRun);

				duplicationFinder = new OrgHeaderDuplicationFinderForTest(null, false);
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; NULL master BizO: should NOT search for duplicates", !shouldRun);

				duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgMaster, false);
				Globals.IsWeb = true;
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; WEB environment; valid master BizO: should search for duplicates", shouldRun);

				Globals.IsWeb = false;
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; valid master BizO: should search for duplicates", shouldRun);

				DeduplicationOrgHeader deduplicationOrgHeader = null;
				duplicationFinder = new OrgHeaderDuplicationFinderForTest(deduplicationOrgHeader, false, false);
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("for UXML matching, deduplicationOrgHeader is null", !shouldRun);

				duplicationFinder = new OrgHeaderDuplicationFinderForTest(new DeduplicationOrgHeader(null), false, false);
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("for UXML matching, deduplicationOrgHeader is not null", shouldRun);
			}
		}

		public void TestTargetTypeBizO()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var orgMaster = list[0];
			var duplicationFinder = new OrgHeaderDuplicationFinderForTest(orgMaster, false);

			AssertEquals(typeof(OrgHeader), duplicationFinder.TargetType);
		}

		public void TestMasterIsDirtyInvokesDuplicationFinderTermination()
		{
			var originalEventText = "InitialValue";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dirtyFinder = new DirtyRecordFinderForTest();
			var orgHeaderFinder = new OrgHeaderDuplicationFinderForDirtyRecordTest(org, false);
			var models = Enumerable.Empty<PatternMatchingResultModel>();
			var scoringResults = Enumerable.Empty<ScoringResult>();
			object targetObjects = new[] { org };

			org.DeduplicationEnded += (sender, arg) =>
			{
				originalEventText = "Hidden Spinner Info";
				models = arg.ResultsModels;
				scoringResults = arg.Results;
				targetObjects = arg.TargetObjects;
			};

			AssertEquals("Pre-condition: Event invocation", "InitialValue", originalEventText);
			AssertNotNull(models);
			AssertNotNull(scoringResults);
			AssertNotNull(targetObjects);

			var result = orgHeaderFinder.IsBizoDirty(org, dirtyFinder);

			Assert(result);
			AssertEquals("Event was invoked", "Hidden Spinner Info", originalEventText);
			AssertNull(models);
			AssertNull(scoringResults);
			AssertNull(targetObjects);
		}

		public void TestDeduplicationDebuggerWindowIsSentADirtyRecordMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dirtyFinder = new DirtyRecordFinderForTest();
			var participantMock = new Mock<IDeduplicationDebuggerParticipant>();

			participantMock.Setup(x => x.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationCustomMessage>>(), "IsBizoDirty", It.IsAny<TimeSpan>(), It.IsAny<object>()));

			var orgHeaderFinder = new OrgHeaderDuplicationFinderForDirtyRecordTest(org, participantMock.Object, false);
			var isDirty = orgHeaderFinder.IsBizoDirty(org, dirtyFinder);

			Assert(isDirty);
			participantMock.Verify(mock => mock.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, It.IsAny<List<DeduplicationCustomMessage>>(), "IsBizoDirty", It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Once);
		}

		public void TestGetOrgCountryCodeWithFallbackLogic()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "";
			org.MainAddress.OA_RN_NKCountryCode = "";
			AssertEquals(string.Empty, OrgHeaderDuplicationFinder.GetOrgCountryCodeWithFallbackLogic(org));

			org.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			AssertEquals(Constants.CountryCodes.UnitedStates, OrgHeaderDuplicationFinder.GetOrgCountryCodeWithFallbackLogic(org));

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals(Constants.CountryCodes.Australia, OrgHeaderDuplicationFinder.GetOrgCountryCodeWithFallbackLogic(org));
		}

		public void TestTimeoutOperation_SendDebuggerInformation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var debuggerWindow = new DummyDebuggerWindow())
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var finder = new OrgHeaderDuplicationFinderForTest(org, OverridableFunctions.StandardizeMasterTimeout);

				finder.FindPotentialDuplicates(true);

				AssertEquals("Operation timeout", DuplicationStatus.Timeout, ((ISupportDuplicationFinder)finder).LastRunStatus);
				AssertEquals("Information has been received from Org_FindPotentialDuplicates", debuggerWindow.ReceivedMessage);
			}

			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestErrorOccurred_SendDebuggerInformation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var debuggerWindow = new DummyDebuggerWindow())
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var finder = new OrgHeaderDuplicationFinderForTest(org, OverridableFunctions.GetPatternMatchingResultModelsThrowSqlException);

				finder.FindPotentialDuplicates(true);

				AssertEquals("Error Occurred", DuplicationStatus.ErrorOccurred, ((ISupportDuplicationFinder)finder).LastRunStatus);
				AssertEquals("Information has been received from Org_FindPotentialDuplicates", debuggerWindow.ReceivedMessage);
			}

			AsyncHelper.WaitAllActiveTasksForTest();

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestPopulateTargetGlows_SendsDebuggerInformation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var debuggerWindow = new DummyDebuggerWindow())
			{
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var finder = new OrgHeaderDuplicationFinderForTest(org, OverridableFunctions.PopulateTargetGlows);

				finder.PopulateTargetGlows(org, Factory);

				AssertEquals("There is one TargetPK", finder.tempTargetPK, finder.TargetGlowsExposed.Single().OH_PK);
				AssertEquals("Information has been received from Org_PopulateTargetGlows_TargetGlows", debuggerWindow.ReceivedMessage);
			}
		}

		public void TestStandardizeMasterNamesScoreExact()
		{
			var fullName = "A.A.L.L. SHIPPING AGENCIES P/L";
			var relatedName = "A.A.L.L.L. SHIPPING AGENCIES P/L";
			var contactName = "Michael Jackson PHD";

			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var orgMaster = list[0];
			var orgTarget = list[1];
			orgMaster.OH_FullName = fullName;
			orgTarget.OH_FullName = fullName;

			var brand1 = orgMaster.BrandsOrRelatedNames.AddNew();
			var brand2 = orgTarget.BrandsOrRelatedNames.AddNew();
			brand1.P1_RelatedName = relatedName;
			brand2.P1_RelatedName = relatedName;

			orgMaster.Contacts[0].OC_ContactName = contactName;
			orgTarget.Contacts[0].OC_ContactName = contactName;
			orgMaster.Contacts[1].Delete();
			orgTarget.Contacts[1].Delete();

			((IDeduplicatable)orgMaster).ShouldRunDeduplication = true;

			var standardizeCompanyName = TextStandardizerHelper.StandardizeCompanyName(orgMaster.OH_FullName, Constants.CountryCodes.Australia);
			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(standardizeCompanyName);
			patternMatchingName.PMN_OH = orgTarget.PK;
			patternMatchingName.PMN_ParentId = orgTarget.PK;
			patternMatchingName.PMN_RN_NKCountryCode = Constants.CountryCodes.Australia;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var finder = new OrgHeaderDuplicationFinderForTest(orgMaster, OverridableFunctions.PopulateTargetGlows);
				var result = finder.FindPotentialDuplicates(false).ToList();
				AssertEquals("Should be exact match", ConfidenceRating.Exact, result.Single().ConfidenceRating);
			}
		}

		public override void AssertCompareBizos<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org2 = list[1];
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.ShouldRunDeduplication = true;

				IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, false, new OrganisationDeduplicationStrategy());
				duplicationFinder.AddIgnore(org2, UserIgnoreStatus.TemporaryIgnore, standardStaffCodeForTest);
				var response = duplicationFinder.CompareBizOs(org2, "OST");
				AssertEquals("Compare message", DuplicationResponseMessages.Success, response.Message);
				AssertEquals("Compare scores", ConfidenceRating.High, response.ScoringResult.ConfidenceRating);

				response = duplicationFinder.CompareBizOs(org2, standardStaffCodeForTest);
				AssertEquals("Compare message", DuplicationResponseMessages.TIGExisted, response.Message);
			}
		}

		public override void AssertAddIgnore<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.ShouldRunDeduplication = true;

				IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, true, new OrganisationDeduplicationStrategy());
				AssertEquals("AddIgnore temporary", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, standardStaffCodeForTest).Message);
				AssertEquals("Ignored message", DuplicationResponseMessages.TIGExisted, duplicationFinder.CompareBizOs(org1, standardStaffCodeForTest).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.TIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, standardStaffCodeForTest).Message);

				AssertEquals("AddIgnore permanent", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, standardStaffCodeForTest).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, "ABC").Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, "ORG").Message);
			}
		}

		public override void AssertAddExclusion<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.ShouldRunDeduplication = true;

				IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, false, new OrganisationDeduplicationStrategy());
				AssertEquals("AddExclusion succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddExclusion(standardStaffCodeForTest).Message);
				AssertEquals("AddExclusion exclusion exists", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion("ABC").Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, standardStaffCodeForTest).Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, "ABC").Message);

				AssertEquals("AddExclusion already existed", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion(standardStaffCodeForTest).Message);
			}
		}

		public override void AssertRemoveExclusion<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.ShouldRunDeduplication = true;

				IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, false, new OrganisationDeduplicationStrategy());
				AssertEquals("AddExclusion succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddExclusion(standardStaffCodeForTest).Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, standardStaffCodeForTest).Message);

				AssertEquals("RemoveExclusion succeeds", DuplicationResponseMessages.Success, duplicationFinder.RemoveExclusion().Message);
				AssertEquals("CompareBizOs succeeds", DuplicationResponseMessages.Success, duplicationFinder.CompareBizOs(org1, standardStaffCodeForTest).Message);

				AssertEquals("RemoveExclusion no exclusion existed", DuplicationResponseMessages.NoExclusionExisted, duplicationFinder.RemoveExclusion().Message);
			}
		}

		public override void AssertRemoveIgnore<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.ShouldRunDeduplication = true;

				IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, false, new OrganisationDeduplicationStrategy());
				AssertEquals("AddIgnore succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, standardStaffCodeForTest).Message);
				AssertEquals("Ignored message", DuplicationResponseMessages.TIGExisted, duplicationFinder.CompareBizOs(org1, standardStaffCodeForTest).Message);

				AssertEquals("RemoveIgnore no ignore existed", DuplicationResponseMessages.NoIgnoranceExisted, duplicationFinder.RemovePermanentIgnore(org1).Message);

				AssertEquals("RemoveIgnore succeeds", DuplicationResponseMessages.Success, duplicationFinder.RemoveTemporaryIgnore(org1).Message);

				AssertEquals("AddIgnore succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, standardStaffCodeForTest).Message);
				AssertEquals("RemoveIgnore succeeds", DuplicationResponseMessages.Success, duplicationFinder.RemovePermanentIgnore(org1).Message);
			}
		}

		public override void AssertAddExclude_ShouldNotThrowConcurrencyError<TMaster, TTarget, TFinder>()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData<TTarget>(Factory);
			var org = list[0];
			var org1 = list[1];
			var org2 = list[2];

			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_TargetPK = org1.PK;
			pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "OH";
			pmt.PMT_Status = "TIG";
			pmt.PMT_GS_NKExcludeBy = "STD";

			pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_TargetPK = org2.PK;
			pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "OH";
			pmt.PMT_Status = "TIG";
			pmt.PMT_GS_NKExcludeBy = "STD";

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var result = newFactory.LoadTop1<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK));
			if (result != null)
			{
				result.Delete();
				newFactory.Save();
			}

			IDuplicationFinder<TMaster, TTarget> duplicationFinder = (TFinder)Activator.CreateInstance(typeof(TFinder), org, false, new OrganisationDeduplicationStrategy());
			AssertEquals("An exception has occurred", DuplicationResponseMessages.Exception, duplicationFinder.AddExclusion(standardStaffCodeForTest).Message);
		}

		#endregion
	}

	class OrgHeaderDuplicationFinderForIntegrationTest : OrgHeaderDuplicationFinder
	{
		readonly string isBizoDirty;

		public OrgHeaderDuplicationFinderForIntegrationTest(OrgHeader header, bool useMaxRecords)
			: base(header, useMaxRecords, new OrganisationDeduplicationStrategy())
		{
		}

		public Task<IEnumerable<DeduplicationResponseStatus>> GetPotentialTargetsThreadSafeAsync(ZString staffCode)
		{
			var instance = Task.Factory.StartNew(() =>
			{
				return ((IDuplicationFinder<OrgHeader, OrgHeader>)this).GetPotentialDuplicatesAsync(staffCode);
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler);

			return instance.Result;
		}

		public bool StandardizeMasterWithDelay;
		public TimeSpan timeDelay = TimeSpan.FromSeconds(7);

		protected override void StandardizeMaster()
		{
			base.StandardizeMaster();

			if (StandardizeMasterWithDelay)
			{
				Thread.Sleep(timeDelay);
			}
		}

		public void StandardizeMasterForTest()
		{
			StandardizeMaster();
		}

		int loadTimes;
		public bool LoadTargetBizOsMockTimeOutAtSecondTimes { get; set; }
		protected override IEnumerable<OrgHeader> LoadTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs)
		{
			var result = base.LoadTargetBizOs(factory, candidatePKs);

			if (LoadTargetBizOsMockTimeOutAtSecondTimes && loadTimes == 1)
			{
				ShouldStopProcessing = true;
				TokenSource.Cancel();
			}

			loadTimes++;

			return result;
		}

		public CancellationTokenSource TokenSourceForTest => TokenSource;

		public Task<IEnumerable<OrgHeader>> GetPotentialTargetsByEmailThreadSafeAsync(string email)
		{
			var instance = Task.Factory.StartNew(() =>
			{
				return GetPotentialTargetsByEmailDomainAsync(email);
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler);

			return instance.Result;
		}

		public OrgHeaderDuplicationFinderForIntegrationTest(OrgHeader header, bool shouldUseCache, bool useMaxRecords)
			: base(header, shouldUseCache, useMaxRecords)
		{
		}

		public OrgHeaderDuplicationFinderForIntegrationTest(OrgHeader header, bool useMaxRecords, string overrideIsBizoDirty)
			: base(header, useMaxRecords, new OrganisationDeduplicationStrategy())
		{
			isBizoDirty = overrideIsBizoDirty;
		}

		public void SetDuplicationTimeout(int timeOut)
		{
			OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, timeOut);
		}

		protected override bool IsBizoDirty(OrgHeader obj)
		{
			return isBizoDirty != null ? bool.Parse(isBizoDirty) : base.IsBizoDirty(obj);
		}

		public int DuplicateDetectionTimeoutExposed => DuplicateDetectionTimeout;

		public Guid GetPatternMatchingResultModelParentPKForTest(PatternMatchingResultModel patternMatchingResultModel) => GetPatternMatchingResultModelParentPK(patternMatchingResultModel);
	}
}
