using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Domains.Builders;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Parser;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Persistence;
#if NET48
using System;
using System.Collections.Generic;
using System.Linq;
#endif

namespace Enterprise.Services.Scim.Business.Test
{
	sealed class GroupRepositoryTest : TestCaseWithFactory
	{
		static readonly ISCIMSchemaQueryRepository scimSchemaQueryRepository = ScimProcessorTestHelper.ReturnSchemaQueryRepository();
		GroupRepository repository = new GroupRepository(scimSchemaQueryRepository);

		#region Create
		public void TestCreate()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			var group = Factory.Load<GlbGroup>(new ZGuid(id));

			AssertNotNull(group);
			AssertEquals("123", group.GG_ExternalId);
			AssertEquals("some group", group.GG_Desc);
			AssertNotEquals("", group.GG_Code);
		}

		public void TestCreate_GoodCategory()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("C1", "Category 1")
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group", "C1");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			var group = Factory.Load<GlbGroup>(new ZGuid(id));

			AssertNotNull(group);
			AssertEquals("123", group.GG_ExternalId);
			AssertEquals("some group", group.GG_Desc);
			AssertNotEquals("", group.GG_Code);
			AssertEquals("C1", group.GG_Category);
		}

		public void TestCreate_BadCategory()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group", "XXX");

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();
			});
		}

		public void TestCreate_LongDisplayName()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("567", "NEW NESTED GROUP MID TIER");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			var group = Factory.Load<GlbGroup>(new ZGuid(id));

			AssertNotNull(group);
			AssertEquals("567", group.GG_ExternalId);
			AssertEquals("NEW NESTED GROUP MID TIER", group.GG_Desc);
			AssertNotEquals("", group.GG_Code);

			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("5678", "NEW NESTED GROUP MID TIER 2");

			string id2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id.ToString();

			var group2 = Factory.Load<GlbGroup>(new ZGuid(id2));

			AssertNotNull(group2);
			AssertEquals("5678", group2.GG_ExternalId);
			AssertEquals("NEW NESTED GROUP MID TIER 2", group2.GG_Desc);
			AssertNotEquals("", group2.GG_Code);
		}

		public void TestCreate_DuplicateExternalId()
		{
			string externalId = "123";
			string displayName1 = "name1";
			string displayName2 = "name2";

			var scimGroup = new ScimGroup();
			scimGroup.DisplayName = displayName1;
			scimGroup.ExternalId = externalId;

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			AssertNotEquals(string.Empty, id);

			scimGroup = new ScimGroup();
			scimGroup.DisplayName = displayName2;
			scimGroup.ExternalId = externalId;

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult(); });
		}

		public void TestCreate_WithMembers_ExistingUser()
		{
			var scimUser = ScimProcessorTestHelper.CreateScimUser("1", "1", "XX first middle last, III", "first", "middle", "last", "street addr", "city", "0499123456", "1@email.com", "title", "III", "XX", "AU", "ENG");
			var userId = new UserRepository(scimSchemaQueryRepository).CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id;

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group", new GroupMember() { Value = userId.ToString() });

			var groupId = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id;

			AssertNotEquals(Guid.Empty, groupId);

			var group = Factory.Load<GlbGroup>(new ZGuid(groupId));
			AssertNotNull(group);

			AssertEquals(1, group.Staff.Count);
			AssertEquals(userId, group.Staff[0].PK.ToGuid());
			AssertEquals(true, group.Staff[0].GS_CanLogin);
			AssertEquals(false, group.Staff[0].GS_IsController);
			AssertEquals(false, group.Staff[0].GS_IsRobot);
		}

		public void TestCreate_WithMembers_NonExistingUser()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group", new GroupMember() { Value = Guid.NewGuid().ToString() });

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{ repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult(); });
		}

		public void TestCreate_WithMembers_NonExistingGroup()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group", new GroupMember() { Value = Guid.NewGuid().ToString(), Type = SCIMResourceTypes.Group });

			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{ repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult(); });
		}

		public void TestCreate_WithMembers_ParentGroup()
		{
			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("123", "some group");

			var id1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("456", "some other group", new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.Group });

			var id2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;
			AssertNotEquals(Guid.Empty, id2);

			var group = Factory.Load<GlbGroup>(new ZGuid(id1));
			AssertNotNull(group);
			AssertNotNull(group.ParentGroup);

			AssertEquals(id2, group.ParentGroup.PK.ToGuid());
		}

		public void TestCreate_WithMembers_ExistingUser_CanLogin_DefaultFalse()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_CanLogin", "UAP-CWDEV-CanLogin")))
			{
				Env.Registry.ScimCanLogin = false;
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-CanLogin", true, false, false, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_CanLogin_LowCase()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_CanLogin", "UAP-CWDEV-CanLogin")))
			{
				AssertCreateGroupAndStaffFlag("uap-cwdev-canlogin", true, false, false, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsController()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsController", "UAP-CWDEV-IsController")))
			{
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-IsController", false, true, false, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsController_LowCase()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsController", "UAP-CWDEV-IsController")))
			{
				AssertCreateGroupAndStaffFlag("uap-cwdev-iscontroller", false, true, false, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsRobot()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsRobot", "UAP-CWDEV-IsRobot")))
			{
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-IsRobot", false, false, true, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsRobot_LowCase()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsRobot", "UAP-CWDEV-IsRobot")))
			{
				AssertCreateGroupAndStaffFlag("uap-cwdev-isrobot", false, false, true, false, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsDatabaseDeveloper()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("IsDatabaseDeveloper", "UAP-CWDEV-DbDev")))
			{
				Env.Registry.ScimCanLogin = false;
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-DbDev", false, false, false, true, false, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsReadOnlyDBUser()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("IsReadOnlyDBUser", "UAP-CWDEV-DbReader")))
			{
				Env.Registry.ScimCanLogin = false;
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-DbReader", false, false, false, false, true, false);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_IsBackupOperator()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("IsBackupOperator", "UAP-CWDEV-Backup")))
			{
				Env.Registry.ScimCanLogin = false;
				AssertCreateGroupAndStaffFlag("UAP-CWDEV-Backup", false, false, false, false, false, true);
			}
		}

		void AssertCreateGroupAndStaffFlag(string groupName, bool canLogin, bool isController, bool isRobot, bool isDbDeveloper, bool isReadOnlyDbUser, bool isBackupOperator)
		{
			Env.Registry.ScimCanLogin = false;
			var scimUser = ScimProcessorTestHelper.CreateScimUser("1", "1", "XX first middle last, III", "first", "middle", "last", "street addr", "city", "0499123456", "1@email.com", "title", "III", "XX", "AU", "ENG");
			var userId = new UserRepository(scimSchemaQueryRepository).CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id;

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", groupName, new GroupMember() { Value = userId.ToString() });

			var groupId = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id;

			AssertNotEquals(Guid.Empty, groupId);

			var group = Factory.Load<GlbGroup>(new ZGuid(groupId));
			AssertNotNull(group);

			AssertEquals(1, group.Staff.Count);
			AssertEquals(userId, group.Staff[0].PK.ToGuid());
			AssertEquals(canLogin, group.Staff[0].GS_CanLogin);
			AssertEquals(isController, group.Staff[0].GS_IsController);
			AssertEquals(isRobot, group.Staff[0].GS_IsRobot);
			AssertEquals(isDbDeveloper, group.Staff[0].IsDatabaseDeveloper);
			AssertEquals(isReadOnlyDbUser, group.Staff[0].IsReadOnlyDBUser);
			AssertEquals(isBackupOperator, group.Staff[0].IsBackupOperator);
		}

		public void TestCreate_OneUserTwoGroups()
		{
			Env.Registry.ScimCanLogin = false;
			var scimUser = ScimProcessorTestHelper.CreateScimUser("1", "1", "XX first middle last, III", "first", "middle", "last", "street addr", "city", "0499123456", "1@email.com", "title", "III", "XX", "AU", "ENG");
			var userId = new UserRepository(scimSchemaQueryRepository).CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id;

			var robotsGroup = ScimProcessorTestHelper.CreateScimGroup("123", "UAP-CWDEV-IsRobot", new GroupMember() { Value = userId.ToString() });
			var controllerGroup = ScimProcessorTestHelper.CreateScimGroup("456", "UAP-CWDEV-IsController", new GroupMember() { Value = userId.ToString() });

			var codesCollection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping = codesCollection.AddNew();
			staffColumnToGroupDescriptionScimMapping.StaffColumnName = "GS_IsController";
			staffColumnToGroupDescriptionScimMapping.GroupDescriptionMapping = "UAP-CWDEV-IsController";
			var staffColumnToGroupDescriptionScimMapping2 = codesCollection.AddNew();
			staffColumnToGroupDescriptionScimMapping2.StaffColumnName = "GS_IsRobot";
			staffColumnToGroupDescriptionScimMapping2.GroupDescriptionMapping = "UAP-CWDEV-IsRobot";

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codesCollection))
			{
				var groupIdRobots = repository.CreateSCIMResource(robotsGroup).GetAwaiter().GetResult().Id;
				var groupIdcontroller = repository.CreateSCIMResource(controllerGroup).GetAwaiter().GetResult().Id;

				AssertNotEquals(Guid.Empty, groupIdRobots);
				AssertNotEquals(Guid.Empty, groupIdcontroller);

				var groupRobots = Factory.Load<GlbGroup>(new ZGuid(groupIdRobots));
				AssertNotNull(groupRobots);

				var groupControllers = Factory.Load<GlbGroup>(new ZGuid(groupIdRobots));
				AssertNotNull(groupControllers);

				AssertEquals(1, groupRobots.Staff.Count);
				AssertEquals(userId, groupRobots.Staff[0].PK.ToGuid());

				AssertEquals(1, groupControllers.Staff.Count);
				AssertEquals(userId, groupControllers.Staff[0].PK.ToGuid());

				AssertEquals(false, groupRobots.Staff[0].GS_CanLogin);
				AssertEquals(true, groupRobots.Staff[0].GS_IsController);
				AssertEquals(true, groupRobots.Staff[0].GS_IsRobot);
			}
		}

		public void TestCreate_WithMembers_ExistingUser_AllFlags()
		{
			Env.Registry.ScimCanLogin = false;
			var scimUser = ScimProcessorTestHelper.CreateScimUser("1", "1", "XX first middle last, III", "first", "middle", "last", "street addr", "city", "0499123456", "1@email.com", "title", "III", "XX", "AU", "ENG");
			var userId = new UserRepository(scimSchemaQueryRepository).CreateSCIMResource(scimUser).GetAwaiter().GetResult().Id;

			var codesCollection = new StaffColumnToGroupDescriptionScimMappingCollection();

			AddGroupAndAssert("GS_IsController", "UAP-CWDEV-IsController", "123", userId, codesCollection,
				canLogin: false, isController: true, isRobot: false, isDbDev: false, isDbReadOnly: false, isBackupOp: false);

			AddGroupAndAssert("GS_IsRobot", "UAP-CWDEV-IsRobot", "456", userId, codesCollection,
				canLogin: false, isController: true, isRobot: true, isDbDev: false, isDbReadOnly: false, isBackupOp: false);

			AddGroupAndAssert("GS_CanLogin", "UAP-CWDEV-CanLogin", "789", userId, codesCollection,
				canLogin: true, isController: true, isRobot: true, isDbDev: false, isDbReadOnly: false, isBackupOp: false);

			AddGroupAndAssert("IsDatabaseDeveloper", "UAP-CWDEV-DbDev", "aaa", userId, codesCollection,
				canLogin: true, isController: true, isRobot: true, isDbDev: true, isDbReadOnly: false, isBackupOp: false);

			AddGroupAndAssert("IsReadOnlyDBUser", "UAP-CWDEV-DbReader", "bbb", userId, codesCollection,
				canLogin: true, isController: true, isRobot: true, isDbDev: true, isDbReadOnly: true, isBackupOp: false);

			AddGroupAndAssert("IsBackupOperator", "UAP-CWDEV-Backups", "ccc", userId, codesCollection,
				canLogin: true, isController: true, isRobot: true, isDbDev: true, isDbReadOnly: true, isBackupOp: true);
		}

		void AddGroupAndAssert(string columnName, string groupDesc, string groupIdStr, Guid userId, StaffColumnToGroupDescriptionScimMappingCollection codesCollection,
			bool canLogin, bool isController, bool isRobot, bool isDbDev, bool isDbReadOnly, bool isBackupOp)
		{
			var mapping = codesCollection.AddNew();
			mapping.StaffColumnName = columnName;
			mapping.GroupDescriptionMapping = groupDesc;

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup(groupIdStr, groupDesc, new GroupMember() { Value = userId.ToString() });

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codesCollection))
			{
				var groupId = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id;

				AssertNotEquals(Guid.Empty, groupId);

				var group = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbGroup>(new ZGuid(groupId));
				AssertNotNull(group);

				AssertEquals(1, group.Staff.Count);
				var staff = group.Staff[0];

				AssertEquals(userId, staff.PK.ToGuid());
				AssertEquals(canLogin, staff.GS_CanLogin);
				AssertEquals(isController, staff.GS_IsController);
				AssertEquals(isRobot, staff.GS_IsRobot);
				AssertEquals(isDbDev, staff.IsDatabaseDeveloper);
				AssertEquals(isDbReadOnly, staff.IsReadOnlyDBUser);
				AssertEquals(isBackupOp, staff.IsBackupOperator);
			}
		}

		StaffColumnToGroupDescriptionScimMappingCollection InitializeStaffColumnToGroupDescriptionScimMappingCollection(string staffColumn, string groupDescription)
		{
			var codesCollection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var staffColumnToGroupDescriptionScimMapping = codesCollection.AddNew();
			staffColumnToGroupDescriptionScimMapping.StaffColumnName = staffColumn;
			staffColumnToGroupDescriptionScimMapping.GroupDescriptionMapping = groupDescription;
			return codesCollection;
		}

		public void TestFindSCIMGroup_Id_SystemAccount()
		{
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_IsSystemDefined, true));
			var repo = new GroupRepository(scimSchemaQueryRepository);

			var found = repo.FindSCIMByResourceId(group.PK.ToString()).GetAwaiter().GetResult();
			AssertNull(found);
		}

		public void TestFindSCIMGroup_Id_OrgGroup()
		{
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Type, "ORG"));
			var repo = new GroupRepository(scimSchemaQueryRepository);

			var found = repo.FindSCIMByResourceId(group.PK.ToString()).GetAwaiter().GetResult();
			AssertNull(found);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ExternalId = "123";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(staff);

			Factory.Save();

			repository.DeleteSCIMResourceById(group.PK.ToGuid().ToString()).GetAwaiter().GetResult();

			var factory = new BusinessObjectFactory();
			group = factory.Load<GlbGroup>(group.PK);
			AssertNull(group);

			staff = factory.Load<GlbStaff>(staff.PK);
			AssertNotNull(staff);
			AssertEquals(1, staff.Groups.Count);
		}

		public void TestDelete_CanLogin()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_CanLogin", "UAP-CWDEV-CanLogin")))
			{
				AssertRemoveFromGroup("UAP-CWDEV-CanLogin", false, true, true);
			}
		}

		public void TestDelete_IsController()
		{
			var otherController = Factory.NewWithValidTestData<GlbStaff>();
			otherController.GS_IsController = true;
			otherController.GS_Code = "CN~";

			Factory.Save();

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsController", "UAP-CWDEV-IsController")))
			{
				AssertRemoveFromGroup("UAP-CWDEV-IsController", true, false, true);
			}
		}

		public void TestDelete_IsRobot()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsRobot", "UAP-CWDEV-IsRobot")))
			{
				AssertRemoveFromGroup("UAP-CWDEV-IsRobot", true, true, false);
			}
		}

		void AssertRemoveFromGroup(string groupName, bool canLogin, bool isController, bool isRobot)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ExternalId = "123";
			group.GG_Desc = groupName;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_CanLogin = true;
			staff.GS_IsController = true;
			staff.GS_IsRobot = true;
			group.Staff.Add(staff);

			Factory.Save();

			repository.DeleteSCIMResourceById(group.PK.ToGuid().ToString()).GetAwaiter().GetResult();

			var factory = new BusinessObjectFactory();
			group = factory.Load<GlbGroup>(group.PK);
			AssertNull(group);

			staff = factory.Load<GlbStaff>(staff.PK);
			AssertNotNull(staff);
			AssertEquals(1, staff.Groups.Count);
			AssertEquals(canLogin, staff.GS_CanLogin);
			AssertEquals(isController, staff.GS_IsController);
			AssertEquals(isRobot, staff.GS_IsRobot);
		}

		public void TestDelete_NotExists()
		{
			AssertNoExceptionThrown(delegate
			{ repository.DeleteSCIMResourceById(ZGuid.NewZGuid().ToString()).GetAwaiter().GetResult(); });
		}

		#endregion

		#region Find

		public void TestFind_ZeroCount()
		{
			CreateSomeGroups();

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 3, 0, null);
			var groups = repository.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(0, groups.Content.Count());
			AssertNotEquals(0, groups.TotalResults);
		}

		public void TestFind_GroupPaging()
		{
			CreateSomeGroups();

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 3, 4, null);
			var users = repository.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(4, users.Content.Count());
			AssertEquals(new Guid("33333333-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[0].Id);
			AssertEquals(new Guid("44444444-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[1].Id);
			AssertEquals(new Guid("55555555-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[2].Id);
			AssertEquals(new Guid("66666666-F5B2-4186-A908-20C66BE7F77D"), users.Content.ToArray()[3].Id);
		}

		void CreateSomeGroups()
		{
			var sql = @"
UPDATE GlbGroup set GG_IsActive = 0, GG_SystemLastEditTimeUtc = DATEADD(MONTH, -2, GETDATE()), GG_SystemLastEditUser = 'X';

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('11111111-F5B2-4186-A908-20C66BE7F77D', '111', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('99999999-F5B2-4186-A908-20C66BE7F77D', '999', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('22222222-F5B2-4186-A908-20C66BE7F77D', '222', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('88888888-F5B2-4186-A908-20C66BE7F77D', '888', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('33333333-F5B2-4186-A908-20C66BE7F77D', '333', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('77777777-F5B2-4186-A908-20C66BE7F77D', '777', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('44444444-F5B2-4186-A908-20C66BE7F77D', '444', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('66666666-F5B2-4186-A908-20C66BE7F77D', '666', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('55555555-F5B2-4186-A908-20C66BE7F77D', '555', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('AAAAAAAA-F5B2-4186-A908-20C66BE7F77D', 'AAA', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('FFFFFFFF-F5B2-4186-A908-20C66BE7F77D', 'FFF', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('BBBBBBBB-F5B2-4186-A908-20C66BE7F77D', 'BBB', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('CCCCCCCC-F5B2-4186-A908-20C66BE7F77D', 'CCC', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);

INSERT INTO GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser, GG_IsActive)
VALUES ('DDDDDDDD-F5B2-4186-A908-20C66BE7F77D', 'DDD', NEWID(), GETDATE(), 'X', GETDATE(), 'X', 1);
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void TestFind_TotalCount()
		{
			CreateSomeGroups();

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 3, 4, null);
			var response = repository.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertEquals(4, response.Content.Count());
			AssertEquals(14, response.TotalResults);
		}

		public void TestFindGroup_ById()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "group name");

			var id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id;

			var group = repository.FindSCIMByResourceId(id.ToString()).GetAwaiter().GetResult();
			AssertNotNull(group);
			if (group == null)
			{
				Fail();
				return;
			}
			AssertEquals("123", group.ExternalId);
		}

		public void TestFindGroup_ExternalId()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
			  .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
			  .Build();

			var group1 = ScimProcessorTestHelper.CreateScimGroup("ext1", "group Name1");
			var group2 = ScimProcessorTestHelper.CreateScimGroup("ext2", "group Name2");

			string id1 = repository.CreateSCIMResource(group1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repository.CreateSCIMResource(group2).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			var foundGroup = repository.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNotNull(foundGroup);
			if (foundGroup == null)
			{
				Fail();
				return;
			}
			AssertEquals("ext2", foundGroup.ExternalId);
			AssertEquals(id2, foundGroup.Id.ToString());
		}

		public void TestFindGroup_MultipleFilters()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				.AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				.Build();

			var group1 = ScimProcessorTestHelper.CreateScimGroup("ext1", "group Name1");
			var group2 = ScimProcessorTestHelper.CreateScimGroup("ext2", "group Name2");

			string id1 = repository.CreateSCIMResource(group1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repository.CreateSCIMResource(group2).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\" and displayName co \"Name2\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			var foundGroup = repository.FindSCIMResource(param).GetAwaiter().GetResult().Content.FirstOrDefault();

			AssertNotNull(foundGroup);
			if (foundGroup == null)
			{
				Fail();
				return;
			}
			AssertEquals("ext2", foundGroup.ExternalId);
			AssertEquals(id2, foundGroup.Id.ToString());
		}

		public void TestFindGroup_MultipleResults()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				.AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				.Build();

			var group1 = ScimProcessorTestHelper.CreateScimGroup("ext1", "group Name1");
			var group2 = ScimProcessorTestHelper.CreateScimGroup("ext2", "group Name2");
			var group3 = ScimProcessorTestHelper.CreateScimGroup("ext3", "group Name3");

			string id1 = repository.CreateSCIMResource(group1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repository.CreateSCIMResource(group2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repository.CreateSCIMResource(group3).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\" or displayName co \"Name3\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain group2", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "ext2"));
			Assert("Results should contain group3", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "ext3"));
		}

		public void TestFindGroup_Complex()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				  .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				  .Build();

			var group1 = ScimProcessorTestHelper.CreateScimGroup("ext1", "group Name1");
			var group2 = ScimProcessorTestHelper.CreateScimGroup("ext2", "group Name2");
			var group3 = ScimProcessorTestHelper.CreateScimGroup("ext3", "group Name3");
			var group4 = ScimProcessorTestHelper.CreateScimGroup("ext4", "group Name4");

			string id1 = repository.CreateSCIMResource(group1).GetAwaiter().GetResult().Id.ToString();
			string id2 = repository.CreateSCIMResource(group2).GetAwaiter().GetResult().Id.ToString();
			string id3 = repository.CreateSCIMResource(group3).GetAwaiter().GetResult().Id.ToString();
			string id4 = repository.CreateSCIMResource(group4).GetAwaiter().GetResult().Id.ToString();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\" or (displayName co \"Name3\" or externalId eq \"ext1\")", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();

			AssertNotNull(result);
			AssertEquals(3, result.TotalResults);
			Assert("Results should contain group1", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "ext1"));
			Assert("Results should contain group2", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "ext2"));
			Assert("Results should contain group3", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "ext3"));
		}

		public void TestFindGroup_NotFound_ByFilter()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				  .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				  .Build();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext2\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);

			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(0, result.TotalResults);
		}

		public void TestFindGroup_NotFound_ById()
		{
			var result = repository.FindSCIMByResourceId(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
			AssertNull(result);
		}

		public void TestFindGroup_ById_Deleted()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("123", "some group");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			repository.DeleteSCIMResourceById(id).GetAwaiter().GetResult();

			var result = repository.FindSCIMByResourceId(id).GetAwaiter().GetResult();
			AssertNull(result);
		}

		[TestDate(2023, 1, 1)]
		public void TestFindGroup_ByFilter_Deleted()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
					 .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
					 .Build();

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("ext1", "some group");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			repository.DeleteSCIMResourceById(id).GetAwaiter().GetResult();

			TestDateAttribute.AddDays(40);

			var filter = SCIMFilterParser.Parse("externalId eq \"ext1\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);

			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(0, result.TotalResults);
		}

		public void TestFindGroup_ByFilter_Deleted_Recent()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
					 .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
					 .Build();

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("ext1", "some group");

			string id = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id.ToString();

			var group = Factory.Load<GlbGroup>(new ZGuid(id));
			group.GG_IsActive = false;
			group.Factory.Save();

			var filter = SCIMFilterParser.Parse("externalId eq \"ext1\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);

			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(1, result.TotalResults);
		}

		public void TestFindGroup_Members()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
					 .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
					 .Build();

			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");
			var user4 = ScimProcessorTestHelper.CreateScimUser("ext4", "userName4", "first4", "middle4", "last4", "address4", "city4", "0499444444", "test4@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;
			var id4 = usersRepo.CreateSCIMResource(user4).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.User }, new GroupMember() { Value = id2.ToString(), Type = SCIMResourceTypes.User });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", new GroupMember() { Value = id3.ToString() }, new GroupMember() { Value = id4.ToString(), Type = SCIMResourceTypes.User });
			var scimGroup3 = ScimProcessorTestHelper.CreateScimGroup("g3", "group3");

			var id5 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;
			var id6 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;
			var id7 = repository.CreateSCIMResource(scimGroup3).GetAwaiter().GetResult().Id;

			var scimGroup4 = ScimProcessorTestHelper.CreateScimGroup("g4", "group4", new GroupMember() { Value = id7.ToString(), Type = SCIMResourceTypes.Group });
			var id8 = repository.CreateSCIMResource(scimGroup4).GetAwaiter().GetResult().Id;

			var filter = SCIMFilterParser.Parse($"members[value eq \"{id1}\"] and displayName ne \"ALL STAFF\"", new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			var result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(1, result.TotalResults);
			var content = result.Content;
			AssertNotNull(content);
			if (content == null)
			{
				Fail();
				return;
			}
			var first = content.First();
			if (first == null)
			{
				Fail();
				return;
			}

			AssertEquals(id5, first.Id);

			var members = first.Members.ToArray();
			AssertEquals(2, members.Length);

			AssertEquals("User", members[0].Type);
			AssertEquals("../Users/" + members[0].Value, members[0].Ref);

			AssertEquals("User", members[1].Type);
			AssertEquals("../Users/" + members[1].Value, members[1].Ref);

			AssertNotEquals(members[0].Value, members[1].Value);

			filter = SCIMFilterParser.Parse($"members[value eq \"{id3}\"] and displayName ne \"ALL STAFF\"", new List<SCIMSchema> { schema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(1, result.TotalResults);

			content = result.Content;
			AssertNotNull(content);
			if (content == null)
			{
				Fail();
				return;
			}

			first = content.First();
			if (first == null)
			{
				Fail();
				return;
			}

			AssertEquals(id6, first.Id);

			filter = SCIMFilterParser.Parse($"members[value eq \"{id7}\"] and displayName ne \"ALL STAFF\"", new List<SCIMSchema> { schema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(1, result.TotalResults);

			first = result.Content.First();
			if (first == null)
			{
				Fail();
				return;
			}

			AssertEquals(id8, first.Id);

			filter = SCIMFilterParser.Parse($"members[value eq \"{id3}\" or value eq \"{id7}\"] and displayName ne \"ALL STAFF\"", new List<SCIMSchema> { schema });
			param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter);
			result = repository.FindSCIMResource(param).GetAwaiter().GetResult();
			AssertEquals(2, result.TotalResults);
			Assert("Results should contain group2", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "g2"));
			Assert("Results should contain group4", result.Content.Cast<ScimGroup>().Any(g => g.ExternalId == "g4"));

			var group4 = result.Content.Cast<ScimGroup>().First(g => g.ExternalId == "g4");
			AssertNotNull(group4);

			members = group4.Members.ToArray();
			AssertEquals(1, members.Length);
			AssertEquals("Group", members[0].Type);
			AssertEquals(id7.ToString(), members[0].Value);
			AssertEquals("../Groups/" + id7, members[0].Ref);
		}

		public void TestFind_ExcludedAttribute_Members()
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				 .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse("externalId ne \"\"", new List<SCIMSchema> { schema });

			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");

			var userId = usersRepo.CreateSCIMResource(user).GetAwaiter().GetResult().Id;

			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember[] { new GroupMember() { Value = userId.ToString(), Type = SCIMResourceTypes.User } });

			var groupId = repository.CreateSCIMResource(scimGroup).GetAwaiter().GetResult().Id;

			var attribute = new SCIMAttributeExpression("members");
			var list = new List<SCIMAttributeExpression>();
			list.Add(attribute);

			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 1, 100, null, filter: filter, excludedAttributes: list);
			var foundGroup = repository.FindSCIMResource(param).GetAwaiter().GetResult().Content.ToList();

			AssertEquals(1, foundGroup.Count);

			var group = foundGroup[0];
			AssertNotNull(group);
			if (group == null)
			{
				Fail();
				return;
			}

			AssertEquals("group1", group.DisplayName);
			AssertEquals("g1", group.ExternalId);
			AssertEquals(0, group.Members.Count());
		}

		#endregion

		#region Update

		public void TestUpdade_Concurrency()
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember[] { new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.User } });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", new GroupMember[] { new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.User } });

			var idGroup = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			using (var cmd = Db.Connection.Command($"delete from GlbGroupLink where GK_GG = '{idGroup}'"))
			{
				cmd.ExecuteNonQuery();
			}

			AssertNoExceptionThrown(delegate
			{
				var updated = repository.UpdateSCIMResourceById(idGroup.ToString(), scimGroup2).GetAwaiter().GetResult();
			});
		}

		public void TestUpdate()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("C1", "Category 1")
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");
			var user4 = ScimProcessorTestHelper.CreateScimUser("ext4", "userName4", "first4", "middle4", "last4", "address4", "city4", "0499444444", "test4@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;
			var id4 = usersRepo.CreateSCIMResource(user4).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember[] { new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.User }, new GroupMember() { Value = id2.ToString(), Type = SCIMResourceTypes.User } });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", "C1", new GroupMember[] { new GroupMember() { Value = id1.ToString() }, new GroupMember() { Value = id2.ToString(), Type = SCIMResourceTypes.User } });

			var id5 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var updated = repository.UpdateSCIMResourceById(id5.ToString(), scimGroup2).GetAwaiter().GetResult();
			AssertNotNull(updated);
			if (updated == null)
			{
				Fail();
				return;
			}

			AssertEquals("g2", updated.ExternalId);
			AssertEquals("group2", updated.DisplayName);
			AssertEquals("C1", updated.Category);

			var loaded = Factory.Load<GlbGroup>(id5);
			AssertEquals("g2", loaded.GG_ExternalId);
			AssertEquals("group2", loaded.GG_Desc);
			AssertEquals("C1", loaded.GG_Category);
		}

		public void TestUpdate_CanLogin()
		{
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_CanLogin", "UAP-CWDEV-CanLogin")))
			{
				AssertUpdate("UAP-CWDEV-CanLogin", true, false, false);
			}
		}

		public void TestUpdate_IsController()
		{
			Env.Registry.ScimCanLogin = false;
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsController", "UAP-CWDEV-IsController")))
			{
				AssertUpdate("UAP-CWDEV-IsController", false, true, false);
			}
		}

		public void TestUpdate_IsRobot()
		{
			Env.Registry.ScimCanLogin = false;
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsRobot", "UAP-CWDEV-IsRobot")))
			{
				AssertUpdate("UAP-CWDEV-IsRobot", false, false, true);
			}
		}

		void AssertUpdate(string groupName, bool canLogin, bool isController, bool isRobot)
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember[] { new GroupMember() { Value = id1.ToString(), Type = SCIMResourceTypes.User }, new GroupMember() { Value = id2.ToString(), Type = SCIMResourceTypes.User } });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", groupName, new GroupMember[] { new GroupMember() { Value = id1.ToString() }, new GroupMember() { Value = id3.ToString(), Type = SCIMResourceTypes.User } });

			var id5 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var updated = repository.UpdateSCIMResourceById(id5.ToString(), scimGroup2).GetAwaiter().GetResult();
			AssertNotNull(updated);
			if (updated == null)
			{
				Fail();
				return;
			}

			AssertEquals("g2", updated.ExternalId);
			AssertEquals(groupName, updated.DisplayName);

			var loaded = Factory.Load<GlbGroup>(id5);
			AssertEquals("g2", loaded.GG_ExternalId);
			AssertEquals(groupName, loaded.GG_Desc);

			var staff1 = Factory.Load<GlbStaff>(new ZGuid(id1));
			var staff2 = Factory.Load<GlbStaff>(new ZGuid(id2));
			var staff3 = Factory.Load<GlbStaff>(new ZGuid(id3));

			AssertEquals(canLogin, staff1.GS_CanLogin);
			AssertEquals(isController, staff1.GS_IsController);
			AssertEquals(isRobot, staff1.GS_IsRobot);

			AssertEquals(false, staff2.GS_CanLogin);
			AssertEquals(false, staff2.GS_IsController);
			AssertEquals(false, staff2.GS_IsRobot);

			AssertEquals(canLogin, staff3.GS_CanLogin);
			AssertEquals(isController, staff3.GS_IsController);
			AssertEquals(isRobot, staff3.GS_IsRobot);
		}

		public void TestUpdate_NotFound()
		{
			var scimGroup = ScimProcessorTestHelper.CreateScimGroup("g1", "group1");

			AssertExceptionThrown<SCIMNotFoundException>(delegate
			{ repository.UpdateSCIMResourceById(ZGuid.NewZGuid().ToString(), scimGroup).GetAwaiter().GetResult(); });
		}

		public void TestUpdate_NotUnique()
		{
			Factory.ClearQueryCache();

			repository = new GroupRepository(ScimProcessorTestHelper.ReturnSchemaQueryRepository());

			var scimGroup1 = CreateScimGroup("g1", "group1");
			var scimGroup2 = CreateScimGroup("g2", "group2");
			var scimGroup3 = CreateScimGroup("g1", "group3");

			var id1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;
			var id2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;

			AssertExceptionThrown<SCIMUniquenessAttributeException>(delegate
			{ repository.UpdateSCIMResourceById(id2.ToString(), scimGroup3); });
		}

		ScimGroup CreateScimGroup(string externalId, string displayName)
		{
			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup(externalId, displayName);
			var query = new ZQuery(GlbGroupSchema.GG_Desc, scimGroup1.DisplayName);
			var newFactory = new BusinessObjectFactory();
			var item = newFactory.Load<GlbGroup>(query).FirstOrDefault();
			if (item != null)
			{
				item.Delete();
			}

			return scimGroup1;
		}
		#endregion

		#region Patch

		public void TestPatchMembers_OwnParent()
		{
			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1");
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2");

			var groupId1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj1 = new JObject
			{
				new JProperty(AttributeNames.Value, groupId1.ToString()),
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj1
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMNoTargetException>(delegate { repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult(); });
		}

		public void TestPatchMembers_NoType()
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember() { Value = id1.ToString() });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", new GroupMember() { Value = id2.ToString() });

			var groupId1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;
			var groupId2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1 });

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj1 = new JObject
			{
				new JProperty(AttributeNames.Value, id3.ToString()),
			};

			var patch1 = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj1
			};

			var jObj2 = new JObject
			{
				new JProperty(AttributeNames.Value, groupId2.ToString()),
			};

			var patch2 = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj2
			};

			param.Operations.Add(patch1);
			param.Operations.Add(patch2);

			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1, user3 }, scimGroup2);
		}

		public void TestPatch_Twice()
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);

			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember() { Value = id1.ToString() });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", new GroupMember() { Value = id2.ToString() });

			var groupId1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;
			var groupId2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1 });

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.Value, id3.ToString()),
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1, user3 });

			AssertNoExceptionThrown(delegate { repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult(); });
			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1, user3 });
		}

		public void TestPatchMembers()
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");
			var user4 = ScimProcessorTestHelper.CreateScimUser("ext4", "userName4", "first4", "middle4", "last4", "address4", "city4", "0499444444", "test4@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;
			var id4 = usersRepo.CreateSCIMResource(user4).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember() { Value = id1.ToString() });
			var scimGroup2 = ScimProcessorTestHelper.CreateScimGroup("g2", "group2", new GroupMember() { Value = id1.ToString() });

			var groupId1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;
			var groupId2 = repository.CreateSCIMResource(scimGroup2).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1 });

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.Value, id2.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id2}")
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1, user2 });

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REMOVE,
				Path = $"members[value eq \"{id1}\"]"
			};

			param.Operations.Add(patch);

			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user2 });

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj1 = new JObject
			{
				new JProperty(AttributeNames.Value, id3.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id3}")
			};

			var jObj2 = new JObject
			{
				new JProperty(AttributeNames.Value, id4.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id4}")
			};

			var jArr = new JArray(jObj1, jObj2);

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "members",
				Value = jArr
			};

			param.Operations.Add(patch);

			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user3, user4 });

			jObj = new JObject
			{
				new JProperty(AttributeNames.Value, groupId2.ToString()),
				new JProperty(AttributeNames.Ref, $"../Groups/{groupId2}")
			};

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj
			};

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();
			loaded = Factory.Load<GlbGroup>(groupId2);
			AssertEquals("group2", loaded.GG_Desc);
			AssertEquals("g2", loaded.GG_ExternalId);
			AssertEquals(groupId1, loaded.GG_GG_ParentGroup.ToGuid());
		}

		public void TestPatch_CanLogin()
		{
			Env.Registry.ScimCanLogin = false;
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_CanLogin", "UAP-CWDEV-CanLogin")))
			{
				AssertPatchFlags("UAP-CWDEV-CanLogin", true, false, false);
			}
		}

		public void TestPatch_IsController()
		{
			Env.Registry.ScimCanLogin = false;
			var otherController = Factory.NewWithValidTestData<GlbStaff>();
			otherController.GS_IsController = true;
			otherController.GS_Code = "CN~";

			Factory.Save();

			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsController", "UAP-CWDEV-IsController")))
			{
				AssertPatchFlags("UAP-CWDEV-IsController", false, true, false);
			}
		}

		public void TestPatch_IsRobot()
		{
			Env.Registry.ScimCanLogin = false;
			using (SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeStaffColumnToGroupDescriptionScimMappingCollection("GS_IsRobot", "UAP-CWDEV-IsRobot")))
			{
				AssertPatchFlags("UAP-CWDEV-IsRobot", false, false, true);
			}
		}

		void AssertPatchFlags(string groupName, bool canLogin, bool isController, bool isRobot)
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");
			var user4 = ScimProcessorTestHelper.CreateScimUser("ext4", "userName4", "first4", "middle4", "last4", "address4", "city4", "0499444444", "test4@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;
			var id3 = usersRepo.CreateSCIMResource(user3).GetAwaiter().GetResult().Id;
			var id4 = usersRepo.CreateSCIMResource(user4).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", groupName, new GroupMember() { Value = id1.ToString() });

			var groupId1 = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals(groupName, loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1 });
			AssertStaffFlags(user1.Id, canLogin, isController, isRobot);
			AssertStaffFlags(user2.Id, false, false, false);
			AssertStaffFlags(user3.Id, false, false, false);
			AssertStaffFlags(user4.Id, false, false, false);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.Value, id2.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id2}")
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals(groupName, loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1, user2 });
			AssertStaffFlags(user1.Id, canLogin, isController, isRobot);
			AssertStaffFlags(user2.Id, canLogin, isController, isRobot);
			AssertStaffFlags(user3.Id, false, false, false);
			AssertStaffFlags(user4.Id, false, false, false);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REMOVE,
				Path = $"members[value eq \"{id1}\"]"
			};

			param.Operations.Add(patch);

			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals(groupName, loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user2 });
			AssertStaffFlags(user1.Id, false, false, false);
			AssertStaffFlags(user2.Id, canLogin, isController, isRobot);
			AssertStaffFlags(user3.Id, false, false, false);
			AssertStaffFlags(user4.Id, false, false, false);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj1 = new JObject
			{
				new JProperty(AttributeNames.Value, id3.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id3}")
			};

			var jObj2 = new JObject
			{
				new JProperty(AttributeNames.Value, id4.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id4}")
			};

			var jArr = new JArray(jObj1, jObj2);

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = "members",
				Value = jArr
			};

			param.Operations.Add(patch);

			repository.PatchSCIMResourceById(groupId1.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(groupId1);
			AssertEquals(groupName, loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user3, user4 });
			AssertStaffFlags(user1.Id, false, false, false);
			AssertStaffFlags(user2.Id, false, false, false);
			AssertStaffFlags(user3.Id, canLogin, isController, isRobot);
			AssertStaffFlags(user4.Id, canLogin, isController, isRobot);
		}

		public void TestPatchMembers_Response()
		{
			var usersRepo = new UserRepository(scimSchemaQueryRepository);
			var user1 = ScimProcessorTestHelper.CreateScimUser("ext1", "userName1", "first1", "middle1", "last1", "address1", "city1", "0499111111", "test1@user.com");
			var user2 = ScimProcessorTestHelper.CreateScimUser("ext2", "userName2", "first2", "middle2", "last2", "address2", "city2", "0499222222", "test2@user.com");
			var user3 = ScimProcessorTestHelper.CreateScimUser("ext3", "userName3", "first3", "middle3", "last3", "address3", "city3", "0499333333", "test3@user.com");
			var user4 = ScimProcessorTestHelper.CreateScimUser("ext4", "userName4", "first4", "middle4", "last4", "address4", "city4", "0499444444", "test4@user.com");

			var id1 = usersRepo.CreateSCIMResource(user1).GetAwaiter().GetResult().Id;
			var id2 = usersRepo.CreateSCIMResource(user2).GetAwaiter().GetResult().Id;

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1", new GroupMember() { Value = id1.ToString() });

			var id = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertMembers(loaded, new[] { user1 });

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.Value, id2.ToString()),
				new JProperty(AttributeNames.Ref, $"../Users/{id2}")
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.ADD,
				Path = "members",
				Value = jObj
			};

			param.Operations.Add(patch);
			var resultGroup = repository.PatchSCIMResourceById(id.ToString(), param).GetAwaiter().GetResult();
			AssertEquals(2, resultGroup.Members.Count());
			AssertContainsExactElementsInAnyOrder(new[] { id1.ToString(), id2.ToString() }, resultGroup.Members.Select(m => m.Value));
		}

		void AssertStaffFlags(Guid userId, bool canLogin = false, bool isController = false, bool isRobot = false)
		{
			var staff = Factory.Load<GlbStaff>(new ZGuid(userId));
			AssertEquals(canLogin, staff!.GS_CanLogin);
			AssertEquals(isController, staff!.GS_IsController);
			AssertEquals(isRobot, staff!.GS_IsRobot);
		}

		void AssertMembers(GlbGroup loaded, ScimUser[] scimUsers, params ScimGroup[] scimGroups)
		{
			AssertEquals(loaded.Staff.Count, scimUsers.Length);
			foreach (var user in scimUsers)
			{
				var staff = loaded.Staff.FirstOrDefault(s => s.PK == new ZGuid(user.Id));
				AssertNotNull(staff);
			}

			foreach (var group in scimGroups)
			{
				var childGroup = loaded.Factory.Load<GlbGroup>(new ZGuid(group.Id));
				AssertNotNull(childGroup);
				AssertEquals(loaded.PK, childGroup.GG_GG_ParentGroup);
			}
		}

		public void TestPatch_NotFound()
		{
			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			PatchOperationParameter patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REMOVE,
				Path = "members"
			};

			param.Operations.Add(patch);

			AssertExceptionThrown<SCIMNotFoundException>(delegate
			{ repository.PatchSCIMResourceById(Guid.NewGuid().ToString(), param).GetAwaiter().GetResult(); });
		}

		public void TestPatch_Multiple()
		{
			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1");

			var id = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var jObj = new JObject
			{
				new JProperty(AttributeNames.DisplayName, "group1upd"),
				new JProperty(AttributeNames.ExternalId, "g1upd"),
			};

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Value = jObj
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(id.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1upd", loaded.GG_Desc);
			AssertEquals("g1upd", loaded.GG_ExternalId);
		}

		public void TestPatch_Category()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("C1", "Category 1"),
				new CodeDescriptionPair("C2", "Category 2"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var scimGroup1 = ScimProcessorTestHelper.CreateScimGroup("g1", "group1");

			var id = repository.CreateSCIMResource(scimGroup1).GetAwaiter().GetResult().Id;

			var loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertEquals("", loaded.GG_Category);

			var param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			var patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = AttributeNames.Category,
				Value = "C1"
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(id.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertEquals("C1", loaded.GG_Category);

			param = new PatchRepresentationParameter();
			param.Operations = new List<PatchOperationParameter>();
			param.Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" };

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = AttributeNames.Category,
				Value = "C2"
			};

			param.Operations.Add(patch);
			repository.PatchSCIMResourceById(id.ToString(), param).GetAwaiter().GetResult();

			loaded = Factory.Load<GlbGroup>(id);
			AssertEquals("group1", loaded.GG_Desc);
			AssertEquals("g1", loaded.GG_ExternalId);
			AssertEquals("C2", loaded.GG_Category);

			patch = new PatchOperationParameter()
			{
				Operation = SCIMPatchOperations.REPLACE,
				Path = AttributeNames.Category,
				Value = "C3"
			};

			param.Operations.Add(patch);
			AssertExceptionThrown<SCIMNoTargetException>(delegate
			{
				repository.PatchSCIMResourceById(id.ToString(), param).GetAwaiter().GetResult();
			});
		}

		#endregion
	}
}
