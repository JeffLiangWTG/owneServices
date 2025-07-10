using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	// This test is used to lock controls' location and size in order to
	// get rid of side effects caused by form designer's automatic incorrect fix.
	[TestedType(typeof(GlbStaffForm))]
	class GlbStaffFormSecurityRightsTabTest : ZFormBasherTest
	{
		public void TestEffSecRightsBoxLocationY_EqualToSecurityRightsHintLabelHeight()
		{
			ShowSecurityRightsTabPage(form =>
			{
				AssertEquals(form.SecurityRightsHintLabelForTest.Height, form.EffSecRightsBoxForTest.Location.Y);
			});
		}

		public void TestGlbSecurityBoundGrid_FillLeftSpaceOfHeight()
		{
			ShowSecurityRightsTabPage(form =>
			{
				var offset = form.GlbSecurityBoundGridForTest.Location.Y -
					(form.StaffRightsLabelForTest.Location.Y + form.StaffRightsLabelForTest.Height);
				AssertCloseEnough(
					expected: ControlDpiScalingHelper.ScaleToCurrentDpiY(4),
					actual: offset,
					allowedVariation: ControlDpiScalingHelper.ScaleToCurrentDpiY(2));

				var leftHeight = form.StaffRightsPanelForTest.Height - form.GlbSecurityBoundGridForTest.Location.Y;
				AssertCloseEnough(
					expected: ControlDpiScalingHelper.ScaleToCurrentDpiY(3),
					actual: leftHeight - form.GlbSecurityBoundGridForTest.Height,
					allowedVariation: ControlDpiScalingHelper.ScaleToCurrentDpiY(3));
			});
		}

		public void TestGroupRightsCollapsedPanel_FillLeftSpaceOfHeight()
		{
			ShowSecurityRightsTabPage(form =>
			{
				AssertEquals(form.StaffRightsPanelForTest.Height, form.GroupRightsCollapsedPanelForTest.Location.Y);
				AssertEquals(
					form.StdSecPanelForTest.Height - form.StaffRightsPanelForTest.Height,
					form.GroupRightsCollapsedPanelForTest.Height);
			});
		}

		public void TestGroupRightsPanel_FillLeftSpaceOfHeightWhenVisible()
		{
			ShowSecurityRightsTabPage(form =>
			{
				ShowShowGroupRightsPanel(form);

				AssertEquals(DockStyle.Bottom, form.SecurityGridsSplitterForTest.Dock);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(3), form.SecurityGridsSplitterForTest.Height);
				AssertEquals(form.StaffRightsPanelForTest.Height, form.SecurityGridsSplitterForTest.Location.Y);

				AssertEquals(DockStyle.Bottom, form.GroupRightsPanelForTest.Dock);
				AssertEquals(
					form.StdSecPanelForTest.Height - form.StaffRightsPanelForTest.Height - form.SecurityGridsSplitterForTest.Height,
					form.GroupRightsPanelForTest.Height);
				AssertGreaterThan(form.GroupRightsPanelForTest.Height, ControlDpiScalingHelper.ScaleToCurrentDpiY(115));
			});
		}

		public void TestGlbGroupSecurityGrid_FillLeftSpaceOfHeight()
		{
			ShowSecurityRightsTabPage(form =>
			{
				ShowShowGroupRightsPanel(form);

				AssertGreaterThan(
					form.GlbGroupSecurityGridForTest.Location.Y,
					form.DefaultGroupRights2LabelForTest.Location.Y + form.DefaultGroupRights2LabelForTest.Height);
				AssertGreaterThan(form.GlbGroupSecurityGridForTest.Height, ControlDpiScalingHelper.ScaleToCurrentDpiY(80));
			});
		}

		[RequiresSTA]
		public void TestAllowedOrgsAndWarehousesSecurityPanel_FillLeftSpaceOfHeight()
		{
			ShowSecurityRightsTabPage(form =>
			{
				ShowAllowedOrgsAndWarehousesSecurityPanel(form);

				AssertEquals(
					form.OrgsAndWarehousesSplitterForTest.Location.Y,
					form.StdSecPanelForTest.Location.Y + form.StdSecPanelForTest.Height);
				AssertEquals(
					form.AllowedOrgsAndWarehousesSecurityPanelForTest.Location.Y,
					form.StdSecPanelForTest.Location.Y + form.StdSecPanelForTest.Height + form.OrgsAndWarehousesSplitterForTest.Height);
				AssertCloseEnough(
					message: "Height of grid is neither too low or too high.",
					expected: ControlDpiScalingHelper.ScaleToCurrentDpiY(175),
					actual: form.AllowedOrgsAndWarehousesSecurityPanelForTest.Height,
					allowedVariation: 20);
			});
		}

		public void TestControlsAlignedInHorizontal()
		{
			ShowSecurityRightsTabPage(form =>
			{
				ShowShowGroupRightsPanel(form);
				ShowAllowedOrgsAndWarehousesSecurityPanel(form);

				var left = form.StdSecPanelForTest.Left;
				var right = form.StdSecPanelForTest.Right;
				AssertEquals(0, left);
				AssertEquals(left, form.OrgsAndWarehousesSplitterForTest.Left);
				AssertEquals(right, form.OrgsAndWarehousesSplitterForTest.Right);

				AssertEquals(left, form.AllowedOrgsAndWarehousesSecurityPanelForTest.Left);
				AssertEquals(right, form.AllowedOrgsAndWarehousesSecurityPanelForTest.Right);

				// -------------- Inside of StdSecPanel --------------
				AssertEquals(left, form.StaffRightsPanelForTest.Left);
				AssertEquals(right, form.StaffRightsPanelForTest.Right);

				AssertEquals(left, form.GroupRightsCollapsedPanelForTest.Left);
				AssertEquals(right, form.GroupRightsCollapsedPanelForTest.Right);

				AssertEquals(left, form.SecurityGridsSplitterForTest.Left);
				AssertEquals(right, form.SecurityGridsSplitterForTest.Right);

				AssertEquals(left, form.GroupRightsPanelForTest.Left);
				AssertEquals(right, form.GroupRightsPanelForTest.Right);

				// -------------- Inside of StaffRightsPanel --------------
				var contentRight = form.EffSecRightsBoxForTest.Right;
				AssertCloseEnough(
					message: "right padding: [0, 16]",
					expected: ControlDpiScalingHelper.ScaleToCurrentDpiX(8),
					actual: right - contentRight,
					allowedVariation: ControlDpiScalingHelper.ScaleToCurrentDpiX(8));

				AssertEquals(left, form.EffSecRightsBoxForTest.Left);

				AssertEquals(left, form.StaffRightsLabelForTest.Left);

				AssertEquals(left, form.GlbSecurityBoundGridForTest.Left);
				AssertEquals(contentRight, form.GlbSecurityBoundGridForTest.Right);

				// -------------- Inside of GroupRightsPanel --------------
				AssertEquals(left, form.DefaultGroupRights2LabelForTest.Left);

				AssertEquals(left, form.GlbGroupSecurityGridForTest.Left);
				AssertEquals(contentRight, form.GlbGroupSecurityGridForTest.Right);
			});
		}

		void ShowSecurityRightsTabPage(Action<GlbStaffFormForTesting> assertion)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (var form = new GlbStaffFormForTesting(staff))
				{
					form.Show();
					Application.DoEvents();

					form.GlbStaffTabControl.SelectedTab = form.SecurityRightsTabPageForTest;
					Application.DoEvents();

					assertion(form);
				}
			}
		}

		static void ShowAllowedOrgsAndWarehousesSecurityPanel(GlbStaffFormForTesting form)
		{
			var whsAllowedClientsNode = FindFirstTreeNode(
				form.SecurityPermissionsTreeViewForTest.Nodes,
				x => ((ZSecurityPointNode)x).Checkpoint.LookupKey == Env.Security.WhsAllowedClients.LookupKey);
			form.SecurityPermissionsTreeViewForTest.PerformSelect(whsAllowedClientsNode);
			Application.DoEvents();

			TreeNode FindFirstTreeNode(TreeNodeCollection nodes, Predicate<TreeNode> predicate)
			{
				foreach (TreeNode node in nodes)
				{
					if (predicate(node))
					{
						return node;
					}

					var childFound = FindFirstTreeNode(node.Nodes, predicate);
					if (childFound != null)
					{
						return childFound;
					}
				}

				return null;
			}
		}

		static void ShowShowGroupRightsPanel(GlbStaffFormForTesting form)
		{
			form.ShowGroupRightsButtonForTest.PerformClick();
			Application.DoEvents();
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert("Has been asserted in Enterprise.MasterFiles.GUI.GlbStaffForm.Test", true);
		}

		protected override Form GetFormToBashCore()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			return new GlbStaffFormForTesting(staff);
		}

		class GlbStaffFormForTesting : GlbStaffForm
		{
			public GlbStaffFormForTesting(GlbStaff staff) : base(staff)
			{
			}

			public ZSecurityTreeView SecurityPermissionsTreeViewForTest => FindControl<ZSecurityTreeView>("SecurityPermissionsTreeView");
			public ZTabPage SecurityRightsTabPageForTest => FindControl<ZTabPage>("SecurityRightsTabPage");
			public ZLabel SecurityRightsHintLabelForTest => FindControl<ZLabel>("SecurityRightsHintLabel");
			public KPanel StdSecPanelForTest => FindControl<KPanel>("StdSecPanel");
			public ZPanel StaffRightsPanelForTest => FindControl<ZPanel>("StaffRightsPanel");
			public ZGroupBox EffSecRightsBoxForTest => FindControl<ZGroupBox>("EffSecRightsBox");
			public ZLabel StaffRightsLabelForTest => FindControl<ZLabel>("StaffRightsLabel");
			public ZGrid GlbSecurityBoundGridForTest => FindControl<ZGrid>("GlbSecurityBoundGrid");
			public ZPanel GroupRightsCollapsedPanelForTest => FindControl<ZPanel>("GroupRightsCollapsedPanel");
			public ZButton ShowGroupRightsButtonForTest => FindControl<ZButton>("ShowGroupRightsButton");
			public KSplitter SecurityGridsSplitterForTest => FindControl<KSplitter>("SecurityGridsSplitter");
			public ZPanel GroupRightsPanelForTest => FindControl<ZPanel>("GroupRightsPanel");
			public ZLabel DefaultGroupRights2LabelForTest => FindControl<ZLabel>("DefaultGroupRights2Label");
			public ZGrid GlbGroupSecurityGridForTest => FindControl<ZGrid>("GlbGroupSecurityGrid");
			public KSplitter OrgsAndWarehousesSplitterForTest => FindControl<KSplitter>("OrgsAndWarehousesSplitter");
			public AllowedOrgsAndWarehousesControl AllowedOrgsAndWarehousesSecurityPanelForTest => FindControl<AllowedOrgsAndWarehousesControl>("AllowedOrgsAndWarehousesSecurityPanel");

			T FindControl<T>(string name) where T : Control
			{
				var controls = Controls.Find(name, true).OfType<T>().ToArray();
				AssertNotEquals($"Specified control [{name}]|[{typeof(T).Name}] should be found", 0, controls.Length);
				return controls[0];
			}
		}
	}
}
