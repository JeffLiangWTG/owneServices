using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class CRMSecurityProviderTest<T> : TestCaseWithFactory where T : BusinessObject
	{
		protected abstract CRMSecurityProvider<T> GetNewProviderForTest();

		protected abstract IEnumerable<T> GetTestObjectWithOrgStaffAssignment();

		protected abstract void AddStaffAssignmentForCompany(T obj, ZString staffCode, ZString role, ZGuid companyPk);

		protected abstract IEnumerable<T> GetTestObjectWithBizObjStaffAssignment();

		protected abstract IEnumerable<T> GetTestObjectWithoutStaffAssignment();

		protected abstract IEnumerable<T> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org);

		protected virtual IEnumerable<(T testObject, bool isVisibleOnStandardLevel, bool isVisibleOnEnhancedLevel, string assertionMessage)> GetTestDataForOrgSecurityGroupsSecurityLevel()
			=> Enumerable.Empty<(T testObject, bool isVisibleOnStandardLevel, bool isVisibleOnEnhancedLevel, string assertionMessage)>();

		protected virtual IEnumerable<T> GetTestObjectWithTaskAssignment()
		{
			if (!typeof(IWorkflowProvider).IsAssignableFrom(typeof(T)))
			{
				return Enumerable.Empty<T>();
			}

			var capability = GlbStaff.CurrentUser.Capabilities.AddNew();
			capability.G4_Code = "CP1";
			capability.Factory.Save();

			var obj1 = Factory.NewWithValidTestData<T>();
			var workflowItem1 = GetWorkflowProvider(obj1).WorkflowItems.AddNew();
			workflowItem1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var obj2 = Factory.NewWithValidTestData<T>();
			var workflowItem2 = GetWorkflowProvider(obj2).WorkflowItems.AddNew();
			workflowItem2.P9_G4_RequiredCapability = capability.PK;

			return new T[] { obj1, obj2 };
		}

		protected virtual IEnumerable<T> GetTestObjectsWithoutOrg()
		{
			return ProviderForTest.ShouldReturnEmptyOrgAddress ? GetTestObjectWithoutStaffAssignment() : Enumerable.Empty<T>();
		}

		protected virtual ZQuery SetupCRMSecurityFilterStripsQuery(ZQuery query)
		{
			return query;
		}

		protected IWorkflowProvider GetWorkflowProvider(T obj)
		{
			var target = ProviderForTest.SecurityTargetObjectReference;
			var result = target != null ? obj.Factory.Load(target.Item1, (ZGuid)obj[target.Item2]) : obj;
			return result as IWorkflowProvider;
		}

		IEnumerable<T> GetAllTestObjs()
		{
			var objs = GetTestObjectWithOrgStaffAssignment().Union(GetTestObjectWithBizObjStaffAssignment()).Union(GetTestObjectWithoutStaffAssignment()).Union(GetTestObjectWithTaskAssignment()).ToArray();
			Factory.Save();
			return objs;
		}

		GlbStaff AnotherUser
		{
			get
			{
				if (anotherUser == null)
				{
					anotherUser = Factory.NewWithValidTestData<GlbStaff>();
					anotherUser.GS_LoginName = "anotheruser";
					Factory.Save();
				}
				return anotherUser;
			}
		}
		GlbStaff anotherUser;

		public void TestStaffAssignmentFilter()
		{
			GetAllTestObjs();
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
			var filter = filters["Assignment and Ownership Security"] as ModuleNkFilter;
			var query = PrepareQuery(SetupCRMSecurityFilterStripsQuery(filter.Query));
			var bizObj = Factory.Load<T>(query);
			AssertNotEquals("Should be business objects loaded", 0, bizObj.Length);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(AnotherUser.GS_LoginName))
			{
				query = PrepareQuery(SetupCRMSecurityFilterStripsQuery(filter.Query));
				bizObj = Factory.Load<T>(query);
				AssertStaffAssignmentFilterResult(bizObj);
			}
		}

		protected virtual void AssertStaffAssignmentFilterResult(IEnumerable<T> results)
		{
			AssertEquals("No business objects should be loaded", 0, results.Count());
		}

		[ExpectNoExceptions]
		public void TestTaskAssignmentFilter()
		{
			GetAllTestObjs();
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;
			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
			var filter = filters["Task Collaboration Security"] as ModuleNkFilter;
			var query = SetupCRMSecurityFilterStripsQuery(filter.Query);
			var bizObj = Factory.Load<T>(query);

			if (typeof(IWorkflowProvider).IsAssignableFrom(typeof(T)))
			{
				AssertNotEquals("Should be business objects loaded", 0, bizObj.Length);

				using (CurrentUserChanger.SwitchToNewUserTemporarily(AnotherUser.GS_LoginName))
				{
					query = PrepareQuery(SetupCRMSecurityFilterStripsQuery(filter.Query));
					bizObj = Factory.Load<T>(query);
					AssertEquals("No business objects should be loaded", 0, bizObj.Length);
				}
			}
		}

		public void TestTaskAssignmentFilter_LogonToOtherCompany()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew();
			anotherBranch.FillWithValidTestData();
			var testBizObjs = GetTestObjectWithTaskAssignment();
			Factory.Save();

			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Task Collaboration Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);
				var loadedBizObjs = Factory.Load<T>(query);
				AssertEquals("No assigned tasks for login company", 0, loadedBizObjs.Length);
			}

			foreach (var bizObj in testBizObjs)
			{
				var workflowProvider = GetWorkflowProvider(bizObj);
				foreach (ProcessTask task in workflowProvider.WorkflowItems)
				{
					task.P9_ShareTasksForAllCompanies = true;
				}
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Task Collaboration Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);
				var loadedBizObjs = Factory.Load<T>(query);
				AssertEquals("Tasks are shared accross all companies", testBizObjs.Count(), loadedBizObjs.Length);
			}
		}

		[ExpectNoExceptions]
		public void TestOSMGFilter()
		{
			GetAllTestObjs();

			OrgOSMG.MiscServ.OM_GG_OrgSecurityGroup = OSMG.PK;
			var testObjectsWithOrgForOSMG = GetTestObjectWithOrgAssignedForOSMG(OrgOSMG);
			Factory.Save();

			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);
				var bizObj = Factory.Load<T>(query);

				AssertOSMGFilterWithOsmgOrgsResults(testObjectsWithOrgForOSMG, bizObj);

				using (CurrentUserChanger.SwitchToNewUserTemporarily(AnotherUser.GS_LoginName))
				{
					query = PrepareQuery(SetupCRMSecurityFilterStripsQuery(filter.Query));
					bizObj = Factory.Load<T>(query);
					AssertOSMGFilterWithAnotherUserResults(bizObj);
				}
			}
		}

		protected virtual void AssertOSMGFilterWithOsmgOrgsResults(IEnumerable<T> orgObjectsForOSMG, IEnumerable<T> results)
		{
			AssertEquals("Business objects with org assigned for OSMG should be loaded", orgObjectsForOSMG.Count(), results.Count());
		}

		protected virtual void AssertOSMGFilterWithAnotherUserResults(IEnumerable<T> results)
		{
			AssertEquals("No business objects should be loaded", 0, results.Count());
		}

		[ExpectNoExceptions]
		public void TestOSMGFilter_NoAccessToOrgWithoutOSMG()
		{
			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				OrgOSMG.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.MiscServ.OM_GG_OrgSecurityGroup = OSMG.PK;

				var org2 = Factory.NewWithValidTestData<OrgHeader>();

				var objsWithSecurityGroup = GetTestObjectWithOrgAssignedForOSMG(org1);
				var objsWithEmptySecurityGroup = GetTestObjectWithOrgAssignedForOSMG(org2);

				Factory.Save();

				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);

				var objs = Factory.Load<T>(query);
				AssertContainsExactElementsInAnyOrder(objsWithSecurityGroup, objs);
			}
		}

		public void TestOSMGFilter_AccessToObjWithoutOrg()
		{
			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null && ProviderForTest.RelatedOrgAddressColumns.Any(c => c.IsNullable))
			{
				OrgOSMG.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;

				GetAllTestObjs();
				var objectsWithoutOrg = GetTestObjectsWithoutOrg();

				Factory.Save();

				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
				var filters = new ModuleFilterCollection();
				ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
				var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query = SetupCRMSecurityFilterStripsQuery(filter.Query);

				var objs = Factory.Load<T>(query);
				AssertContainsExactElementsInAnyOrder(objectsWithoutOrg, objs);
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestMixedFilter()
		{
			GetAllTestObjs();
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			}

			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
			var filter1 = filters["Assignment and Ownership Security"] as ModuleNkFilter;
			var filter2 = filters["Task Collaboration Security"] as ModuleNkFilter;

			var query1 = PrepareQuery(filter1.Query);
			var query2 = PrepareQuery(filter2.Query);

			var bizObj1 = Factory.Load<T>(query1);
			var bizObj2 = Factory.Load<T>(query2);

			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				var filter3 = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query3 = PrepareQuery(SetupCRMSecurityFilterStripsQuery(filter3.Query));
				var bizObj3 = Factory.Load<T>(query3);
			}
		}

		[ExpectNoExceptions]
		public void TestStaffAssignmentCheckpoint()
		{
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			if (ProviderForTest.CRMSecurity.EditByStaffNotAssigned != null)
			{
				ProviderForTest.CRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup["ACT"].IsAllowed = false;
			}

			var objs = GetAllTestObjs();
			foreach (var obj in objs)
			{
				var view1 = ProviderForTest.GetSecurityCheckpoint(null, FormAction.View, Env.Security.None);
				var view2 = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);

				var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
				var delete = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Delete, Env.Security.None);
			}
		}

		[ExpectNoExceptions]
		public void TestStaffAssignmentCheckpoint_ViewInOtherCompany()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew();
			anotherBranch.FillWithValidTestData();
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			var objs = GetTestObjectWithOrgStaffAssignment();
			Factory.Save();

			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			if (ProviderForTest.CRMSecurity.EditByStaffNotAssigned != null)
			{
				ProviderForTest.CRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup["ACT"].IsAllowed = false;
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				foreach (var obj in objs)
				{
					AddStaffAssignmentForCompany(obj, newStaff.GS_Code, "SAL", anotherCompany.PK);
					Factory.Save();
				}
			}

			foreach (var obj in objs)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				Assert("Should have access even staff assignment is not for current login company", view.IsAllowed);
			}
		}

		public virtual void TestObjectFoundByOtherFilter()
		{
			var staffAssigned = GetTestObjectWithOrgStaffAssignment();
			var staffNone = GetTestObjectWithoutStaffAssignment();

			Factory.Save();

			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;
			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			}

			foreach (var obj in staffAssigned)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				Assert("+VIEW +ViewByStaffNotAssigned=N", view.IsAllowed);
			}
		}

		public void TestStaffAssignmentCheckpoint_DenyOrgWithoutStaffAssignments()
		{
			var staffAssigned = GetTestObjectWithOrgStaffAssignment();
			var staffNone = GetTestObjectWithoutStaffAssignment();

			Factory.Save();

			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			if (ProviderForTest.CRMSecurity.EditByStaffNotAssigned != null)
			{
				ProviderForTest.CRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
			}

			foreach (var obj in staffNone)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				Assert("-VIEW -ViewByStaffNotAssigned=N", !view.IsAllowed);

				var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
				Assert("-EDIT -ViewByStaffNotAssigned=N", !edit.IsAllowed);
			}
			foreach (var obj in staffAssigned)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				Assert("+VIEW +ViewByStaffNotAssigned=N", view.IsAllowed);
			}

			if (ProviderForTest.CRMSecurity.EditByStaffNotAssigned != null)
			{
				if (ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup != null)
				{
					foreach (var role in ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup.Values)
					{
						role.IsAllowed = true;
					}

					foreach (var obj in staffNone)
					{
						var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
						Assert("-VIEW -ViewByStaffNotAssigned=N +EditByStaffNotAssigned=N", !view.IsAllowed);
					}

					foreach (var obj in staffAssigned)
					{
						var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
						Assert("+EDIT +ViewByStaffNotAssigned=N +EditByStaffNotAssigned=N", edit.IsAllowed);
					}

					foreach (var role in ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup.Values)
					{
						role.IsAllowed = false;
					}

					foreach (var obj in staffAssigned)
					{
						var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
						Assert("-EDIT +ViewByStaffNotAssigned=N -EditByStaffNotAssigned=N", !edit.IsAllowed);

						var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
						Assert("+VIEW +ViewByStaffNotAssigned=N -EditByStaffNotAssigned=N", view.IsAllowed);
					}

					ProviderForTest.CRMSecurity.EditByStaffNotAssigned.IsAllowed = true;

					foreach (var obj in staffAssigned)
					{
						var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
						Assert("-EDIT +ViewByStaffNotAssigned=N -EditByStaffNotAssigned=Y", !edit.IsAllowed);

						var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
						Assert("+VIEW +ViewByStaffNotAssigned=N -EditByStaffNotAssigned=Y", view.IsAllowed);
					}
				}
			}

			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;

			foreach (var obj in staffNone)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				Assert("+VIEW +ViewByStaffNotAssigned=Y", view.IsAllowed);
			}
		}

		public void TestSecurityLevel_OrgSecurityGroups()
		{
			var testData = GetTestDataForOrgSecurityGroupsSecurityLevel();
			AssertSecurityLevel(ProviderForTest.OSMGSecurityLevelRegistryItem, ProviderForTest.CRMSecurity.IgnoreOSMG, "Org. Security Group Security", testData);
		}

		public void TestSecurityLevel_OrgSecurityGroupsWithTaskAndStaffAssignmentsDenied()
		{
			var testData = GetTestDataForOrgSecurityGroupsSecurityLevel();
			var crmSecurity = ProviderForTest.CRMSecurity;
			if (crmSecurity.ViewByStaffNotAssigned != null)
			{
				crmSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			}
			if (crmSecurity.IgnoreTaskAssignment != null)
			{
				crmSecurity.IgnoreTaskAssignment.IsAllowed = false;
			}
			AssertSecurityLevel(ProviderForTest.OSMGSecurityLevelRegistryItem, crmSecurity.IgnoreOSMG, "Org. Security Group Security", testData);
		}

		void AssertSecurityLevel(CodePairRegistryItem securityLevelRegistryItem, SecurityCheckpoint securityCheckpoint, string filterName, IEnumerable<(T testObject, bool isVisibleOnStandardLevel, bool isVisibleOnEnhancedLevel, string assertionMessage)> testData)
		{
			if (securityLevelRegistryItem != null)
			{
				securityCheckpoint.IsAllowed = false;
				var dataList = testData.ToList();
				Factory.Save();

				var bizos = GetBizosMatchingFilter(securityLevelRegistryItem, Core.Constants.OSMGSecurityLevels.Standard, filterName);

				foreach (var (testObject, isVisibleOnStandardLevel, _, assertionMessage) in dataList)
				{
					var bizoIsVisible = bizos.Any(r => r.PK == testObject.PK);
					AssertEquals(assertionMessage + $". Expected value for 'Standard' is '{isVisibleOnStandardLevel}' but real value was '{bizoIsVisible}'.", isVisibleOnStandardLevel, bizoIsVisible);
				}

				ProviderForTest = GetNewProviderForTest();
				bizos = GetBizosMatchingFilter(securityLevelRegistryItem, Core.Constants.OSMGSecurityLevels.Enhanced, filterName);

				foreach (var (testObject, _, isVisibleOnEnhancedLevel, assertionMessage) in dataList)
				{
					var bizoIsVisible = bizos.Any(r => r.PK == testObject.PK);
					AssertEquals(assertionMessage + $". Expected value for 'Enhanced' is '{isVisibleOnEnhancedLevel}' but real value was '{bizoIsVisible}'.", isVisibleOnEnhancedLevel, bizoIsVisible);
				}
			}
			else
			{
				Assert(true);
			}
		}

		BusinessObject[] GetBizosMatchingFilter(CodePairRegistryItem securityLevelRegistryItem, string securityLevel, string filterName)
		{
			securityLevelRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, securityLevel);
			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);
			var query = SetupCRMSecurityFilterStripsQuery(filters.GetFilterQuery(filters));
			return Factory.Load<T>(query);
		}

		[ExpectNoExceptions]
		public void TestTaskAssignmentCheckpoint()
		{
			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			var objs = GetAllTestObjs();
			foreach (var obj in objs)
			{
				var view1 = ProviderForTest.GetSecurityCheckpoint(null, FormAction.View, Env.Security.None);
				var view2 = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);

				var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
				var delete = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Delete, Env.Security.None);
			}
		}

		[ExpectNoExceptions]
		public void TestOSMGCheckpoint()
		{
			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			}

			var objs = GetAllTestObjs();
			foreach (var obj in objs)
			{
				var view1 = ProviderForTest.GetSecurityCheckpoint(null, FormAction.View, Env.Security.None);
				var view2 = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);

				var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
				var delete = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Delete, Env.Security.None);
			}

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = OSMG.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var objsWithSecurityGroup = GetTestObjectWithOrgAssignedForOSMG(org1);
			var objsWithEmptySecurityGroup = GetTestObjectWithOrgAssignedForOSMG(org2);

			Factory.Save();

			foreach (var obj in objsWithSecurityGroup)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				AssertEquals(true, view.IsAllowed);
			}

			foreach (var obj in objsWithEmptySecurityGroup)
			{
				var view = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);
				AssertEquals(false, view.IsAllowed);
			}
		}

		[ExpectNoExceptions]
		public void TestMixedCheckpoint()
		{
			ProviderForTest.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			if (ProviderForTest.CRMSecurity.EditByStaffNotAssigned != null)
			{
				ProviderForTest.CRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				ProviderForTest.CRMSecurity.EditByStaffRoleAssignedLookup["ACT"].IsAllowed = false;
			}

			ProviderForTest.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (ProviderForTest.CRMSecurity.IgnoreOSMG != null)
			{
				ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			}

			var objs = GetAllTestObjs();
			foreach (var obj in objs)
			{
				var view1 = ProviderForTest.GetSecurityCheckpoint(null, FormAction.View, Env.Security.None);
				var view2 = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.View, Env.Security.None);

				var edit = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Edit, Env.Security.None);
				var delete = ProviderForTest.GetSecurityCheckpoint(obj, FormAction.Delete, Env.Security.None);
			}
		}

		#region Implementation

		public static void AssertController(ZController controller, T bizObjWithoutAccess, CRMSecurity crmSecurity)
		{
			bizObjWithoutAccess.Factory.Save();

			crmSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			if (crmSecurity.EditByStaffNotAssigned != null)
			{
				crmSecurity.EditByStaffNotAssigned.IsAllowed = false;
				crmSecurity.EditByStaffRoleAssignedLookup["ACT"].IsAllowed = false;
			}

			crmSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (crmSecurity.IgnoreOSMG != null)
			{
				crmSecurity.IgnoreOSMG.IsAllowed = false;
			}

			var view1 = controller.GetCheckPointForView(null);
			var view2 = controller.GetCheckPointForView(bizObjWithoutAccess);
			var edit = controller.GetCheckPointForEdit(bizObjWithoutAccess);
			var delete = controller.GetCheckPointForDelete(bizObjWithoutAccess);

			Assert(true);
		}

		public static void AssertFilterStrip(Func<FilterStripBusinessObject> filterStripBusinessObjectGetter, CRMSecurity crmSecurity)
		{
			var factory = new BusinessObjectFactory();
			crmSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			crmSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (crmSecurity.IgnoreOSMG != null)
			{
				crmSecurity.IgnoreOSMG.IsAllowed = false;
			}

			var filters = filterStripBusinessObjectGetter();
			var filter1 = filters["Assignment and Ownership Security"] as ModuleNkFilter;
			var filter2 = filters["Task Collaboration Security"] as ModuleNkFilter;

			var query1 = PrepareQuery(filter1.Query);
			var query2 = PrepareQuery(filter2.Query);

			var bizObj1 = factory.Load<T>(query1);
			var bizObj2 = factory.Load<T>(query2);

			if (crmSecurity.IgnoreOSMG != null)
			{
				var filter3 = filters["Org. Security Group Security"] as ModuleNkFilter;
				var query3 = PrepareQuery(filter3.Query);
				var bizObj3 = factory.Load<T>(query3);
			}

			Assert(true);
		}

		protected static ZQuery PrepareQuery(ZQuery query)
		{
			if (typeof(T) == typeof(OrgHeader))
			{
				query.MaximumRows = 1;
			}

			if (typeof(T) == typeof(JobHeader))
			{
				var mainQuery = new ZDBOnlyQuery(typeof(T));
				mainQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				mainQuery.AddToFilter(query);
				return mainQuery;
			}
			else
			{
				return query;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrgOSMG = Factory.NewWithValidTestData<OrgHeader>();
			var address = OrgOSMG.Addresses.AddNewMainAddress();
			OSMG = Factory.NewWithValidTestData<GlbGroup>();
			NonOSMG = Factory.NewWithValidTestData<GlbGroup>();
			OSMG.Staff.Add(GlbStaff.CurrentUser);
			OSMG.Organisation.Add(OrgOSMG);
			Factory.Save();
			GlbStaff.CurrentUser.Factory.Save();
			ProviderForTest = GetNewProviderForTest();
		}

		protected CRMSecurityProvider<T> ProviderForTest;
		protected OrgHeader OrgOSMG;
		protected GlbGroup OSMG;
		protected GlbGroup NonOSMG;

		#endregion
	}
}
