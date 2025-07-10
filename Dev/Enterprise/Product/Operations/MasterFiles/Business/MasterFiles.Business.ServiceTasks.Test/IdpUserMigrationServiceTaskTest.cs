using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterFiles.Business.ServiceTasks.Test
{
	[TestFixture]
	public class IdpUserMigrationServiceTaskDummyTest
	{
		[Test]
		public void AlwaysPasses()
		{
			Assert.Pass("this dummy test is to satisfy DAT");
		}
	}

	[TestedType(typeof(IdpUserMigrationServiceTask))]
	class IdpUserMigrationServiceTaskTest : ServiceTaskTestCase<IdpUserMigrationServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "IDP", hostedServiceAttribute.Code);
				AssertEquals("Description", "IDP Import Service Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "ACC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "15minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "1hour", hostedServiceAttribute.DefaultScheduleRunEvery);
				Assert("ActiveByDefault", hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestTaskDoesntRunWithDisabledRegistry()
		{
			SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);
			service.RunTask(CancellationToken.None);

			AssertEquals($"Information|Please enable registry {SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.GetLocation()} to run this task.", log[0]);
		}

		public void TestRunTask_ShouldMigrateStaffAndSetIdpUserId()
		{
			var staff = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);

			var idpUser = new MigratedUser
			{
				FullName = "John Doe",
				WorkEmail = "jdoe@company.com",
				LoginName = "jdoe"
			};

			var createdUserInfo = new CreatedUserInfo
			{
				LoginName = idpUser.LoginName,
				Email = idpUser.WorkEmail,
				ErrorMessage = null
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(new[] { createdUserInfo });
			service.SetStaffForMigration(new[] { staff });

			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff.PK);
			AssertContains("Information|Staff records migrated: " + staff.GS_EmailAddress, log[1]);
		}

		public void TestSingleStaff_Fail()
		{
			var staff = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);

			var idpUser = new MigratedUser
			{
				FullName = "John Doe",
				WorkEmail = "jdoe@company.com",
				LoginName = "jdoe"
			};

			var createdUserInfo = new CreatedUserInfo
			{
				LoginName = idpUser.LoginName,
				Email = idpUser.WorkEmail,
				ErrorMessage = "Failed to migrate user."
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(new[] { createdUserInfo });
			service.SetStaffForMigration(new[] { staff });

			service.RunTask(CancellationToken.None);

			AssertBadStaff(staff.PK, "Failed to migrate user.", createdUserInfo);
			AssertContains("Warning|Error importing user [" + staff.GS_EmailAddress + "]: " + createdUserInfo.ErrorMessage, log[1]);
		}

		public void TestBulk_Success()
		{
			var staff1 = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);
			var staff2 = GetStaff("asmith", "Alice Smith", "asmith@company.com", "alicesmith@gmail.com", true);

			var idpUser1 = new MigratedUser
			{
				FullName = staff1.GS_FullName,
				WorkEmail = staff1.GS_EmailAddress,
				LoginName = staff1.GS_LoginName
			};

			var idpUser2 = new MigratedUser
			{
				FullName = staff2.GS_FullName,
				WorkEmail = staff2.GS_EmailAddress,
				LoginName = staff2.GS_LoginName
			};

			var expectedResults = new List<CreatedUserInfo>
			{
				new CreatedUserInfo { LoginName = idpUser1.LoginName, Email = idpUser1.WorkEmail,ErrorMessage = null },
				new CreatedUserInfo { LoginName = idpUser2.LoginName, Email = idpUser2.WorkEmail, ErrorMessage = null }
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(expectedResults.ToArray());
			service.SetStaffForMigration(new[] { staff1, staff2 });

			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff1.PK);
			AssertGoodStaff(staff2.PK);

			AssertContains("Information|Staff records migrated: " + staff1.GS_EmailAddress + ", " + staff2.GS_EmailAddress, log[1]);
		}

		public void TestBulk_SomeSuccess()
		{
			var staff1 = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);
			var staff2 = GetStaff("asmith", "Alice Smith", "asmith@company.com", "alicesmith@gmail.com", true);

			var idpUser1 = new MigratedUser
			{
				FullName = staff1.GS_FullName,
				WorkEmail = staff1.GS_EmailAddress,
				LoginName = staff1.GS_LoginName,
			};

			var idpUser2 = new MigratedUser
			{
				FullName = staff2.GS_FullName,
				WorkEmail = staff2.GS_EmailAddress,
				LoginName = staff2.GS_LoginName,
			};

			var expectedResults = new List<CreatedUserInfo>
			{
				new CreatedUserInfo { LoginName = idpUser1.LoginName, Email = idpUser1.WorkEmail, ErrorMessage = null },
				new CreatedUserInfo { LoginName = idpUser2.LoginName, Email = idpUser2.WorkEmail, ErrorMessage = "Error during migration" } // Failing migration
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(expectedResults.ToArray());
			service.SetStaffForMigration(new[] { staff1, staff2 });

			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff1.PK);
			AssertBadStaff(staff2.PK, "Error during migration", expectedResults[1]);

			AssertContainsLog($"Information|Staff records migrated: {staff1.GS_EmailAddress}", log);
			AssertContainsLog($"Warning|Error importing user [{staff2.GS_EmailAddress}]: Error during migration", log);
		}

		public void TestBulk_DuplicateEmail()
		{
			var staff1 = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);
			var staff2 = GetStaff("asmith", "Alice Smith", "jdoe@company.com", "alicesmith@gmail.com", true);

			var idpUser1 = new MigratedUser
			{
				FullName = staff1.GS_FullName,
				WorkEmail = staff1.GS_EmailAddress,
				LoginName = staff1.GS_LoginName
			};

			var idpUser2 = new MigratedUser
			{
				FullName = staff2.GS_FullName,
				WorkEmail = "alicesmith@gmail.com",
				LoginName = staff2.GS_LoginName
			};

			var expectedResults = new List<CreatedUserInfo>
			{
				new CreatedUserInfo { LoginName = idpUser1.LoginName, Email = idpUser1.WorkEmail,ErrorMessage = null },
				new CreatedUserInfo { LoginName = idpUser2.LoginName, Email = idpUser2.WorkEmail, ErrorMessage = null }
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(expectedResults.ToArray());
			service.SetStaffForMigration(new[] { staff1, staff2 });

			service.RunTask(CancellationToken.None);

			AssertContains($"Warning|Duplicate emails ignored: {staff1.GS_EmailAddress}", log[1]);
			AssertContains($"Warning|Unexpected user in Response: [{idpUser1.WorkEmail}]", log[2]);
			AssertContains($"Warning|Unexpected user in Response: [{idpUser2.WorkEmail}]", log[3]);
			AssertContains("Information|Staff records migrated:", log[4]);
		}

		void AssertContainsLog(string expectedLog, TestServiceLogger logs)
		{
			for (int i = 0; i < logs.Count; i++)
			{
				if (logs[i] == expectedLog)
				{
					Assert(true);
					return;
				}
			}

			Fail($"Log [{expectedLog}] not found");
		}

		public void TestBulk_Fail()
		{
			var staff1 = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);
			var staff2 = GetStaff("asmith", "Alice Smith", "asmith@company.com", "alicesmith@gmail.com", true);

			var expectedResults = new List<CreatedUserInfo>
			{
				new CreatedUserInfo { ErrorMessage = "Error during migration" },
				new CreatedUserInfo { ErrorMessage = "Error during migration" }
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(expectedResults.ToArray());
			service.SetStaffForMigration(new[] { staff1, staff2 });

			service.RunTask(CancellationToken.None);

			AssertBadStaff(staff1.PK, "Error during migration", expectedResults[0]);
			AssertBadStaff(staff2.PK, "Error during migration", expectedResults[1]);

			AssertContains($"Warning|User not found in Response: [{staff1.GS_EmailAddress}], [{staff1.PK}]", log[1]);
			AssertContains($"Warning|User not found in Response: [{staff2.GS_EmailAddress}], [{staff2.PK}]", log[2]);
		}

		public void TestBadPersonWontSave()
		{
			var staff = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);

			var idpUser = new MigratedUser
			{
				FullName = staff.GS_FullName,
				WorkEmail = staff.GS_EmailAddress,
				LoginName = staff.GS_LoginName,
			};

			var expectedResults = new List<CreatedUserInfo>
			{
				new CreatedUserInfo { LoginName = idpUser.LoginName, Email = idpUser.WorkEmail, ErrorMessage = null },
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(expectedResults.ToArray());
			service.SetStaffForMigration(new[] { staff });

			staff.Person.PER_FullName = ZString.Empty;
			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff.PK);
			AssertContains("Information|Staff records migrated: " + staff.GS_EmailAddress, log[1]);
		}

		public void TestMigrateTwice_FailSuccess()
		{
			var staff = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);

			var idpUser = new MigratedUser
			{
				FullName = staff.GS_FullName,
				WorkEmail = staff.GS_EmailAddress,
				LoginName = staff.GS_LoginName
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			// First migration (Fail)
			var result = new CreatedUserInfo { LoginName = staff.GS_LoginName, Email = idpUser.WorkEmail, ErrorMessage = "Error during migration" };
			service.SetExpectedCreatedUserInfos(new[] { result });
			service.SetStaffForMigration(new[] { staff });
			service.RunTask(CancellationToken.None);

			AssertBadStaff(staff.PK, "Error during migration", result);

			// Second migration (Success)
			service.SetExpectedCreatedUserInfos(new[] { new CreatedUserInfo { LoginName = idpUser.LoginName, Email = idpUser.WorkEmail, ErrorMessage = null } });
			service.SetStaffForMigration(new[] { staff });
			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff.PK);
		}

		public void TestSingleSuccess_InactiveAccount()
		{
			var staff = GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);

			var idpUser = new MigratedUser
			{
				FullName = "John Doe",
				WorkEmail = "jdoe@company.com",
				LoginName = "jdoe"
			};

			var createdUserInfo = new CreatedUserInfo
			{
				LoginName = idpUser.LoginName,
				Email = idpUser.WorkEmail,
				ErrorMessage = null
			};

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);

			service.SetExpectedCreatedUserInfos(new[] { createdUserInfo });
			service.SetStaffForMigration(new[] { staff });

			service.RunTask(CancellationToken.None);

			AssertGoodStaff(staff.PK);
			AssertContains("Information|Staff records migrated: " + staff.GS_EmailAddress, log[1]);
		}

		public void TestDoesNotMigrateNonHumans()
		{
			var systemStaff = GetStaff("syslogin", "Test system", string.Empty, string.Empty, true, true, false, false, false);
			var resourceStaff = GetStaff("reslogin", "Test resource", string.Empty, string.Empty, true, false, true, false, false);
			var robotStaff = GetStaff("roblogin", "Test robot", string.Empty, string.Empty, true, false, false, true, false);
			var deviceStaff = GetStaff("devicelogin", "Test device", string.Empty, string.Empty, true, false, false, false, true);

			var name = "Boberson";
			var userStaff = GetStaff("userlogin", name, "boberson@gmail.com", "", true);

			Factory.Save();

			var query = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, false);
			query.AddToFilter(GlbStaffSchema.GS_IsRobot, false);
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(GlbStaffSchema.GS_IsDevice, false);

			var count = Factory.GetDatabaseCount(typeof(GlbStaff), query);

			var service = new IdpUserMigrationServiceTaskForTest();
			var log = InitialiseTaskSchedule(service);
			service.RunTask();

			var staffForMigration = service.LastBatch;

			AssertNotNull(staffForMigration);
			AssertEquals(count, staffForMigration!.Length);

			var bobersonFound = false;
			foreach (var staff in staffForMigration)
			{
				if (staff.GS_FullName.Equals(name))
				{
					bobersonFound = true;
				}
			}

			Assert(bobersonFound);
		}

		void AssertGoodStaff(ZGuid pk)
		{
			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(pk);
			AssertEquals(Guid.Empty, staffReloaded.Person.PER_IDPUserId);
			var staffLog = staffReloaded.Logs.Find(l => l.SL_SE_NKEvent.Equals(Events.IDPImportCode));
			AssertEquals(1, staffLog.Count());
		}

		void AssertBadStaff(ZGuid pk, string expectedError, CreatedUserInfo result)
		{
			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(pk);
			AssertEquals(Guid.Empty, staffReloaded.Person.PER_IDPUserId);
			AssertEquals(expectedError, result.ErrorMessage);

			var log = staffReloaded.Logs.Find(l => l.SL_SE_NKEvent.Equals(Events.IDPImportCode));
			AssertEquals(0, log.Count());

			log = staffReloaded.Logs.Find(l => l.SL_SE_NKEvent.Equals(Events.IDPImportFailureCode));
			AssertEquals(1, log.Count());
		}

		public void TestQueueLengthIsZeroWithDisabledRegistry()
		{
			var result = GetQueueResultForLengthTest(false);

			AssertEquals(QueueResult.Zero, result);
		}

		public void TestQueueLengthWithEnabledRegistry()
		{
			var result = GetQueueResultForLengthTest(true);

			AssertEquals(2, result.QueueSize);
		}

		QueueResult GetQueueResultForLengthTest(bool enableRegistry)
		{
			SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegistry);
			GetStaff("jdoe", "John Doe", "jdoe@company.com", "johndoe@gmail.com", true);
			GetStaff("asmith", "Alice Smith", "asmith@company.com", "alicesmith@gmail.com", true);

			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(IdpUserMigrationServiceTask.Code);
			return queueProvider.QueueResult;
		}

		GlbStaff GetStaff(string username, string fullName, string email, string personalEmail, bool isActive, bool isSystem = false, bool isResource = false, bool isRobot = false, bool isDevice = false)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsSystemAccount = isSystem;
			staff.GS_IsResource = isResource;
			staff.GS_IsRobot = isRobot;
			staff.GS_IsDevice = isDevice;

			staff.GS_LoginName = username;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			staff.GS_IsActive = isActive;

			Factory.Save();

			staff.Person.PER_EmailAddress = personalEmail;

			Factory.Save();

			return staff;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		class IdpUserMigrationServiceTaskForTest : IdpUserMigrationServiceTask
		{
			internal GlbStaff[]? LastBatch;
			protected override IEnumerable<CreatedUserInfo> MigrateUsers(IEnumerable<GlbStaff> staffs)
			{
				LastBatch = staffs.ToArray();
				return CreatedUserInfosOverride ?? Enumerable.Empty<CreatedUserInfo>();
			}

			CreatedUserInfo[]? CreatedUserInfosOverride;

			internal void SetExpectedCreatedUserInfos(CreatedUserInfo[] createdUserInfos)
			{
				CreatedUserInfosOverride = createdUserInfos;
			}

			GlbStaff[]? staffForMigration;
			public void SetStaffForMigration(GlbStaff[] staff)
			{
				staffForMigration = staff;
			}

			protected override ZQuery GetStaffQuery()
			{
				var query = base.GetStaffQuery();

				if (staffForMigration != null)
				{
					query.AddToFilter(GlbStaffSchema.PK, staffForMigration.Select(s => s.PK));
				}

				return query;
			}
		}
	}
}
