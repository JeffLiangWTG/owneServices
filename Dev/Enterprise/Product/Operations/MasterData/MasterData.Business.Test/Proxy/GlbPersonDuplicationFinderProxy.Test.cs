using System;
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
	public class GlbPersonDuplicationFinderProxyTestWithFactory : DuplicationFinderProxyTestWithFactory
	{
		readonly ZString staffCode = GlbStaff.CurrentUser.GS_Code;

		public override void TestAddExclusion()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(org);
				AssertEquals("AddExclusion succeeds", DuplicationResponseMessages.Success, duplicationFinder.AddExclusion(staffCode).Message);
				AssertEquals("AddExclusion exclusion exists", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion("ABC").Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, staffCode).Message);
				AssertEquals("Excluded message", DuplicationResponseMessages.Exclusion, duplicationFinder.CompareBizOs(org1, "ABC").Message);

				AssertEquals("AddExclusion already existed", DuplicationResponseMessages.Exclusion, duplicationFinder.AddExclusion(staffCode).Message);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestAddIgnore()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			Factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(org);
				AssertEquals("AddIgnore temporary", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, staffCode).Message);
				AssertEquals("Ignored message", DuplicationResponseMessages.TIGExisted, duplicationFinder.CompareBizOs(org1, staffCode).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.TIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, staffCode).Message);

				AssertEquals("AddIgnore permanent", DuplicationResponseMessages.Success, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, staffCode).Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.PermanentIgnore, "ABC").Message);
				AssertEquals("AddIgnore already exists", DuplicationResponseMessages.PIGExisted, duplicationFinder.AddIgnore(org1, UserIgnoreStatus.TemporaryIgnore, "ORG").Message);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestCompareBusinessObjects()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];
			org3.PER_IsActive = false;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = org.TablePrefix;
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(org);
				var response = duplicationFinder.CompareBizOs(org2, ZString.Empty);
				AssertEquals("Compare master PK", org.PK, response.ScoringResult.MasterPK);
				AssertEquals("Compare target PK", org2.PK, response.ScoringResult.TargetPK);
				AssertEquals("Compare ratings", ConfidenceRating.High, response.ScoringResult.ConfidenceRating);

				org2.PER_FullName = "Confidence Rating Medium";
				response = duplicationFinder.CompareBizOs(org2, ZString.Empty);
				AssertEquals("Compare ratings", ConfidenceRating.Medium, response.ScoringResult.ConfidenceRating);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestCompareBusinessObjects_WithIgnores()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];
			var org3 = list[2];
			org3.PER_IsActive = false;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "PER";
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(org);
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
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestChangesOnPropertiesCancelCurrentDuplicationFinderProcess()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var person = Factory.NewWithValidTestData<GlbPerson>();
				((IDeduplicatable)person).ShouldRunDeduplication = true;

				person.FindDuplicates();
				var currentDuplicationFinder = person.CurrentDuplicationFinder;
				Assert(!((GlbPersonDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
				person.PER_FullName = "Test Name Modified";
				Assert(((GlbPersonDuplicationFinderProxy)currentDuplicationFinder).TokenSource.IsCancellationRequested);
			}
		}
	}

	public class GlbPersonDuplicationFinderProxyTest : DuplicationFinderProxyTest
	{
		readonly ZString staffCode = "STD";
		[UseSnapshotProtection]
		public override void TestGetPotentialTargets()
		{
			var address1 = "PADDINGTON NSW";
			var email = "ABCD@TEST.COM";
			var factory = new BusinessObjectFactory();
			var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);

			var masterPerson = list[0];
			masterPerson.PER_EmailAddress = email;
			masterPerson.Address1 = address1;

			var targetPerson = list[1];
			targetPerson.PER_EmailAddress = email;
			targetPerson.Address1 = address1;

			factory.Save();

			var hashedAddress = TextStandardizerHelper.ComputeStringHashFast(address1);
			var hashedEmail = TextStandardizerHelper.ComputeStringHashFast(email);
			var hashedName = TextStandardizerHelper.ComputeStringHashFast(targetPerson.PER_FullName);

			var addTargetName = @"INSERT INTO dbo.PatternMatchingName 
								(PMN_PK, PMN_HashedValue, PMN_PER, PMN_ParentTableCode, PMN_ParentId, PMN_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedName + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";
			var addTargetAddress = @"INSERT INTO dbo.PatternMatchingAddress 
								(PMA_PK, PMA_HashedValue, PMA_PER, PMA_ParentTableCode, PMA_ParentId, PMA_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedAddress + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";
			var addTargetEmail = @"INSERT INTO dbo.PatternMatchingEmail 
								(PME_PK, PME_HashedValue, PME_PER, PME_ParentTableCode, PME_ParentId, PME_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedEmail + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";

			Db.Connection.ExecuteNonQuery(addTargetName);
			Db.Connection.ExecuteNonQuery(addTargetAddress);
			Db.Connection.ExecuteNonQuery(addTargetEmail);

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;
				var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(masterPerson);
				var duplicates = duplicationFinder.GetPotentialTargets(ZString.Empty);

				AssertEquals(1, duplicates.Count());
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
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
					var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);
					var address1 = "PADDINGTON NSW";
					var email = "ABCD@TEST.COM";

					var masterPerson = list[0];
					masterPerson.PER_EmailAddress = email;
					masterPerson.Address1 = address1;

					var targetPerson = list[1];
					targetPerson.PER_EmailAddress = email;
					targetPerson.Address1 = address1;

					factory.Save();

					var hashedAddress = TextStandardizerHelper.ComputeStringHashFast(address1);
					var hashedEmail = TextStandardizerHelper.ComputeStringHashFast(email);
					var hashedName = TextStandardizerHelper.ComputeStringHashFast(targetPerson.PER_FullName);

					var addTargetName = @"INSERT INTO dbo.PatternMatchingName 
								(PMN_PK, PMN_HashedValue, PMN_PER, PMN_ParentTableCode, PMN_ParentId, PMN_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedName + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";
					var addTargetAddress = @"INSERT INTO dbo.PatternMatchingAddress 
								(PMA_PK, PMA_HashedValue, PMA_PER, PMA_ParentTableCode, PMA_ParentId, PMA_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedAddress + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";
					var addTargetEmail = @"INSERT INTO dbo.PatternMatchingEmail 
								(PME_PK, PME_HashedValue, PME_PER, PME_ParentTableCode, PME_ParentId, PME_RN_NKCountryCode) 
								VALUES (NEWID(), '" + hashedEmail + "', '" + targetPerson.PK + "', 'PER', '" + targetPerson.PK + "', 'AU')";

					Db.Connection.ExecuteNonQuery(addTargetName);
					Db.Connection.ExecuteNonQuery(addTargetAddress);
					Db.Connection.ExecuteNonQuery(addTargetEmail);

					factory.Save();

					var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
					try
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

						var duplicationFinder = GlbPersonDuplicationFinderProxy.GetInstance(masterPerson);

						return duplicationFinder.GetPotentialDuplicatesAsync(staffCode).Result;
					}
					finally
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			var result2 = instance.Result.ToList();

			AssertEquals(1, result2.Count);
		}
	}
}
