using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Internal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DocAddressTestForm))]
	sealed class DocAddressUserControlTest : BasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList");
		}

		#region TestDeletingAddress

		[RequiresSTA]
		public void TestDocAddressDeleteOptionEnabledForShipments()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			using (var form = new ZForm(shipment))
			using (var tabControl = new ZTabControl())
			{
				form.Controls.Add(tabControl);
				tabControl.PlugIns.Add(ControllerIDs.DocAddresses);
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)tabControl.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var addressGrid = (DocAddressGrid)control.Controls.Find("AddressGrid", true)[0];
					var list = new JobDocAddressCollectionForPlugin(((IDocAddresses)shipment).DocAddresses);
					var address = list.AddNew();
					address.DocAddressType = Integration.DocAddressType.NotifyParty;
					Assert("Address is not overriden", !address.E2_AddressOverride);
					addressGrid.SetDataBinding(list, "");
					addressGrid.Select(0);
					addressGrid.ContextMenu.DoPopup();
					Assert("Delete menu item is enabled", addressGrid.DeleteMenuItem.Enabled);
					AssertEquals("Remove", addressGrid.DeleteMenuItem.Text);
					address.E2_AddressOverride = true;
					addressGrid.ContextMenu.DoPopup();
					AssertEquals("Delete", addressGrid.DeleteMenuItem.Text);
				}
			}
		}

		public void TestDeletingAddressAndDpsScreenMenu()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var parent = new JobDocAddressParentForTesting(Factory);
			var docAddress1 = parent.DocAddresses.AddNew();
			var docAddress2 = parent.DocAddresses.AddNew();
			docAddress1.OrganisationPK = org.PK;
			docAddress2.E2_AddressOverride = true;
			using (var form = new ZForm(parent))
			{
				form.PlugIns.Add(ControllerIDs.DocAddresses);
				form.Show();
				using (DocAddressesPlugIn plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.SetDataBinding(parent.DocAddresses, "");
					((Core.Forms.ZGridColumnInfo)addressGrid.ColumnStyles[1]).IsReadOnly = false;

					var list = (IBusinessObjectCollection)addressGrid.List;
					AssertEquals(2, list.Count);
					AssertCollectionContains(docAddress1, list);
					AssertCollectionContains(docAddress2, list);
					addressGrid.Select(0);
					addressGrid.Select(1);
					AssertEquals(2, addressGrid.SelectedRowCount);
					KeySender.PostKeyDown(addressGrid, Keys.Delete);
					Application.DoEvents();
					list = (IBusinessObjectCollection)addressGrid.List;
					AssertEquals(0, list.Count);
					AssertEquals(false, docAddress1.IsDeleted);
					AssertEquals(ZGuid.Empty, docAddress1.OrganisationPK);
					AssertEquals(false, docAddress1.E2_AddressOverride);
					AssertEquals(false, docAddress2.IsDeleted);
					AssertEquals(ZGuid.Empty, docAddress2.OrganisationPK);
					AssertEquals(false, docAddress2.E2_AddressOverride);

					parent.CanDeleteAddressForTesting = new JobDocAddressParentForTesting.CanDeleteAddressDelegateForTesting(x => x == docAddress2);
					list.Add(docAddress1);
					list.Add(docAddress2);
					docAddress1.OrganisationPK = org.PK;
					docAddress2.E2_AddressOverride = true;
					addressGrid.SelectAllElements();
					Application.DoEvents();
					KeySender.PostKeyDown(addressGrid, Keys.Delete);
					Application.DoEvents();
					list = (IBusinessObjectCollection)addressGrid.List;
					AssertEquals(0, list.Count);
					AssertEquals(false, docAddress1.IsDeleted);
					AssertEquals(ZGuid.Empty, docAddress1.OrganisationPK);
					AssertEquals(false, docAddress1.E2_AddressOverride);
					AssertEquals(true, docAddress2.IsDeleted);

					//try to click screen menu when the list is empty
					AssertEquals(0, list.Count);
					var screen = addressGrid.ContextMenu.MenuItems.FindByText("Screen");
					AssertNoExceptionThrown(screen.PerformClick);  //there was an exception.
					AssertNull(addressGrid.ContextMenu.MenuItems.FindByText("Screen (Full List)"));
				}
			}
		}

		public void TestScreenWhenJobDocAddressParentChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var parent = new JobDocAddressParentForTesting(Factory);
			var docAddress1 = parent.DocAddresses.AddNew();
			docAddress1.OrganisationPK = org.PK;
			Factory.Save();

			using (var form = new ZForm(parent))
			{
				form.PlugIns.Add(ControllerIDs.DocAddresses);
				form.Show();
				using (DocAddressesPlugIn plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.SetDataBinding(parent.DocAddresses, "");
					((Core.Forms.ZGridColumnInfo)addressGrid.ColumnStyles[1]).IsReadOnly = false;

					var list = (IBusinessObjectCollection)addressGrid.List;
					AssertEquals(1, list.Count);
					AssertCollectionContains(docAddress1, list);
					addressGrid.Select(0);
					AssertEquals(1, addressGrid.SelectedRowCount);

					Assert("Precondition", !docAddress1.HasChanges);
					Assert("Precondition", docAddress1.IsInDatabase);

					parent.HasChanges = true;
					Assert("Precondition", (docAddress1.Parent as BusinessObject).HasChanges);

					var screen = addressGrid.ContextMenu.MenuItems.FindByText("Screen");
					screen.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Please save before screening for denied parties."));
				}
			}
		}

		public void TestDocAddressCanBeDeleted_WhenAddressRowIsReadonly()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			using (var form = new ZForm(shipment))
			using (var tabControl = new ZTabControl())
			{
				form.Controls.Add(tabControl);
				tabControl.PlugIns.Add(ControllerIDs.DocAddresses);
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)tabControl.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var addressGrid = (DocAddressGrid)control.Controls.Find("AddressGrid", true)[0];
					var list = new JobDocAddressCollectionForPlugin(((IDocAddresses)shipment).DocAddresses);
					var address = list.AddNew();
					address.DocAddressType = Integration.DocAddressType.NotifyParty;
					address.ReadOnly = true;

					addressGrid.SetDataBinding(list, "");
					addressGrid.Select(0);
					addressGrid.ContextMenu.DoPopup();
					Assert("Delete menu item is enabled", addressGrid.DeleteMenuItem.Enabled);

					UnitTestUserNotification.Instance.ClearMessages();
					addressGrid.DeleteMenuItem.PerformClick();
					AssertEquals("No message shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		#endregion

		#region TestContacts_OnlyShowActiveContact

		public void TestContacts_OnlyShowActiveContact()
		{
			var org = Factory.New<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var contact3 = org.Contacts.AddNew();
			contact2.OC_IsActive = false;

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.OrganisationPK = org.PK;

			using (var form = new ZForm(docAddress))
			{
				form.PlugIns.Add(ControllerIDs.DocAddresses);
				form.Show();
				using (DocAddressesPlugIn plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.OnUserControlShown();
					var addressOrganisationControl = ControlTestHelper.FindControls<ZDocAddressOrganisationControl>(plugIn.UserControl, true).FirstOrDefault();
					AssertNotNull(addressOrganisationControl);
					addressOrganisationControl.SetDataBinding(docAddress, "OrganisationPK");
					AssertEquals("Should only show the 2 active contact", 2, addressOrganisationControl.ContactEdit.List.Count);
					AssertEquals("Should not have any inactive contact", false, addressOrganisationControl.ContactEdit.List.Cast<OrgContact>().Any(x => !x.OC_IsActive));
				}
			}
		}

		#endregion

		#region TestDeniedPartyTabPageVisible

		public void TestDeniedPartyTabPageVisible()
		{
			DocAddressTestForm.HostBusinessEntity = new DummyBizObj();
			using (var form = new DocAddressTestForm(new JobDocAddressParentForTesting(Factory)))
			{
				Assert(!form.Control.TabControl.Controls.Contains(form.Control.ScreeningLogsTabPage));
			}

			DocAddressTestForm.HostBusinessEntity = new DummyScreeningPartyProvider();
			using (var form = new DocAddressTestForm(new JobDocAddressParentForTesting(Factory)))
			{
				Assert(form.Control.TabControl.Controls.Contains(form.Control.ScreeningLogsTabPage));
			}

			DocAddressTestForm.HostBusinessEntity = new DummyAUDec();
			using (var form = new DocAddressTestForm(new JobDocAddressParentForTesting(Factory)))
			{
				Assert(!form.Control.TabControl.Controls.Contains(form.Control.ScreeningLogsTabPage));
			}

			DocAddressTestForm.HostBusinessEntity = new DummyUSDec();
			using (var form = new DocAddressTestForm(new JobDocAddressParentForTesting(Factory)))
			{
				Assert(form.Control.TabControl.Controls.Contains(form.Control.ScreeningLogsTabPage));
			}
		}

		#endregion

		#region TestAddressGridContextMenu_Popup

		[RequiresSTA]
		public void TestAddressGridContextMenu_Popup()
		{
			var org = Factory.New<OrgHeader>();
			var parent = new JobDocAddressParentForTesting(Factory);
			var docAddress1 = parent.DocAddresses.AddNew();
			docAddress1.OrganisationPK = org.PK;
			AssertNotNull(docAddress1.Organisation);

			var docAddress2 = parent.DocAddresses.AddNew();
			AssertNull(docAddress2.Organisation);
			parent.CanDeleteAddressForTesting = new JobDocAddressParentForTesting.CanDeleteAddressDelegateForTesting(x => x == docAddress2);

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
					addressGrid.SetDataBinding(parent.DocAddresses, "");
					var list = (IBusinessObjectCollection)addressGrid.List;
					AssertEquals(2, list.Count);

					addressGrid.CurrentRowIndex = 0;
					addressGrid.SetCurrentHitTestForTest(0, 0);
					addressGrid.OnPopup_CallForTesting();
					AssertEquals("This record should not be able to be deleted", false, addressGrid.DeleteMenuItem.Enabled);

					addressGrid.CurrentRowIndex = 1;
					addressGrid.SetCurrentHitTestForTest(1, 0);
					addressGrid.OnPopup_CallForTesting();
					AssertEquals("This record should be able to be deleted", true, addressGrid.DeleteMenuItem.Enabled);
				}
			}
		}

		#endregion

		public void TestAddressDetailsLabelMaximumSize()
		{
			using (var form = new DocAddressTestForm(new JobDocAddressParentForTesting(Factory)))
			{
				form.Show();
				var control = form.Control;

				AssertLabelMaximumWidthAndAutoEllipsis("Company Name", labelName: "zLabel1", expectedMaximumWidth: 204);
				AssertLabelMaximumWidthAndAutoEllipsis("Address 1", labelName: "zLabel12", expectedMaximumWidth: 204);
				AssertLabelMaximumWidthAndAutoEllipsis("Address 2", labelName: "zLabel13", expectedMaximumWidth: 204);

				void AssertLabelMaximumWidthAndAutoEllipsis(string message, string labelName, int expectedMaximumWidth)
				{
					var label = control.FindSingle<ZLabel>(x => x.Name == labelName);
					AssertNotNull($"{message}: {labelName}", label);
					CombineAssertions($"{message} [{labelName}]", () =>
					{
						AssertEquals("Maximum Size Width", expectedMaximumWidth, label.MaximumSize.Width);
						AssertEquals("AutoEllipsis", true, label.AutoEllipsis);
					});
				}
			}
		}

		#region Implementation

		public override Form GetFormToBash()
		{
			return new DocAddressTestForm(new JobDocAddressParentForTesting(Factory));
		}

		#endregion
	}
}
