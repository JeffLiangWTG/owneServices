using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ClientIntelligenceFormTestWithOutFactory : TestCase
	{
		[UseSnapshotProtection]
		public void TestFindDuplicatesInClientIntelligence()
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
				using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(masterOrg))
				{
					var duplicationFinder = OrgHeaderDuplicationFinderProxy.GetInstance(masterOrg);
					var duplicates = duplicationFinder.GetPotentialTargets(ZString.Empty);
					AssertEquals("Pre-Condition: Duplicates found for current organization", 1, duplicates.Count());

					form.Show();
					AssertNoExceptionThrown("Find duplications functions works well in client intelligence form", () =>
					{
						var findDuplicates = Task.Factory.StartNew(() =>
						{
							masterOrg.FindDuplicates();
						}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest());

						findDuplicates.Wait();
						AssertEquals("Duplicates found", (form.Controls.Find("DuplicateDetectionStatusLabel", true)[0] as ZLabel).Text);
						findDuplicates.Dispose();
					});
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Collect logs to fix Amnesty WI00591831
				Assert(ex.ToString(), false);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}
	}
}
