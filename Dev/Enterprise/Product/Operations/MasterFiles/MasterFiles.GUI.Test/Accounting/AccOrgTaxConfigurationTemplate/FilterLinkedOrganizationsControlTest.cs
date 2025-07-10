using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(FilterLinkedOrganizationsControl))]
	public class FilterLinkedOrganizationsControlTest : BasherTest
	{
		[RequiresSTA]
		public void TestLinkedOrganizationsButtonGrid()
		{
			using (var form = GetForm())
			{
				var grid = form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid;

				var expectedListOfColumns = new[]
				{
					"OH_Code",
					"OH_FullName",
					"MainAddressCountryCodes",
					"MainAddress+OA_City",
					"MainAddress+OA_State",
					"OH_Category",
					"OH_RL_NKClosestPort",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Where(x => x.IsVisible)
					.Select(x => x.ColumnName)
					.ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestClearButtonClicked()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				PrepareOrgCodeFilter(form, org1);
				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();
				AssertEquals("Pre-condition", 1, collection.Count);
				AssertEquals(org1.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.ClickToolStripClearButton_ForTestOnly();

				AssertEquals("Should reset elements properly", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { org1.PK, org2.PK }, collection.Select(x => x.PK).ToArray());
				AssertEquals("Found record(s): 2 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		public void TestPerformSearch()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				AssertEquals("Pre-condition", 0, collection.Count);
				AssertEquals("Found record(s): 0 of 2", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();

				AssertEquals("Should load all elements properly", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { org1.PK, org2.PK }, collection.Select(x => x.PK).ToArray());
				AssertEquals("Found record(s): 2 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				PrepareOrgCodeFilter(form, org1);
				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();

				AssertEquals("Should filter elements properly", 1, collection.Count);
				AssertEquals(org1.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestPerformSearch_TooManyRecords()
		{
			PrepareTestData();
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				AssertEquals("Pre-condition", 0, collection.Count);
				AssertEquals("Found record(s): 0 of 2", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();

				AssertEquals("Should not show records due to too many result", 0, collection.Count);
				AssertEquals("Found 2 of 2 records. This is too many records to display", form.RecordsFoundText);
			}
		}

		[RequiresSTA]
		public void TestClearButtonClickedWithChanges()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();
				AssertEquals("Pre-condition", 2, collection.Count);
				AssertEquals("Found record(s): 2 of 2", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.SimulateDetach_ForTestOnly(org1);
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertEquals("Detached one record", 1, collection.Count);
				AssertEquals("Found record(s): 1 of 1\r\nModified: 1", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.ClickToolStripClearButton_ForTestOnly();

				AssertContains("Linked Organizations have been changed, Please Save the form before applying filter.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not clear any filter", 1, collection.Count);
				AssertEquals(org2.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 1\r\nModified: 1", form.RecordsFoundText);
			}
		}

		public void TestPerformSearchWithChanges()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				AssertEquals("Pre-condition", 0, collection.Count);
				AssertEquals("Found record(s): 0 of 2", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org3);
				AssertEquals("Attached one record", 1, collection.Count);
				AssertEquals("Found record(s): 1 of 3\r\nModified: 1", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();

				AssertContains("Linked Organizations have been changed, Please Save the form before applying filter.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not filter any element", 1, collection.Count);
				AssertEquals(org3.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 3\r\nModified: 1", form.RecordsFoundText);
			}
		}

		[RequiresSTA]
		public void TestAttachRows()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;

				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();
				AssertEquals("Pre-condition", 2, collection.Count);
				AssertEquals("Found record(s): 2 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org3);

				AssertEquals("Should clear filtered records and attach new record to Grid", 1, collection.Count);
				AssertEquals(org3.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 3\r\nModified: 1", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org4);

				AssertEquals("Should attach record to Grid", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { org3.PK, org4.PK }, collection.Select(x => x.PK).ToArray());
				AssertEquals("Found record(s): 2 of 4\r\nModified: 2", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestAttachTooManyElements()
		{
			PrepareTestData();
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				AssertEquals("Pre-condition", 0, collection.Count);
				AssertEquals("Found record(s): 0 of 2", form.RecordsFoundText);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org3);

				AssertEquals("Should attach record to Grid", 1, collection.Count);
				AssertEquals(org3.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 3\r\nModified: 1", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org4);
				UnitTestUserNotification.Instance.AddOKAnswer();

				AssertEquals("Please Save the form before attaching more organizations to this Template.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should NOT attach any new record", 1, collection.Count);
				AssertEquals(org3.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 3\r\nModified: 1", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.SimulateApply_ForTestOnly(Factory);
				AssertEquals(org3.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 3", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateAttach_ForTestOnly(template, org4);

				AssertEquals("Should clear filter result and attach new record to Grid", 1, collection.Count);
				AssertEquals(org4.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 4\r\nModified: 1", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestDetachRows()
		{
			PrepareTestData();

			using (var form = GetForm(template))
			{
				form.Show();
				var collection = template.LinkedOrganisations;
				form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.FirePerformSearch();
				AssertEquals("Pre-condition", 2, collection.Count);
				AssertEquals("Found record(s): 2 of 2", form.RecordsFoundText);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateDetach_ForTestOnly(org1);

				AssertEquals("Should remove target record from Grid", 1, collection.Count);
				AssertEquals(org2.PK, collection[0].PK);
				AssertEquals("Found record(s): 1 of 1\r\nModified: 1", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);

				form.FilterLinkedOrganizationsControl.SimulateDetach_ForTestOnly(org2);

				AssertEquals("Should remove target record from Grid", 0, collection.Count);
				AssertEquals("Found record(s): 0 of 0\r\nModified: 2", form.RecordsFoundText);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.FilterLinkedOrganizationsControl.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		#region Implementation

		void PrepareTestData()
		{
			org1 = TestObjectCreator.CreateOrgHeader("OH1", true, true);
			org2 = TestObjectCreator.CreateOrgHeader("OH2", true, true);
			org3 = TestObjectCreator.CreateOrgHeader("OH3", true, true);
			org4 = TestObjectCreator.CreateOrgHeader("OH4", true, true);
			Factory.Save();

			template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = false;
			Factory.Save();

			org1.CompanyData.OB_OCT_APTaxTemplate = template.PK;
			org2.CompanyData.OB_OCT_APTaxTemplate = template.PK;
			Factory.Save();
		}

		void PrepareOrgCodeFilter(DummyForm form, OrgHeader org)
		{
			var strip = form.FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.AddNewFilterStrip();
			strip.CurrentDataItem.FilterDescription = "Code";
			((ModuleTextFilter)strip.CurrentDataItem.CurrentModuleFilter).Property = org.OH_Code;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		public override Form GetFormToBash() => GetForm();

		DummyForm GetForm(AccOrgTaxConfigurationTemplate dataSource = null)
		{
			dataSource = dataSource ?? Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			return new DummyForm(dataSource) { CaptionRenderingEnabled = true };
		}

		class DummyForm : ZChildForm
		{
			public DummyForm(IBusiness businessEntity) : base(businessEntity)
			{
				FilterLinkedOrganizationsControl = new DummyLinkedOrganizations()
				{
					Dock = DockStyle.Fill
				};
				Controls.Add(FilterLinkedOrganizationsControl);
				BindingSource.SetBindingMember(FilterLinkedOrganizationsControl, ".");
			}

			public DummyLinkedOrganizations FilterLinkedOrganizationsControl { get; }

			public void SimulateApply_ForTestOnly(BusinessObjectFactory factory)
			{
				factory.Save();
				FilterLinkedOrganizationsControl.ResetStatus();
			}

			public string RecordsFoundText => FilterLinkedOrganizationsControl.FilterControl_ForTestOnly.GetToolStripRecordsFoundLabelText().Trim();
		}

		class DummyLinkedOrganizations : FilterLinkedOrganizationsControl
		{
			public LinkedOrganizationsControl Control_ForTestOnly => FilterPanel.Controls.Find("LinkedOrganizationsControl", false).FirstOrDefault() as LinkedOrganizationsControl;

			public ZModuleButtonGridForDesigner Grid_ForTestOnly => Control_ForTestOnly.Controls.Find("zModuleButtonGrid", false).FirstOrDefault() as ZModuleButtonGridForDesigner;

			public LinkedOrganizationsFilterControl FilterControl_ForTestOnly => FilterPanel.Controls.Find("LinkedOrganizationsFilterControl", false).FirstOrDefault() as LinkedOrganizationsFilterControl;

			public void ClickToolStripClearButton_ForTestOnly()
			{
				var button = typeof(LinkedOrganizationsFilterControl).GetField("ToolStripClearButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(FilterControl_ForTestOnly) as ToolStripButton;
				button.PerformClick();
			}

			public void SimulateDetach_ForTestOnly(params OrgHeader[] orgsToDetach)
			{
				foreach (var org in orgsToDetach)
				{
					Control_ForTestOnly.BoundedOrgCollection.Remove(org);
				}
				var handler = typeof(FilterLinkedOrganizationsControl).GetMethod("LinkedOrganizationsControl_OnDetached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				handler.Invoke(this, new object[] { this, new ModuleButtonGridOnDetachedEventArgs(orgsToDetach) });
			}

			public void SimulateAttach_ForTestOnly(AccOrgTaxConfigurationTemplate template, params OrgHeader[] orgsToAttach)
			{
				var handler1 = typeof(FilterLinkedOrganizationsControl).GetMethod("LinkedOrganizationsControl_BeforeAttach", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var handler2 = typeof(FilterLinkedOrganizationsControl).GetMethod("LinkedOrganizationsControl_OnAttaching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var handler3 = typeof(FilterLinkedOrganizationsControl).GetMethod("LinkedOrganizationsControl_OnAttached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var argsAttach = new object[] { this, new ModuleButtonGridOnAttachEventArgs(orgsToAttach) };
				var argsCancel = new object[] { this, new ModuleButtonGridOperationCancelEventArgs(orgsToAttach) };

				handler1.Invoke(this, argsAttach);
				handler2.Invoke(this, argsCancel);

				if (!((ModuleButtonGridOperationCancelEventArgs)argsCancel[1]).Cancel)
				{
					foreach (var org in orgsToAttach)
					{
						Control_ForTestOnly.BoundedOrgCollection.Add(org);
					}

					handler3.Invoke(this, argsAttach);
				}
			}

			Panel FilterPanel => Controls.Find("FilterPanel", false).FirstOrDefault() as Panel;
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		OrgHeader org1, org2, org3, org4;
		AccOrgTaxConfigurationTemplate template;

		#endregion
	}
}
