using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroup))]
	partial class GlbGroupTest : EnterpriseBusinessObjectTestCase
	{
		#region Delete

		[ExpectException(typeof(CannotDeleteException))]
		public void TestDeleteAll()
		{
			Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK).Delete();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestDeletePostMaster()
		{
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Delete();
		}

		public void TestDelete_ShouldDeleteLayoutLinks()
		{
			var testHelper = ObjectFactory.Get<IBMTestHelper>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var layout = testHelper.CreateControlCustomisation(Factory);
			var link = (BusinessObject)testHelper.CreateControlCustomisationLink(Factory, group, layout);

			Factory.Save();

			var loadedGroup = Factory.CreateNewFactory().Load<GlbGroup>(group.PK);
			loadedGroup.Delete();
			loadedGroup.Factory.Save();

			AssertEquals(true, link.IsDeleted);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestUniversalCopy()
		{
			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GlbGroupSchema.Constants.GG_Code, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
			NUnit.Framework.Assert.That(delegate
			{
				_ = new BusinessObjectCopyManager().Copy(group, copyTree).Object;
			}, CustomConstraints.InnermostExceptionThrown(typeof(UniversalCopyAbortException)));
		}

		public void TestCodeReadOnly()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals(false, group.GG_CodeInfo.ReadOnly);
			group = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
			AssertEquals(true, group.GG_CodeInfo.ReadOnly);
			group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			AssertEquals(true, group.GG_CodeInfo.ReadOnly);
		}

		public void TestDomainNameReadOnly()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals(false, group.GG_DomainNameInfo.ReadOnly);
			group = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
			AssertEquals(true, group.GG_DomainNameInfo.ReadOnly);
			group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			AssertEquals(true, group.GG_DomainNameInfo.ReadOnly);

			var loginWithoutRight = CreateStaffWithSecurityRights("GroupsModify", false);
			using (Env.SetTemporaryUserContext(new UserContext(loginWithoutRight.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				group = Factory.New<GlbGroup>();
				AssertEquals(true, group.GG_DomainNameInfo.ReadOnly);
			}
		}

		public void TestSetDefaultValues()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			AssertEquals("Default values should be set", GlbBranch.CurrentBranch.PK, group.SecurityBranch);
			AssertEquals("Default values should be set", GlbDepartment.CurrentDepartment.PK, group.SecurityDepartment);
		}

		public void TestCurrentGroupLink()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			AssertNull("Group link should be null", group.CurrentGroupLink);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			group.CurrentGroupLink = link;
			AssertEquals("Group link should be Link", link, group.CurrentGroupLink);
		}

		public void TestSecurityChangedLog()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "Config";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			security.GU_GB = branch.PK;
			security.GU_GG = group.PK;
			security.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var collection = group.Logs.GetAllLogs();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "SEC");
			BusinessObject[] businessObjects = collection.Find(query);
			AssertEquals($"{ExpectedDefaultSecurityPermissions} logs should be created on group created + 1 on security modified", ExpectedDefaultSecurityPermissions + 1, businessObjects.Length);

			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Branch: " + branch.GB_Code);
			businessObjects = collection.Find(query);
			AssertEquals("1 log should be added on security modified", 1, businessObjects.Length);
		}

		public void TestSecurityChangedLogFormat()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "Operations";
			security.GU_GG = group.PK;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			security.GU_GB = branch.PK;
			security.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			StmALogDependentCollection collection = group.Logs.GetAllLogs();
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "SEC");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Operate");

			BusinessObject[] businessObjects = collection.Find(query);
			AssertEquals("SL_Reference should be in correct format", "ADD - Operate, Is Allowed: N, Branch: All, Dept: All, Company: All", ((StmALog)businessObjects[0]).SL_Reference);
			AssertEquals("SL_Reference should be in correct format", "ADD - Operate, Is Allowed: Y, Branch: 7VQ, Dept: All, Company: All", ((StmALog)businessObjects[1]).SL_Reference);
		}

		public void TestISupportChangeOthersSecurityMembers()
		{
			ISupportChangeOthersSecurity support = Group;
			AssertEquals("CompleteGroupList", Group.Lookups.CompleteGroupList, support.CompleteGroupList);
			AssertEquals("CompleteStaffList", Group.Lookups.CompleteStaffList, support.CompleteStaffList);
			AssertEquals("GlbSecurityRightHolderColumn", GlbSecuritySchema.GU_GG, support.GlbSecurityRightHolderColumn);
			AssertEquals("PK", Group.PK, support.PK);
			AssertNotNull("SecurityChangeOthersView", support.SecurityChangeOthersView);
		}

		public void TestIsCurrentUserGroupOwnerForThisGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var otherGroup = Factory.NewWithValidTestData<GlbGroup>();
			var groupOwner = Factory.NewWithValidTestData<GlbStaff>();
			var nonGroupOwner = Factory.NewWithValidTestData<GlbStaff>();

			groupOwner.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
			groupOwner.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(groupOwner.GS_LoginName))
			{
				AssertEquals(true, group.IsCurrentUserGroupOwnerForThisGroup);
				AssertEquals(false, otherGroup.IsCurrentUserGroupOwnerForThisGroup);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(nonGroupOwner.GS_LoginName))
			{
				AssertEquals(false, group.IsCurrentUserGroupOwnerForThisGroup);
				AssertEquals(false, otherGroup.IsCurrentUserGroupOwnerForThisGroup);
			}
		}

		public void TestTogglingNonSecurityErasesSecurityRights()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			Assert(!group.IsNonSecurityGroup);
			GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "GroupsModify";
			security.GU_GG = group.PK;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			security.GU_GB = branch.PK;
			security.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			var groupInNewFactory = new BusinessObjectFactory().Load<GlbGroup>(group.PK);
			var securityInNewFactory = groupInNewFactory.SecurityPermissions.Where(x => x.GU_SecurityRight == "GroupsModify").First();

			groupInNewFactory.IsNonSecurityGroup = true;
			Assert(groupInNewFactory.IsNonSecurityGroup);
			Assert(securityInNewFactory.IsDeleted);
		}

		#region TestAddSecurityToAccessOrgOrWarehouse

		public void TestAddSecurityToAccessPrincipal()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "RANDOM";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			IOrgsAndWarehousesAccessProvider provider = group;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("RANDOM");
			AssertEquals("should now have access to a principal", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct principal", principal.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbGroup>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Rebuild();
			AssertEquals("access to a principal should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct principal should have been persisted", principal.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		public void TestAddSecurityToAccessWhsClient()
		{
			OrgHeader whsClient = Factory.New<OrgHeader>();
			whsClient.OH_Code = "CLIENT";
			whsClient.OH_IsWarehouseClient = true;
			Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			IOrgsAndWarehousesAccessProvider provider = group;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("CLIENT");
			AssertEquals("should now have access to a client", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct client", whsClient.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbGroup>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Rebuild();
			AssertEquals("access to a client should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct client should have been persisted", whsClient.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		public void TestAddSecurityToAccessWarehouse()
		{
			IWhsTransactionTestHelper helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			BusinessObject warehouse = helper.CreateWarehouse("Warehouse", "WHS", "AA");

			Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			IOrgsAndWarehousesAccessProvider provider = group;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			AssertEquals("precondition:", 0, provider.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("WHS");
			AssertEquals("should now have access to a warehouse", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct warehouse", warehouse.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			IOrgsAndWarehousesAccessProvider providerInAnotherFactory = anotherFactory.Load<GlbGroup>(provider.PK);
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Rebuild();
			AssertEquals("access to a warehouse should have been persisted", 1, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("access to the correct warehouse should have been persisted", warehouse.PK, providerInAnotherFactory.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
		}

		#endregion

		#region Default Security for New Group
		public void TestDefaultPermissionsForNewGroup_AccessUnpublishedCustomizedDocumentsAndReports()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals("Default Security Permissions should be added", ExpectedDefaultSecurityPermissions, group.SecurityPermissions.Count);
			Assert("Has changes should be false on Group", !group.HasChanges);
			Assert("Has changes should be false on SecurityPermissions", !group.SecurityPermissions.HasChanges);

			var securityAccessUnpublishedCustomizedDocumentsAndReports = (GlbSecurity)group.SecurityPermissions.FirstOrDefault(sp => (sp as GlbSecurity).GU_SecurityRight == Env.Security.AccessUnpublishedCustomizedDocumentsAndReports.Code);
			AssertEquals("All Security permissions added should be denied", "No", securityAccessUnpublishedCustomizedDocumentsAndReports.IsAllowed);
			Assert("Should not calculate Change Others Right as a default denial", !securityAccessUnpublishedCustomizedDocumentsAndReports.ChangeOthersRightsNeedsValidation);
			Assert("Should not calculate Change Others Right as a default denial", securityAccessUnpublishedCustomizedDocumentsAndReports.HasRightsToChangeOthers);
		}

		public void TestDefaultPermissionsForNewGroup_EditUserDefinedFilter()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals("Default Security Permissions should be added", ExpectedDefaultSecurityPermissions, group.SecurityPermissions.Count);
			Assert("Has changes should be false on Group", !group.HasChanges);
			Assert("Has changes should be false on SecurityPermissions", !group.SecurityPermissions.HasChanges);

			var securityEditUserDefinedFilter = (GlbSecurity)group.SecurityPermissions.FirstOrDefault(sp => (sp as GlbSecurity).GU_SecurityRight == Env.Security.EditUserDefinedFilters.Code);
			AssertEquals("All Security permissions added should be denied", "No", securityEditUserDefinedFilter.IsAllowed);
			Assert("Should not calculate Change Others Right as a default denial", !securityEditUserDefinedFilter.ChangeOthersRightsNeedsValidation);
			Assert("Should not calculate Change Others Right as a default denial", securityEditUserDefinedFilter.HasRightsToChangeOthers);
		}

		public void TestDefaultPermissionsForNewGroup()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals("Default Security Permissions should be added", ExpectedDefaultSecurityPermissions, group.SecurityPermissions.Count);
			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				AssertEquals("All Security permissions added should be denied", "No", securityPermission.IsAllowed);
				Assert("Should not calculate Change Others Right as a default denial", !securityPermission.ChangeOthersRightsNeedsValidation);
				Assert("Should not calculate Change Others Right as a default denial", securityPermission.HasRightsToChangeOthers);
			}

			Assert("Has changes should be false on Group", !group.HasChanges);
			Assert("Has changes should be false on SecurityPermissions", !group.SecurityPermissions.HasChanges);
		}

		#endregion

		#region Test IDocManagerSupport

		public void TestDocManagerCode()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			AssertEquals("Code should be Group. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "GRP", ((IDocManagerSupport)group).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Group Security

		public void TestGroupSecurity()
		{
			var security1 = Group.SecurityPermissions.AddNew();
			security1.GU_SecurityRight = "Staff";
			security1.GU_GG = Group.PK;
			security1.GU_SecurityItemIsAllowed = true;

			var security2 = Group.SecurityPermissions.AddNew();
			security2.GU_SecurityRight = "StaffOwnSetSalesRep";
			security2.GU_GG = Group.PK;
			security2.GU_SecurityItemIsAllowed = false;

			Group.Factory.Save();

			AssertNotNull(Group.GroupSecurity);
			Assert(Group.GroupSecurity.Count > 0);

			var summary1 = Group.GroupSecurity.OfType<GlbGroupSecurity>().First(x => x.SecurityRightPart4 == "Edit -> Modify Own");
			AssertEquals("Granted", summary1.Summary);

			var summary2 = Group.GroupSecurity.OfType<GlbGroupSecurity>().First(x => x.SecurityRightPart4 == "Edit -> Modify Own -> Details");
			AssertEquals("Granted", summary2.Summary);

			var summary3 = Group.GroupSecurity.OfType<GlbGroupSecurity>().First(x => x.SecurityRightPart4 == "Edit -> Modify Own -> Details -> Set Is Sales Rep");
			AssertEquals("Denied", summary3.Summary);
		}

		public void TestResetGroupPermissions()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			AssertEquals("Default Security Permissions should be added", ExpectedDefaultSecurityPermissions, group.SecurityPermissions.Count);

			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				AssertEquals("All Security permissions added should be denied", "No", securityPermission.IsAllowed);
			}

			var security1 = group.SecurityPermissions.AddNew();
			security1.GU_SecurityRight = "Customs";
			security1.GU_GG = group.PK;
			security1.GU_SecurityItemIsAllowed = true;

			var security2 = group.SecurityPermissions.AddNew();
			security2.GU_SecurityRight = "Forwarding";
			security2.GU_GG = group.PK;
			security2.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			AssertEquals(ExpectedDefaultSecurityPermissions + 2, group.SecurityPermissions.Count);

			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				securityPermission.GU_SecurityItemIsAllowed = true;
			}

			group.ResetGroupPermissions();

			AssertEquals("Default Security Permissions should be kept", ExpectedDefaultSecurityPermissions, group.SecurityPermissions.Count);

			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				AssertEquals("All Security permissions added should be reset to denied", "No", securityPermission.IsAllowed);
			}
		}

		public void TestGroupSecurityShouldIncludeRegistrySecurities()
		{
			AssertNotNull(Group.GroupSecurity);
			Assert(Group.GroupSecurity.Count > 0);
			AssertGreaterThan("It should load all specific registry securities.", Group.GroupSecurity.OfType<GlbGroupSecurity>().Count(s => s.SecurityRight.Contains(Env.Security.SystemRegistryEditSpecificSettings.DisplayTextPathToSecurityRight.ToString())), 1);
		}

		#endregion

		#region Filtering by Branch / Department Event

		public void TestFilteringByBranchDepartment()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbGroupFormForTest form = new GlbGroupFormForTest(group);
			Assert("Event should not have been fired", !form.EventFired);
			group.SecurityBranch = ZGuid.Empty;
			Assert("Event should have been fired", form.EventFired);
		}
		#endregion

		#region Related Business Objects

		public void TestGlbStaff()
		{
			AssertNotNull(Group.Staff);
			AssertEquals("Staff count", 0, Group.Staff.Count);
			GlbStaff staff = Factory.New<GlbStaff>();
			Group.Staff.Add(staff);
			AssertEquals("Staff count", 1, Group.Staff.Count);
		}

		public void TestGlbSecurityPermissions()
		{
			AssertNotNull(Group.SecurityPermissions);
			AssertEquals("Security Permissions count", ExpectedDefaultSecurityPermissions, Group.SecurityPermissions.Count);
		}

		public void TestGlbSecurityPermissionsView()
		{
			AssertEquals("Security Permisisons View count", 0, Group.SecurityPermissionsView.Count);

			GlbSecurity securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_SecurityRight = "Test";
			securityRecord.GU_GG = Group.PK;
			Group.SecurityPermissions.Add(securityRecord);

			Group.SecurityPermissionsView.FilterBySecurityKey("Test", ZGuid.Empty);
			AssertEquals("Security Permisisons View count", 1, Group.SecurityPermissionsView.Count);
		}

		#endregion

		#region New Bound Properties

		public void TestIsGlobal_SettingToTrueClearsGG_GC()
		{
			var group = Factory.New<GlbGroup>();

			group.IsGlobal = false;
			group.GG_GC = GlbCompany.CurrentCompany.PK;

			group.IsGlobal = true;
			AssertEquals(ZGuid.Empty, group.GG_GC);
		}

		public void TestIsGlobal_OnLoaded()
		{
			var companySpecificGroup = Factory.NewWithValidTestData<GlbGroup>();
			companySpecificGroup.GG_GC = GlbCompany.CurrentCompany.PK;
			companySpecificGroup.GG_IsSales = true;
			var globalGroup = Factory.NewWithValidTestData<GlbGroup>();
			globalGroup.GG_GC = ZGuid.Empty;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals(false, otherFactory.Load<GlbGroup>(companySpecificGroup.PK).IsGlobal);
			AssertEquals(true, otherFactory.Load<GlbGroup>(globalGroup.PK).IsGlobal);
		}

		public void TestGG_GC_ReadOnly()
		{
			var group = Factory.New<GlbGroup>();

			group.IsGlobal = true;
			AssertEquals(true, group.GG_GCInfo.ReadOnly);

			group.IsGlobal = false;
			AssertEquals(false, group.GG_GCInfo.ReadOnly);
		}

		public void TestSecurityBranch()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			Group.SecurityBranch = branch.PK;
			AssertEquals("SecurityBranch", branch.PK, Group.SecurityBranch);
			AssertEquals("HasChanges", false, Group.HasChanges);
		}

		public void TestSecurityDepartment()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			Group.SecurityDepartment = department.PK;
			AssertEquals("SecurityDepartment", department.PK, Group.SecurityDepartment);
			AssertEquals("HasChanges", false, Group.HasChanges);
		}

		public void TestSecurityCompany()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbBranch branchForCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK));
			AssertEquals("Security Company to filter by should be set", branchForCompany.GB_GC, group.SecurityCompany);

			group.SecurityBranch = ZGuid.Empty;
			AssertEquals("Security Company to filter by should not be set", ZGuid.Empty, group.SecurityCompany);
		}

		public void TestHumanReadableNameCore()
		{
			Group.GG_Code = "ELF";
			AssertEquals("HumanReadableNameCore is human readable", "Group (ELF)", Group.HumanReadableName);
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", true, PreventDeleteAttribute.IsTrue(typeof(GlbGroup)));
		}

		public void TestPreventCancel()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			group.GG_IsActive = false;
			group.Validation.ValidateAll();
			AssertEquals(null, group.CanCancel());

			group.Staff.Add(Factory.New<GlbStaff>());
			group.Validation.ValidateAll();
			AssertEquals("Members need to be Detached from the Group.", group.CanCancel());

			var all = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
			all.GG_IsActive = false;
			all.Validation.ValidateAll();
			AssertEquals("The 'ALL' group may not be deactivated", all.CanCancel());

			var systemDefinedGroup = Factory.New<GlbGroup>();
			systemDefinedGroup.GG_Code = "RNG";
			systemDefinedGroup.GG_IsActive = false;
			systemDefinedGroup.Validation.ValidateAll();

			AssertEquals(null, systemDefinedGroup.CanCancel());

			systemDefinedGroup.GG_IsSystemDefined = true;

			AssertEquals("Group (RNG) is a system defined group and cannot be deactivated.", systemDefinedGroup.CanCancel());

			var scimGroup = Factory.New<GlbGroup>();
			scimGroup.GG_Code = "SCIM";
			scimGroup.GG_IsActive = true;
			scimGroup.GG_ExternalId = "21";
			scimGroup.Validation.ValidateAll();
			AssertEquals("Group (SCIM) is controlled externally and cannot be deactivated.", scimGroup.CanCancel());
		}

		public void TestRemoveAllStaff_WhenIsCancelledIsSetFalse()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			group.GG_IsActive = false;
			group.Staff.Add(Factory.New<GlbStaff>());

			AssertEquals(1, group.Staff.Count);

			group.IsCancelled = true;

			AssertEquals(0, group.Staff.Count);
		}

		public void TestRemoveAllStaff_WhenIsCancelledIsSetFalse_ScimGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "RNG";
			group.GG_IsActive = false;
			group.Staff.Add(Factory.New<GlbStaff>());
			group.GG_ExternalId = "123";

			AssertEquals(1, group.Staff.Count);

			group.IsCancelled = true;

			AssertEquals(1, group.Staff.Count);
		}

		#endregion

		#region Active Directory

		public void TestNewGroup_WhenADEnabled_ShouldSetInvalidADGuid()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			group = Factory.New<GlbGroup>();
			group.Validation.ValidateAll();
			AssertEquals(ZGuid.Invalid, group.GG_ActiveDirectoryObjectGuid);
			AssertNoErrors("Should be able to save an invalid GUID", group.GG_ActiveDirectoryObjectGuidInfo);
		}

		public void TestWhenReactivate_ShouldSetInvalidADGuid()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var group = Factory.New<GlbGroup>();

			group.DisconnectFromAD();
			AssertEquals("Inactive", ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);

			group.GG_IsActive = true;
			AssertEquals("Active", ZGuid.Invalid, group.GG_ActiveDirectoryObjectGuid);
		}

		public void TestGG_ActiveDirectoryObjectGuid_WhenReactivating()
		{
			var adGroup = new Mock<IADEntity>();
			adGroup.Setup(m => m.HasExistingDirectoryEntry()).Returns(false);

			var adGroup2 = new Mock<IADEntity>();
			adGroup2.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);

			var adEntityProviderMock = new Mock<IADEntityProvider>();
			adEntityProviderMock.Setup(m => m.GetADGroup(It.Is<IGlbGroup>(g => g.GG_Desc != "group2"))).Returns(adGroup.Object);
			adEntityProviderMock.Setup(m => m.GetADGroup(It.Is<IGlbGroup>(g => g.GG_Desc == "group2"))).Returns(adGroup2.Object);
			ObjectFactory.Substitute(adEntityProviderMock.Object);

			var adObjectGuid = ZGuid.NewZGuid();

			// Not AD-linked
			var group1 = Factory.New<GlbGroup>();
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
			group1.GG_IsActive = false;
			Assert(!group1.IsADLinked);

			// AD-linked and AD object exists
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Desc = "group2";
			group2.GG_ActiveDirectoryObjectGuid = adObjectGuid;
			group2.GG_IsActive = false;
			Assert(group2.IsADLinked);

			// AD-linked but AD object missing
			var group3 = Factory.New<GlbGroup>();
			group3.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			group3.GG_IsActive = false;
			Assert(group3.IsADLinked);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			group1.GG_IsActive = true;
			group2.GG_IsActive = true;
			group3.GG_IsActive = true;

			AssertEquals("group1's Empty AdObjectGuid should be set to Invalid", ZGuid.Invalid, group1.GG_ActiveDirectoryObjectGuid);
			AssertEquals("group2's Valid AdObjectGuid should remain", adObjectGuid, group2.GG_ActiveDirectoryObjectGuid);
			AssertEquals("group3's Valid AdObjectGuid should be reset as the AD Object is missing", ZGuid.Invalid, group3.GG_ActiveDirectoryObjectGuid);
			adGroup.VerifyAll();
			adGroup2.VerifyAll();
			adEntityProviderMock.VerifyAll();
		}

		public void TestIsADLinked()
		{
			var group = Factory.New<GlbGroup>();

			group.GG_ActiveDirectoryObjectGuid = Guid.Empty;
			Assert(!group.IsADLinked);

			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Assert(group.IsADLinked);
		}

		public void TestIsDatabaseAccess()
		{
			var group_databasedeveloper_1 = Factory.NewWithPrimaryKey<GlbGroup>(new Guid("a99e7f0e-8379-4f50-8560-9b4bb804c0de"));
			var group_databasereader_1 = Factory.NewWithPrimaryKey<GlbGroup>(new Guid("6f0eb310-fc5c-4696-9594-f8ce156542c6"));
			var group_backupoperator_1 = Factory.NewWithPrimaryKey<GlbGroup>(new Guid("208068b6-3383-44bf-8e0d-dbd827f9d675"));

			var group_databasedeveloper_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var group_databasereader_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var group_backupoperator_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var group_hrmstaff = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var glbGroupRole1 = Factory.New<GlbGroupRole>();
			glbGroupRole1.GGR_RoleName = "db_datawriter";
			var glbGroupRole2 = Factory.New<GlbGroupRole>();
			glbGroupRole2.GGR_RoleName = "cwRestrictedReaderRole";
			var glbGroupRole3 = Factory.New<GlbGroupRole>();
			glbGroupRole3.GGR_RoleName = "db_backupoperator";
			var glbGroupRole4 = Factory.New<GlbGroupRole>();
			glbGroupRole4.GGR_RoleName = "cwHRMStaffRole";

			group_databasedeveloper_2.Roles.Add(glbGroupRole1);
			group_databasereader_2.Roles.Add(glbGroupRole2);
			group_backupoperator_2.Roles.Add(glbGroupRole3);
			group_hrmstaff.Roles.Add(glbGroupRole4);

			var group_randomPK = Factory.New<GlbGroup>();

			Assert(group_databasedeveloper_1.IsFixedDatabaseAccessGroup);
			Assert(group_databasereader_1.IsFixedDatabaseAccessGroup);
			Assert(group_backupoperator_1.IsFixedDatabaseAccessGroup);
			Assert(!group_databasedeveloper_2.IsFixedDatabaseAccessGroup);
			Assert(!group_databasereader_2.IsFixedDatabaseAccessGroup);
			Assert(!group_backupoperator_2.IsFixedDatabaseAccessGroup);
			Assert(!group_hrmstaff.IsFixedDatabaseAccessGroup);
			Assert(!group_randomPK.IsFixedDatabaseAccessGroup);
		}

		public void TestIsADLinkable()
		{
			var group = Factory.New<GlbGroup>();
			Assert(((IADLinkedEntity)group).IsADLinkable);
		}

		public void TestDisconnectFromAD()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			group.GG_IsActive = true;

			group.DisconnectFromAD();
			AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
			Assert(!group.GG_IsActive);
		}

		public void TestDisconnectFromAD_SecurityCheck()
		{
			var loginWithRight = CreateStaffWithSecurityRights("GroupsModify", true);
			var loginWithoutRight = CreateStaffWithSecurityRights("GroupsModify", false);
			AssertNotNull(loginWithRight);
			AssertNotNull(loginWithoutRight);
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			group.GG_IsActive = true;

			using (Env.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => group.DisconnectFromAD());
			}

			using (Env.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => group.DisconnectFromAD());
			}
		}

		GlbStaff CreateStaffWithSecurityRights(string securityKey, bool allowed)
		{
			var checkPoint = Env.Instance.Security.FindCheckPoint(securityKey);
			if (checkPoint == null)
			{
				return null;
			}

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var security = Factory.New<GlbSecurity>();
			security.GU_GS = staff.PK;

			security.GU_SecurityRight = checkPoint.Code;
			security.GU_SecurityItemIsAllowed = allowed;
			return staff;
		}

		void MockAndSynchroniseWithAD()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var adEntityProvider = mocks.Create<IADEntityProvider>(MockBehavior.Strict);
			var adGroup = mocks.Create<IADEntity>();
			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var group = Factory.New<GlbGroup>();

				adEntityProvider.Setup(m => m.GetADGroup(group)).Returns(adGroup.Object);
				adGroup.Setup(m => m.Synchronise(It.IsAny<SyncMode>())).Returns(true);
				adGroup.Setup(m => m.CommitChanges()).Returns(true);
				group.SynchroniseWithAD();
			}
		}

		[ExpectNoExceptions]
		public void TestSynchroniseWithAD()
		{
			MockAndSynchroniseWithAD();
		}

		public void TestSynchroniseWithAD_SecurityCheck()
		{
			var loginWithRight = CreateStaffWithSecurityRights("GroupsModify", true);
			var loginWithoutRight = CreateStaffWithSecurityRights("GroupsModify", false);
			AssertNotNull(loginWithRight);
			AssertNotNull(loginWithoutRight);
			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => MockAndSynchroniseWithAD());
			}

			using (Env.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => MockAndSynchroniseWithAD());
			}
		}

		public void TestSynchroniseWithAD_SystemGroup()
		{
			var sysGroup = Factory.New<GlbGroup>();
			sysGroup.GG_IsSystemDefined = true;
			AssertNotNull(sysGroup);
			AssertExceptionThrown<InvalidOperationException>("Syncing system group should throw", "Cannot synchronize system-defined group " + sysGroup.GG_Code, () => sysGroup.SynchroniseWithAD());
		}

		#endregion

		#region ExternalPasswords

		public void TestSendUpdateExternalPasswords()
		{
			var accessPassword = Factory.New<GlbExternalPasswordForConfigurationTest>();
			accessPassword.GP_GG = Group.PK;

			Group.GG_Code = "GX9";
			accessPassword.GP_UserID = "U00001";
			accessPassword.CurrentDecryptedPassword = "NewAccess";
			accessPassword.NextDecryptedPassword = "NextAccess";

			Factory.Save();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var messages = Factory.Load<IEDIInterchange>(zQuery);

			AssertEquals("1 message should be created", 1, messages.Length);
			AssertContains("One SGA mesage should be created", "Name=\"Configuration\"", messages[0].EI_BodyText);

			accessPassword.CurrentDecryptedPassword = string.Empty;

			Factory.Save();
			Group.OnSaved(true);

			messages = Factory.Load<IEDIInterchange>(zQuery);
			AssertEquals("1 new message1 should be created", 2, messages.Length);
			AssertContains("One SGA mesage should be created", "Name=\"Configuration\"", messages[1].EI_BodyText);

			var message = messages.FirstOrDefault(c => c.EI_BodyText.Contains(@"Status=""VAL"""));
			AssertEquals("Full Message", Message, Regex.Replace(message.EI_BodyText, "<Password>.*</Password>", "<Password>Pass</Password>"));
		}

		class GlbExternalPasswordForConfigurationTest : GlbExternalPassword
		{
			public GlbExternalPasswordForConfigurationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString ConfigurationName => "Configuration";

			protected override object[] CreateCredentialItems()
			{
				var result = new List<object>();

				var currentDecryptedPassword = CurrentDecryptedPassword;
				if (!currentDecryptedPassword.IsEmpty)
				{
					result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.Current, GP_UserID, currentDecryptedPassword));
				}

				var nextDecryptedPassword = NextDecryptedPassword;
				if (!nextDecryptedPassword.IsEmpty)
				{
					result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.NextPassword, GP_UserID, nextDecryptedPassword));
				}

				return result.ToArray();
			}

			protected override ZString GetCredentialStatus()
			{
				return GP_PasswordStatus == Core.Constants.PasswordOK ? PasswordStatusList.Codes.Valid : PasswordStatusList.Codes.Invalid;
			}

			protected override string GetCredentialType() => "CustomsAccount";
		}

