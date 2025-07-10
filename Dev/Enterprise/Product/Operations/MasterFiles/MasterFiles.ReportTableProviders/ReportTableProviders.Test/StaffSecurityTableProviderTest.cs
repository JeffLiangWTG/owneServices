using System;
using System.Data;
using System.Text;
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
	internal class StaffSecurityTableProviderTest : TestCaseWithFactory
	{
		SecurityFilterField filterField;
		StaffSecurityTableProvider provider;
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

		StaffSecurityTableProvider Provider
		{
			get { return provider ?? (provider = new StaffSecurityTableProvider()); }
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

		void AssertRow(DataRow row, string securityRight, string staffName, string summary)
		{
			AssertEquals("SecurityRight", securityRight, row[0].ToString());
			AssertEquals("StaffName", staffName, row[1].ToString());
			AssertEquals("Summary", summary, row[2].ToString());
		}

		void AssertSchedules_GroupByStaff_ReportAll(DataTable table, int schedulesIndex)
		{
			AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Bob", "Denied");
			AssertRow(table.Rows[schedulesIndex + 1], "Operate -> Schedules", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 2], "Operate -> Schedules", "Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 3], "Operate -> Schedules -> Sailing Schedule", "Bob", "Denied");
			AssertRow(table.Rows[schedulesIndex + 4], "Operate -> Schedules -> Sailing Schedule", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 5], "Operate -> Schedules -> Sailing Schedule", "Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 6], "Operate -> Schedules -> Sailing Schedule -> Module Access", "Bob", "Denied");
			AssertRow(table.Rows[schedulesIndex + 7], "Operate -> Schedules -> Sailing Schedule -> Module Access", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 8], "Operate -> Schedules -> Sailing Schedule -> Module Access", "Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 9], "Operate -> Schedules -> Sailing Schedule -> Edit", "Bob", "Denied");
			AssertRow(table.Rows[schedulesIndex + 10], "Operate -> Schedules -> Sailing Schedule -> Edit", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 11], "Operate -> Schedules -> Sailing Schedule -> Edit", "Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 12], "Operate -> Schedules -> Sailing Schedule -> Delete", "Bob", "Denied");
			AssertRow(table.Rows[schedulesIndex + 13], "Operate -> Schedules -> Sailing Schedule -> Delete", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 14], "Operate -> Schedules -> Sailing Schedule -> Delete", "Tom", "Denied");

			int flightScheduleIndex = GetIndexOfSecurity(table, Env.Security.FlightSchedule);

			AssertRow(table.Rows[flightScheduleIndex], "Operate -> Schedules -> Flight Schedule", "Bob", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 1], "Operate -> Schedules -> Flight Schedule", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 2], "Operate -> Schedules -> Flight Schedule", "Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 3], "Operate -> Schedules -> Flight Schedule -> Module Access", "Bob", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 4], "Operate -> Schedules -> Flight Schedule -> Module Access", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 5], "Operate -> Schedules -> Flight Schedule -> Module Access", "Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 6], "Operate -> Schedules -> Flight Schedule -> Edit", "Bob", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 7], "Operate -> Schedules -> Flight Schedule -> Edit", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 8], "Operate -> Schedules -> Flight Schedule -> Edit", "Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 9], "Operate -> Schedules -> Flight Schedule -> Delete", "Bob", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 10], "Operate -> Schedules -> Flight Schedule -> Delete", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 11], "Operate -> Schedules -> Flight Schedule -> Delete", "Tom", "Denied");
		}

		void AssertSchedules_GroupBySummary_ReportAll(DataTable table, int schedulesIndex)
		{
			AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Bob, Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 1], "Operate -> Schedules", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 2], "Operate -> Schedules -> Sailing Schedule", "Bob, Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 3], "Operate -> Schedules -> Sailing Schedule", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 4], "Operate -> Schedules -> Sailing Schedule -> Module Access", "Bob, Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 5], "Operate -> Schedules -> Sailing Schedule -> Module Access", "Jane", "Granted");
			AssertRow(table.Rows[schedulesIndex + 6], "Operate -> Schedules -> Sailing Schedule -> Edit", "Bob, Tom", "Denied");
			AssertRow(table.Rows[schedulesIndex + 7], "Operate -> Schedules -> Sailing Schedule -> Edit", "Jane", "Granted");

			int flightScheduleIndex = GetIndexOfSecurity(table, Env.Security.FlightSchedule);

			AssertRow(table.Rows[flightScheduleIndex], "Operate -> Schedules -> Flight Schedule", "Bob, Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 1], "Operate -> Schedules -> Flight Schedule", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 2], "Operate -> Schedules -> Flight Schedule -> Module Access", "Bob, Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 3], "Operate -> Schedules -> Flight Schedule -> Module Access", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 4], "Operate -> Schedules -> Flight Schedule -> Edit", "Bob, Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 5], "Operate -> Schedules -> Flight Schedule -> Edit", "Jane", "Granted");
			AssertRow(table.Rows[flightScheduleIndex + 6], "Operate -> Schedules -> Flight Schedule -> Delete", "Bob, Tom", "Denied");
			AssertRow(table.Rows[flightScheduleIndex + 7], "Operate -> Schedules -> Flight Schedule -> Delete", "Jane", "Granted");
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

		GlbGroup GroupUnisex { get; set; }
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
			staff1.GS_IsOperational = false;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = "Bob";
			staff2.GS_FullName = "Jane";
			staff3.GS_FullName = "Tom";

			GlbSecurity security1 = staff3.StaffSecurityPermissionsCollection.AddNew();
			security1.GU_SecurityRight = Env.Security.Operations.Code;
			security1.GU_SecurityItemIsAllowed = false;

			GlbSecurity security2 = staff2.StaffSecurityPermissionsCollection.AddNew();
			security2.GU_SecurityRight = Env.Security.Manage.Code;
			security2.GU_SecurityItemIsAllowed = false;

			GroupUnisex = Factory.New<GlbGroup>();
			GroupUnisex.GG_Code = "UNI";
			GroupUnisex.GG_Desc = "Unisex";
			GroupUnisex.SecurityPermissions.RemoveAndDeleteAll();

			GroupBoys = Factory.New<GlbGroup>();
			GroupBoys.GG_Code = "BOY";
			GroupBoys.GG_Desc = "Boys";
			GroupBoys.SecurityPermissions.RemoveAndDeleteAll();

			GroupGirls = Factory.New<GlbGroup>();
			GroupGirls.GG_Code = "GRL";
			GroupGirls.GG_Desc = "Girls";
			GroupGirls.SecurityPermissions.RemoveAndDeleteAll();

			GroupUnisex.Staff.Add(staff1);
			GroupUnisex.Staff.Add(staff2);
			GroupUnisex.Staff.Add(staff3);

			GroupBoys.Staff.Add(staff1);
			GroupBoys.Staff.Add(staff3);
			GroupGirls.Staff.Add(staff2);

			Factory.Save();

			// remove controller staff
			controllerStaff.GS_IsActive = false;
			factory2.Save();
		}

		public void TestGetDataTable_AllSecurities_GroupByStaff()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupByStaff = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Bob", "Denied");
					AssertRow(table.Rows[1], "Operate", "Jane", "Granted");
					AssertRow(table.Rows[2], "Operate", "Tom", "Denied");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertSchedules_GroupByStaff_ReportAll(table, schedulesIndex);

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Bob", "Denied");
					AssertRow(table.Rows[accountsIndex + 1], "Manage", "Jane", "Denied");
					AssertRow(table.Rows[accountsIndex + 2], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 3], "Manage -> Client Relationship Management", "Bob", "Denied");
					AssertRow(table.Rows[accountsIndex + 4], "Manage -> Client Relationship Management", "Jane", "Denied");
					AssertRow(table.Rows[accountsIndex + 5], "Manage -> Client Relationship Management", "Tom", "Granted");
				}
			}
		}

		public void TestGetDataTable_AllSecurities_GroupByStaff_HideDeniedRights()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupByStaff = true;
				FilterField.HideDeniedRights = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Jane", "Granted");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Jane", "Granted");

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 1], "Manage -> Client Relationship Management", "Tom", "Granted");
				}
			}
		}

		public void TestGetDataTable_AllSecurities_GroupBySummary()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupBySummary = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Bob, Tom", "Denied");
					AssertRow(table.Rows[1], "Operate", "Jane", "Granted");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertSchedules_GroupBySummary_ReportAll(table, schedulesIndex);

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Bob, Jane", "Denied");
					AssertRow(table.Rows[accountsIndex + 1], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 2], "Manage -> Client Relationship Management", "Bob, Jane", "Denied");
					AssertRow(table.Rows[accountsIndex + 3], "Manage -> Client Relationship Management", "Tom", "Granted");
				}
			}
		}

		public void TestGetDataTable_AllSecurities_GroupBySummary_HideDeniedRights()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupBySummary = true;
				FilterField.HideDeniedRights = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Jane", "Granted");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Jane", "Granted");
					AssertRow(table.Rows[schedulesIndex + 1], "Operate -> Schedules -> Sailing Schedule", "Jane", "Granted");

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 1], "Manage -> Client Relationship Management", "Tom", "Granted");
				}
			}
		}

		public void TestGetDataTable_AllSecurities_GroupByStaff_HideDeniedRights_GroupToFilterBy()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
				FilterField.GroupByStaff = true;
				FilterField.HideDeniedRights = true;

				var groupCollection = GroupMultipleSelectionLookup.GetCollection<IBusinessObjectCollection>();
				groupCollection.Clear();
				groupCollection.AddRange(GroupUnisex);

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Jane", "Granted");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Jane", "Granted");

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 1], "Manage -> Client Relationship Management", "Tom", "Granted");

					AssertEquals(2, SecurityIterator<string>.LastSecurityCollection.Count);
				}

				groupCollection.Clear();
				groupCollection.AddRange(GroupBoys);

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);
					AssertRow(table.Rows[accountsIndex], "Manage", "Tom", "Granted");
					AssertRow(table.Rows[accountsIndex + 1], "Manage -> Client Relationship Management", "Tom", "Granted");

					AssertEquals(1, SecurityIterator<string>.LastSecurityCollection.Count);
				}

				groupCollection.Clear();
				groupCollection.AddRange(GroupGirls);

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate", "Jane", "Granted");

					int schedulesIndex = GetIndexOfSecurity(table, Env.Security.Schedules);
					AssertRow(table.Rows[schedulesIndex], "Operate -> Schedules", "Jane", "Granted");

					int accountsIndex = GetIndexOfSecurity(table, Env.Security.Manage);

					Assert("There shouldn't be Manage section", accountsIndex < 0);

					AssertEquals(1, SecurityIterator<string>.LastSecurityCollection.Count);
				}

				groupCollection.Clear();
				groupCollection.AddRange(GroupBoys);
				groupCollection.AddRange(GroupGirls);

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertEquals("Table should be empty", 0, table.Rows.Count);

					AssertEquals(0, SecurityIterator<string>.LastSecurityCollection.Count);
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

		public void TestGetDataTable_SingleSecurityWithChildren_GroupByStaff()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.Schedules.LookupKey;
				FilterField.GroupByStaff = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertSchedules_GroupByStaff_ReportAll(table, 0);
					AssertEquals("Manage should not be reported.", -1, GetIndexOfSecurity(table, Env.Security.Manage));

					AssertEquals("there are too many children of Schedules, so we get all rows", 2, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}

		public void TestGetDataTable_SingleSecurityWithChildren_GroupBySummary()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.Schedules.LookupKey;
				FilterField.GroupBySummary = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertSchedules_GroupBySummary_ReportAll(table, 0);
					AssertEquals("Manage should not be reported.", -1, GetIndexOfSecurity(table, Env.Security.Manage));

					AssertEquals("there are too many children of Schedules, so we get all rows", 2, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}

		public void TestGetDataTable_SingleSecurityWithoutChildren_GroupByStaff()
		{
			SetUpData();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.SailingScheduleEdit.LookupKey;
				FilterField.GroupByStaff = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], "Operate -> Schedules -> Sailing Schedule -> Edit", "Bob", "Denied");
					AssertEquals("Manage should not be reported.", -1, GetIndexOfSecurity(table, Env.Security.Manage));

					AssertEquals("get just rows relating to SailingScheduleEdit", 1, SecurityIterator<string>.LastSecurityCollection.Count);
				}
			}
		}

		public void TestGetDataTableWithGroupBySummaryWrapsRows()
		{
			GlbStaffCollection staffs = new GlbStaffCollection(Factory);
			MakeCollectionMembersInactive(staffs, GlbStaffSchema.GS_IsActive);

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			for (int i = 0; i < 50; i++)
			{
				GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = i.ToString().PadLeft(20, '0');
			}

			Factory.Save();

			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = Env.Security.FlightScheduleEdit.LookupKey;
				FilterField.GroupBySummary = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					int index = GetIndexOfSecurity(table, Env.Security.FlightScheduleEdit);

					StringBuilder builder = new StringBuilder();
					for (int i = 0; i < 35; i++)
					{
						builder.Append(i.ToString().PadLeft(20, '0'));
						builder.Append(i < 34 ? ", " : ",");
					}
					AssertRow(table.Rows[index], "Operate -> Schedules -> Flight Schedule -> Edit", builder.ToString(), "Denied");

					builder = new StringBuilder();
					for (int i = 35; i < 50; i++)
					{
						builder.Append(i.ToString().PadLeft(20, '0'));
						if (i < 49)
						{
							builder.Append(", ");
						}
					}
					AssertRow(table.Rows[index + 1], "", builder.ToString(), "");
				}
			}
		}

		public void TestGetDataTable_AllSecurities_SystemRegistryEditSpecificSettings()
		{
			SetUpData();

			AssertSystemRegistryEditSpecificSettings("RegCatCustoms", "Maintain -> System -> Registry -> Edit Specific Settings -> Customs");
			AssertSystemRegistryEditSpecificSettings("RegASSFTPConnectionTimeout", "Maintain -> System -> Registry -> Edit Specific Settings -> System -> FTP Service -> FTP Request Time-out");
		}

		void AssertSystemRegistryEditSpecificSettings(string securityCode, string securityRight)
		{
			using (Report report = GetNewReport())
			{
				FilterField.FilterContainer.LookupKey = new CheckpointLookupKey(securityCode);
				FilterField.GroupByStaff = true;

				using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
				{
					AssertRow(table.Rows[0], securityRight, "Bob", "Granted");
					AssertRow(table.Rows[1], securityRight, "Jane", "Granted");
					AssertRow(table.Rows[2], securityRight, "Tom", "Granted");
				}
			}
		}
	}
}
