using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurity))]
	sealed class GlbSecurityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSecurityModificationOnly(BusinessObject parentBizEntity, GlbSecurityCollection securityPermissions, BusinessObjectCollection otherChild, int expectedSecurityLogsAfterFirstSave, bool hasLastEditColumn = false)
		{
			typeof(BusinessObject).GetProperty("IsTopLevel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(parentBizEntity, true, null);
			parentBizEntity.GetLogs().RemoveAndDeleteAll();
			Factory.Save();

			ZQuery securityModifiedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SecurityModified.Code);
			ZQuery editQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);

			AssertEquals("There should be as many security logs as permissions.", expectedSecurityLogsAfterFirstSave, parentBizEntity.GetLogs().Find(securityModifiedQuery).Length);
			AssertEquals("There should not be any edit log.", 0, parentBizEntity.GetLogs().Find(editQuery).Length);

			GlbSecurity security = securityPermissions.AddNew();
			security.FillWithValidTestData();
			Factory.Save();

			AssertEquals("1 security permission add. 1 security log created in the parent.", expectedSecurityLogsAfterFirstSave + 1, parentBizEntity.GetLogs().Find(securityModifiedQuery).Length);
			AssertEquals("There should not be any edit log.", 0, parentBizEntity.GetLogs().Find(editQuery).Length);

			security.Delete();
			Factory.Save();

			AssertEquals("1 security permission deleted. 1 more security log created in the parent.", expectedSecurityLogsAfterFirstSave + 2, parentBizEntity.GetLogs().Find(securityModifiedQuery).Length);
			AssertEquals("There should be 1 edit log.", 1, parentBizEntity.GetLogs().Find(editQuery).Length);

			otherChild.AddNew().FillWithValidTestData();
			Factory.Save();

			AssertEquals("No security logs created this time.", expectedSecurityLogsAfterFirstSave + 2, parentBizEntity.GetLogs().Find(securityModifiedQuery).Length);
			AssertEquals("There should be 2 edit logs.", 2, parentBizEntity.GetLogs().Find(editQuery).Length);
		}

		public void TestIsSecurityModificationOnly_Group()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			TestIsSecurityModificationOnly(group, group.SecurityPermissions, group.Staff, group.SecurityPermissions.Count, hasLastEditColumn: true);
		}

		public void TestIsSecurityModificationOnly_Staff()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			TestIsSecurityModificationOnly(staff, staff.StaffSecurityPermissionsCollection, staff.Holidays, 0, hasLastEditColumn: true);
		}

		public void TestRelatedItem_Group()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			group.GG_Code = "GGG";
			group.GG_Desc = "Some group";
			security.GU_ItemGUID = group.PK;

			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemCode.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemName.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemType.IsEmpty);

			security.GU_SecurityRight = GlbSecurity.ChangeOtherGroupSecurityRightName;

			AssertEquals("ItemCode", "GGG", security.ItemCode);
			AssertEquals("ItemName", "Some group", security.ItemName);
			AssertEquals("ItemType", DataBoundResourceStrings.GetTableDescriptiveName(GlbGroup.Schema.TableName), security.ItemType);
			AssertEquals("GlbGroup should have a plain-English name.", "Group", DataBoundResourceStrings.GetTableDescriptiveName(GlbGroup.Schema.TableName));
		}

		public void TestRelatedItem_GroupOwner()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			group.GG_Code = "GGG";
			group.GG_Desc = "Some group";
			security.GU_ItemGUID = group.PK;

			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemCode.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemName.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemType.IsEmpty);

			security.GU_SecurityRight = GlbSecurity.GroupOwnerSecurityRightName;

			AssertEquals("ItemCode", "GGG", security.ItemCode);
			AssertEquals("ItemName", "Some group", security.ItemName);
			AssertEquals("ItemType", DataBoundResourceStrings.GetTableDescriptiveName(GlbGroup.Schema.TableName), security.ItemType);
			AssertEquals("GlbGroup should have a plain-English name.", "Group", DataBoundResourceStrings.GetTableDescriptiveName(GlbGroup.Schema.TableName));
		}

		public void TestRelatedItem_Staff()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			staff.GS_Code = "SSS";
			staff.GS_FullName = "Some staff";
			security.GU_ItemGUID = staff.PK;

			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemCode.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemName.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown.", security.ItemType.IsEmpty);

			security.GU_SecurityRight = GlbSecurity.ChangeOtherStaffSecurityRightName;

			AssertEquals("ItemCode", "SSS", security.ItemCode);
			AssertEquals("ItemName", "Some staff", security.ItemName);
			AssertEquals("ItemType", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaff.Schema.TableName), security.ItemType);
			AssertEquals("GlbStaff should have a plain-English name.", "Staff", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaff.Schema.TableName));
		}

		public void TestRelatedItem_Organisation()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Random Org";
			org.OH_Code = "NPAJ";

			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_ItemGUID = org.PK;

			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemCode.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemName.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemType.IsEmpty);

			security.GU_SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;

			AssertEquals("ItemCode", "NPAJ", security.ItemCode);
			AssertEquals("ItemName", "Random Org", security.ItemName);
			AssertEquals("ItemType", DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName), security.ItemType);
			AssertEquals("Organisation should have a plain-english name", "Organization", DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName));

			security.GU_SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;

			AssertEquals("ItemCode", "NPAJ", security.ItemCode);
			AssertEquals("ItemName", "Random Org", security.ItemName);
			AssertEquals("ItemType", DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName), security.ItemType);
			AssertEquals("Organisation should have a plain-english name", "Organization", DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName));
		}

		public void TestRelatedItem_Warehouse()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			BusinessObject warehose = helper.CreateWarehouse("WHS", "AA");

			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_ItemGUID = warehose.PK;

			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemCode.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemName.IsEmpty);
			Assert("GU_SecurityRight has not been set, so the related type is unknown", security.ItemType.IsEmpty);

			security.GU_SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;

			AssertEquals("ItemCode", "WHS", security.ItemCode);
			AssertEquals("ItemName", "WHS", security.ItemName);
			AssertEquals("ItemType", "Warehouse", security.ItemType);
			AssertEquals("Warehouse should have a plain-english name", "WhsWarehouse", DataBoundResourceStrings.GetTableDescriptiveName(WhsWarehouseSchema.Constants.TableName));
		}

		public void TestLogs()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = Env.Security.StaffViewEmergencyContact.Code;
			security.GU_GS = staff1.PK;
			security.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var securityEvent01 = staff1.Logs.GetAllLogs()
				.ToList<StmALog>().SingleOrDefault(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode);
			AssertNotNull("Event should be added to security right: 1st SEC event", securityEvent01);
			ZString expectedDescription = String.Format(
				"ADD - {0}, Is Allowed: Y, Branch: All, Dept: All, Company: All",
				Env.Security.StaffViewEmergencyContact.HumanReadableName);
			AssertEquals(expectedDescription, securityEvent01.SL_Reference);

			security.GU_GB = branch.PK;
			security.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			var securityEvent02 = staff1.Logs.GetAllLogs()
				.ToList<StmALog>().SingleOrDefault(l =>
					l.SL_SE_NKEvent == Events.SecurityModifiedCode
					&& l.PK != securityEvent01.PK);
			AssertNotNull("Event should be added to security right: 2nd SEC event", securityEvent02);
			expectedDescription = String.Format(
				"EDT - {0}, Is Allowed: N (was Y), Branch: {1}, Dept: All, Company: All",
				Env.Security.StaffViewEmergencyContact.HumanReadableName,
				branch.GB_Code);
			AssertEquals(expectedDescription, securityEvent02.SL_Reference);

			security.GU_GE = department.PK;
			Factory.Save();

			var securityEvent03 = staff1.Logs.GetAllLogs()
				.ToList<StmALog>().SingleOrDefault(l =>
					l.SL_SE_NKEvent == Events.SecurityModifiedCode
					&& l.PK != securityEvent01.PK
					&& l.PK != securityEvent02.PK);
			AssertNotNull("Event should be added to security right: 3rd SEC event", securityEvent03);
			expectedDescription = String.Format(
				"EDT - {0}, Is Allowed: N, Branch: {1}, Dept: {2}, Company: All",
				Env.Security.StaffViewEmergencyContact.HumanReadableName,
				branch.GB_Code,
				department.GE_Code);
			AssertEquals(expectedDescription, securityEvent03.SL_Reference);

			security.GU_GC = company.PK;
			Factory.Save();

			var securityEvent04 = staff1.Logs.GetAllLogs()
				.ToList<StmALog>().SingleOrDefault(l =>
					l.SL_SE_NKEvent == Events.SecurityModifiedCode
					&& l.PK != securityEvent01.PK
					&& l.PK != securityEvent02.PK
					&& l.PK != securityEvent03.PK);
			AssertNotNull("Event should be added to security right: 4th SEC event", securityEvent04);
			expectedDescription = String.Format(
				"EDT - {0}, Is Allowed: N, Branch: {1}, Dept: {2}, Company: {3}",
				Env.Security.StaffViewEmergencyContact.HumanReadableName,
				branch.GB_Code,
				department.GE_Code,
				company.GC_Code);
			AssertEquals(expectedDescription, securityEvent04.SL_Reference);
		}

		public void TestLogsForGlbSecuritiesWithItemGuid()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			SecurityCore security = new SecurityCore(staff.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			staff.StaffSecurityPermissionsCollection.Security = security;

			Guid itemGuid = Guid.NewGuid();
			SecurityCheckpoint checkpoint = new SecurityCheckpoint("Goober", (NoResString)"Grape", null, security, itemGuid);

			GlbSecurity element1 = staff.StaffSecurityPermissionsCollection.AddNew();
			element1.GU_ItemGUID = itemGuid;
			element1.GU_SecurityRight = "Goober";

			GlbSecurity element2 = staff.StaffSecurityPermissionsCollection.AddNew();
			element2.GU_SecurityRight = "Moo";
			element2.Delete();

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SecurityModified.Code);
			StmALog[] logs = staff.Logs.Find(query);
			AssertEquals("There should only be 1 log.", 1, logs.Length);
			AssertEquals("ADD - Grape, Is Allowed: N, Branch: All, Dept: All, Company: All", logs[0].SL_Reference);

			element1.GU_SecurityItemIsAllowed = true;
			element1.Delete();

			Factory.Save();

			query.AddToFilter(StmALogSchema.PK, SQLComparisonOperator.NotEqual, logs[0].PK);
			AssertEquals("DEL - Grape, Is Allowed: N, Branch: All, Dept: All, Company: All", staff.Logs.Find(query)[0].SL_Reference);
		}

		public void TestLogIsRolledBackIfDeleteFails()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			var mockSecurity = Factory.NewMoq<GlbSecurity>();
			GlbSecurity security = mockSecurity.Object;

			security.GU_GS = staff.PK;
			staff.StaffSecurityPermissionsCollection.Add(security);
			staff.GS_Code = "ZAC";
			security.GU_SecurityRight = Env.Security.SystemRegistry.Code;

			Factory.Save();

			int logCount = staff.Logs.GetAllLogs().Count;

			try
			{
				mockSecurity.Protected().Setup("BeforeSuccessfulDelete").Throws(new Exception("Delete failed."));
				security.Delete();
				Fail("The delete should have failed");
			}
			catch (Exception ex)
			{
				if (ex.Message == "Delete failed.")
				{
					AssertEquals("No new logs should have been added if security delete fails.", logCount, staff.Logs.GetAllLogs().Count);
				}
				else
				{
					throw;
				}
			}
		}

		public void TestLogIsRolledBackIfSaveFails()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ZAC";
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "BAS";
			GlbSecurity security = staff1.StaffSecurityPermissionsCollection.AddNew();

			security.GU_SecurityRight = Env.Security.SystemRegistry.Code;

			try
			{
				Factory.Save();
				Fail("The save should have failed");
			}
			catch (ZSaveException)
			{
				AssertEquals("No logs should exist if the save fails.", 0, staff1.Logs.GetAllLogs().Count);
			}
		}

		#region Test Overrides

		public void TestCompanyCode()
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			AssertEquals("Company code should be empty", "*", security.CompanyCode);
			AssertEquals("Company PK on Security record should not be set", ZGuid.Empty, security.GU_GC);

			security.CompanyCode = ZString.Empty;
			AssertEquals("Company code should be empty", "*", security.CompanyCode);
			AssertEquals("Company PK on Security record should not be set", ZGuid.Empty, security.GU_GC);

			security.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			AssertEquals("Company PK on Security record should be set", GlbCompany.CurrentCompany.PK, security.GU_GC);
			AssertEquals("Company code should be set", GlbCompany.CurrentCompany.GC_Code, security.CompanyCode);
		}

		public void TestBranchCode()
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			AssertEquals("Branch code should be empty", "*", security.BranchCode);
			AssertEquals("Branch PK on Security record should not be set", ZGuid.Empty, security.GU_GB);

			security.BranchCode = ZString.Empty;
			AssertEquals("Branch code should be empty", "*", security.BranchCode);
			AssertEquals("Branch PK on Security record should not be set", ZGuid.Empty, security.GU_GB);

			security.BranchCode = GlbBranch.CurrentBranch.GB_Code;
			AssertEquals("Branch PK on Security record should be set", GlbBranch.CurrentBranch.PK, security.GU_GB);
			AssertEquals("Branch code should be set", GlbBranch.CurrentBranch.GB_Code, security.BranchCode);
		}

		public void TestDepartmentCode()
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			AssertEquals("Department code should be empty", "*", security.DepartmentCode);
			AssertEquals("Department PK on Security record should not be set", ZGuid.Empty, security.GU_GE);

			security.DepartmentCode = ZString.Empty;
			AssertEquals("Department code should be empty", "*", security.DepartmentCode);
			AssertEquals("Department PK on Security record should not be set", ZGuid.Empty, security.GU_GE);

			security.DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			AssertEquals("Department PK on Security record should be set", GlbDepartment.CurrentDepartment.PK, security.GU_GE);
			AssertEquals("Department code should be set", GlbDepartment.CurrentDepartment.GE_Code, security.DepartmentCode);
		}

		public void TestDomainName()
		{
			var group1 = Factory.New<GlbGroup>();
			var security1 = Factory.New<GlbSecurity>();
			group1.GG_Code = "GGG";
			group1.GG_Desc = "Some group";
			group1.GG_DomainName = "Domain123";
			security1.GU_ItemGUID = group1.PK;

			security1.GU_SecurityRight = GlbSecurity.GroupOwnerSecurityRightName;
			AssertEquals("Security1.DomainName is the same as the group's Domain Name", "Domain123", security1.DomainName);

			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			var group2 = Factory.New<GlbGroup>();
			var security2 = Factory.New<GlbSecurity>();
			group2.GG_Code = "AAA";
			group2.GG_DomainName = "Domain456";
			security2.GU_ItemGUID = group2.PK;
			security2.GU_SecurityRight = GlbSecurity.GroupOwnerSecurityRightName;
			user1.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			user1.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group2.GG_Code);

			AssertEquals("Security2.DomainName is the same as the group's Domain Name", "Domain456", security2.DomainName);
		}

		#endregion
	}
}
