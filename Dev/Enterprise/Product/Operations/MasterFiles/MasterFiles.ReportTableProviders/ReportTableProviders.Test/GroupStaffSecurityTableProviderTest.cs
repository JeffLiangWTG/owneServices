using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.ReportTableProviders.Testing
{
	internal class GroupStaffSecurityTableProviderTest : TestCaseWithFactory
	{
		SecurityFilterField filterField;
		GroupStaffSecurityTableProvider provider;
		CollectionProvider groupCollectionProvider;
		MultipleSelectionLookup groupMultipleSelectionLookup;

		SecurityFilterField FilterField
		{
			get
			{
				if (filterField == null)
				{
					filterField = new SecurityFilterField(Factory);
					filterField.DisplayName = "Security Right";
				}
				return filterField;
			}
		}

		GroupStaffSecurityTableProvider Provider
		{
			get { return provider ?? (provider = new GroupStaffSecurityTableProvider()); }
		}

		MultipleSelectionLookup GroupMultipleSelectionLookup
		{
			get
			{
				if (groupMultipleSelectionLookup == null)
				{
					groupMultipleSelectionLookup = new MultipleSelectionLookup(Factory);
					groupMultipleSelectionLookup.DisplayName = "Limit Staff To Those That Belong To All These Groups";
					groupMultipleSelectionLookup.SetCollectionProvider(GroupCollectionProvider);
				}
				return groupMultipleSelectionLookup;
			}
		}

		CollectionProvider GroupCollectionProvider
		{
			get { return groupCollectionProvider ?? (groupCollectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "groups")); }
		}

		void AssertRow(DataRow row, string securityRight, string code, string fullName, string groups, string isActive, string isController)
		{
			AssertEquals("SecurityRight", securityRight, row[0].ToString());
			AssertEquals("Code", code, row[1].ToString());
			AssertEquals("FullName", fullName, row[2].ToString());
			AssertEquals("Groups", groups, row[5].ToString());
			AssertEquals("IsActive", isActive, row[6].ToString());
			AssertEquals("IsController", isController, row[7].ToString());
		}

		void AssertSchedules_ReportAll(DataTable table, int schedulesIndex)
		{
			AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 1], "Operate -> Schedules", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 2], "Operate -> Schedules -> Sailing Schedule", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 3], "Operate -> Schedules -> Sailing Schedule", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 4], "Operate -> Schedules -> Sailing Schedule -> Module Access", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 5], "Operate -> Schedules -> Sailing Schedule -> Module Access", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 6], "Operate -> Schedules -> Sailing Schedule -> Edit", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 7], "Operate -> Schedules -> Sailing Schedule -> Edit", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 8], "Operate -> Schedules -> Sailing Schedule -> Delete", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[schedulesIndex + 9], "Operate -> Schedules -> Sailing Schedule -> Delete", "J", "Jane", "ALL,GRL", "Y", "N");

			int flightScheduleIndex = GetIndexOfSecurity(table, Env.Security.FlightSchedule);

			AssertRow(table.Rows[flightScheduleIndex], "Operate -> Schedules -> Flight Schedule", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 1], "Operate -> Schedules -> Flight Schedule", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 2], "Operate -> Schedules -> Flight Schedule -> Module Access", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 3], "Operate -> Schedules -> Flight Schedule -> Module Access", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 4], "Operate -> Schedules -> Flight Schedule -> Edit", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 5], "Operate -> Schedules -> Flight Schedule -> Edit", "J", "Jane", "ALL,GRL", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 6], "Operate -> Schedules -> Flight Schedule -> Delete", "B", "Bob", "ALL,BOY", "Y", "N");
			AssertRow(table.Rows[flightScheduleIndex + 7], "Operate -> Schedules -> Flight Schedule -> Delete", "J", "Jane", "ALL,GRL", "Y", "N");
		}

		Report GetNewReport()
		{
			Report result = new Report(null, null, Guid.Empty, Constants.DataContext.None);
			result.FilterCollection.Add(FilterField);
			result.FilterCollection.Add(GroupMultipleSelectionLookup);
			return result;
		}

		int GetIndexOfSecurity(DataTable table, SecurityCheckpoint checkpoint)
		{
			for (int i = 0; i < table.Rows.Count; i++)
			{
				if (table.Rows[i][0].ToString().Trim() == checkpoint.DisplayTextPathToSecurityRight)
				{
					return i;
				}
			}
			return -1;
		}

		void MakeCollectionMembersInactive(IBusinessObjectCollection collection, SchemaColumn activeColumn)
		{
			if (collection is BusinessObjectCollection)
			{
				((BusinessObjectCollection)collection).Load();
			}
			foreach (BusinessObject element in collection)
			{
				element[activeColumn] = ZBool.False;
			}
		}

		GlbGroup GroupBoys { get; set; }
		GlbGroup GroupGirls { get; set; }

		void SetUpData()
		{
			TestCaseHelper.ClearTable(GlbSecuritySchema.Constants.TableName);

			GlbStaffCollection staffs = new GlbStaffCollection(Factory);
			MakeCollectionMembersInactive(staffs, GlbStaffSchema.GS_IsActive);

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = "Bob";
			staff1.GS_Code = "B";
			staff2.GS_FullName = "Jane";
			staff2.GS_Code = "J";
			staff3.GS_FullName = "Tom";
			staff3.GS_Code = "T";

			staff4.GS_IsOperational = false;

			GlbSecurity security1 = staff3.StaffSecurityPermissionsCollection.AddNew();
			security1.GU_SecurityRight = Env.Security.Operations.Code;
			security1.GU_SecurityItemIsAllowed = false;

			GlbSecurity security2 = staff2.StaffSecurityPermissionsCollection.AddNew();
			security2.GU_SecurityRight = Env.Security.Manage.Code;
			security2.GU_SecurityItemIsAllowed = false;

			GlbSecurity security3 = staff1.StaffSecurityPermissionsCollection.AddNew();
			security3.GU_SecurityRight = Env.Security.Manage.Code;
			security3.GU_SecurityItemIsAllowed = true;

			GroupBoys = Factory.New<GlbGroup>();
			GroupBoys.GG_Code = "BOY";
			GroupBoys.GG_Desc = "Boys";
			GroupBoys.SecurityPermissions.RemoveAndDeleteAll();
			var security4 = GroupBoys.SecurityPermissions.AddNew();
			security4.GU_SecurityRight = Env.Security.Manage.Code;
			security4.GU_SecurityItemIsAllowed = false;

			GroupGirls = Factory.New<GlbGroup>();
			GroupGirls.GG_Code = "GRL";
			GroupGirls.GG_Desc = "Girls";
			GroupGirls.SecurityPermissions.RemoveAndDeleteAll();

			GroupBoys.Staff.Add(staff1);
			GroupBoys.Staff.Add(staff3);

			GroupGirls.Staff.Add(staff2);
			Factory.Save();

			// remove controller staff
			controllerStaff.GS_IsActive = false;
			factory2.Save();
		}

		public void TestGetDataTable_AllSecurities()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupByStaff = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "B", "Bob", "ALL,BOY", "Y", "N");
					AssertRow(table.Rows[1], "Operate", "J", "Jane", "ALL,GRL", "Y", "N");

					int securityIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertSchedules_ReportAll(table, securityIndex);

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "B", "Bob", "*StaffProfile,ALL", "Y", "N");
					AssertRow(table.Rows[accountsIndex + 1], "Manage", "T", "Tom", "ALL", "Y", "N");
					AssertRow(table.Rows[accountsIndex + 2], "Manage -> Client Relationship Management", "B", "Bob", "*StaffProfile,ALL", "Y", "N");
					AssertRow(table.Rows[accountsIndex + 3], "Manage -> Client Relationship Management", "T", "Tom", "ALL", "Y", "N");

					AssertEquals(4, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}

		public void TestGetDataTable_InvalidFilterField()
		{
			using (Report report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AssertExceptionThrown<DataProviderException>(() => Provider.GetDataTable("TableName", null, report, false));
			}
		}

		public void TestGetDataTable_SingleSecurityWithChildren()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.Schedules.LookupKey;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertSchedules_ReportAll(table, 0);
					AssertEquals("Manage should not be reported.", -1, GetIndexOfSecurity(table, Env.Security.Manage));

					AssertEquals("all securities loaded - too many children", 4, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}

		public void TestGetDataTable_SingleSecurityWithoutChildren()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.SailingScheduleEdit.LookupKey;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate -> Schedules -> Sailing Schedule -> Edit", "B", "Bob", "ALL,BOY", "Y", "N");
					AssertEquals("Manage should not be reported.", -1, GetIndexOfSecurity(table, Env.Security.Manage));

					AssertEquals("loaded only relevant securities", 1, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}
	}
}
