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
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderDuplicationFinderProxyTestWithFactory : DuplicationFinderProxyTestWithFactory
	{
		readonly ZString staffCode = GlbStaff.CurrentUser.GS_Code;

		public static SortedList<int, OrgHeader> ValidTestData(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABX";
			org.OH_FullName = "TOLL PTY LTD";
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Phone = "0449743938";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Phone = "0449743938";

			var address1 = org.Addresses.AddNew();
			address1.Address1 = "Bnt Crescent";
			address1.OA_Phone = "0449743938";
			var address2 = org.Addresses.AddNew();
			address2.Address1 = "River Drive";
			address2.OA_Phone = "0449743938";

			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ABV";
			org2.OH_FullName = "TOLL PTY";
			org2.OH_RL_NKClosestPort = "AUSYD";

			var org3 = factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "TOLL LTD";
			org3.OH_Code = "TEST-O1";
			org3.OH_RL_NKClosestPort = "AUSYD";

			var org4 = factory.NewWithValidTestData<OrgHeader>();
			org4.OH_FullName = "TOLL LTD";
			org4.OH_Code = "TEST-O2";
			org4.OH_RL_NKClosestPort = "AUSYD";

			var org5 = factory.NewWithValidTestData<OrgHeader>();
			org5.OH_FullName = "TOLL LTD";
			org5.OH_Code = "TEST-O3";
			org5.OH_RL_NKClosestPort = "AUSYD";

			var org6 = factory.NewWithValidTestData<OrgHeader>();
			org6.OH_FullName = "TOLL LTD";
			org6.OH_Code = "TEST-O4";
			org6.OH_RL_NKClosestPort = "AUSYD";

			var contact3 = org2.Contacts.AddNew();
			contact3.OC_ContactName = "Contact A";
			contact3.OC_Phone = "0449743938";
			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "Contact B";
			contact4.OC_Phone = "0449743938";

			var address3 = org2.Addresses.AddNew();
			address3.Address1 = "Bnt Crescent";
			address3.OA_Phone = "0449743938";
			var address4 = org2.Addresses.AddNew();
			address4.Address1 = "River Drive";
			address4.OA_Phone = "0449743938";

			org2.Contacts.AddRange(contact3, contact4);
			org2.Addresses.AddRange(address3, address4);

			return new SortedList<int, OrgHeader>
			{
				[0] = org,
				[1] = org2,
				[2] = org3,
				[3] = org4,
				[4] = org5,
				[5] = org6
			};
		}

		public override void TestAddExclusion()
		{
			var list = ValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(org);
				AssertEquals("AddExclusion succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddExclusion(staffCode).Message);
				AssertEquals("AddExclusion exclusion exists", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion("ABC").Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, staffCode).Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, "ABC").Message);

				AssertEquals("AddExclusion already existed", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion(staffCode).Message);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestAddIgnore()
		{
			var list = ValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(org);
				AssertEquals("AddIgnore temporary", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, staffCode).Message);
				AssertEquals("Ignored message", DuplicationResponseMessages.TIGExisted, duplicationFinder.CompareBizOs(org1, staffCode).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.TIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, staffCode).Message);

				AssertEquals("AddIgnore permanent", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, staffCode).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, "ABC").Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, "ORG").Message);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestCompareBusinessObjects()
		{
			var list = ValidTestData(Factory);
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

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(org);
				var response = duplicationFinder.CompareBizOs(org2, ZString.Empty);
				AssertEquals("Compare master PK", org.PK, response.ScoringResult.MasterPK);
				AssertEquals("Compare target PK", org2.PK, response.ScoringResult.TargetPK);
				AssertEquals("Compare ratings", ConfidenceRating.High, response.ScoringResult.ConfidenceRating);

				org2.OH_FullName = "Confidence Rating Medium";
				response = duplicationFinder.CompareBizOs(org2, ZString.Empty);
				AssertEquals("Compare ratings", ConfidenceRating.Medium, response.ScoringResult.ConfidenceRating);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestCompareBusinessObjects_WithIgnores()
		{
			var list = ValidTestData(Factory);
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

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(org);
				duplicationFinder.AddIgnore(org2, UserIgnoreStatus.TemporaryIgnore, staffCode);
				var response = duplicationFinder.CompareBizOs(org2, staffCode);
				AssertEquals("Ignore existed", DuplicationResponseMessages.TIGExisted, response.Message);
				AssertEquals("Compare succeeds", DuplicationResponseMessages.TIGExisted, duplicationFinder.CompareBizOs(org2, staffCode).Message);
				duplicationFinder.RemoveTemporaryIgnore(org2);
				response = duplicationFinder.CompareBizOs(org2, staffCode);
				AssertEquals("Compare succeeds", DuplicationResponseMessages.Success, response.Message);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestChangesOnPropertiesCancelCurrentDuplicationFinderProcess()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				var cusCode = org.CustomsCodes.AddNew();
				var brandName = org.BrandsOrRelatedNames.AddNew();
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				org.FindDuplicates();
				var currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				org.OH_FullName = "Test Name Modified";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				org.MainAddress.OA_ValidationStatus = "VAD";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				org.MainAddress.OA_Email = "modified@test.com";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				contact.OC_ContactName = "modified name";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				org.MainWebURL.PU_URL = "modified.test.com";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				cusCode.OK_CustomsRegNo = "123456";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);

				org.FindDuplicates();
				currentDuplicationFinder = org.CurrentDuplicationFinder;
				Assert(!((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				brandName.P1_RelatedName = "modified brand";
				Assert(((OrgHeaderDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
			}
		}
	}

	public class OrgHeaderDuplicationFinderProxyTest : DuplicationFinderProxyTest
	{
		readonly ZString staffCode = "STD";
		[UseSnapshotProtection]
		public override void TestGetPotentialTargets()
		{
			var address1 = "PADDINGTON NSW";
			var email = "ABCD@TEST.COM";
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

			factory.Save();

			var hashedAddress = TextStandardizerHelper.ComputeStringHashFast(address1);
			var hashedEmail = TextStandardizerHelper.ComputeStringHashFast(email);
			var hashedName = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);

			var addTargetName = @"INSERT INTO dbo.PatternMatchingName 
								(PMN_PK, PMN_HashedValue, PMN_OH, PMN_ParentTableCode, PMN_ParentId, PMN_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedName + "', '" + targetOrg.PK + "', 'OH', '" + targetOrg.PK + "', 'AU')";
			var addTargetAddress = @"INSERT INTO dbo.PatternMatchingAddress 
								(PMA_PK, PMA_HashedValue, PMA_OH, PMA_ParentTableCode, PMA_ParentId, PMA_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedAddress + "', '" + targetOrg.PK + "', 'OA', '" + targetAddress.PK + "', 'AU')";
			var addTargetEmail = @"INSERT INTO dbo.PatternMatchingEmail 
								(PME_PK, PME_HashedValue, PME_OH, PME_ParentTableCode, PME_ParentId, PME_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedEmail + "', '" + targetOrg.PK + "', 'OA', '" + targetAddress.PK + "', 'AU')";

			Db.Connection.ExecuteNonQuery(addTargetName);
			Db.Connection.ExecuteNonQuery(addTargetAddress);
			Db.Connection.ExecuteNonQuery(addTargetEmail);

			var registrySetting = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)masterOrg).ShouldRunDeduplication = true;
				var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(masterOrg);
				var duplicates = duplicationFinder.GetPotentialTargets(ZString.Empty);

				AssertEquals(1, duplicates.Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargetsAsync()
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

						var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(org);

						return duplicationFinder.GetPotentialDuplicatesAsync(staffCode).Result;
					}
					finally
					{
						OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			var result2 = instance.Result.ToList();

			AssertEquals(1, result2.Count);
		}
	}
}
