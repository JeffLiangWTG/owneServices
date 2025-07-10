using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(LinkedOrganizationsControl))]
	public class LinkedOrganizationsControlTest : BasherTest
	{
		public void TestLinkedOrganizationsButtonGrid()
		{
			using (var form = GetForm())
			{
				form.Show();
				var grid = form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid;

				AssertNotNull("Check DataSource is bounded successfully.", form.LinkedOrgControl_ForTestOnly.BoundedOrgCollection);

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

		public void TestEventBindings()
		{
			int eventResult = 0;

			using (var control = new DummyLinkedOrganizations())
			{
				var target = control.Grid_ForTestOnly;

				AssertEventHandler("Pre-condition", target, "Attached");
				AssertEventHandler("Pre-condition", target, "BeforeAttached");
				InvokeEvent(target, "OnAttaching", new ModuleButtonGridOperationCancelEventArgs(null));
				AssertEquals(0, eventResult);
				InvokeEvent(target, "OnDetached", new ModuleButtonGridOnDetachedEventArgs(null));
				AssertEquals(0, eventResult);

				control.OnAttached += OnAttachedEventForTest;
				control.BeforeAttach += BeforeAttachEventForTest;
				control.OnAttaching += OnAttachingEventForTest;
				control.OnDetached += OnDetachedEventForTest;

				AssertEventHandler("Should match event", target, "Attached", "OnAttachedEventForTest");
				AssertEventHandler("Should match event", target, "BeforeAttached", "BeforeAttachEventForTest");
				InvokeEvent(target, "OnAttaching", new ModuleButtonGridOperationCancelEventArgs(null));
				AssertEquals(3, eventResult);
				InvokeEvent(target, "OnDetached", new ModuleButtonGridOnDetachedEventArgs(null));
				AssertEquals(4, eventResult);
			}

			void OnAttachedEventForTest(object sender, ModuleButtonGridOnAttachEventArgs e)
			{
			}

			void BeforeAttachEventForTest(object sender, ModuleButtonGridOnAttachEventArgs e)
			{
			}

			void OnAttachingEventForTest(object sender, ModuleButtonGridOperationCancelEventArgs e)
			{
				eventResult = 3;
			}

			void OnDetachedEventForTest(object sender, ModuleButtonGridOnDetachedEventArgs e)
			{
				eventResult = 4;
			}
		}

		public void TestBoundedOrgCollection()
		{
			using (var form = GetForm())
			{
				form.Show();
				AssertNotNull(form.LinkedOrgControl_ForTestOnly.BoundedOrgCollection);
				AssertEquals(form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid.List, form.LinkedOrgControl_ForTestOnly.BoundedOrgCollection);
			}
		}

		[RequiresSTA]
		public void TestConfigureExportColumnsToExcelMenuItems()
		{
			using (var form = GetForm())
			{
				form.Show();
				form.LinkedOrgControl_ForTestOnly.ConfigureExportColumnsToExcelMenuItems(false);
				AssertEquals(false, form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(false, form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
				form.LinkedOrgControl_ForTestOnly.ConfigureExportColumnsToExcelMenuItems(true);
				AssertEquals(true, form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				AssertEquals(true, form.LinkedOrgControl_ForTestOnly.Grid_ForTestOnly.InnerGrid.ExportVisibleColumnsToExcelMenuItem.Enabled);
			}
		}

		#region Implementation

		void AssertEventHandler(string message, object target, string eventName, string targetEventHandlerName = null)
		{
			var fieldInfo = target.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
			var eventDelegate = fieldInfo.GetValue(target) as MulticastDelegate;

			if (targetEventHandlerName == null)
			{
				AssertEquals(message, null, eventDelegate);
			}
			else
			{
				var invocationList = eventDelegate.GetInvocationList();

				AssertEquals(message, 1, invocationList.Length);
				AssertContains(message, targetEventHandlerName, invocationList[0].Method.Name);
			}
		}

		void InvokeEvent(object target, string invokeMethodName, object eventArgs)
		{
			var methodInfo = target.GetType().GetMethod(invokeMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(target, new object[] { eventArgs });
		}

		public override Form GetFormToBash() => GetForm();

		protected override bool AllowHasChangesOnFormOpen => true;

		DummyForm GetForm()
		{
			return new DummyForm(Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>()) { CaptionRenderingEnabled = true };
		}

		class DummyForm : ZChildForm
		{
			public DummyForm(IBusiness businessEntity) : base(businessEntity)
			{
				LinkedOrgControl_ForTestOnly = new DummyLinkedOrganizations()
				{
					Dock = DockStyle.Fill
				};
				Controls.Add(LinkedOrgControl_ForTestOnly);
				BindingSource.SetBindingMember(LinkedOrgControl_ForTestOnly, ".");
			}

			public DummyLinkedOrganizations LinkedOrgControl_ForTestOnly { get; }
		}

		class DummyLinkedOrganizations : LinkedOrganizationsControl
		{
			public ZModuleButtonGridForDesigner Grid_ForTestOnly => Controls.Find("zModuleButtonGrid", false).FirstOrDefault() as ZModuleButtonGridForDesigner;
		}

		#endregion
	}
}
