using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Internal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DummyForm))]
	sealed class DocAddressesPlugInTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestContextMenu()
		{
			var allAddressTypes = Enum.GetValues(typeof(DocAddressType)).Cast<DocAddressType>().Where(type => type != DocAddressType.None);
			var allAddressTypesCount = allAddressTypes.Count();

			var parent = new JobDocAddressParentForTesting(Factory);
			using (var form = new DummyForm(parent))
			{
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();
					var initialMenuItemsCount = addressGrid.ContextMenu.MenuItems.Count;
					for (int i = 1; i <= allAddressTypesCount; i++)
					{
						parent.SetSupportedAddressTypes(allAddressTypes.Take(i).ToArray());
						addressGrid.OnPopup_CallForTesting();
						AssertEquals("Menu items change each time", initialMenuItemsCount + i, addressGrid.ContextMenu.MenuItems.Count);

						var menuItemText = "Add " + DocAddressTypes.GetPair(Factory, allAddressTypes.ElementAt(i - 1)).Description;
						AssertNotNull("New supported address type added", addressGrid.ContextMenu.MenuItems.FindByText(menuItemText));
						AssertEquals("Add new address should be enabled", true, addressGrid.ContextMenu.MenuItems.FindByText(menuItemText).Enabled);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestAddAddressMenuItems_AreNotEnabledWhenReadOnly()
		{
			var allAddressTypes = Enum.GetValues(typeof(DocAddressType)).Cast<DocAddressType>().Where(type => type != DocAddressType.None);
			var allAddressTypesCount = allAddressTypes.Count();

			var parent = new JobDocAddressParentForTesting(Factory);
			parent.ReadOnly = true;
			using var form = new DummyForm(parent);
			form.Show();

			using var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses);
			plugIn.OnUserControlShown();

			var control = (DocAddressUserControl)plugIn.UserControl;
			var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
			var addressGrid = (DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
			addressGrid.OnPopup_CallForTesting();

			var initialMenuItemsCount = addressGrid.ContextMenu.MenuItems.Count;
			for (int i = 1; i <= allAddressTypesCount; i++)
			{
				parent.SetSupportedAddressTypes(allAddressTypes.Take(i).ToArray());
				addressGrid.OnPopup_CallForTesting();
				AssertEquals("Menu items change each time", initialMenuItemsCount + i, addressGrid.ContextMenu.MenuItems.Count);

				var menuItemText = "Add " + DocAddressTypes.GetPair(Factory, allAddressTypes.ElementAt(i - 1)).Description;
				AssertEquals("Add new address should not be enabled", false, addressGrid.ContextMenu.MenuItems.FindByText(menuItemText).Enabled);
			}
		}

		[RequiresSTA]
		public void TestDocAddresses_HostBizo()
		{
			var element1 = new JobDocAddressParentForTesting(Factory);
			var host = new DummyBizObjWithDependentsThatImplementIDocAddress(Factory);
			host.DependentsCollection.Add(element1);

			using (var form = new DummyFormForTest(host))
			{
				form.Show();
				form.grid1.Select(0);
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					AssertEquals(host, plugIn.DocAddresses.HostParentBizo);
				}
			}

			using (var form = new DummyForm(element1))
			{
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					AssertEquals(element1, plugIn.DocAddresses.HostParentBizo);
				}
			}
		}

		public void TestOnCurrentChanged()
		{
			JobDocAddressParentForTesting element1 = new JobDocAddressParentForTesting(Factory);
			JobDocAddressParentForTesting element2 = new JobDocAddressParentForTesting(Factory);

			DummyBizObjWithDependentsThatImplementIDocAddress host = new DummyBizObjWithDependentsThatImplementIDocAddress(Factory);
			host.DependentsCollection.Add(element1);
			host.DependentsCollection.Add(element2);

			OrgAddress orgA = Factory.New<OrgAddress>();
			OrgAddress orgB = Factory.New<OrgAddress>();

			JobDocAddress dA1 = element1.DocAddresses.AddNew(orgA, DocAddressType.ConsigneeDocumentaryAddress);
			JobDocAddress dA2 = element1.DocAddresses.AddNew();
			dA2.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			JobDocAddress dA3 = element2.DocAddresses.AddNew(orgB, DocAddressType.BuyerDocumentaryAddress);
			JobDocAddress dA4 = element2.DocAddresses.AddNew(orgB, DocAddressType.AssuredPartyDocumentaryAddress);

			using (DummyFormForTest form = new DummyFormForTest(host))
			{
				form.Show();

				form.grid1.Select(0);
				using (DocAddressesPlugIn plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					AssertEquals("Collection loaded properly, has DA1", element1, plugIn.Host);
					AssertEquals(1, plugIn.DocAddresses.Count);
					JobDocAddress address = plugIn.Host.DocAddresses[0];
					AssertEquals(orgA.PK, address.E2_OA_Address);
					AssertEquals(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, address.E2_AddressType);

					form.grid1.UnSelect(0);
					form.grid1.Select(1);
					form.grid1.ListManager.Position = 1;
					AssertEquals("Collection loaded properly, has DA1", element2, plugIn.Host);
					AssertEquals(2, plugIn.DocAddresses.Count);
				}
			}
		}

		public void TestDocAddressCollection()
		{
			OrgAddress orgA = Factory.New<OrgAddress>();
			JobDocAddress dA1 = Host.DocAddresses.FindOrCreateWithDocAddressType(orgA.PK, DocAddressType.LocalCartageExporter);
			JobDocAddress dA2 = Host.DocAddresses.FindOrCreateWithDocAddressType(ZGuid.Empty, DocAddressType.LocalCartageImporter);
			JobDocAddress dA3 = Host.DocAddresses.FindOrCreateWithDocAddressType(ZGuid.Empty, DocAddressType.LocalCartageCTO);

			dA2.E2_AddressOverride = true;

			using (DocAddressesPlugIn testPlugIn = new DocAddressesPlugIn(Host))
			{
				testPlugIn.OnUserControlShown();
				Assert("Collection loaded properly, has DA1", testPlugIn.DocAddresses.Contains(dA1.PK));
				Assert("Collection loaded properly, has DA2", testPlugIn.DocAddresses.Contains(dA2.PK));
				Assert("Collection loaded properly, Shouldn't have DA3", !testPlugIn.DocAddresses.Contains(dA3.PK));
			}
		}

		[RequiresSTA]
		public void TestListChangedEventIsNotSuspendedWhenRefreshDataSoThatNoIndexOutOfRangeExceptionThrownWhenDataBinding()
		{
			Host.RegisterEditableChildObject(Host.DocAddresses);
			var address = Host.DocAddresses.FindOrCreateWithDocAddressType(ZGuid.Empty, DocAddressType.LocalCartageImporter);
			address.E2_AddressOverride = true;

			using (var form = new DummyForm(Host, true))
			{
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					plugIn.RefreshData();
					form.FireValidateAllForTest();
					Application.DoEvents();
					plugIn.TabPage.ImageIndex = -1;
					plugIn.SelectTabPage();
					Assert(plugIn.DocAddresses.HasNotifications());
				}
			}
		}

		public void TestPluginIsDockedToFill()
		{
			using (DocAddressesPlugIn testPlugIn = new DocAddressesPlugIn(Host))
			{
				AssertEquals("Control is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}

		#region Test Objects

		public class DummyBizObjWithDependentsThatImplementIDocAddress : NonPersistentBusinessObject
		{
			public DummyBizObjWithDependentsThatImplementIDocAddress(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public JobDocAddressParentForTestingCollection DependentsCollection
			{
				get
				{
					if (dependentsCollection == null)
					{
						dependentsCollection = new JobDocAddressParentForTestingCollection(this);
					}
					return dependentsCollection;
				}
			}
			JobDocAddressParentForTestingCollection dependentsCollection;
		}

		public class JobDocAddressParentForTestingCollection : NonPersistentBusinessObjectCollection<JobDocAddressParentForTesting>
		{
			// This constructor id used in Enterprise.Winzor.Architecture.Test.BalloonWindowTest.TestBalloonWindowinSmallestParentWindow which will fail if remove the constructor
			public JobDocAddressParentForTestingCollection(DummyBizObjWithDependentsThatImplementIDocAddress parent)
			{
			}
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new JobDocAddressParentForTesting(Factory);
			}
		}

		public class DummyFormForTest : ZForm
		{
			public DummyFormForTest(DummyBizObjWithDependentsThatImplementIDocAddress host)
				: base(host)
			{
				this.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.DocAddresses, grid1);
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				Size = new System.Drawing.Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(900), ControlDpiScalingHelper.ScaleToCurrentDpiY(600));
				grid1.BindTo = "DependentsCollection";

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.Caption = "Container No.";
				zTextBoxColumnStyleInfo1.ColumnName = "SomethingToExpose";
				zTextBoxColumnStyleInfo1.IsMandatory = true;
				grid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.Controls.Add(grid1);

				this.CaptionRenderingEnabled = true;
			}

			public ZGrid grid1 = new ZGrid();
		}

		public class DummyForm : ZForm
		{
			public DummyForm(JobDocAddressParentForTesting host, bool insertDummyPlugin = false)
				: base(host)
			{
				this.Host = host;

				if (insertDummyPlugin)
				{
					PlugIns.Add(DummyControllerIDs.Dummy1);
				}

				PlugIns.Add(ControllerIDs.DocAddresses);
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				Size = new System.Drawing.Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(900), ControlDpiScalingHelper.ScaleToCurrentDpiY(600));
			}

			internal protected override ZTabControl TopLevelTabControl
			{
				get { return TabControl; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.TabControl = new ZTemplateTabControl();
				TabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				TabControl.Height = this.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
				TabControl.Width = this.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
				this.Controls.Add(TabControl);
				this.CaptionRenderingEnabled = true;
			}

			#region Implementation

			ZTemplateTabControl TabControl;
			protected JobDocAddressParentForTesting Host;

			#endregion
		}

		protected override Form GetFormToBashCore()
		{
			return new DummyForm(Host);
		}

		JobDocAddressParentForTesting Host
		{
			get
			{
				if (fHost == null)
				{
					fHost = new JobDocAddressParentForTesting(Factory);
				}
				return fHost;
			}
		}

		JobDocAddressParentForTesting fHost;

		#endregion
	}
}