#if NET
		const string Message = @"<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""Configuration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Group"" Reference=""GX9"">
        <Group Type=""CustomsAccount"" Status=""VAL"">
          <Credential Name=""Current"">
            <UserName>U00001</UserName>
            <Password>Pass</Password>
          </Credential>
          <Credential Name=""Next"">
            <UserName>U00001</UserName>
            <Password>Pass</Password>
          </Credential>
        </Group>
      </Group>
    </Group>
  </Group>
</Configuration>";
#else
		const string Message = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""Configuration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Group"" Reference=""GX9"">
        <Group Type=""CustomsAccount"" Status=""VAL"">
          <Credential Name=""Current"">
            <UserName>U00001</UserName>
            <Password>Pass</Password>
          </Credential>
          <Credential Name=""Next"">
            <UserName>U00001</UserName>
            <Password>Pass</Password>
          </Credential>
        </Group>
      </Group>
    </Group>
  </Group>
</Configuration>";
#endif

		#endregion

		#region Logging for Attaching / Detaching Staff

		public void TestLoggingForAttachingOrDetachingStaff()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DDD";
			group1.GG_Desc = "Group DDD";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GGG";
			group2.GG_Desc = "Group GGG";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			staff1.GS_FullName = "Tester";

			group1.Staff.Add(staff1);

			Factory.Save();
			AssertEquals("Attached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_FullName = "Tester";
			group1.Staff.Add(staff2);
			group1.Staff.Remove(staff2);

			Factory.Save();
			AssertEquals("Attached log is not added to group since staff is removed", false, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester").Any());
			AssertEquals("Attached log is not added to staff since staff is removed", false, staff2.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			group1.Staff.Add(staff2);
			Factory.Save();
			AssertEquals("Attached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff2.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Any());

			group1.Staff.Remove(staff1);
			group1.Staff.Add(staff1);
			Factory.Save();
			AssertEquals("Attached log is not added to group since staff is added back", 1, group1.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Count());
			AssertEquals("Attached log is not added to staff since staff is added back", 1, staff1.Logs.Find(log => log.SL_Reference == "Attached - (DDD) Group DDD").Count());
			AssertEquals("Detached log is not added to group since staff is added back", false, group1.Logs.Find(log => log.SL_Reference == "Detached - (TS1) Tester").Any());
			AssertEquals("Detached log is not added to staff since staff is added back", false, staff1.Logs.Find(log => log.SL_Reference == "Detached - (DDD) Group DDD").Any());

			group1.Staff.Remove(staff1);
			group2.Staff.Add(staff1);
			Factory.Save();
			AssertEquals("Detached log is added to group", true, group1.Logs.Find(log => log.SL_Reference == "Detached - (TS1) Tester").Any());
			AssertEquals("Detached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Detached - (DDD) Group DDD").Any());
			AssertEquals("Attached log is added to group", true, group2.Logs.Find(log => log.SL_Reference == "Attached - (TS1) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Attached - (GGG) Group GGG").Any());

			group2.Delete();
			Factory.Save();
			AssertEquals("Detached log is added to staff", true, staff1.Logs.Find(log => log.SL_Reference == "Detached - (GGG) Group GGG").Any());
		}

		#endregion

		#region Validation

		public void TestLightValidationDisabled()
		{
			var group = Factory.New<GlbGroup>();
			AssertEquals(false, group.LightValidationEnabled);
		}

		public void TestOrganisationValidationDisabled()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var org = Factory.New<OrgHeader>();
			group.Organisation.Add(org);
			org.OH_Code = "AAA";
			org.OH_FullName = "AAA test";
			org.MiscServ.OM_GG_OrgSecurityGroup = group.PK;
			Factory.Save();

			org.Validation.ValidateAll();
			AssertEquals(group.Organisation.HasErrors(), false);
		}

		#endregion

		#region Organisation

		public void TestOrganisation()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			AssertEquals(0, group.Organisation.Count);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(ZGuid.Empty, org.MiscServ.OM_GG_OrgSecurityGroup);
			org.MiscServ.OM_GG_OrgSecurityGroup = group.PK;
			Factory.Save();

			group = new BusinessObjectFactory().Load<GlbGroup>(group.PK);
			AssertEquals(1, group.Organisation.Count);
			AssertEquals(org.PK, group.Organisation[0].PK);
		}

		#endregion

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();
			IWorkflowProvider workflowProvider = glbGroup;

			AssertNotNull(workflowProvider);
			AssertEquals(glbGroup.PK, workflowProvider.PK);
			AssertEquals(WorkflowDescriptors.GlbGroupWorkflowDescriptorCode, workflowProvider.WorkflowType);
			AssertEquals(typeof(GlbGroupProcessTaskCollection), workflowProvider.WorkflowItems.GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		#endregion

		#region helper functions

		public void TestDatabasAccessGroupRole()
		{
			var group_DbOperator_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			group_DbOperator_2.GG_Code = "TG1";
			var group_DbReader_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			group_DbReader_2.GG_Code = "TG2";
			var group_BkOperator_2 = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			group_BkOperator_2.GG_Code = "TG3";

			Factory.Save();

			var glbGroupRole1 = Factory.New<GlbGroupRole>();
			glbGroupRole1.GGR_RoleName = "db_datawriter";
			var glbGroupRole2 = Factory.New<GlbGroupRole>();
			glbGroupRole2.GGR_RoleName = "cwRestrictedReaderRole";
			var glbGroupRole3 = Factory.New<GlbGroupRole>();
			glbGroupRole3.GGR_RoleName = "db_backupoperator";

			group_DbOperator_2.Roles.Add(glbGroupRole1);
			group_DbReader_2.Roles.Add(glbGroupRole2);
			group_BkOperator_2.Roles.Add(glbGroupRole3);

			Factory.Save();

			var group_DbOperator = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbDeveloperGroupPK));
			var group_DbReader = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbReaderGroupPK));
			var group_BkOperator = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.BackupOperatorGroupPK));

			AssertEquals(1, group_DbOperator.Roles.Count);
			AssertEquals("db_datawriter", group_DbOperator.Roles.First().GGR_RoleName);
			AssertEquals(1, group_DbReader.Roles.Count);
			AssertEquals("cwRestrictedReaderRole", group_DbReader.Roles.First().GGR_RoleName);
			AssertEquals(1, group_BkOperator.Roles.Count);
			AssertEquals("db_backupoperator", group_BkOperator.Roles.First().GGR_RoleName);
			AssertEquals(1, group_DbOperator_2.Roles.Count);
			AssertEquals("db_datawriter", group_DbOperator_2.Roles.First().GGR_RoleName);
			AssertEquals(1, group_DbReader_2.Roles.Count);
			AssertEquals("cwRestrictedReaderRole", group_DbReader_2.Roles.First().GGR_RoleName);
			AssertEquals(1, group_BkOperator_2.Roles.Count);
			AssertEquals("db_backupoperator", group_BkOperator_2.Roles.First().GGR_RoleName);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Group = GetNewBusinessObject() as GlbGroup;
		}

		GlbGroup Group;

		const int ExpectedDefaultSecurityPermissions = 20;

		#endregion
	}
}
