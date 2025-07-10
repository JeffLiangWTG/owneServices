using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	public class PackingUserControlTest : PackingTestCaseWithFactory
	{
		#region TestAutoPackToolStripMenuItem

		public void TestAutoPackToolStripMenuItem()
		{
			ToolStripMenuItem autoPackMenuItem = null;
			string autoPackMenuItemText = null;

			Data.CreatePackingData();
			Data.PackageJob.MassPackageProcessFinished += (sender, e) =>
			{
				autoPackMenuItemText = autoPackMenuItem.Text;
			};

			using (var form = new ZForm(Data.Dummy))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.Tree.TopNode.Nodes.Count);

				autoPackMenuItem = control.AutoPackToolStripMenuItem;
				AssertEquals("Auto-Pack", autoPackMenuItem.Text);

				// Add this to ensure that Packages Count Changed does Change the Menu Item Text
				autoPackMenuItem.TextChanged += TextChangedHandler;

				using (Data.PackageJob.InitiateMassPackageProcess())
				{
					var outer = Data.PackageJob.Packages.AddNew();
					autoPackMenuItem.TextChanged -= TextChangedHandler;
					AssertEquals("Precondition: Adding outers adds new nodes in GUI.", 1, control.Tree.TopNode.Nodes.Count);
					AssertEquals("Auto-Pack", autoPackMenuItem.Text);
				}

				AssertEquals("Remove Packages to enable Auto-Pack", autoPackMenuItem.Text);
			}

			void TextChangedHandler(object sender, EventArgs e)
			{
				autoPackMenuItem.TextChanged -= TextChangedHandler;
				autoPackMenuItem.Text = "ChangeMe";
			}
		}

		#endregion

		#region TestDoesNotAddOuterNodeWhenAddingNodesIsSuspended

		public void TestDoesNotAddOuterNodeWhenAddingNodesIsSuspended()
		{
			Data.CreatePackingData();

			using (var form = new ZForm(Data.Dummy))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.Tree.TopNode.Nodes.Count);

				using (AddingNodesSuspender.SuspendAddingNodes(Factory))
				{
					Data.PackageJob.Packages.AddNew();
					AssertEquals("While Adding Nodes is suspended, any outers added in business will not reflect in GUI.", 0, control.Tree.TopNode.Nodes.Count);
				}

				var outer = Data.PackageJob.Packages.AddNew();
				AssertEquals("Adding outers in business should normally add new nodes in GUI.", 1, control.Tree.TopNode.Nodes.Count);
				AssertEquals(outer, ((PackingTreeNode)control.Tree.TopNode.Nodes[0]).Package);
			}
		}

		#endregion

		#region TestPackageJobEventsHooked

		public void TestPackageJobEventsHooked()
		{
			var dummy1 = Factory.New<DummyWithPacking>();
			var dummy2 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(dummy1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(dummy2);
			AssertEquals("OnPackageJobCreatedOrLoadedCount must be called only once when creating package job.", 1, dummy1.OnPackageJobCreatedOrLoadedCount);
			AssertEquals("OnPackageJobCreatedOrLoadedCount must be called only once when creating package job.", 1, dummy2.OnPackageJobCreatedOrLoadedCount);
			Factory.Save();

			using (var form = new ZForm())
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				packingUserControl.Visible = false;
				AssertEquals("Precondition.", false, packingUserControl.Visible);
				AssertEquals("Precondition.", 0, packingUserControl.HookHitCountForTesting);

				packingUserControl.SetDataBinding(packageJob1, "");
				AssertEquals("PackageJobEvents should not yet be hooked.", 0, packingUserControl.HookHitCountForTesting);

				packingUserControl.Visible = true;
				AssertEquals("Precondition.", true, packingUserControl.Visible);
				AssertEquals("Should have hooked the events.", 1, packingUserControl.HookHitCountForTesting);

				packingUserControl.SetDataBinding(packageJob2, "");
				AssertEquals("Should have hooked the event again for the new Package Job.", 2, packingUserControl.HookHitCountForTesting);
			}
		}

		#endregion

		#region TestParentJobNumberChanged

		public void TestParentJobNumberChanged()
		{
			var dummy1 = Factory.New<DummyWithPacking>();
			dummy1.JobNoForPackingParent = "D123";
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(dummy1);
			Factory.Save();

			using (var form = new ZForm(dummy1))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(packageJob1, "");
				control.Visible = true;
				AssertEquals("Dummy D123", control.Tree.TopNode.Text);

				dummy1.JobNoForPackingParent = "D456";
				packageJob1.OnParentJobNumberChanged();
				AssertEquals("Dummy D456", control.Tree.TopNode.Text);
			}
		}

		public void TestParentJobNumberChanged_IPackingParentCustomDescription_FullDescription()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingCustomDescription);

			var dummy1 = Factory.New<DummyWithPackingCustomDescription>();
			dummy1.FullJobDescriptionForPackingParent = "Single Booking Bla";
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(dummy1);
			Factory.Save();

			using (var form = new ZForm(dummy1))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(packageJob1, "");
				control.Visible = true;
				AssertEquals("Single Booking Bla", control.Tree.TopNode.Text);

				dummy1.FullJobDescriptionForPackingParent = "Multi Booking";
				packageJob1.OnParentJobNumberChanged();
				AssertEquals("Multi Booking", control.Tree.TopNode.Text);
			}
		}

		class DummyWithPackingCustomDescription : DummyWithPacking, IPackingParentCustomDescription
		{
			public DummyWithPackingCustomDescription(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString IPackingParentCustomDescription.FullJobDescription
			{
				get { return FullJobDescriptionForPackingParent; }
			}

			public ZString FullJobDescriptionForPackingParent;
		}

		#endregion

		#region TestChangingBackAndForthBetweenTabsDoesntHookEventsMultipleTimes

		[RequiresSTA]
		public void TestChangingBackAndForthBetweenTabsDoesntHookEventsMultipleTimes()
		{
			Data.CreatePackingData();
			Factory.Save();

			using (var form = new ZForm(Data.Dummy))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Insert(new ZTabPage(), 0);

				form.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn); // sets up form hooks
				var userControl = (PackingUserControl)plugin.UserControl;

				form.Show();
				AssertEquals("Precondition.", 0, userControl.HookHitCountForTesting);

				tabControl.SelectTab(1);
				tabControl.SelectTab(0);
				tabControl.SelectTab(1);
				tabControl.SelectTab(0);
				tabControl.SelectTab(1);
				AssertEquals("Should have only hooked the event once.", 1, userControl.HookHitCountForTesting);
			}
		}

		#endregion

		#region TestNewPackageJobHasChangesIsFalseOnBind

		public void TestNewPackageJobHasChangesIsFalseOnBind()
		{
			var dummy = Factory.New<DummyWithPacking>();

			bool hasChangesChanged = false;
			dummy.HasChangesChanged += delegate
			{
				hasChangesChanged = true;
			};

			using (var form = new ZForm(dummy))
			{
				form.Controls.Add(new PackingUserControl());
				AssertNull("Precondition - PackageJob should not exist.", PkgPackageJob.LoadPackageJob(dummy));
				AssertEquals("Since package job is not loaded it should not call OnPackageJobCreatedOrLoadedCount.", 0, dummy.OnPackageJobCreatedOrLoadedCount);

				form.Show();
				AssertEquals("Since package job is created it should call OnPackageJobCreatedOrLoadedCount once.", 1, dummy.OnPackageJobCreatedOrLoadedCount);

				var packageJob = PkgPackageJob.LoadPackageJob(dummy);
				AssertEquals("PackageJob created on Bind should *not* have changes.", false, packageJob.HasChanges);
				AssertEquals("Creating a new PackageJob on Bind should *not* fire HasChanges.", false, hasChangesChanged);
				AssertEquals("Since package job is loaded it should call OnPackageJobCreatedOrLoadedCount twice.", 2, dummy.OnPackageJobCreatedOrLoadedCount);

				Factory.Save();
				AssertEquals("New + Empty PackageJob was not edited and should not be saved.", false, packageJob.IsInDatabase);

				packageJob.Packages.AddNew();
				Factory.Save();
				AssertEquals("Non-Empty PackageJob should be saved.", true, packageJob.IsInDatabase);
			}
		}

		#endregion

		#region TestNonSystemEmptyBarcodeScanned

		public void TestNonSystemEmptyBarcodeScanned()
		{
			Data.CreatePackingData();
			Data.PackageJob.Packages.AddNew();

			using (var form = new ZForm(Data.Dummy))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 1, control.Tree.TopNode.Nodes.Count);
				AssertNoExceptionThrown(() => SendKeys(form, Keys.Control | Keys.L, Keys.Space, Keys.Control | Keys.L));
			}
		}

		void SendKeys(Control control, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.SendKeyDown(control, control.Handle, key);
			}
		}

		#endregion

		#region TestScanQtyModeDefaultedFromPackingParent

		public void TestScanQtyModeDefaultedFromPackingParent()
		{
			Data.CreatePackingData();
			AssertDefaultScanMode(Data.Dummy, isScanQtyModeDefaulted: false);

			Data.Dummy.IsScanQtyAllowed = YesNoWithReasonForNo.Yes;
			AssertDefaultScanMode(Data.Dummy, isScanQtyModeDefaulted: true);
			AssertDefaultScanMode(Data.Dummy, isScanQtyModeDefaulted: true, ensureScanModeButtonIsCheckedBeforeShow: true);
		}

		void AssertDefaultScanMode(DummyWithPacking dummy, bool isScanQtyModeDefaulted, bool ensureScanModeButtonIsCheckedBeforeShow = false)
		{
			using (var form = new ZForm(dummy))
			{
				var userControl = new TestPackingUserControl();
				userControl.ScanQtyModeButton.Checked = ensureScanModeButtonIsCheckedBeforeShow;

				form.Controls.Add(userControl);
				form.Show();

				var message = isScanQtyModeDefaulted
					? "Scan Qty is allowed for Parent, Scan Mode should default to Scan Qty."
					: "Scan Qty is not allowed for Parent, Scan Mode should default to Scan All.";

				AssertEquals(message, isScanQtyModeDefaulted, userControl.ScanQtyModeButton.Checked);
			}
		}

		class TestPackingUserControl : PackingUserControl
		{
			public new ZToolStripButton ScanQtyModeButton
			{
				get { return base.ScanQtyModeButton; }
			}

			public new ZToolStripButton ScanPackModeButton
			{
				get { return base.ScanPackModeButton; }
			}
		}

		#endregion

		#region TestModeButtonWidthsSufficientForTranslation

		public void TestModeButtonWidthsSufficientForTranslation()
		{
			var minButtonWidthRequiredForTranslation = 170;
			using (var form = new ZForm(Data.Dummy))
			{
				var control = new TestPackingUserControl();
				form.Controls.Add(control);
				form.Show();
				Assert("ScanPackModeButton is too narrow for translation", control.ScanPackModeButton.Width >= minButtonWidthRequiredForTranslation);
				Assert("ScanQtyModeButton is too narrow for translation", control.ScanQtyModeButton.Width >= minButtonWidthRequiredForTranslation);
			}
		}

		#endregion

		#region TestLooseIDsButton

		public void TestLooseIDsButton_Availability()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var dummyParent = Data.Dummy;
			dummyParent.IsLoosePackageIDsSupported = true;
			AssertLooseIDsButtonVisibility(packageJob, true);

			dummyParent.IsLoosePackageIDsSupported = false;
			AssertLooseIDsButtonVisibility(packageJob, false);

			dummyParent.IsLoosePackageIDsSupported = true;
			AssertLooseIDsButtonVisibility(packageJob, true);

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			AssertLooseIDsButtonVisibility(packageJob, false);
		}

		void AssertLooseIDsButtonVisibility(PkgPackageJob packageJob, bool expectedVisibility)
		{
			using (var form = new ZForm())
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				packingUserControl.SetDataBinding(packageJob, "");
				AssertEquals(expectedVisibility, packingUserControl.LooseIDsButton.Visible);
			}
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly_HandlingUnit()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();

			using (var form = new ZForm())
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				packingUserControl.SetDataBinding(handlingUnit, "");

				handlingUnit.PackageJob.OnParentJobIsReadOnlyChanged();

				var splitContainer = packingUserControl.FindSingle<KSplitContainer>("SplitContainer");
				var toolstrip = splitContainer.Panel2.FindSingle<KToolStrip>("Toolstrip");
				var addPackageButton = toolstrip.Items.Find("AddPackageButton", true).FirstOrDefault() as ZToolStripButton;
				var closePackageButton = toolstrip.Items.Find("ClosePackageButton", true).FirstOrDefault() as ZToolStripButton;
				var generateIDsButton = toolstrip.Items.Find("GenerateIDsButton", true).FirstOrDefault() as ZToolStripButton;
				var packButton = toolstrip.Items.Find("PackButton", true).FirstOrDefault() as ZToolStripDropDownButton;
				var removeButton = toolstrip.Items.Find("RemoveButton", true).FirstOrDefault() as ZToolStripButton;
				var scanPackModeButton = toolstrip.Items.Find("ScanPackModeButton", true).FirstOrDefault() as ZToolStripButton;
				var scanQtyModeButton = toolstrip.Items.Find("ScanQtyModeButton", true).FirstOrDefault() as ZToolStripButton;

				CombineAssertions(() =>
				{
					AssertEquals("The control should be readonly for handling unit", false, addPackageButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, closePackageButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, generateIDsButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, packButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, removeButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, scanPackModeButton.Enabled);
					AssertEquals("The control should be readonly for handling unit", false, scanQtyModeButton.Enabled);
				});
			}
		}

		public void TestReadOnly_PackageJobIsReadOnly()
		{
			Data.CreatePackingData();
			Data.PackageJob.ReadOnly = true;

			using (var form = new ZForm(Data.Dummy))
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				AssertEquals("Packing User Control should be read only.", true, packingUserControl.ReadOnly);
			}
		}

		public void TestReadOnly_PackageJobIsNotReadOnly()
		{
			Data.CreatePackingData();
			AssertEquals("Precondition: PackageJob should not be ReadOnly.", false, Data.PackageJob.ReadOnly);

			using (var form = new ZForm(Data.Dummy))
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				AssertEquals("Packing User Control should NOT be read only.", false, packingUserControl.ReadOnly);
			}
		}

		public void TestReadOnly_IsPackingJobReadOnly_True()
		{
			TestReadOnly_IsPackingJobReadOnlyCore(true, true);
		}

		public void TestReadOnly_IsPackingJobReadOnly_False()
		{
			TestReadOnly_IsPackingJobReadOnlyCore(false, false);
		}

		void TestReadOnly_IsPackingJobReadOnlyCore(bool isPackingJobReadOnly, bool expectedReadOnly)
		{
			var dummy = Factory.New<DummyWithPacking>();
			dummy.IsPackingJobReadOnly = isPackingJobReadOnly;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			using (var form = new ZForm(packageJob))
			{
				var packingUserControl = new PackingUserControl();
				form.Controls.Add(packingUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotEquals(null, form);
					AssertNotEquals(ODisplayMode.ReadOnly, form.DisplayMode);
					AssertNotEquals(ODisplayMode.Delete, form.DisplayMode);
					AssertNotEquals(null, packageJob);
					AssertNotEquals(true, packageJob.IsDeleted);
					AssertNotEquals(true, packageJob.KJ_IsFinalized);
					AssertNotEquals(true, packageJob.ReadOnly);
					AssertNotEquals(null, packageJob.ParentJob);
					AssertNotEquals(true, packageJob.ParentJob.IsReadOnly);
					AssertEquals("Packing User Control readonly status is determined by IsPackingJobReadOnly", expectedReadOnly, packingUserControl.ReadOnly);
				});
			}
		}

		#endregion

		#region TestSavingForm_SuspendsImageIndexAndPackageCountChanged

		public void TestSavingForm_SuspendsImageIndexAndPackageCountChanged()
		{
			Data.CreatePackingData();
			// force index population so count changed is fired on Save()
			((IBindingList)Data.PackageJob.Packages).ListChanged += ListChangedToForceIndexPopulation;

			var massPackageProcessFinishedHitCount = 0;

			using (var form = new ZForm(Data.Dummy))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.Tree.TopNode.Nodes.Count);

				for (var index = 0; index < 10; index++)
				{
					Data.PackageJob.Packages.AddNew("PLT");
				}

				AssertEquals("Precondition: Adding outers adds new nodes in GUI.", 10, control.Tree.TopNode.Nodes.Count);

				var countChangedHitCount = 0;
				var countChangedWasFired = false;
				Data.PackageJob.MassPackageProcessFinished += (sender, e) => massPackageProcessFinishedHitCount++;
				Data.PackageJob.Packages.CountChanged += (sender, e) =>
				{
					countChangedWasFired = true;

					if (!Data.PackageJob.IsMassPackageProcessRunning)
					{
						countChangedHitCount++;
					}

					if (Data.PackageJob.IsInDatabase)
					{
						using (Data.PackageJob.SuspendValidationTesting())
						{
							Data.PackageJob.KJ_GS_NKReleasedByInfo.AddError("Some Error");
						}
					}
				};

				Factory.Saved += (sender, e) =>
				{
					using (Data.PackageJob.SuspendValidationTesting())
					{
						Data.PackageJob.KJ_GS_NKReleasedByInfo.AddError("Another Error");
					}
				};

				var textChangedHitCount = 0;
				var autoPackMenuText = "";
				control.AutoPackToolStripMenuItem.Text = "ChangeMe";
				control.AutoPackToolStripMenuItem.TextChanged += AutoPackToolStripMenuItem_TextChanged;

				Factory.Save();
				AssertEquals("Package Count Changed should be suspended during Save.", 0, countChangedHitCount);
				AssertEquals("Package Count Changed (with Mass Package Process) should have been called.", true, countChangedWasFired);
				AssertEquals("Mass Package Process should never fire.", 0, massPackageProcessFinishedHitCount);
				AssertEquals("Image Update should be suspended during Save.", control.Tree.TopNode.NormalImageIndex, control.Tree.TopNode.ImageIndex);
				AssertEquals("Auto-Pack Menu Text should be updated.", "Remove Packages to enable Auto-Pack", autoPackMenuText);
				AssertEquals("Auto-Pack Menu Text should be updated only once.", 1, textChangedHitCount);

				void AutoPackToolStripMenuItem_TextChanged(object sender, EventArgs e)
				{
					textChangedHitCount++;
					control.AutoPackToolStripMenuItem.TextChanged -= AutoPackToolStripMenuItem_TextChanged;
					autoPackMenuText = control.AutoPackToolStripMenuItem.Text;

					control.AutoPackToolStripMenuItem.Text = "ChangeMe";
					control.AutoPackToolStripMenuItem.TextChanged += AutoPackToolStripMenuItem_TextChanged;
				}
			}

			void ListChangedToForceIndexPopulation(object sender, EventArgs e)
			{
				// Force Index Population
				_ = Data.PackageJob.Packages.Count;
			}
		}

		public void TestSavingForm_SuspendsImageIndexAndPackageCountChanged_WhenSaveFails()
		{
			Data.CreatePackingData();
			Factory.Save();

			// Add Notifications Change on Saved (Update Image should still be suspended)
			Factory.Saved += (sender, e) =>
			{
				using (Data.PackageJob.SuspendValidationTesting())
				{
					Data.PackageJob.KJ_GS_NKReleasedByInfo.AddError("Another Error");
				}
			};

			using (var form = new ZForm(Data.Dummy))
			{
				var control = new PackingUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.Tree.TopNode.Nodes.Count);

				for (var index = 0; index < 10; index++)
				{
					Data.PackageJob.Packages.AddNew("PLT");
				}

				AssertEquals("Precondition: Adding outers adds new nodes in GUI.", 10, control.Tree.TopNode.Nodes.Count);

				var massPackageProcessFinishedHitCount = 0;
				Data.PackageJob.MassPackageProcessFinished += (sender, e) => massPackageProcessFinishedHitCount++;

				Factory.ServiceContainer.AddAfterOnSavingService(new ThrowErrorOnSaveService(Data.PackageJob));

				var textChangedHitCount = 0;
				var autoPackMenuText = "";
				control.AutoPackToolStripMenuItem.Text = "ChangeMe";
				control.AutoPackToolStripMenuItem.TextChanged += AutoPackToolStripMenuItem_TextChanged;

				try
				{
					Factory.Save();
				}
				catch (ZCannotSaveException)
				{
				}

				AssertEquals("Mass Package Process should never fire.", 0, massPackageProcessFinishedHitCount);
				AssertEquals("Image Update should be suspended during Save.", control.Tree.TopNode.NormalImageIndex, control.Tree.TopNode.ImageIndex);
				AssertEquals("Auto-Pack Menu Text should be updated.", "Remove Packages to enable Auto-Pack", autoPackMenuText);
				AssertEquals("Auto-Pack Menu Text should be updated only once per save.", 1, textChangedHitCount);

				try
				{
					Factory.Save();
				}
				catch (ZCannotSaveException)
				{
				}

				AssertEquals("Mass Package Process should never fire.", 0, massPackageProcessFinishedHitCount);
				AssertEquals("Image Update should be suspended during Save.", control.Tree.TopNode.NormalImageIndex, control.Tree.TopNode.ImageIndex);
				AssertEquals("Auto-Pack Menu Text should be updated.", "Remove Packages to enable Auto-Pack", autoPackMenuText);
				AssertEquals("Auto-Pack Menu Text should be updated only once per save.", 2, textChangedHitCount);

				void AutoPackToolStripMenuItem_TextChanged(object sender, EventArgs e)
				{
					textChangedHitCount++;
					control.AutoPackToolStripMenuItem.TextChanged -= AutoPackToolStripMenuItem_TextChanged;
					autoPackMenuText = control.AutoPackToolStripMenuItem.Text;

					control.AutoPackToolStripMenuItem.Text = "ChangeMe";
					control.AutoPackToolStripMenuItem.TextChanged += AutoPackToolStripMenuItem_TextChanged;
				}
			}
		}

		class ThrowErrorOnSaveService : IAfterOnSavingBOProcessingService
		{
			public ThrowErrorOnSaveService(PkgPackageJob packageJob)
			{
				PackageJob = packageJob;
			}

			readonly PkgPackageJob PackageJob;

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				using (PackageJob.SuspendValidationTesting())
				{
					PackageJob.KJ_GS_NKReleasedByInfo.AddError("Another Error");
				}

				throw new ZCannotSaveException("ABORT!", "HEADING");
			}
		}

		#endregion
	}
}
