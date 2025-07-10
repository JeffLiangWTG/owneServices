using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignTrackingControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		#region Drip Marketing

		public void TestMenuItems()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				var suspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var removeSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.RemoveSuspensionMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var commenceFromSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);

				AssertNull("Not available for normal campaign", suspendMenuItem);
				AssertNull("Not available for normal campaign", removeSuspendMenuItem);
				AssertNull("Not available for normal campaign", commenceFromSuspendMenuItem);
			}

			campaign.G0_G0_Master = master.PK;
			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				var suspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var removeSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.RemoveSuspensionMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var commenceFromSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);

				AssertNotNull("Not available for touch campaign", suspendMenuItem);
				AssertNotNull("Not available for touch campaign", removeSuspendMenuItem);
				AssertNull("Not available for touch campaign", commenceFromSuspendMenuItem);
			}

			using (DummyCampaignForm form = new DummyCampaignForm(master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				var suspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var removeSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.RemoveSuspensionMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				var commenceFromSuspendMenuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);

				AssertNotNull("Should be available for master campaign", suspendMenuItem);
				AssertNotNull("Should be available for master campaign", removeSuspendMenuItem);
				AssertNotNull("Should be available for master campaign", commenceFromSuspendMenuItem);
			}
		}

		public void TestAvailableMoves_FromMaster()
		{
			Helper.SetupDripCampaign();

			var contact = Helper.Org.Contacts.AddNew();
			contact.OC_Email = "tobi@test.com";
			contact.OC_ContactName = "Tobi";

			var itemMaster = Helper.Master.CampaignsItemsSent.AddNew();
			itemMaster.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			itemMaster.G8_RecipientID = contact.PK;
			itemMaster.G8_TrackingStatus = "UNV";

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("4 campaign item in grid", 4, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(3);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(5, menuItem.MenuItems.Count);
				AssertEquals(Helper.Touch1A.TouchFullName, menuItem.MenuItems[0].Text);
				AssertEquals(Helper.Touch1B.TouchFullName, menuItem.MenuItems[1].Text);
				AssertEquals(Helper.Touch2A.TouchFullName, menuItem.MenuItems[2].Text);
				AssertEquals(Helper.Touch2B.TouchFullName, menuItem.MenuItems[3].Text);
				AssertEquals(Helper.Touch3A.TouchFullName, menuItem.MenuItems[4].Text);
			}
		}

		public void TestAvailableMoves()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(4, menuItem.MenuItems.Count);
				AssertEquals(Helper.Touch1B.TouchFullName, menuItem.MenuItems[0].Text);
				AssertEquals(Helper.Touch2A.TouchFullName, menuItem.MenuItems[1].Text);
				AssertEquals(Helper.Touch2B.TouchFullName, menuItem.MenuItems[2].Text);
				AssertEquals(Helper.Touch3A.TouchFullName, menuItem.MenuItems[3].Text);

				form.TrackingControl.FilterStripControl.FilteredGrid.UnSelectAll();
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(2, menuItem.MenuItems.Count);
				AssertEquals(Helper.Touch2B.TouchFullName, menuItem.MenuItems[0].Text);
				AssertEquals(Helper.Touch3A.TouchFullName, menuItem.MenuItems[1].Text);

				form.TrackingControl.FilterStripControl.FilteredGrid.UnSelectAll();
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(2);
				menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(1, menuItem.MenuItems.Count);
				AssertEquals(Helper.Touch3A.TouchFullName, menuItem.MenuItems[0].Text);
			}
		}

		public void TestSelectOneRow()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(1, menuItem.MenuItems.Count);
				AssertEquals("Please select one row", menuItem.MenuItems[0].Text);
			}
		}

		public void TestSuspendItem()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.PerformClick();

				AssertRecipientSuspendStatus(Helper.Master, Helper.Contact2.PK, true);
				foreach (var touch in Helper.Master.AllTouches)
				{
					AssertRecipientSuspendStatus(touch, Helper.Contact2.PK, true);
				}
			}
		}

		public void TestSuspendItem_Touch()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Touch1A))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("2 campaign item in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.PerformClick();

				var master = new BusinessObjectFactory().Load<GlbCompanyCampaign>(helper.Master.PK);

				AssertRecipientSuspendStatus(master, Helper.Contact2.PK, true);
				foreach (var touch in master.AllTouches)
				{
					AssertRecipientSuspendStatus(touch, Helper.Contact2.PK, true);
				}
			}
		}

		public void TestUnsuspendItem()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.SuspendDeliveryMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.PerformClick();

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				menuItem = FindMenuItemWithName(CampaignTrackingControl.RemoveSuspensionMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.PerformClick();

				AssertRecipientSuspendStatus(Helper.Master, Helper.Contact2.PK, false);
				foreach (var touch in Helper.Master.AllTouches)
				{
					AssertRecipientSuspendStatus(touch, Helper.Contact2.PK, false);
				}
			}
		}

		void AssertRecipientSuspendStatus(GlbCompanyCampaign campaign, ZGuid recipientId, bool isSuspended)
		{
			foreach (var item in campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals(item.G8_RecipientID == recipientId && isSuspended, item.G8_IsSuspended);
			}
		}

		public void TestMoveContact_NextHorizontal()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				//select item with contact1
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				//move to 2a
				var destinationMenuItem = FindMenuItemWithName(Helper.Touch2A.TouchFullName, menuItem.MenuItems);
				AssertNotNull(destinationMenuItem);

				destinationMenuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.LastShownScheduleForm.FireSaveButton();

				var foundItem = GetItemForRecipient(Helper.Master, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(true, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1A, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(true, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1B, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch2A, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);
				AssertEquals("QUE", foundItem.G8_TrackingStatus);

				foundItem = GetItemForRecipient(Helper.Touch2B, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch3A, Helper.Contact1.PK);
				AssertNull(foundItem);

				//verify other items were not affected
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact3.PK));
			}
		}

		public void TestMoveContact_InactiveRecipient()
		{
			Helper.SetupDripCampaign();
			Helper.Contact1.OC_IsActive = false;
			Factory.Save();

			var master = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbCompanyCampaign>(Helper.Master.PK);
			var touch1a = master.AllTouches.FirstOrDefault(t => t.PK == Helper.Touch1A.PK);
			var touch1b = master.AllTouches.FirstOrDefault(t => t.PK == Helper.Touch1B.PK);
			var touch2a = master.AllTouches.FirstOrDefault(t => t.PK == Helper.Touch2A.PK);
			var touch2b = master.AllTouches.FirstOrDefault(t => t.PK == Helper.Touch2B.PK);
			var touch3a = master.AllTouches.FirstOrDefault(t => t.PK == Helper.Touch3A.PK);

			using (DummyCampaignForm form = new DummyCampaignForm(master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				//select item with contact1
				form.TrackingControl.FilterStripControl.FilteredGrid.SelectSingleElement(Helper.ItemMaster1);

				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				//move to 2a
				var destinationMenuItem = FindMenuItemWithName(Helper.Touch2A.TouchFullName, menuItem.MenuItems);
				AssertNotNull(destinationMenuItem);

				destinationMenuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.LastShownScheduleForm?.FireSaveButton();

				var foundItem = GetItemForRecipient(Helper.Master, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(touch1a, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(touch1b, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(touch2a, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(touch2b, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(touch3a, Helper.Contact1.PK);
				AssertNull(foundItem);

				//verify other items were not affected
				AssertNotNull(GetItemForRecipient(master, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(master, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(touch1a, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(touch1a, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(touch1b, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(touch1b, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(touch2a, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(touch2a, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(touch2b, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(touch2b, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(touch3a, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(touch3a, Helper.Contact3.PK));
			}
		}

		public void TestMoveContact_TwoHorizontalsAway()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				//select item with contact1
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				//move to 3a
				var destinationMenuItem = FindMenuItemWithName(Helper.Touch3A.TouchFullName, menuItem.MenuItems);
				AssertNotNull(destinationMenuItem);

				destinationMenuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.LastShownScheduleForm.FireSaveButton();

				var foundItem = GetItemForRecipient(Helper.Master, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(true, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1A, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(true, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1B, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch2A, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch2B, Helper.Contact1.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch3A, Helper.Contact1.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);
				AssertEquals("QUE", foundItem.G8_TrackingStatus);

				//verify other items were not affected
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact2.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact2.PK));
				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact3.PK));
			}
		}

		public void TestMoveContact_SameVertical()
		{
			Helper.SetupDripCampaign();

			using (DummyCampaignForm form = new DummyCampaignForm(Helper.Master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				AssertEquals("3 campaign item in grid", 3, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				//select item with contact2
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(1);
				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				//move to 2b
				var destinationMenuItem = FindMenuItemWithName(Helper.Touch2B.TouchFullName, menuItem.MenuItems);
				AssertNotNull(destinationMenuItem);

				destinationMenuItem.PerformClick();

				var foundItem = GetItemForRecipient(Helper.Master, Helper.Contact2.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1A, Helper.Contact2.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);

				foundItem = GetItemForRecipient(Helper.Touch1B, Helper.Contact2.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch2A, Helper.Contact2.PK);
				AssertNull(foundItem);

				foundItem = GetItemForRecipient(Helper.Touch2B, Helper.Contact2.PK);
				AssertNotNull(foundItem);
				AssertEquals(false, foundItem.G8_IsSuspended);
				AssertEquals(false, foundItem.G8_IsBlocked);
				AssertEquals("QUE", foundItem.G8_TrackingStatus);

				foundItem = GetItemForRecipient(Helper.Touch3A, Helper.Contact2.PK);
				AssertNull(foundItem);

				//verify other items were not affected
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact1.PK));
				AssertNotNull(GetItemForRecipient(Helper.Master, Helper.Contact3.PK));

				AssertNotNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact1.PK));
				AssertNull(GetItemForRecipient(Helper.Touch1A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact1.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch1B, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact1.PK));
				AssertNull(GetItemForRecipient(Helper.Touch2A, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact1.PK));
				AssertNotNull(GetItemForRecipient(Helper.Touch2B, Helper.Contact3.PK));

				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact1.PK));
				AssertNull(GetItemForRecipient(Helper.Touch3A, Helper.Contact3.PK));
			}
		}

		public void TestCommenceFromMenuItemOnPopupWithoutTouches()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "Test";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "Test Campaign";

			var item = master.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_TrackingStatus = "UNV";
			Factory.Save();

			using (var form = new DummyCampaignForm(master))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);

				var menuItem = FindMenuItemWithName(CampaignTrackingControl.CommenceFromMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems);
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(1, menuItem.MenuItems.Count);
				AssertEquals("Campaign has no touch points defined", menuItem.MenuItems[0].Text);
			}
		}

		GlbCompanyCampaignItem GetItemForRecipient(GlbCompanyCampaign campaign, ZGuid recipientId)
		{
			return campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().FirstOrDefault(i => i.G8_RecipientID == recipientId);
		}

		#endregion

		#region Load

		public void TestOnLoadDoesNotRunAgainOnDispose()
		{
			var loadedTimes = 0;

			var campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = new DummyCampaignForm(campaign))
			{
				form.Load += (sender, e) =>
				{
					loadedTimes++;
				};

				form.Show();
				AssertEquals(1, loadedTimes);
			}

			AssertEquals("Should not run load again when disposing form", 1, loadedTimes);
		}

		public void TestSalesRelationControlOnLoadDoesNotRunAgainOnDispose()
		{
			var salesRelationControlLoadedTimes = 0;

			var campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = new DummyCampaignForm(campaign))
			{
				form.TrackingControl.SalesRelationControlExposed.Load += (sender, e) =>
				{
					salesRelationControlLoadedTimes++;
				};

				form.Show();

				AssertEquals(1, salesRelationControlLoadedTimes);

				form.TrackingControl.CampaignItemContactCrossReferencesControl.Focus();
			}

			AssertEquals("Should not run Sales Relation Control load again when disposing form", 1, salesRelationControlLoadedTimes);
		}

		#endregion

		#region Show Campaign Item Form

		public void TestOnLoaded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "contact@org.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "contact2@org.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			GlbCompanyCampaignItem item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.Count);

				Assert(form.DeliveryDetailsButtonExposed.Enabled);

				form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.Position = 1;
				Assert(!form.DeliveryDetailsButtonExposed.Enabled);

				form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.Position = 0;
				Assert(form.DeliveryDetailsButtonExposed.Enabled);
			}
		}

		public void TestShowCampaignItemForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			item.G8_FollowedUp = new ZDateTime(2005, 1, 1);
			item.G8_GS_NKFollowedUpBy = "MM";

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("1 campaign item in grid", 1, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);

				form.TrackingControl.FilterItemModule.DoubleClick();

				AssertEquals("Form Shown", typeof(MasterFiles.GUI.ZOrganisationsForm), form.TrackingControl.FilterItemModule.LastShownForm.GetType());
				MasterFiles.GUI.ZOrganisationsForm activeForm = (MasterFiles.GUI.ZOrganisationsForm)form.TrackingControl.FilterItemModule.LastShownForm;
				Assert("Item should have been reloaded", item != activeForm.BusinessEntity);

				activeForm.Close();
			}

			campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.TrackingControl.FilterItemModule.DoubleClick();
				AssertNull("Form NOT Shown", form.TrackingControl.FilterItemModule.LastShownForm);
				AssertEquals("Msg shown about no item selected", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.TrackingControl.ShowCampaignItemForm();
				AssertNull("Form NOT Shown", form.TrackingControl.FilterItemModule.LastShownForm);
				AssertEquals("Msg shown about no item selected", "Please select a Sent Campaign to edit.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				FindMenuItemWithName("&Edit", form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNull("Form NOT Shown", form.TrackingControl.FilterItemModule.LastShownForm);
				AssertEquals("Msg shown about no item selected", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		MenuItem FindMenuItemWithName(string menuItemName, Menu.MenuItemCollection menuItems)
		{
			foreach (MenuItem item in menuItems)
			{
				if (item.Text.Equals(menuItemName))
				{
					return item;
				}
			}

			return null;
		}

		public void TestSetupControlsForTargetListCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				form.TrackingControl.SetupControlsForTargetListCampaign();

				Assert(form.TrackingControl.linkTrackingTab.TabVisible);
				AssertNotNull(FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
				AssertNotNull(FindMenuItemWithName(CampaignTrackingControl.DeleteScheduleMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
				AssertNotNull(FindMenuItemWithName(CampaignTrackingControl.EditScheduleSendTimeMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
			}

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				form.TrackingControl.SetupControlsForTargetListCampaign();

				Assert(!form.TrackingControl.linkTrackingTab.TabVisible);
				AssertNull(FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
				AssertNull(FindMenuItemWithName(CampaignTrackingControl.DeleteScheduleMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
				AssertNull(FindMenuItemWithName(CampaignTrackingControl.EditScheduleSendTimeMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems));
			}
		}

		#endregion

		#region Create Task

		public void TestCreateTask()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();

				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("1 campaign item in grid", 1, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				AssertEquals("first item should now be selected", 0, form.TrackingControl.FilterStripControl.FilteredGrid.CurrentRowIndex);
				form.TrackingControl.CreateTaskButton_Click(null, EventArgs.Empty);
				AssertNotNull(form.TrackingControl.TaskManagementController.LastShownForm);

				ZForm taskForm = (ZForm)form.TrackingControl.TaskManagementController.LastShownForm;
				AssertNotNull("Correct process task type", (CRMProcessTask)taskForm.BusinessEntity);
				AssertEquals(contact.PK, ((ProcessTask)taskForm.BusinessEntity).P9_OC);
				AssertEquals(contact.OC_OH, ((ProcessTask)taskForm.BusinessEntity).OrganisationPK);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, ((ProcessTask)taskForm.BusinessEntity).P9_GS_NKAssignedStaffMember);
				AssertEquals(("Campaign Task - ___" + campaign.G0_CampaignName).Substring(0, 50), ((ProcessTask)taskForm.BusinessEntity).P9_Description);

				form.TrackingControl.TaskManagementController.LastShownForm.Dispose();
			}

			campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				form.TrackingControl.CreateTaskButton_Click(null, EventArgs.Empty);
				AssertEquals("Msg shown about no item selected", "Please select a Sent Campaign to Create a Task for.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Refresh Data

		public void TestRefresh()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "Contact@org.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "Contact2@org.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			GlbCompanyCampaignItem item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();

				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				form.TrackingControl.RefreshButton_Click(null, EventArgs.Empty);
				GlbCompanyCampaignItem currentItem = (GlbCompanyCampaignItem)form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.GetCurrent();
				AssertEquals("", currentItem.TrackingStatusDescription);

				item.G8_TrackingStatus = "NDR";
				currentItem = (GlbCompanyCampaignItem)form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.GetCurrent();
				form.TrackingControl.RefreshButton_Click(null, EventArgs.Empty);
				AssertEquals("", currentItem.TrackingStatusDescription);

				item.G8_TrackingStatus = "NDR";
				Factory.Save();

				currentItem = (GlbCompanyCampaignItem)form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.GetCurrent();
				form.TrackingControl.RefreshButton_Click(null, EventArgs.Empty);
				AssertEquals("Non-Delivery Receipt", currentItem.TrackingStatusDescription);
			}
		}

		#endregion

		#region Force Send

		public void TestForceSend()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "m@test.com";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();

				Factory.Save();
				Env.OutgoingMailManager.EmailsCreated.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestForceSend_NDR()
		{
			AssertForceSend("NDR", ZDateTime.Empty);
		}

		public void TestForceSend_VER()
		{
			AssertForceSend("VER", ZDateTime.Empty);
		}

		public void TestForceSend_UNV()
		{
			AssertForceSend("UNV", ZDateTime.Empty);
		}

		public void TestForceSend_Queued()
		{
			AssertForceSend("QUE", ZDateTime.Today);
		}

		public void AssertForceSend(string status, ZDateTime scheduled)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "m@test.com";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = status;
			campaignItem.G8_ScheduleTimeUtc = scheduled;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();

				Factory.Save();
				Env.OutgoingMailManager.EmailsCreated.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		#endregion

		#region Resend

		public void TestResend()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "m@test.com";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select a Sent Campaign to resend.", Equals(UnitTestUserNotification.Instance.LastMessage.Text));

				Factory.Save();
				Env.OutgoingMailManager.EmailsCreated.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select a Sent Campaign to resend.", (UnitTestUserNotification.Instance.LastMessage.Text));
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Factory.Save();

			using (DummyCampaignForm form1 = new DummyCampaignForm(campaign))
			{
				form1.Show();
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form1.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertEquals("Please select a Sent Campaign to send now.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form1.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals("Please select a Sent Campaign to send now.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResend_ForTouch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "m@test.com";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory, true);
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			GlbCompanyCampaign campaign = master.AllTouches.AddNew();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign, Factory, true);

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select a Sent Campaign to resend.", Equals(UnitTestUserNotification.Instance.LastMessage.Text));

				Factory.Save();
				Env.OutgoingMailManager.EmailsCreated.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select a Sent Campaign to resend.", (UnitTestUserNotification.Instance.LastMessage.Text));
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Factory.Save();

			using (DummyCampaignForm form1 = new DummyCampaignForm(campaign))
			{
				form1.Show();
				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form1.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertEquals("Please select a Sent Campaign to send now.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form1.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals("Please select a Sent Campaign to send now.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendShowsProgressForm()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "edward.onwodi@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@cargowise.com";
			contact.OC_ContactName = "Test person";
			contact.OC_OH = org.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaignTest.GlbCompanyCampaignForTest>();
			campaign.G0_CampaignName = "test asf asf asf asf as fas fas f";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_Category = campaign.Lookups.MediaCategoryList[0].Code;
			campaign.G0_Type = campaign.Lookups.ActiveMediaTypesList[0].Code;
			campaign.G0_EmailSubject = "hasda asfdjas n fan oasdnf oif";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.Show();
				AssertNull(form.TrackingControl.LastSendProgressForm);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((GlbCompanyCampaignItemCampaignDependentCollection)form.TrackingControl.FilterItemModule.GridCollection).Load(new ZQuery(GlbCompanyCampaignItemSchema.PK, campaignItem.PK));
				SelectCampaignItemsInDisplayGrid(form, campaignItem);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);

				AssertNotNull(form.TrackingControl.LastSendProgressForm);
				Assert(form.TrackingControl.LastSendProgressForm.IsDisposed);
			}
		}

		static void SelectCampaignItemsInDisplayGrid(DummyCampaignForm form, GlbCompanyCampaignItem campaignItem)
		{
			if (form.TrackingControl.FilterItemModule.DisplayGrid.ListManager == null)
			{
				throw new DeveloperNotificationException("form has to be shown so ListManager can be initialised");
			}
			form.TrackingControl.FilterItemModule.DisplayGrid.Select(form.TrackingControl.FilterItemModule.DisplayGrid.ListManager.List.IndexOf(campaignItem));
		}

		#endregion

		#region Delete Schedule

		public void TestDeleteSchedule()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact2.OC_ContactName = "M";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = "UNV";

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_TrackingStatus = "QUE";

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.DeleteScheduleMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select a Scheduled Campaign to delete.", Equals(UnitTestUserNotification.Instance.LastMessage.Text));

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.SelectAllElements();
				form.TrackingControl.DeleteSchedule_Click(null, EventArgs.Empty);
				AssertEquals("1 CampaignItem should have been deleted", 1, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
			}
		}

		#endregion

		#region Edit Schedule

		public void TestEditSchedule()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact2.OC_ContactName = "M";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem.G8_TrackingStatus = "UNV";

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_TrackingStatus = "QUE";

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.EditScheduleSendTimeMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select an item to edit.", Equals(UnitTestUserNotification.Instance.LastMessage.Text));

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.SelectAllElements();
				form.TrackingControl.EditScheduleSendTime_Click(null, EventArgs.Empty);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		public void TestEditSchedule_Unscheduled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact2.OC_ContactName = "M";
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = "UNV";

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "QUE";

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today; //Has Changes now true
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals("2 campaign items in grid", 2, form.TrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Count);
				FindMenuItemWithName(CampaignTrackingControl.EditScheduleSendTimeMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				AssertNotEquals("Should have displayed error message from BizO, not form", "Please select an item to edit.", Equals(UnitTestUserNotification.Instance.LastMessage.Text));

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.TrackingControl.FilterStripControl.FilteredGrid.SelectAllElements();
				form.TrackingControl.EditScheduleSendTime_Click(null, EventArgs.Empty);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		#endregion

		#region CaptionResourceString

		public void TestCaptionResourceString()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Factory.Save();

			using (var form = new DummyCampaignForm(campaign))
			{
				var salesRelationTab = form.Controls.Find("salesRelationTab", true);
				AssertEquals(1, salesRelationTab.Length);
				AssertEquals("", salesRelationTab[0].Text);
				AssertEquals("Sales Relations", (salesRelationTab[0] as ZTabPage).CaptionResourceString.Caption);

				var linkTrackingTab = form.Controls.Find("linkTrackingTab", true);
				AssertEquals(1, linkTrackingTab.Length);
				AssertEquals("", linkTrackingTab[0].Text);
				AssertEquals("Link Activity", (linkTrackingTab[0] as ZTabPage).CaptionResourceString.Caption);
			}
		}

		public void TestCreateNow()
		{
			var campaignOppCreation = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignOppCreation.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			Factory.Save();

			using (var formOppCreation = new DummyCampaignForm(campaignOppCreation))
			{
				formOppCreation.Show();
				AssertEquals("Create Now", formOppCreation.TrackingControl.ResendButton.Text);

				var menuItems = formOppCreation.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Cast<MenuItem>().ToList();
				AssertEquals(1, menuItems.Count(mi => mi.Text == CampaignTrackingControl.CreateNowItemName));
				AssertEquals(1, menuItems.Count(mi => mi.Text == CampaignTrackingControl.OpenOpportunityItemName));
				AssertEquals(0, menuItems.Count(mi => mi.Text == CampaignTrackingControl.ResendByEmailMenuItemName));
			}
		}

		public void TestSendNow()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			Factory.Save();

			using (var form = new DummyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals("Send Now", form.TrackingControl.ResendButton.Text);

				var menuItems = form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Cast<MenuItem>().ToList();
				AssertEquals(1, menuItems.Count(mi => mi.Text == CampaignTrackingControl.ResendByEmailMenuItemName));
				AssertEquals(0, menuItems.Count(mi => mi.Text == CampaignTrackingControl.CreateNowItemName));
				AssertEquals(0, menuItems.Count(mi => mi.Text == CampaignTrackingControl.OpenOpportunityItemName));
			}
		}

		public void TestResendButton_SendNow()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "M";
			contact2.OC_Email = "m@test.com";

			var campaign = Helper.GetCampaignWithoutErrors();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			Factory.Save();

			using (var form = new DummyCampaignForm(campaign))
			{
				campaign.G0_ActualCompletedDate = ZDateTime.Today;
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				FindMenuItemWithName(CampaignTrackingControl.ResendByEmailMenuItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestResendButton_CreateNow()
		{
			var campaignCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			campaignCoordinator.GS_Code = "TSC";
			campaignCoordinator.GS_EmailAddress = "tsc@test.org";

			var campaignManager = Factory.NewWithValidTestData<GlbStaff>();
			campaignManager.GS_Code = "TSM";
			campaignManager.GS_EmailAddress = "tsm@test.org";

			var salesPerson = Factory.NewWithValidTestData<GlbStaff>();
			salesPerson.GS_Code = "TS1";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_Category = "PRINT";
			touch.G0_Type = "PREAP";
			touch.G0_EstimatedStartedDate = ZDateTime.Now;
			touch.G0_GS_NKCampaignCoordinator = "TSC";
			touch.G0_GS_NKCampaignManager = "TSM";

			var oppCreationTemplate = touch.OpportunityCreationTemplate;
			oppCreationTemplate.PackageType = "STD";
			oppCreationTemplate.OpportunityType = "UDF";
			oppCreationTemplate.OpportunityDescription = "Test";
			oppCreationTemplate.OpportunityStatus = "CRT";
			oppCreationTemplate.OpportunityStage = "UDF";
			oppCreationTemplate.Source = "WEB";
			oppCreationTemplate.ActiveSourceDetails = "NOT";
			oppCreationTemplate.OpportunityNotes = ZBlob.FromUTF8("Test Notes");
			oppCreationTemplate.SalesPerson = "TS1";
			oppCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;

			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.Now;
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			using (var form = new DummyCampaignForm(touch))
			{
				touch.G0_ActualCompletedDate = ZDateTime.Today;
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				FindMenuItemWithName(CampaignTrackingControl.CreateNowItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.ResendButton_Click(null, EventArgs.Empty);
				AssertEquals("1 opportunities were successfully set to be created now.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenOpportunity()
		{
			var campaignCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			campaignCoordinator.GS_Code = "TSC";
			campaignCoordinator.GS_EmailAddress = "tsc@test.org";

			var campaignManager = Factory.NewWithValidTestData<GlbStaff>();
			campaignManager.GS_Code = "TSM";
			campaignManager.GS_EmailAddress = "tsm@test.org";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Blob";
			contact.OC_Email = "blob@test.com";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_Category = "PRINT";
			touch.G0_Type = "PREAP";
			touch.G0_EstimatedStartedDate = ZDateTime.Now;
			touch.G0_GS_NKCampaignCoordinator = "TSC";
			touch.G0_GS_NKCampaignManager = "TSM";

			_ = new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityNotes = ZBlob.FromUTF8("Test Notes"),
				SalesPerson = "TS1",
				OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson
			};
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.Now;
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OC = campaignItem.G8_RecipientID;
			opportunity.P8_G0 = campaignItem.G8_G0;
			Factory.Save();

			using (var form = new DummyCampaignForm(touch))
			{
				touch.G0_ActualCompletedDate = ZDateTime.Today;
				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();

				FindMenuItemWithName(CampaignTrackingControl.OpenOpportunityItemName, form.TrackingControl.FilterStripControl.FilteredGrid.ContextMenu.MenuItems).PerformClick();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				form.TrackingControl.OpenOpportunity_Click(null, EventArgs.Empty);
				AssertEquals(typeof(MasterFiles.GUI.OpportunityForm), form.TrackingControl.OpportunityController.LastShownForm.GetType());

				var formOpportunity = (OrgOpportunity)form.TrackingControl.OpportunityController.LastShownForm.BusinessEntityForPersistingForm;
				AssertEquals(formOpportunity.PK, opportunity.PK);

				form.TrackingControl.OpportunityController.LastShownForm.Dispose();
				UserIdleWorker.Flush();
			}
		}

		#endregion

		#region IsSuspended

		public void TestSuspendDelivery_InsideSales()
		{
			AssertSuspendDelivery(CampaignTypeList.Codes.InsideSales);
		}

		public void TestSuspendDelivery_DripMarketing()
		{
			AssertSuspendDelivery(CampaignTypeList.Codes.DripMarketing);
		}

		void AssertSuspendDelivery(string campaignType)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@test.com";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = campaignType;

			var campaignItem = masterCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem.G8_IsSuspended = false;
			Factory.Save();

			using (var form = new DummyCampaignForm(masterCampaign))
			{
				form.Show();

				form.TrackingControl.FilterStripControl.FirePerformSearch();
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				Assert(!campaignItem.G8_IsSuspended);

				form.TrackingControl.SuspendDelivery_Click(null, EventArgs.Empty);
				Assert(campaignItem.G8_IsSuspended);
			}
		}

		public void TestRemoveSuspension_InsideSales()
		{
			AssertRemoveSuspension(CampaignTypeList.Codes.InsideSales);
		}

		public void TestRemoveSuspension_DripMarketing()
		{
			AssertRemoveSuspension(CampaignTypeList.Codes.DripMarketing);
		}

		void AssertRemoveSuspension(string campaignType)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@test.com";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = campaignType;

			var campaignItem = masterCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem.G8_IsSuspended = true;
			Factory.Save();

			using (var form = new DummyCampaignForm(masterCampaign))
			{
				form.Show();

				form.TrackingControl.FilterStripControl.FirePerformSearch();
				form.TrackingControl.FilterStripControl.FilteredGrid.Select(0);
				Assert(campaignItem.G8_IsSuspended);

				form.TrackingControl.RemoveSuspension_Click(null, EventArgs.Empty);
				Assert(!campaignItem.G8_IsSuspended);
			}
		}

		#endregion

		#region Subscriptions Security

		public void TestSubscriptionsSecurityCheck()
		{
			var staffWithAccess = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithAccess.PK;
			staffWithAccess.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Subscriptions.AddNew();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "e@ma.il";
			contact.Subscriptions.AddNew();
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var reloadedOrg = Factory.Load<OrgHeader>(org.PK);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new DummyCampaignForm(campaign))
				using (var control = new CampaignTrackingControlForTest())
				{
					form.Controls.Add(control);
					form.Show();

					control.UnsubscribeButton_Click(this, EventArgs.Empty);

					Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(Env.Security.OrganisationControlSubscriptionPreferences.ErrorMessageForNotAllowed));
				}
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new DummyCampaignForm(campaign))
				using (var control = new CampaignTrackingControlForTest())
				{
					form.Controls.Add(control);
					form.Show();

					control.UnsubscribeButton_Click(this, EventArgs.Empty);

					Assert(UnitTestUserNotification.Instance.LastMessage.Contains(Env.Security.OrganisationControlSubscriptionPreferences.ErrorMessageForNotAllowed));
				}
			}
		}

		#endregion

		#region Implementation

		public class DummyCampaignForm : ZChildForm
		{
			public DummyCampaignForm(GlbCompanyCampaign campaign)
				: base(campaign)
			{
				this.CaptionRenderingEnabled = true;
			}

			public CampaignTrackingControl TrackingControl;

			protected override void InitializeComponent()
			{
				TrackingControl = new CampaignTrackingControlForTest();
				Controls.Add(TrackingControl);
			}

			public ZButton DeliveryDetailsButtonExposed
			{
				get { return TrackingControl.DeliveryDetailsButton; }
			}

			public CampaignItemScheduleForm LastShownScheduleForm
			{
				get { return (TrackingControl as CampaignTrackingControlForTest).LastShownScheduleForm; }
			}
		}

		class CampaignTrackingControlForTest : CampaignTrackingControl
		{
			protected override CampaignItemScheduleForm GetCampaignItemScheduleForm(Collection<IScheduleItemsProvider> contacts, GlbCompanyCampaign campaignInNewFactory)
			{
				var form = base.GetCampaignItemScheduleForm(contacts, campaignInNewFactory);
				LastShownScheduleForm = form;

				return form;
			}

			public CampaignItemScheduleForm LastShownScheduleForm;
		}

		GlbCompanyCampaignTestHelper Helper
		{
			get { return helper ?? (helper = new GlbCompanyCampaignTestHelper(Factory)); }
		}
		GlbCompanyCampaignTestHelper helper;

		#endregion
	}
}
