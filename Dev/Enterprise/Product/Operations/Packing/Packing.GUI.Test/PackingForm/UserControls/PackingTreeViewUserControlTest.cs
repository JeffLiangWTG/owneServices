using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	public class PackingTreeViewUserControlTest : PackingTestCaseWithFactory
	{
		#region TestViewJobClick_OpensEditForm

		[RequiresSTA]
		public void TestViewJobClick_OpensEditForm()
		{
			Form.Show();
			Data.PackageJob.Packages.AddNew(); // empty packageJobs are deleted on save
			Factory.Save(); // Dummy must exist in DB for the edit form to open it

			var dummy = (IPackingParent)Data.Dummy;
			TreeUserControl.ViewJobMenuItem.Available = true;
			TreeUserControl.ViewJobMenuItem.PerformClickEnableFirst();

			using (var dummyForm = (ZDummyForm)TreeUserControl.ControllerForTest.LastShownForm)
			{
				AssertEquals(ODisplayMode.Browse, dummyForm.DisplayMode);
			}

			Form.Controls.Remove(TreeUserControl);
			TreeUserControl.ViewMode = PackingViewMode.NoEditing;
		}

		#endregion

		#region TestGenerateIDs

		#region TestGenerateIDs_CreatesSSCC

		public void TestGenerateIDs_CreatesSSCC()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Form.Show();
			var package = Data.PackageJob.Packages.AddNew();
			var dummy = Data.Dummy;
			dummy.SSCCPrefix = "1111111";
			dummy.JobNoForPackingParent = "D0000010011";
			Factory.Save();
			AssertEquals("Precondition", null, dummy.LastSSCCGenerationContext);

			TreeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
			AssertEquals("011111110000000014", package.KP_PackageID);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestGenerateIDs_AutoSaves

		[RequiresSTA]
		public void TestGenerateIDs_AutoSaves()
		{
			TestGenerateIDs_AutoSaves(ssccPrefix: ZString.Empty, expectedPackageID: "D0000010011-001");
		}

		public void TestGenerateIDs_AutoSavesWhenSSCC()
		{
			TestGenerateIDs_AutoSaves(ssccPrefix: "1111111", expectedPackageID: "011111110000000014");
		}

		[RequiresSTA]
		void TestGenerateIDs_AutoSaves(string ssccPrefix, string expectedPackageID)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Form.Show();
			var package = Data.PackageJob.Packages.AddNew();
			var dummy = Data.Dummy;
			dummy.SSCCPrefix = ssccPrefix;
			dummy.JobNoForPackingParent = "D0000010011";
			AssertEquals("Precondition", true, dummy.HasChanges);
			AssertEquals("Precondition", null, dummy.LastSSCCGenerationContext);

			TreeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
			AssertEquals(false, dummy.HasChanges);
			AssertEquals(expectedPackageID, package.KP_PackageID);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestGenerateIDs_DoesNotAutoSaveOnValidationError

		public void TestGenerateIDs_DoesNotAutoSaveOnValidationError()
		{
			TestGenerateIDs_DoesNotAutoSaveOnValidationError(ssccPrefix: ZString.Empty);
		}

		public void TestGenerateIDs_DoesNotAutoSaveOnValidationError_WithSSCC()
		{
			TestGenerateIDs_DoesNotAutoSaveOnValidationError(ssccPrefix: "1111111");
		}

		void TestGenerateIDs_DoesNotAutoSaveOnValidationError(string ssccPrefix)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Form.Show();
			var package1 = Data.PackageJob.Packages.AddNew();
			var package2 = Data.PackageJob.Packages.AddNew();
			var package3 = Data.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "111";
			package2.KP_PackageID = "111";

			var dummy = Data.Dummy;
			dummy.SSCCPrefix = ssccPrefix;
			dummy.JobNoForPackingParent = "D0000010011";
			AssertEquals("Precondition", true, dummy.HasChanges);
			AssertEquals("Precondition", true, package2.HasErrors);

			TreeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
			AssertEquals("Should not have saved.", true, dummy.HasChanges);
			AssertEquals("", package3.KP_PackageID);
			AssertEquals("Fix any Errors then Save before attempting to Generate IDs.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals(null, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestGenerateIDs_ShowsErrorWhenAutoSaveFails

		public void TestGenerateIDs_ShowsErrorWhenAutoSaveFails()
		{
			TestGenerateIDs_ShowsErrorWhenAutoSaveFails(ssccPrefix: ZString.Empty);
		}

		public void TestGenerateIDs_ShowsErrorWhenAutoSaveFails_WithSSCC()
		{
			TestGenerateIDs_ShowsErrorWhenAutoSaveFails(ssccPrefix: "1111111");
		}

		void TestGenerateIDs_ShowsErrorWhenAutoSaveFails(string ssccPrefix)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Form.Show();
			var package = Data.PackageJob.Packages.AddNew();
			var dummy = Data.Dummy;
			dummy.SSCCPrefix = ssccPrefix;
			dummy.JobNoForPackingParent = "D0000010011";
			AssertEquals("Precondition", true, dummy.HasChanges);

			// create a dodgy package to break the save
			Factory.New<PkgPackage>().KP_KJ_ParentPackageJob = Guid.NewGuid();

			TreeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
			AssertEquals(true, dummy.HasChanges);
			AssertEquals("", package.KP_PackageID);
			AssertEquals("Fix any Errors then Save before attempting to Generate IDs.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(null, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#endregion

		#region TestDocumentsMenuItem

		#region TestDocumentsMenu_DoesNotThrowExceptionWhenPackageJobIsNull()

		public void TestDocumentsMenu_DoesNotThrowExceptionWhenPackageJobIsNull()
		{
			try
			{
				Form.Show();
				TreeUserControl.MakeDataSourceNull();
				TreeUserControl.PackingMenuStrip.Select();
				AssertNoExceptionThrown("Should not throw null reference exception", TreeUserControl.PackingMenuStrip.Show);
				AssertNull("TreeUserControl.DocumentsMenuItem", TreeUserControl.DocumentsMenuItem);
				TreeUserControl.PackingMenuStrip.Close();
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestDocumentsMenu_GetsRebuiltWhenDataSourceChanges

		public void TestDocumentsMenu_GetsRebuiltWhenDataSourceChanges()
		{
			try
			{
				Form.Show();

				var package = Data.PackageJob.Packages.AddNew();
				Factory.Save();

				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				var oldDocumentsMenu = TreeUserControl.DocumentsMenuItem;
				AssertNotNull("TreeUserControl.DocumentsMenuItem", oldDocumentsMenu);
				AssertEquals(1, TreeUserControl.PackingMenuStrip.Items.Find("Documents", false).Length);
				TreeUserControl.PackingMenuStrip.Close();

				var newPackageJob = Factory.NewWithValidTestData<PkgPackageJob>();
				newPackageJob.Packages.AddNew();
				Factory.Save();
				TreeUserControl.ChangeDataSource(newPackageJob);
				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				AssertNotNull("TreeUserControl.DocumentsMenuItem", TreeUserControl.DocumentsMenuItem);
				AssertNotEquals("Documents menu is rebuilt when there is a different Package Job", oldDocumentsMenu, TreeUserControl.DocumentsMenuItem);
				AssertEquals(1, TreeUserControl.PackingMenuStrip.Items.Find("Documents", false).Length);
				TreeUserControl.PackingMenuStrip.Close();
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestDocumentsMenu_DoesNotAppearWhenPackableItemIsSelected

		public void TestDocumentsMenu_DoesNotAppearWhenPackableItemIsSelected()
		{
			try
			{
				Form.Show();

				var package = Data.PackageJob.Packages.AddNew();
				var divot = package.PackedItemDivots.AddNew();
				divot.KI_PackedQty = 1m;
				divot.KI_ParentTableCode = "ZD1";
				Factory.Save();

				using (var node = new PackingTreeNode(PkgPackageItemDivotsWrapper.New(divot, Data.DummyLine1)))
				{
					TreeUserControl.Tree.NodeSelector.SelectNode(node);
					TreeUserControl.PackingMenuStrip.Select();
					TreeUserControl.PackingMenuStrip.Show();
					AssertNull("Documents menu does not appear when packable item is selected", TreeUserControl.DocumentsMenuItem);
					AssertEquals(0, TreeUserControl.PackingMenuStrip.Items.Find("Documents", false).Length);
					TreeUserControl.PackingMenuStrip.Close();

					TreeUserControl.Tree.NodeSelector.DeselectNode(node);
					TreeUserControl.PackingMenuStrip.Select();
					TreeUserControl.PackingMenuStrip.Show();
					AssertNotNull("Documents menu will appear if packable item is not selected", TreeUserControl.DocumentsMenuItem);
					AssertEquals(1, TreeUserControl.PackingMenuStrip.Items.Find("Documents", false).Length);
					TreeUserControl.PackingMenuStrip.Close();

					TreeUserControl.Tree.NodeSelector.SelectNode(node);
					TreeUserControl.PackingMenuStrip.Select();
					TreeUserControl.PackingMenuStrip.Show();
					AssertNull("Documents Menu is removed upon selecting packable item again", TreeUserControl.DocumentsMenuItem);
					AssertEquals(0, TreeUserControl.PackingMenuStrip.Items.Find("Documents", false).Length);
					TreeUserControl.PackingMenuStrip.Close();
				}
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestDocumentsMenu_ChangesWhenPackageIsSelected

		public void TestDocumentsMenu_ChangesWhenPackageIsSelected()
		{
			try
			{
				Form.Show();

				var package = Data.PackageJob.Packages.AddNew();
				var divot = package.PackedItemDivots.AddNew();
				divot.KI_PackedQty = 1m;
				divot.KI_ParentTableCode = "ZD1";
				Factory.Save();

				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				var packingJobDocumentsMenu = TreeUserControl.DocumentsMenuItem;
				AssertNotNull("TreeUserControl.DocumentsMenuItem", packingJobDocumentsMenu);

				packingJobDocumentsMenu.ShowDropDown();
				var packingJobDocumentsMenuText = GetMenuItemsAsText(packingJobDocumentsMenu);
				TreeUserControl.PackingMenuStrip.Close();

				using (var node = new PackingTreeNode(package))
				{
					TreeUserControl.Tree.NodeSelector.SelectNode(node);
					TreeUserControl.PackingMenuStrip.Select();
					TreeUserControl.PackingMenuStrip.Show();

					var packageDocumentsMenu = TreeUserControl.DocumentsMenuItem;
					AssertNotNull("TreeUserControl.DocumentsMenuItem", packageDocumentsMenu);

					packageDocumentsMenu.ShowDropDown();
					AssertNotEquals("Ensure packingJobMenu is not the same as packageMenu", packingJobDocumentsMenuText, GetMenuItemsAsText(packageDocumentsMenu));
					TreeUserControl.PackingMenuStrip.Close();

					TreeUserControl.Tree.NodeSelector.DeselectNode(node);
					TreeUserControl.PackingMenuStrip.Select();
					TreeUserControl.PackingMenuStrip.Show();
					var newPackingJobMenu = TreeUserControl.DocumentsMenuItem;
					AssertNotNull("Package Job Documents menu should appear", newPackingJobMenu);

					newPackingJobMenu.ShowDropDown();
					AssertEquals("Ensure packingJobMenu is the same as previous packingJobMenu", packingJobDocumentsMenuText, GetMenuItemsAsText(newPackingJobMenu));
					TreeUserControl.PackingMenuStrip.Close();
				}
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestDocumentsMenu_DoesNotThrowExceptionWhenMultiplePackagesAreSelected

		public void TestDocumentsMenu_DoesNotThrowExceptionWhenMultiplePackagesAreSelected()
		{
			try
			{
				Form.Show();

				var packageJob = Data.PackageJob;
				packageJob.Selected.UpdateSelectedPackages(new[] { packageJob.Packages.AddNew(), packageJob.Packages.AddNew() });
				Factory.Save();

				TreeUserControl.PackingMenuStrip.Select();
				AssertNoExceptionThrown(TreeUserControl.PackingMenuStrip.Show);
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		ZString GetMenuItemsAsText(ToolStripMenuItem menuItem)
		{
			return new ZStringBuilder(menuItem.DropDownItems.Cast<ToolStripItem>().Select(d => d.Text)).ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region TestPackageDetailsControl

		#region TestContainerDetailsTab_VisibilityChangesWhenSelectPackageNodeBetweenContainerTypeAndNonContainerType

		public void TestContainerDetailsTab_VisibilityChangesWhenSelectPackageNodeBetweenContainerTypeAndNonContainerType()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packageContainerType = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var packageNonContainerType = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Pallet);
			Factory.Save();

			Form.Show(); // binds

			var node1 = TreeUserControl.Tree.FindNodeByPackage(packageContainerType);
			TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
			TreeUserControl.Tree.NodeSelector.SelectNode(node1);

			AssertContainerTabs(showContainerTabs: true, failMsg: "Should show");
			AssertNonContainerTabs(showNonContainerTabs: false, failMsg: "Should NOT show");

			var node2 = TreeUserControl.Tree.FindNodeByPackage(packageNonContainerType);
			TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
			TreeUserControl.Tree.NodeSelector.SelectNode(node2);

			AssertContainerTabs(showContainerTabs: false, failMsg: "Should NOT show");
			AssertNonContainerTabs(showNonContainerTabs: true, failMsg: "Should show");
		}

		#endregion

		#region TestContainerDetailsTab_AppearsWhenSelectedNonContainerPakcageChangeToContainerPackageType

		public void TestContainerDetailsTab_AppearsWhenSelectedNonContainerPakcageChangeToContainerPackageType()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Pallet);
			Factory.Save();

			Form.Show(); // binds

			var node = TreeUserControl.Tree.FindNodeByPackage(package);
			TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
			TreeUserControl.Tree.NodeSelector.SelectNode(node);

			AssertContainerTabs(showContainerTabs: false, failMsg: "Should NOT show");
			AssertNonContainerTabs(showNonContainerTabs: true, failMsg: "Should show");

			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			AssertContainerTabs(showContainerTabs: true, failMsg: "Should show");
			AssertNonContainerTabs(showNonContainerTabs: false, failMsg: "Should NOT show");
		}

		#endregion

		#region TestContainerDetailsTab_DisappearsWhenSelectedContainerPackageChangeToNonContainerPackageType

		public void TestContainerDetailsTab_DisappearsWhenSelectedContainerPackageChangeToNonContainerPackageType()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			Factory.Save();

			Form.Show(); // binds

			var node = TreeUserControl.Tree.FindNodeByPackage(package);
			TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
			TreeUserControl.Tree.NodeSelector.SelectNode(node);
			AssertContainerTabs(showContainerTabs: true, failMsg: "Should show");
			AssertNonContainerTabs(showNonContainerTabs: false, failMsg: "Should NOT show");

			UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
			userNotify.ClearMessagesAndAnswers();
			userNotify.AddAnswer(DialogResult.Yes); // will ask 'do you want to continue to change to container type, because you have container data and you will lose it
			package.KP_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertContainerTabs(showContainerTabs: false, failMsg: "Should NOT show");
			AssertNonContainerTabs(showNonContainerTabs: true, failMsg: "Should show");
		}

		#endregion

		#region TestContainerDetailsTab_SplitterIsNotFixed

		[RequiresSTA]
		public void TestContainerDetailsTab_SplitterIsNotFixed()
		{
			AssertEquals("The splitter between the Tree and the Packing Detail Controls should not be fixed", false, TreeUserControl.TreeSplitContainer.IsSplitterFixed);
		}

		#endregion

		#region TestContainerDetailsTab_NoExceptionWhenDisposing

		[ExpectNoExceptions]
		public void TestContainerDetailsTab_NoExceptionWhenDisposing()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var container = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var pallet = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Pallet);
			Factory.Save();

			using (var form = Form)
			{
				form.Show(); // binds

				var palletNode = TreeUserControl.Tree.FindNodeByPackage(pallet);
				var containerNode = TreeUserControl.Tree.FindNodeByPackage(container);
				var packageDetailControl = TreeUserControl.PackageDetailControl;

				TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
				TreeUserControl.Tree.NodeSelector.SelectNode(palletNode);
				packageDetailControl.DetailsTabControl.SelectTab(packageDetailControl.PackageTemperaturesTabPage);

				TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
				TreeUserControl.Tree.NodeSelector.SelectNode(containerNode);
				packageDetailControl.DetailsTabControl.SelectTab(packageDetailControl.ContainerTabPage);
			}
		}

		#endregion

		#region AssertTabs

		void AssertContainerTabs(bool showContainerTabs, ZString failMsg)
		{
			AssertEquals("Container Details: " + failMsg, showContainerTabs, TreeUserControl.PackageDetailControl.DetailsTabControl.TabPages.Contains(TreeUserControl.PackageDetailControl.ContainerTabPage));
			AssertEquals("Seals: " + failMsg, showContainerTabs, TreeUserControl.PackageDetailControl.DetailsTabControl.TabPages.Contains(TreeUserControl.PackageDetailControl.ContainerTemperaturesTabPage));
		}

		void AssertNonContainerTabs(bool showNonContainerTabs, ZString failMsg)
		{
			AssertEquals("Loose Package Temperature:" + failMsg, showNonContainerTabs, TreeUserControl.PackageDetailControl.DetailsTabControl.TabPages.Contains(TreeUserControl.PackageDetailControl.PackageTemperaturesTabPage));
		}

		#endregion

		#endregion

		#region TestBind

		public void TestBind_FocusOnTree_WhenOriginallyFocussed()
		{
			Data.CreatePackingData();

			using (var form = new ZForm())
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			using (var otherControl = new PackageDetailUserControl())
			{
				form.Controls.Add(treeUserControl);
				form.Controls.Add(otherControl);
				form.Show();

				otherControl.Focus();
				AssertEquals("Pre-condition:", false, treeUserControl.ContainsFocus);
				AssertEquals("Pre-condition:", true, otherControl.ContainsFocus);

				treeUserControl.Bind();

				AssertEquals("Should not have received focus.", false, treeUserControl.ContainsFocus);
				AssertEquals("Should maintain focus.", true, otherControl.ContainsFocus);
			}
		}

		#endregion

		#region TestHoldMenuItem_DisplayName

		public void TestHoldMenuItem_DisplayName()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packagePLT1 = packageJob.Packages.AddNew("PLT", "PID01");
			var packagePLT2 = packageJob.Packages.AddNew("PLT", "PID02");

			AssertEquals("Pre-condition:", 2, packageJob.Packages.Count);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT1 = treeUserControl.Tree.FindNodeByPackage(packagePLT1);
				var nodeForPackagePLT2 = treeUserControl.Tree.FindNodeByPackage(packagePLT2);
				var allNodes = new[] { nodeForPackagePLT1, nodeForPackagePLT2 };

				var holdMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("HoldPackageMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNodes(allNodes, 2);
				contextMenu.Show();
				AssertEquals("This should be the text when several packages are selected.", "Select a Single Package to Hold", holdMenuItem.Text);
				Assert("This should be disabled because several packages are selected.", !holdMenuItem.Enabled);

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT1);
				contextMenu.Show();
				AssertEquals("This should be the text when the package is not held.", "Hold", holdMenuItem.Text);
				Assert("This should be enabled because several packages are selected.", holdMenuItem.Enabled);

				holdMenuItem.PerformClick();
				contextMenu.Show();
				AssertEquals("This should be the text when the package is not held.", "Remove Hold", holdMenuItem.Text);
				Assert("This should be enabled because several packages are selected.", holdMenuItem.Enabled);

				holdMenuItem.PerformClick();
				contextMenu.Show();
				AssertEquals("This should be the text when the package is not held.", "Hold", holdMenuItem.Text);
				Assert("This should be enabled because several packages are selected.", holdMenuItem.Enabled);
			}
		}

		#endregion

		#region TestExpandMenuItemClick

		public void TestExpandMenuItemClick()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var parentPackage1 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var childPackageInParentPackage1 = Helper.CreatePackage(parentPackage1, 100, Constants.PkgUnit.Bag);

			var parentPackage2 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var childPackage1InParentPackage2 = Helper.CreatePackage(parentPackage2, 500, Constants.PkgUnit.Bottle);
			var childPackage2InParentPackage2 = Helper.CreatePackage(parentPackage2, 50, Constants.PkgUnit.Box);

			Factory.Save();

			Form.Show();

			try
			{
				TreeUserControl.Tree.CollapseAll();
				TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForParentPackage1 = TreeUserControl.Tree.FindNodeByPackage(parentPackage1);
				TreeUserControl.Tree.NodeSelector.SelectNode(nodeForParentPackage1);

				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				AssertEquals("Since all selected packages are collapsed, Collapse menu item is disabled.", false, TreeUserControl.CollapseMenuItem.Enabled);
				Assert(TreeUserControl.ExpandMenuItem.Enabled);

				TreeUserControl.ExpandMenuItem.PerformClick();
				Assert("Since ParentPackage1 node is selected, it should be expanded.", nodeForParentPackage1.IsExpanded);
				AssertEquals("Since ParentPackage2 node is not selected, it should remain collapsed.", false, TreeUserControl.Tree.FindNodeByPackage(parentPackage2).IsExpanded);

				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				Assert(TreeUserControl.CollapseMenuItem.Enabled);
				AssertEquals(false, TreeUserControl.ExpandMenuItem.Enabled);
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestCollpaseMenuItemClick

		public void TestCollpaseMenuItemClick()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var parentPackage1 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var childPackageInParentPackage1 = Helper.CreatePackage(parentPackage1, 100, Constants.PkgUnit.Bag);

			var parentPackage2 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var childPackage1InParentPackage2 = Helper.CreatePackage(parentPackage2, 500, Constants.PkgUnit.Bottle);
			var childPackage2InParentPackage2 = Helper.CreatePackage(parentPackage2, 50, Constants.PkgUnit.Box);
			Factory.Save();

			Form.Show();

			TreeUserControl.Tree.ExpandAll();
			TreeUserControl.Tree.NodeSelector.DeselectAllNodes();
			var nodeForParentPackage1 = TreeUserControl.Tree.FindNodeByPackage(parentPackage1);
			TreeUserControl.Tree.NodeSelector.SelectNode(nodeForParentPackage1);

			try
			{
				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				Assert(TreeUserControl.CollapseMenuItem.Enabled);
				AssertEquals(false, TreeUserControl.ExpandMenuItem.Enabled);

				TreeUserControl.CollapseMenuItem.PerformClick();
				AssertEquals("Since ParentPackage1 node is selected, it should be collapsed.", false, nodeForParentPackage1.IsExpanded);
				Assert("Since ParentPackage2 node is not selected, it should remain expanded.", TreeUserControl.Tree.FindNodeByPackage(parentPackage2).IsExpanded);

				TreeUserControl.PackingMenuStrip.Select();
				TreeUserControl.PackingMenuStrip.Show();
				AssertEquals(false, TreeUserControl.CollapseMenuItem.Enabled);
				Assert(TreeUserControl.ExpandMenuItem.Enabled);
			}
			finally
			{
				TreeUserControl.PackingMenuStrip.Dispose();
			}
		}

		#endregion

		#region TestAssignButton_Click

		[RequiresSTA]
		public void TestAssignButton_Click()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var dummyParent = Data.Dummy.IsLoosePackageIDsSupported = true;

			var packagePLT = packageJob.Packages.AddNew("PLT", 5);
			var packageID_1 = Helper.CreatePackageHeader(packageJob, "abc");
			Factory.Save();
			AssertEquals("Pre-condition: PackageQty", 5, packagePLT.KP_PackageQty);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();
				AssertEquals(true, !treeUserControl.TreeGridSplitContainer.Panel2Collapsed);
				AssertEquals(1 + 1, treeUserControl.PackageIDsGrid.VisibleRowCount);

				treeUserControl.PackageIDsGrid.SelectAllElements();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);
				AssertEquals(true, nodeForPackagePLT.IsSelected);

				treeUserControl.AssignButton.PerformClick();

				AssertEquals(2, packageJob.Packages.Count);
				AssertEquals(4, packagePLT.KP_PackageQty);
				AssertEquals("", packagePLT.KP_PackageID);
				var packageWithID = Data.PackageJob.Packages.Single(p => p.KP_PackageID == "abc");
				AssertEquals(1, packageWithID.KP_PackageQty);
			}
		}

		#endregion

		#region TestUnassignButton_Click

		public void TestUnassignButton_Click()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var dummyParent = Data.Dummy.IsLoosePackageIDsSupported = true;

			var packagePLT = packageJob.Packages.AddNew("PLT", 1);
			var packageID = Helper.CreatePackageHeader(packageJob, "123");
			packagePLT.KP_KPH_PackageHeader = packageID.PK;
			Factory.Save();
			AssertEquals("Pre-condition:", 1, packageJob.Packages.Count);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT_1 = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT_1);

				AssertEquals("Pre-condition: package id: '123' has been assigned.", true, packageJob.IsPackageIDAlreadyAssigned(packageID));

				treeUserControl.UnassignButton.PerformClick();

				AssertNull(packagePLT.GetPackageHeader());
				AssertEquals("package id: '123' is now not assigned.", false, packageJob.IsPackageIDAlreadyAssigned(packageID));
			}
		}

		#endregion

		#region TestHoldPackageMenuItem_Click

		public void TestHoldPackageMenuItem_Click()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");

			AssertEquals("Pre-condition:", 1, packageJob.Packages.Count);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				AssertEquals("Pre-condition: package is not hold.", false, packagePLT.KP_IsHeld);
				var holdMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("HoldPackageMenuItem", false).First();
				holdMenuItem.PerformClick();
				AssertEquals("Package should be held now.", true, packagePLT.KP_IsHeld);
				holdMenuItem.PerformClick();
				AssertEquals("Package should be without hold now.", false, packagePLT.KP_IsHeld);
			}
		}

		#endregion

		#region TestRemoveHoldAllPackagesMenuItem_Click

		public void TestRemoveHoldAllPackagesMenuItem_Click()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;

			var packagePLT1 = packageJob.Packages.AddNew("PLT", "PID01");
			var packagePLT2 = packageJob.Packages.AddNew("PLT", "PID02");
			packagePLT1.KP_IsHeld = true;
			packagePLT2.KP_IsHeld = true;

			AssertEquals("Pre-condition:", 2, packageJob.Packages.Count);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(treeUserControl.Tree.TopNode);
				var removeHoldMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("RemoveHoldAllPackagesMenuItem", false).First();

				AssertEquals("Pre-condition: all packages are held", true, packagePLT1.KP_IsHeld);
				AssertEquals("Pre-condition: all packages are held", true, packagePLT2.KP_IsHeld);
				removeHoldMenuItem.PerformClick();
				AssertEquals("Pre-condition: all packages are not held", false, packagePLT1.KP_IsHeld);
				AssertEquals("Pre-condition: all packages are not held", false, packagePLT2.KP_IsHeld);
			}
		}

		#endregion

		#region TestRemoveHoldAllPackagesMenuItemVisibility

		public void TestRemoveHoldAllPackagesMenuItemVisibility()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;

			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.KP_IsHeld = true;

			AssertEquals("Pre-condition:", 1, packageJob.Packages.Count);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(treeUserControl.Tree.TopNode);
				var removeHoldMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("RemoveHoldAllPackagesMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				Assert("The menu item should be visible if it's a Package Job.", removeHoldMenuItem.Visible);

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);
				contextMenu.Show();
				Assert("The menu item should not be visible if it's not a Package Job.", !removeHoldMenuItem.Visible);
			}
		}

		#endregion

		#region TestViewJobMenuItemVisibility

		public void TestViewJobMenuItemVisibility()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(treeUserControl.Tree.TopNode);
				var viewJobMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("ViewJobMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();

				AssertEquals("The menu item should not be visible if it's a Package Job for handling unit.", false, viewJobMenuItem.Visible);
			}
		}

		#endregion

		#region TestPackageIDsGrid

		public void TestPackageIDsGrid_Visibility()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var dummyParent = Data.Dummy;
			dummyParent.IsLoosePackageIDsSupported = true;
			AssertLooseIDsGridVisibility(packageJob, true);

			dummyParent.IsLoosePackageIDsSupported = false;
			AssertLooseIDsGridVisibility(packageJob, false);

			dummyParent.IsLoosePackageIDsSupported = true;
			AssertLooseIDsGridVisibility(packageJob, true);

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			AssertLooseIDsGridVisibility(packageJob, false);
		}

		public void TestPackageIDsGrid_Binding()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals(0, packageJob.LoosePackageIDs.Count);

			var newPackageID1 = Helper.CreatePackageHeader(packageJob, "ABC");
			AssertEquals(1, packageJob.LoosePackageIDs.Count);

			var newPackageID2 = Helper.CreatePackageHeader(packageJob, "DEF");
			AssertEquals(2, packageJob.LoosePackageIDs.Count);

			var dummyParent = Data.Dummy;
			dummyParent.IsLoosePackageIDsSupported = true;
			AssertLooseIDsGridVisibility(packageJob, true, 2); // header + "ABC" + "DEF"
		}

		public void TestPackageIDsGrid_SetCurrentPackageJob_ForDocuments()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals(0, packageJob.LoosePackageIDs.Count);

			var newPackageID1 = Helper.CreatePackageHeader(packageJob, "ABC");
			AssertEquals(1, packageJob.LoosePackageIDs.Count);

			var newPackageID2 = Helper.CreatePackageHeader(packageJob, "DEF");
			AssertEquals(2, packageJob.LoosePackageIDs.Count);

			var dummyParent = Data.Dummy;
			dummyParent.IsLoosePackageIDsSupported = true;
			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();
				Application.DoEvents();

				var index1 = treeUserControl.PackageIDsGrid.List.Cast<PkgPackageJobPackageHeaderPivot>()
				.Select((p, index) => new { p, index }).First(t => t.p.PackageHeader == newPackageID1).index;
				treeUserControl.PackageIDsGrid.Select(index1);
				treeUserControl.PackageIDsGrid.ContextMenu.DoPopup();
				AssertEquals(packageJob, newPackageID1.CurrentPackageJob);

				treeUserControl.PackageIDsGrid.UnSelectAll();

				var index2 = treeUserControl.PackageIDsGrid.List.Cast<PkgPackageJobPackageHeaderPivot>()
				.Select((p, index) => new { p, index }).First(t => t.p.PackageHeader == newPackageID2).index;
				treeUserControl.PackageIDsGrid.Select(index2);
				treeUserControl.PackageIDsGrid.ContextMenu.DoPopup();
				AssertEquals(packageJob, newPackageID2.CurrentPackageJob);
			}
		}

		void AssertLooseIDsGridVisibility(PkgPackageJob packageJob, bool expectedVisibility, int expectedIDRows = 0)
		{
			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();
				AssertEquals(expectedVisibility, !treeUserControl.TreeGridSplitContainer.Panel2Collapsed);
				if (expectedVisibility)
				{
					AssertEquals(expectedIDRows + 1, treeUserControl.PackageIDsGrid.VisibleRowCount);
				}
			}
		}

		#endregion

		#region TestControlOpenWithoutProblemWithHigherDpiSettings

		public void TestControlOpenWithoutProblemWithHigherDpiSettings()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(150, 150))
			using (var form = new ZForm(packageJob))
			{
				AssertNoExceptionThrown(() =>
				{
					var treeUserControl = new PackingTreeViewUserControlForTest();
					form.Controls.Add(treeUserControl);
					form.Show();
				});
			}
		}

		#endregion

		public void TestAssignUnAssignButtonPadding()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				AssertButtonMargins(treeUserControl.AssignButton, 0, 130, 0, 20);
				AssertButtonMargins(treeUserControl.UnassignButton, 0, 20, 0, 130);
			}
		}

		void AssertButtonMargins(ToolStripButton button, int left, int top, int right, int bottom)
		{
			AssertEquals("PaddingLeft", left, button.Margin.Left);
			AssertEquals("PaddingTop", top, button.Margin.Top);
			AssertEquals("PaddingRight", right, button.Margin.Right);
			AssertEquals("PaddingBottom", bottom, button.Margin.Bottom);
		}

		#region TestPrintPackageLabelMenuItem

		public void TestPrintPackageLabelMenuItem()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);
				contextMenu.Show();
				AssertEquals("Menu item exists.", true, printPackageLabelMenuItem.Visible);
				AssertEquals("Menu item is enabled.", true, printPackageLabelMenuItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestPrintPackageLabelMenuItem_EnabledOnlyForPackages()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(treeUserControl.Tree.TopNode);
				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				AssertEquals("The menu item should not be enabled if it's a Package Job.", true, printPackageLabelMenuItem.Visible);
				AssertEquals("The menu item should not be enabled if it's a Package Job.", false, printPackageLabelMenuItem.Enabled);
			}
		}

		public void TestPrintPackageLabelMenuItem_EnabledOnlyForSinglePackageSelected()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT1 = packageJob.Packages.AddNew("PLT", "PID01");
			var packagePLT2 = packageJob.Packages.AddNew("PLT", "PID02");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", true, packageJob.Packages.Any());
			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT1.CanPrintCarrierLabel);
			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT2.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT1 = treeUserControl.Tree.FindNodeByPackage(packagePLT1);
				var nodeForPackagePLT2 = treeUserControl.Tree.FindNodeByPackage(packagePLT2);
				var allNodes = new[] { nodeForPackagePLT1, nodeForPackagePLT2 };

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNodes(allNodes, 2);
				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				AssertEquals("The menu item should not be enabled multiple packages are selected.", true, printPackageLabelMenuItem.Visible);
				AssertEquals("The menu item should not be enabled multiple packages are selected.", false, printPackageLabelMenuItem.Enabled);
				AssertEquals("The menu item label is correct.", "Select a Single Package to Print Package Label", printPackageLabelMenuItem.Text);
			}
		}

		public void TestPrintPackageLabelMenuItem_MenuItemLabel_PackageNotSentToRTUS()
		{
			TestPrintPackageLabelMenuItem_MenuItemLabelCore(isSentToRTUS: false, expectedLabelMenuText: "Print Package Label");
		}

		public void TestPrintPackageLabelMenuItem_MenuItemLabel_PackageSentToRTUS()
		{
			TestPrintPackageLabelMenuItem_MenuItemLabelCore(isSentToRTUS: true, expectedLabelMenuText: "Reprint Package Label");
		}

		void TestPrintPackageLabelMenuItem_MenuItemLabelCore(bool isSentToRTUS, string expectedLabelMenuText)
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = isSentToRTUS;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();

				AssertEquals("The menu item should be visible.", true, printPackageLabelMenuItem.Visible);
				AssertEquals("The menu item label is correct.", expectedLabelMenuText, printPackageLabelMenuItem.Text);
			}
		}

		[RequiresSTA]
		public void TestPrintPackageLabelMenuItem_NotShownIfPackageJobTypeIsNotConfigured()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot print carrier label.", false, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestPrintPackageLabelMenuItem_NotShownIfPackageHasNoID()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot print carrier label.", false, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestPrintPackageLabelMenuItem_NotShownIfParentHasNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot print carrier label.", false, packagePLT.CanPrintCarrierLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestPrintPackageLabelMenuItem_ShowsErrorWhenPrintCarrierLabelFails()
		{
			Data.CreatePackingData();
			Data.Dummy.JobNoForPackingParent = "DUMMY1";
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER1";
			printer.SQ_AllowPrinting = true;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.RTUSLabelPrinterPK = printer.PK;
			packagePLT.IsSentToRTUS = true;
			AssertEquals("Pre-condition:", 1, packageJob.Packages.Count);
			Factory.Save();

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("PrintPackageLabelMenuItem", false).First();
				printPackageLabelMenuItem.PerformClick();
				AssertEquals("Dummy 'DUMMY1' has no Carrier Booking Agent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCancelPackageLabelMenuItem

		public void TestCancelPackageLabelMenuItem()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can cancel package label.", true, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);
				contextMenu.Show();
				AssertEquals("Menu item exists.", true, cancelPackageLabelMenuItem.Visible);
				AssertEquals("Menu item is enabled.", true, cancelPackageLabelMenuItem.Enabled);
			}
		}

		public void TestCancelPackageLabelMenuItem_EnabledOnlyForPackages()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can cancel package label.", true, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(treeUserControl.Tree.TopNode);
				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				AssertEquals("The menu item should not be enabled if it's a Package Job.", true, cancelPackageLabelMenuItem.Visible);
				AssertEquals("The menu item should not be enabled if it's a Package Job.", false, cancelPackageLabelMenuItem.Enabled);
			}
		}

		public void TestCancelPackageLabelMenuItem_MultiplePackagesSelected()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT1 = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT1.IsSentToRTUS = true;
			var packagePLT2 = packageJob.Packages.AddNew("PLT", "PID02");
			packagePLT2.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", true, packageJob.Packages.Any());
			AssertEquals("Pre-condition: Package can cancel carrier label.", true, packagePLT1.CanCancelPackageLabel);
			AssertEquals("Pre-condition: Package can cancel carrier label.", true, packagePLT2.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT1 = treeUserControl.Tree.FindNodeByPackage(packagePLT1);
				var nodeForPackagePLT2 = treeUserControl.Tree.FindNodeByPackage(packagePLT2);
				var allNodes = new[] { nodeForPackagePLT1, nodeForPackagePLT2 };

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNodes(allNodes, 2);
				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				AssertEquals("The menu item should not be enabled multiple packages are selected.", true, cancelPackageLabelMenuItem.Visible);
				AssertEquals("The menu item should not be enabled multiple packages are selected.", true, cancelPackageLabelMenuItem.Enabled);
			}
		}

		public void TestCancelPackageLabelMenuItem_MultiplePackagesSelected_NotAllCanCancelPackageLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;

			var packagePLT1 = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT1.IsSentToRTUS = true;
			var packagePLT2 = packageJob.Packages.AddNew("PLT", "PID02");
			packagePLT2.IsSentToRTUS = false;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", true, packageJob.Packages.Any());
			AssertEquals("Pre-condition: Package can cancel carrier label.", true, packagePLT1.CanCancelPackageLabel);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT2.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackagePLT1 = treeUserControl.Tree.FindNodeByPackage(packagePLT1);
				var nodeForPackagePLT2 = treeUserControl.Tree.FindNodeByPackage(packagePLT2);
				var allNodes = new[] { nodeForPackagePLT1, nodeForPackagePLT2 };

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNodes(allNodes, 2);
				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;

				contextMenu.Show();
				AssertEquals("The menu item should not be enabled multiple packages are selected.", false, cancelPackageLabelMenuItem.Visible);
			}
		}

		public void TestCancelPackageLabelMenuItem_NotShownIfPackageJobTypeIsNotConfigured()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestCancelPackageLabelMenuItem_NotShownIfPackageHasNoID()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestCancelPackageLabelMenuItem_NotShownIfParentHasNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestCancelPackageLabelMenuItem_NotShownIfPackageNotSentToRTUS()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = false;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var printPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();
				AssertEquals("The menu item should not be visible.", false, printPackageLabelMenuItem.Visible);
			}
		}

		public void TestCancelPackageLabelMenuItem_CorrectMessageDisplayedToTheUser()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package can cancel carrier label.", true, packagePLT.CanCancelPackageLabel);

			var cancellationMock = new Mock<ICarrierLabelCancellation>();
			using (ObjectFactory.Substitute(cancellationMock.Object))
			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();

				cancelPackageLabelMenuItem.PerformClick();
				AssertEquals("Packages have been marked for package label cancellation. Please save the package job for the cancellation to take effect.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, packageJob.HasChanges);
			}
		}

		public void TestCancelPackageLabelMenuItem_ErrorWithCancellationDisplayedToUser()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: PackageJob has packages.", 1, packageJob.Packages.Count);
			AssertEquals("Pre-condition: Package cannot cancel carrier label.", false, packagePLT.CanCancelPackageLabel);

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				var nodeForPackagePLT = treeUserControl.Tree.FindNodeByPackage(packagePLT);
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackagePLT);

				var cancelPackageLabelMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("CancelPackageLabelMenuItem", false).First();
				var contextMenu = treeUserControl.Tree.ContextMenuStrip;
				contextMenu.Show();

				// explicitly set to true to allow perform click and display error message
				cancelPackageLabelMenuItem.Visible = true;

				cancelPackageLabelMenuItem.PerformClick();
				AssertEquals("One or more packages cannot cancel package label.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestClosePackage

		public void TestClosePackage_Scanning()
		{
			AssertClosePackage(isScanning: true);
		}

		public void TestClosePackage_NotScanning()
		{
			AssertClosePackage(isScanning: false);
		}

		void AssertClosePackage(bool isScanning)
		{
			var package = Data.PackageJob.Packages.AddNew();
			var dummy = Data.Dummy;
			dummy.SSCCPrefix = "1111111";
			dummy.JobNoForPackingParent = "D0000010011";
			Factory.Save();

			using (var form = new ZForm(Data.PackageJob))
			using (var packingTreeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(packingTreeUserControl);
				form.Show();

				var nodeForPackage = packingTreeUserControl.Tree.FindNodeByPackage(package);
				packingTreeUserControl.Tree.NodeSelector.DeselectAllNodes();
				packingTreeUserControl.Tree.NodeSelector.SelectNode(nodeForPackage);
				AssertEquals("Precondition", null, dummy.LastSSCCGenerationContext);

				packingTreeUserControl.ClosePackage(isScanning);
				AssertEquals("011111110000000014", package.KP_PackageID);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(isScanning ? SSCCGenerationContext.ScanPacking : SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

				if (isScanning)
				{
					AssertEquals(SSCCGenerationContext.ScanPacking, dummy.LastSSCCGenerationContext);
					AssertNotNull(dummy.LastNotifications);
					AssertNotEquals(packingTreeUserControl, dummy.LastNotifications);
					AssertEquals(false, typeof(PackingTreeViewUserControl).IsAssignableFrom(dummy.LastNotifications.GetType()));
				}
				else
				{
					AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
					AssertEquals(packingTreeUserControl, dummy.LastNotifications);
				}
			}
		}

		public void TestClosePackage_DoesNotThrowExceptionWhenGeneratingPackageId_ShowingMessageInGetSSCCPrefix()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var package = packageJob.Packages.AddNew();
			var dummy = Data.Dummy;
			dummy.SSCCPrefix = "1111111";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D0000010011";
			Factory.Save();

			using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(true), () => Globals.SetIsUnitTestingProductionFunctionality(false)))
			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				treeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
				AssertEquals("011111110000000014", package.KP_PackageID);
				AssertEquals("Some message was shown!", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
			}
		}

		#endregion

		#region TestReleasePackageMenuItem

		public void TestReleasePackageMenuItem_CannotReleasePackage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Data.CreatePackingData();
			Data.Dummy.CannotReleasePackageMessageForTest = "Some cannot release message.";
			Data.Dummy.CanReleasePackageForTest = false;

			var packageJob = Data.PackageJob;
			var package = packageJob.Packages.AddNew("PLT", "P1");
			Factory.Save();

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackage = treeUserControl.Tree.FindNodeByPackage(package);
				var releasePackageMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("ReleasePackageMenuItem", false).First();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackage);

				AssertEquals("Precondition: package is not released.", false, package.IsReleased);
				releasePackageMenuItem.PerformClick();

				AssertEquals("Some cannot release message.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Package is not released.", false, package.IsReleased);
			}
		}

		public void TestReleasePackageMenuItem_CanReleasePackage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Data.CreatePackingData();
			Data.Dummy.CanReleasePackageForTest = true;

			var packageJob = Data.PackageJob;
			var package = packageJob.Packages.AddNew("PLT", "P1");
			Factory.Save();

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackage = treeUserControl.Tree.FindNodeByPackage(package);
				var releasePackageMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("ReleasePackageMenuItem", false).First();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackage);

				AssertEquals("Precondition: package is not released.", false, package.IsReleased);
				releasePackageMenuItem.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Package is released.", true, package.IsReleased);
			}
		}

		public void TestReleasePackageMenuItem_UnreleasePackage_CannotReleasePackage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Data.CreatePackingData();
			Data.Dummy.CannotReleasePackageMessageForTest = "Some cannot release message.";
			Data.Dummy.CanReleasePackageForTest = false;

			var packageJob = Data.PackageJob;
			var package = packageJob.Packages.AddNew("PLT", "P1");
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			using (var form = new ZForm(packageJob))
			using (var treeUserControl = new PackingTreeViewUserControlForTest())
			{
				form.Controls.Add(treeUserControl);
				form.Show();

				var nodeForPackage = treeUserControl.Tree.FindNodeByPackage(package);
				var releasePackageMenuItem = treeUserControl.Tree.ContextMenuStrip.Items.Find("ReleasePackageMenuItem", false).First();

				treeUserControl.Tree.NodeSelector.DeselectAllNodes();
				treeUserControl.Tree.NodeSelector.SelectNode(nodeForPackage);

				AssertEquals("Precondition: package is released.", true, package.IsReleased);
				releasePackageMenuItem.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Package is not released.", false, package.IsReleased);
			}
		}

		#endregion

		#region Implementation

		class PackingTreeViewUserControlForTest : PackingTreeViewUserControl
		{
			public ToolStripMenuItem ViewJobMenuItem => (ToolStripMenuItem)typeof(PackingTreeViewUserControl).GetField("ViewJobMenuItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			public ToolStripMenuItem ExpandMenuItem => (ToolStripMenuItem)typeof(PackingTreeViewUserControl).GetField("ExpandMenuItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			public ToolStripMenuItem CollapseMenuItem => (ToolStripMenuItem)typeof(PackingTreeViewUserControl).GetField("CollapseMenuItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			protected override object DataSourceCore
			{
				get
				{
					if (makeDataSourceNull)
					{
						return null;
					}

					return dataSource ?? base.DataSourceCore;
				}
			}

			PkgPackageJob dataSource;

			public void ChangeDataSource(PkgPackageJob dataSource)
			{
				this.dataSource = dataSource;
			}

			bool makeDataSourceNull;

			public void MakeDataSourceNull()
			{
				makeDataSourceNull = true;
			}

			public ToolStripButton AssignButton => (ToolStripButton)typeof(PackingTreeViewUserControl).GetField("AssignButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			public ToolStripButton UnassignButton => (ToolStripButton)typeof(PackingTreeViewUserControl).GetField("UnassignButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Data.CreatePackingData();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (form != null)
			{
				form.Dispose();
			}

			if (treeUserControl != null)
			{
				treeUserControl.Dispose();
			}
		}

		protected ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Data.PackageJob);
					form.Controls.Add(TreeUserControl);
				}
				return form;
			}
		}
		ZForm form;

		PackingTreeViewUserControlForTest TreeUserControl => treeUserControl ?? (treeUserControl = new PackingTreeViewUserControlForTest());
		PackingTreeViewUserControlForTest treeUserControl;

		#endregion
	}
}
