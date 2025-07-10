using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignTrackingControl : ZUserControl
	{
		internal GlbCompanyCampaignItemModule FilterItemModule;
		internal GlbCompanyCampaignItemFilterControl FilterStripControl;

		bool filterGridAdded;

		public CampaignTrackingControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!filterGridAdded)
			{
				var campaign = dataSource as GlbCompanyCampaign;
				if (campaign != null)
				{
					AddFilterGrid(campaign);
				}
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		internal void FindCampaignItemOnTrackingTab(IGlbCompanyCampaignItem campaignItem)
		{
			FilterStripControl.FindCampaignItemOnModule(campaignItem);
		}

		internal void FindCampaignItemWithDestinationURL(ZString destinationURL)
		{
			FilterStripControl.FindCampaignItemWithDestinationURL(destinationURL);
		}

		internal void FindCampaignItemWithContextName(ZString contextName)
		{
			FilterStripControl.FindCampaignItemWithContextName(contextName);
		}

		internal void FindCampaignItemWithDeliveryStatus(ZString status)
		{
			FilterStripControl.FindCampaignItemWithDeliveryStatus(status);
		}

		internal void FindCampaignItemWithUnsubscribeStatus(ZBool status)
		{
			FilterStripControl.FindCampaignItemWithUnsubscribeStatus(status);
		}

		internal void FindCampaignItemWithTransitionStatus(ZBool transitioned)
		{
			FilterStripControl.FindCampaignItemWithTransitionStatus(transitioned);
		}

		internal void DisplayCampaignItems(GlbCompanyCampaignItemCampaignDependentCollection campaignItems)
		{
			FilterStripControl.GridCollection.Clear();
			FilterStripControl.GridCollection.AddRange(campaignItems);
		}

		void AddFilterGrid(GlbCompanyCampaign campaign)
		{
			filterGridAdded = true;

			FilterItemModule = (GlbCompanyCampaignItemModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCompanyCampaignItem);
			((IGlbCompanyCampaignItemModule)FilterItemModule).Campaign = campaign;
			FilterItemModule.PerformedSearch += FilterItemModule_PerformedSearch;

			FilterStripControl = (GlbCompanyCampaignItemFilterControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			FilterStripControl.Size = ControlDpiScalingHelper.NewScaledSize(TopPanel.Width, TopPanel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(30), false);
			FilterStripControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

			TopPanel.Controls.Add(FilterStripControl);

			this.CampaignItemContactCrossReferencesControl.SetDataBinding(FilterStripControl.GridCollection, "");
			this.salesRelationControl.SetDataBinding(FilterStripControl.GridCollection, "SalesRelationModel");
			this.linkActivityUserControl.SetDataBinding(FilterStripControl.GridCollection, "StatModel");
		}

		void FilterItemModule_PerformedSearch(object sender, EventArgs e)
		{
			ListManager_PositionChanged(sender, e);
			this.UpdateContactsButton.Enabled = CurrentDataItem.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Any(item => item.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR);
			this.UnsubscribeButton.Enabled = CurrentDataItem.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Any();
		}

		void SetupButtonVisibilities()
		{
			if (CurrentDataItem != null && (CurrentDataItem.IsTargetList || CurrentDataItem.IsMasterCampaign))
			{
				if (linkTrackingTab.TabVisible)
				{
					ResendButton.Visible = false;
					DeliveryDetailsButton.Visible = false;
					UpdateContactsButton.Visible = false;
					linkTrackingTab.TabVisible = false;

					ControlDpiScalingHelper.SetLeft(ref LastCommunicationButton, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(LastCommunicationButton.Left) + ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ResendButton.Width) + 6, true);
					ControlDpiScalingHelper.SetLeft(ref UnsubscribeButton, LastCommunicationButton.Left - UnsubscribeButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
				}
			}
			else
			{
				if (!linkTrackingTab.TabVisible)
				{
					ResendButton.Visible = true;
					DeliveryDetailsButton.Visible = true;
					UpdateContactsButton.Visible = true;
					linkTrackingTab.TabVisible = true;

					ControlDpiScalingHelper.SetLeft(ref LastCommunicationButton, LastCommunicationButton.Left - ResendButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
					ControlDpiScalingHelper.SetLeft(ref UnsubscribeButton, UpdateContactsButton.Left - UpdateContactsButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
				}
			}
		}

		internal void SetupControlsForTargetListCampaign()
		{
			SetupButtonVisibilities();
			var menuItems = FilterStripControl?.FilteredGrid.ContextMenu.MenuItems.Cast<MenuItem>().ToList();
			if (menuItems != null)
			{
				var startIndex = menuItems.FindIndex(s => s.Text == CreateTaskMenuItemName) - 1;
				for (int i = startIndex; i < menuItems.Count; i++)
				{
					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.RemoveAt(startIndex);
				}
				SetContextMenuItems();
			}
		}

		void CreateSendNowMenuItems()
		{
			if (CurrentDataItem != null && !CurrentDataItem.IsMasterCampaign && !CurrentDataItem.IsTargetList)
			{
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add("-");

				if (CurrentDataItem != null && CurrentDataItem.IsOpportunityCreationCampaign)
				{
					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(CreateNowItemName, ResendButton_Click));
					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(OpenOpportunityItemName, OpenOpportunity_Click));
				}
				else
				{
					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResendByEmailMenuItemName, ResendButton_Click));
				}
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add("-");
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(DeleteScheduleMenuItemName, new EventHandler(DeleteSchedule_Click)));
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(EditScheduleSendTimeMenuItemName, new EventHandler(EditScheduleSendTime_Click)));
			}
		}
		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				EnableDeliveryDetailsButton();
				FilterStripControl.FilteredGrid.ListManager.PositionChanged += ListManager_PositionChanged;

				SetupResendButtonText();
				SetupMenuItems();
				SetupButtonVisibilities();
				SetContextMenuItems();

				salesRelationControl.ShowPopupButton = false;

				if (CurrentDataItem != null && CurrentDataItem.IsHRCampaign)
				{
					salesRelationTab.TabVisible = false;
					campaignItemContactCrossReferencesGroupBox.Visible = false;
				}
			}
		}

		void SetupResendButtonText()
		{
			ResendButton.CaptionResourceString = CurrentDataItem != null && CurrentDataItem.IsOpportunityCreationCampaign
				? Res.GetData("CampaignTrackingControl|0CCDDB3D-7448-47EA-A268-2D0EA7F39DAC", "Create Now")
				: Res.GetData("CampaignTrackingControl|f6890fa9-8e3b-4aac-b367-28b643b7e4cd", "Send Now");
		}

		void SetupMenuItems()
		{
			string newMenu = Res.GetString("MarketingManager.GUI.GlbCompanyCampaign.GlbCompanyCampaignItem.New", "&New");

			if (FilterItemModule != null)
			{
				foreach (var menuItem in FilterItemModule.FormActionMenu)
				{
					if (menuItem.Text != newMenu)
					{
						ToolStrip.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, FilterItemModule));
					}
				}
			}

			for (int i = 0; i < FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Count; i++)
			{
				var menuItem = FilterStripControl.FilteredGrid.ContextMenu.MenuItems[i];
				if (menuItem.Text == newMenu)
				{
					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Remove(menuItem);
				}
			}
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			EnableDeliveryDetailsButton();

			if (FilterStripControl.GridCollection.Count == 0)
			{
				this.CampaignItemContactEDocsControl.ShowEdocUserControl(false);
			}
			else
			{
				this.CampaignItemContactEDocsControl.UpdateEDocs((GlbCompanyCampaignItem)this.FilterStripControl.Grid.GetCurrent());
			}
		}

		#endregion

		#region Enable Delivery Details Button

		void EnableDeliveryDetailsButton()
		{
			if (SelectedItem != null)
			{
				this.DeliveryDetailsButton.Enabled = SelectedItem.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR;
			}
		}

		#endregion

		#region Context Menu

		void SetContextMenuItems()
		{
			FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add("-");
			FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(CreateTaskMenuItemName, new EventHandler(CreateTaskButton_Click)));

			CreateSendNowMenuItems();

			if (CurrentDataItem != null && (CurrentDataItem.IsMasterCampaign || CurrentDataItem.IsTouchCampaign))
			{
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add("-");
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(SuspendDeliveryMenuItemName, SuspendDelivery_Click));
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(RemoveSuspensionMenuItemName, RemoveSuspension_Click));
				FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add("-");

				if (CurrentDataItem.IsMasterCampaign)
				{
					var commenceFromMenuItem = new ZMenuItem(CommenceFromMenuItemName);
					commenceFromMenuItem.MenuItems.Add("-");
					commenceFromMenuItem.Popup += CommenceFromMenuItemOnPopup;

					FilterStripControl.FilteredGrid.ContextMenu.MenuItems.Add(commenceFromMenuItem);
				}
			}
		}

		void CommenceFromMenuItemOnPopup(object sender, EventArgs eventArgs)
		{
			var menuItem = sender as ZMenuItem;
			if (menuItem != null)
			{
				menuItem.MenuItems.Clear();

				var items = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().ToArray();
				if (items.Length != 1)
				{
					menuItem.MenuItems.Add(ResString.GetMultilingualString("8FF99139-8C41-46EB-95DC-3E94594106C9", "Please select one row"));
					return;
				}

				var campaignItem = items.FirstOrDefault();

				if (campaignItem == null)
				{
					return;
				}

				var master = campaignItem.CompanyCampaign.IsMasterCampaign
					? campaignItem.CompanyCampaign
					: campaignItem.CompanyCampaign.MasterCampaign;

				if (!master.Horizontals.Any())
				{
					menuItem.MenuItems.Add(ResString.GetMultilingualString("95E1B2AF-F71B-44DD-8E0E-22C7F8FA3486", "Campaign has no touch points defined"));
					return;
				}

				var transitions = master.SummaryStats.GetTransitionResultsForRecipient(campaignItem.G8_RecipientID.ToGuid()).ToArray();

				var maxHorizontal = transitions.Any() ? transitions.Max(t => t.HorizontalId) : (ZInt)0;
				var transitionAtMaxHorizontal = maxHorizontal > 0
					? transitions.FirstOrDefault(t => t.HorizontalId == maxHorizontal)
					: null;

				foreach (var horizontal in master.Horizontals.Where(h => h.Id >= maxHorizontal))
				{
					foreach (var campaign in horizontal.Campaigns)
					{
						if (horizontal.Id > maxHorizontal)
						{
							menuItem.MenuItems.Add(new ZMenuItem(campaign.TouchFullName, CommenceFrom_Click) { Tag = campaign });
						}
						else if (horizontal.Id == maxHorizontal
								 && (
									 transitionAtMaxHorizontal == null
									 ||
									 (transitionAtMaxHorizontal.TrackingStatus == TrackingStatusCodes.Codes.QUE
										&& campaign.PK != transitionAtMaxHorizontal.CampaignId)
									 )
							)
						{
							menuItem.MenuItems.Add(new ZMenuItem(campaign.TouchFullName, MoveToVertical_Click) { Tag = campaign });
						}
					}
				}

				if (menuItem.MenuItems.Count == 0)
				{
					menuItem.MenuItems.Add(ResString.GetMultilingualString("222808F1-A335-42EE-B888-710B369DEC7E", "Recipient has reached final Touch"));
				}
			}
		}

		void MoveToVertical_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ZMenuItem;
			if (menuItem == null)
			{
				return;
			}

			var campaign = menuItem.Tag as GlbCompanyCampaign;
			if (campaign == null)
			{
				return;
			}

			var selectedCampaignItem = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().FirstOrDefault();
			if (selectedCampaignItem == null)
			{
				return;
			}

			var master = selectedCampaignItem.CompanyCampaign.IsMasterCampaign
					? selectedCampaignItem.CompanyCampaign
					: selectedCampaignItem.CompanyCampaign.MasterCampaign;

			var targetHorizontal = master.Horizontals.FirstOrDefault(h => h.Id == campaign.G0_HorizontalId);
			if (targetHorizontal == null)
			{
				return;
			}

			var itemToMove =
				targetHorizontal.Campaigns.SelectMany(c => c.CampaignsItemsSent).Cast<GlbCompanyCampaignItem>()
					.FirstOrDefault(i => i.G8_RecipientID == selectedCampaignItem.G8_RecipientID);

			if (itemToMove != null)
			{
				itemToMove.CompanyCampaign.CampaignsItemsSent.Remove(itemToMove.PK);
				itemToMove.G8_G0 = campaign.PK;
				campaign.CampaignsItemsSent.Add(itemToMove);
				master.SummaryStats.UpdateTransitions(new[] { itemToMove });
			}
		}

		void CommenceFrom_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ZMenuItem;
			if (menuItem == null)
			{
				return;
			}

			var campaign = menuItem.Tag as GlbCompanyCampaign;
			if (campaign == null)
			{
				return;
			}

			var campaignItem = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().FirstOrDefault();

			if (campaignItem == null || campaignItem.RecipientFromView == null)
			{
				return;
			}

			campaignSender = new GlbCompanyCampaignSender(campaign, new[] { campaignItem.RecipientFromView }, true);
			SubscribeCampaignSender();
			campaignSender.SendScheduleEvent += CampaignSender_SendScheduleEvent;

			try
			{
				using (campaign.SuspendValidationOnNonPersistentProperties())
				{
					campaignSender.CheckAndSendCampaigns();
				}
			}
			finally
			{
				UnsubscribeCampaignSender();
				campaignSender.SendScheduleEvent -= CampaignSender_SendScheduleEvent;
				if (campaignSender.SendErrors.Any())
				{
					Globals.Message.ShowWarning(string.Join("\r\n", campaignSender.SendErrors), Res.GetString("9dff7042-7f94-4d35-ab5c-81a4951b3c66", "Search Results"));
				}
			}
		}

		void CampaignSender_SendScheduleEvent(object sender, GlbCompanyCampaignSender.ContactsToSendToEventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(campaignSender.Campaign);

			campaignInNewFactory.SuspendValidationOnNonPersistentProperties();
			GlbCompanyCampaignItem sentItem = null;
			campaignInNewFactory.CampaignsItemsSent.CountChanged += delegate(object o, CollectionCountChangedEventArgs args)
			{
				if (args.ItemAdded)
				{
					sentItem = args.BizObject as GlbCompanyCampaignItem;
				}
			};

			var form = GetCampaignItemScheduleForm(e.ContactsToSendTo, campaignInNewFactory);
			form.Saved += delegate
			{
				ChangeCampaignItemsStatus(false, true);

				if (sentItem != null)
				{
					campaignSender.Campaign.MasterCampaign.SummaryStats.AddCampaignItem(sentItem);
				}
			};

			ZFormModaliser.Show(form, ParentForm);
		}

		protected virtual CampaignItemScheduleForm GetCampaignItemScheduleForm(Collection<IScheduleItemsProvider> contacts, GlbCompanyCampaign campaignInNewFactory)
		{
			return new CampaignItemScheduleForm(campaignInNewFactory, contacts);
		}

		internal void SuspendDelivery_Click(object sender, EventArgs eventArgs)
		{
			ChangeCampaignItemsStatus(true, false);
		}

		internal void RemoveSuspension_Click(object sender, EventArgs eventArgs)
		{
			ChangeCampaignItemsStatus(false, false);
		}

		void ChangeCampaignItemsStatus(ZBool suspend, ZBool block)
		{
			var itemsToUpdate = new List<ItemWithBlockSuspendStatus>();
			var items = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>();

			foreach (var campaignItem in items)
			{
				var master = campaignItem.CompanyCampaign.IsMasterCampaign
					? campaignItem.CompanyCampaign
					: campaignItem.CompanyCampaign.MasterCampaign;

				var pks = master.SummaryStats.GetTransitionResultsForRecipient(campaignItem.G8_RecipientID.ToGuid()).Select(t => t.CampaignItemId);

				if (pks.Any())
				{
					var allItemsForRecipient = campaignItem.Factory.Load<GlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.PK, pks.ToArray()));
					foreach (var item in allItemsForRecipient)
					{
						itemsToUpdate.Add(new ItemWithBlockSuspendStatus(item.PK, suspend, block));
					}
				}

				var itemInMaster =
					master.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
						.FirstOrDefault(i => i.G8_RecipientID == campaignItem.G8_RecipientID);

				if (itemInMaster != null)
				{
					itemsToUpdate.Add(new ItemWithBlockSuspendStatus(itemInMaster.PK, suspend, block));
				}
			}

			if (itemsToUpdate.Count > 0)
			{
				var query = new ZQuery(GlbCompanyCampaignItemSchema.PK, itemsToUpdate.Select(i => i.PK)) { ReLoadExistingRows = true };
				var newFactory = new BusinessObjectFactory();

				var itemsData = newFactory.Load<GlbCompanyCampaignItem>(query).Join(itemsToUpdate, l => l.PK, u => u.PK, (item, status) =>
				new
				{
					Item = item,
					Data = status
				});

				foreach (var entry in itemsData)
				{
					entry.Item.G8_IsSuspended = entry.Data.Suspended;

					if (entry.Data.Block)
					{
						entry.Item.G8_IsBlocked = true;
					}
				}

				newFactory.Save();

				var updatedItems = CurrentDataItem.Factory.Load<GlbCompanyCampaignItem>(query);

				CurrentDataItem.SummaryStats.UpdateTransitions(updatedItems);
			}
		}

		class ItemWithBlockSuspendStatus
		{
			public ItemWithBlockSuspendStatus(ZGuid pk, ZBool suspended, ZBool block)
			{
				PK = pk;
				Suspended = suspended;
				Block = block;
			}

			public ZGuid PK { get; }
			public ZBool Suspended { get; }
			public ZBool Block { get; }
		}

		internal void EditScheduleSendTime_Click(object sender, EventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			Collection<IScheduleItemsProvider> elementsToEdit = new Collection<IScheduleItemsProvider>();
			var selectedElements = FilterItemModule.DisplayGrid.SelectedElements.Cast<GlbCompanyCampaignItem>();
			if (selectedElements.Any())
			{
				foreach (GlbCompanyCampaignItem campaignItem in selectedElements)
				{
					elementsToEdit.Add(newFactory.ImportFromAnotherFactorySafe(campaignItem));
				}
				ZFormModaliser.Show(new CampaignItemScheduleForm(campaignInNewFactory, elementsToEdit), ParentForm);
			}
			else
			{
				string message = Res.GetString("a418d264-5009-4f83-9e46-d7d82ab5e07c", "Please select an item to edit.");
				Globals.Message.ShowInformation(message, Res.GetString("4d6ec0b1-0d2e-42b6-855f-8a8528b9821e", "No item selected"));
			}
		}

		internal void DeleteSchedule_Click(object sender, EventArgs e)
		{
			IEnumerable<GlbCompanyCampaignItem> trackingCampaignItems = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().Where(item => item.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE);

			if (trackingCampaignItems == null || !trackingCampaignItems.Any())
			{
				Globals.Message.Show(Res.GetString("e471d769-d4e2-4aa7-a3de-e3921e07335b", "Please select a Scheduled Campaign to delete."), Res.GetString("9c52e65a-15d7-4430-8c72-4643333f5acb", "Cannot Delete"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				campaignSender = new GlbCompanyCampaignSender(CurrentDataItem, trackingCampaignItems);
				campaignSender.ShouldContinueWithSending += new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(CampaignTrackingControl_ShouldContinueWithDeleting);
				campaignSender.ItemDeleteBegin += new EventHandler(campaignSender_ItemDeleteBegin);
				campaignSender.ItemSent += new GlbCompanyCampaignSender.ItemSentEventHandler(campaignSender_ItemSent);
				campaignSender.CampaignSendEnd += new EventHandler(BusinessEntity_CampaignSendEnd);
				try
				{
					campaignSender.Requeue(trackingCampaignItems.ToArray());
				}
				finally
				{
					campaignSender.ShouldContinueWithSending -= new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(CampaignTrackingControl_ShouldContinueWithDeleting);
					campaignSender.ItemDeleteBegin -= new EventHandler(campaignSender_ItemDeleteBegin);
					campaignSender.ItemSent -= new GlbCompanyCampaignSender.ItemSentEventHandler(campaignSender_ItemSent);
					campaignSender.CampaignSendEnd -= new EventHandler(BusinessEntity_CampaignSendEnd);
				}
			}
		}

		void campaignSender_ItemSent(object sender, GlbCompanyCampaignSender.ItemSentEventArgs e)
		{
			if (SendProgressForm != null)
			{
				SendProgressForm.Status = Res.GetString("f88be0b5-91c5-499d-b235-47503a4be9b9", "Re-queuing campaign to contacts ({0} of {1}).", e.Sent, e.Total);
				SendProgressForm.PercentComplete = (int)((e.Sent / (decimal)e.Total) * 100m);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void campaignSender_ItemDeleteBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("a22a813e-cc8d-4fb6-8f56-12d9f2a4a9d9", "Re-queuing campaign items...");
			SendProgressForm.ShowCancelButton = true;
			SendProgressForm.ShowProgressBar = true;
			SendProgressForm.Cancelled += new EventHandler(SendProgressForm_Cancelled);
#if DEBUG
			LastSendProgressForm = SendProgressForm;
#endif
			SendProgressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

		bool CampaignTrackingControl_ShouldContinueWithDeleting(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
		{
			ZString message = campaignToResend != null ? campaignToResend[0].RequeueConfirmationMessage : ZString.Empty;
			return DialogResult.Yes == Globals.Message.Show(message, Res.GetString("bb9bc76a-1ffb-48fc-8f88-e14ecaf7fcf3", "Re-queue Campaign"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		internal static MultilingualString ResendByEmailMenuItemName
		{
			get { return ResString.GetMultilingualString("0be7b9d4-173b-4bf7-b4db-f35f96707902", "Send Now"); }
		}

		internal static MultilingualString CreateNowItemName => ResString.GetMultilingualString("1B4B0F71-9917-45B6-8371-FAE03D4EC4E5", "Create Now");

		internal static MultilingualString OpenOpportunityItemName => ResString.GetMultilingualString("54349CB0-50A1-41C2-B9DE-CCCC1D22423E", "Open Opportunity");

		static MultilingualString CreateTaskMenuItemName
		{
			get { return ResString.GetMultilingualString("c518467c-1e2f-43d7-ae51-202d46e34ff8", "Create Task"); }
		}

		internal static MultilingualString DeleteScheduleMenuItemName
		{
			get { return ResString.GetMultilingualString("ee775132-ce82-47cc-8683-cb135d1dad60", "Delete Schedule"); }
		}

		internal static MultilingualString EditScheduleSendTimeMenuItemName
		{
			get { return ResString.GetMultilingualString("6cce2218-f810-45b6-9d24-8fc2b9f6eed2", "Edit Schedule Send Time"); }
		}

		internal static MultilingualString SuspendDeliveryMenuItemName
		{
			get { return ResString.GetMultilingualString("F5146202-FE7B-421D-8DB6-E682997D06A8", "Suspend Delivery"); }
		}

		internal static MultilingualString RemoveSuspensionMenuItemName
		{
			get { return ResString.GetMultilingualString("32EB9664-1796-412D-80C8-28238A18E159", "Remove Suspension"); }
		}

		internal static MultilingualString CommenceFromMenuItemName
		{
			get { return ResString.GetMultilingualString("1404778A-C15C-4BAF-BB59-6B7D0E0858F0", "Commence From"); }
		}

		#endregion

		#region Show Campaign Item Form

		internal void ShowCampaignItemForm()
		{
			if (SelectedCampaignItemInControllerFactory != null)
			{
				SelectedCampaignItemInControllerFactory.G8_FollowedUp = SelectedItem.G8_FollowedUp;
				SelectedCampaignItemInControllerFactory.G8_GS_NKFollowedUpBy = SelectedItem.G8_GS_NKFollowedUpBy;

				GlbCompanyCampaignItemController.ShowEditForm(SelectedCampaignItemInControllerFactory);
			}
			else
			{
				Globals.Message.Show(Res.GetString("d962f66d-4519-40ab-84af-190e4fa1697f", "Please select a Sent Campaign to edit."), Res.GetString("a7c06207-7e63-4ff0-a931-cd3ad976fb6f", "Cannot Edit"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		ZController GlbCompanyCampaignItemController
		{
			get
			{
				if (fGlbCompanyCampaignItemController == null)
				{
					fGlbCompanyCampaignItemController = ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItem);
				}
				return fGlbCompanyCampaignItemController;
			}
		}

		GlbCompanyCampaignItem SelectedCampaignItemInControllerFactory
		{
			get
			{
				GlbCompanyCampaignItem campaignItem = SelectedItem;
				return (campaignItem != null) ? GlbCompanyCampaignItemController.Factory.Load<GlbCompanyCampaignItem>(campaignItem.PK) : null;
			}
		}

		ZController fGlbCompanyCampaignItemController;

		#endregion

		#region New Communication

		void NewCommunication_SingleSelectedItem()
		{
			if (SelectedItem != null)
			{
				OrgSalesCall newCommunication = (OrgSalesCall)((ZControllerInternals)CommunicationController).GetNewBusinessEntityInLocalFactory();
				if (SelectedItem.ClientOrg != null)
				{
					newCommunication.OQ_OH = SelectedItem.ClientOrg.PK;
					newCommunication.OQ_OC = SelectedItem.RecipientAsSalesEnquiry != null ? SelectedItem.RecipientAsSalesEnquiry.O1_OC_LinkedContact : SelectedItem.RecipientAsOrgContact.PK;
				}

				BulkCommunication.CreateRelationshipForNewEntity(newCommunication, SelectedItem, (message) =>
				{
					Globals.Message.ShowError(message, ResString.GetMultilingualString("79a86bd2-a602-4fad-be4d-b28259b4cb90", "Can not create relationship"));
				});
				CommunicationController.ShowFormForNewEntity(newCommunication);
			}
			else
			{
				Globals.Message.Show(Res.GetString("4a77ed82-071f-4761-a1f8-270b9c28adcd", "Please select an item to edit."), Res.GetString("e258daa0-78e8-4b7d-9274-348f6e59d0eb", "Cannot Create Communication"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		void NewCommunication_MulipleSelectedItems()
		{
			BulkCommunication bulkCommunication = new BulkCommunication(new BusinessObjectFactory(), CurrentDataItem);
			var bulkCommunicationForm = new BulkCommunicationForm(bulkCommunication);
			bulkCommunication.CommunicationCollection.CreateFromCampaignItems(FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>());
			ZFormModaliser.Show(bulkCommunicationForm, ParentForm);
		}

		void NewCommunicationButton_Click(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if (FilterStripControl.FilteredGrid.SelectedElements.Length > 1)
				{
					NewCommunication_MulipleSelectedItems();
				}
				else
				{
					NewCommunication_SingleSelectedItem();
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		ZController CommunicationController
		{
			get
			{
				if (communicationController == null)
				{
					communicationController = ZControllerFactory.Create(ControllerIDs.Communication);
				}
				return communicationController;
			}
		}
		ZController communicationController;

		#endregion

		#region Update Contacts

		void UpdateContactsButton_Click(object sender, EventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(newFactory, campaignInNewFactory);
			var selectedCampaignItems = CurrentDataItem.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(item => item.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR);
			contactsWithNDR.ContactsCollection.AddContacts(selectedCampaignItems);
			if (contactsWithNDR.ContactsCollection.Any())
			{
				var updateContactsForm = new UpdateContactsForm(contactsWithNDR);
				ZFormModaliser.Show(updateContactsForm, ParentForm);
			}
		}

		#endregion

		#region Unsubscribe Contacts

		internal void UnsubscribeButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.OrganisationControlSubscriptionPreferences.IsAllowed)
			{
				Env.Security.OrganisationControlSubscriptionPreferences.ShowError();
				return;
			}

			var selectedGlbCompanyCampaignItems = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().ToArray();

			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			var unsubscribeContactsBusinessObject = new UnsubscribeContactsBusinessObject(campaignInNewFactory);
			unsubscribeContactsBusinessObject.ContactsCollection.AddItems(selectedGlbCompanyCampaignItems);

			if (unsubscribeContactsBusinessObject.ContactsCollection.Any())
			{
				var unsubscribeContactsForm = new UnsubscribeContactsForm(unsubscribeContactsBusinessObject);
				ZFormModaliser.Show(unsubscribeContactsForm, ParentForm);
			}
		}

		#endregion

		#region Create Task

		internal void CreateTaskButton_Click(object sender, EventArgs e)
		{
			if (SelectedItem != null)
			{
				var task = SelectedItem.GetPopulatedTask();
				if (task != null)
				{
					TaskManagementController.ShowFormForNewEntity(task);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("5257fce7-0672-490b-8bbe-f542228d5560", "Please select a Sent Campaign to Create a Task for."), Res.GetString("80adc06b-0246-4ae6-ad41-6be875886d30", "Cannot Create Task"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		internal ZController TaskManagementController
		{
			get
			{
				if (fTaskManagementController == null)
				{
					fTaskManagementController = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
				}
				return fTaskManagementController;
			}
		}

		ZController fTaskManagementController;

		#endregion

		#region Refresh Entries

		internal void RefreshButton_Click(object sender, EventArgs e)
		{
			foreach (GlbCompanyCampaignItem item in CurrentDataItem.CampaignsItemsSent)
			{
				item.ReloadSafe();
				item.TrackingStatusDescriptionInfo.RefreshBinding();
				if (item.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR)
				{
					item.ReloadNotesFromDB();
				}
			}
		}

		#endregion

		#region Resend

		GlbCompanyCampaignSender campaignSender;

		internal ZController OpportunityController => opportunityController ?? (opportunityController = ZControllerFactory.Create(ControllerIDs.Opportunity));
		ZController opportunityController;

		internal void OpenOpportunity_Click(object sender, EventArgs e)
		{
			var itemsSelected = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().Where(i => i.G8_TrackingStatus == TrackingStatusCodes.Codes.OPC).ToList();
			if (!itemsSelected.Any() || itemsSelected.Count > 1)
			{
				Globals.Message.Show(Res.GetString("8C0FB588-E86F-4ECE-97F2-A34B4467CA7B", "Please select one Opp Created item to open the opportunity."), Res.GetString("E0985EBF-D5BC-411F-9A5A-054D32F56713", "Open Opportunity"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			var item = itemsSelected.First();
			var query = new ZQuery(new ZQuery(OrgOpportunitySchema.P8_OC, item.G8_RecipientID), JoinCondition.And, new ZQuery(OrgOpportunitySchema.P8_G0, item.G8_G0));

			var orgOpportunity = CurrentDataItem.Factory.LoadTop1<OrgOpportunity>(query);
			OpportunityController.ShowEditForm(orgOpportunity);
		}

		internal void ResendButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem.IsOpportunityCreationCampaign)
			{
				CreateNowOpportunityItems();
			}
			else
			{
				SendNowCampaignItems();
			}
		}

		void CreateNowOpportunityItems()
		{
			var oppQueuedItems = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().Where(i => i.G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ).ToList();
			if (!oppQueuedItems.Any())
			{
				Globals.Message.Show(Res.GetString("B4C93C51-79B6-4FBC-9BA8-DFB62AA238D7", "Please select a Opp Queued item to create now."), Res.GetString("E48F14CC-A817-4BF2-B34B-D4928D22DAD4", "Cannot create"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			var dialogResult = Globals.Message.Show(ResString.GetMultilingualString("EE245860-14DC-4AE2-B472-A8673A7EE5E6", "{0} opportunities will be created on the next service task run. Do you want to continue?", oppQueuedItems.Count),
				ResString.GetMultilingualString("53B934C0-8607-4FA5-9428-CE9B53B7A7F5", "Create now"), MessageBoxButtons.YesNo, DialogResult.Yes);

			if (dialogResult == DialogResult.Yes)
			{
				ResendCampaignItems(oppQueuedItems);
			}
		}

		void SendNowCampaignItems()
		{
			var trackingCampaignItems = FilterStripControl.FilteredGrid.SelectedElements.Cast<GlbCompanyCampaignItem>().ToList();
			if (!trackingCampaignItems.Any())
			{
				Globals.Message.Show(Res.GetString("f47037c2-bf4c-40eb-8442-970B0a44972f", "Please select a Sent Campaign to send now."), Res.GetString("9e4daa26-d377-428b-8df4-364a33e68fc3", "Cannot send"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			ResendCampaignItems(trackingCampaignItems);
		}

		void ResendCampaignItems(List<GlbCompanyCampaignItem> campaignItemsList)
		{
			campaignSender = new GlbCompanyCampaignSender(CurrentDataItem, campaignItemsList);
			SubscribeCampaignSender();

			if (CurrentDataItem.IsTouchCampaign)
			{
				if (CurrentDataItem.TouchSourceCampaignPKs == null || CurrentDataItem.TouchSourceCampaignPKs.All(s => s.IsEmpty))
				{
					CurrentDataItem.TouchSourceCampaignPKs = CurrentDataItem.TransitionRulesToThisCampaign.SelectMany(r => r.ParentHorizontalsTouches.Select(t => t.PK)).ToArray();
				}
			}

			campaignSender.CheckAndSendCampaigns();
			UnsubscribeCampaignSender();
		}

		void UnsubscribeCampaignSender()
		{
			if (campaignSender != null)
			{
				campaignSender.MessageOnCampaignSending -= CampaignTrackingControl_MessageOnCampaignSending;
				campaignSender.ShouldContinueWithSending -= CampaignTrackingControl_ShouldContinueWithResending;
				campaignSender.ContactsNotSentCampaign -= CampaignTrackingControl_ContactsNotSentCampaign;
				campaignSender.CampaignSendBegin -= BusinessEntity_CampaignSendBegin;
				campaignSender.CampaignSendEnd -= BusinessEntity_CampaignSendEnd;
				campaignSender.ItemSent -= BusinessEntity_ItemSent;
			}
		}

		void SubscribeCampaignSender()
		{
			if (campaignSender != null)
			{
				campaignSender.MessageOnCampaignSending += CampaignTrackingControl_MessageOnCampaignSending;
				campaignSender.ShouldContinueWithSending += CampaignTrackingControl_ShouldContinueWithResending;
				campaignSender.ContactsNotSentCampaign += CampaignTrackingControl_ContactsNotSentCampaign;
				campaignSender.ItemSent += BusinessEntity_ItemSent;
				campaignSender.CampaignSendBegin += BusinessEntity_CampaignSendBegin;
				campaignSender.CampaignSendEnd += BusinessEntity_CampaignSendEnd;
			}
		}

		ProgressForm SendProgressForm;

#if DEBUG
		internal ProgressForm LastSendProgressForm;
#endif

		void BusinessEntity_ItemSent(object sender, GlbCompanyCampaignSender.ItemSentEventArgs e)
		{
			if (SendProgressForm != null)
			{
				SendProgressForm.Status = Res.GetString("55627c84-919f-444c-b3c0-7bb73793cf67", "Resending campaign to contacts ({0} of {1}).", e.Sent, e.Total);
				SendProgressForm.PercentComplete = (int)((e.Sent / (decimal)e.Total) * 100m);
			}
		}

		void SendProgressForm_Cancelled(object sender, EventArgs e)
		{
			campaignSender.CancelSendingContacts();
			SendProgressForm.Close();
			SendProgressForm.Dispose();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void BusinessEntity_CampaignSendBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("fd5bf2bb-93d7-4e34-ae54-f41d1cc2b421", "Resending campaign items to selected...");
			SendProgressForm.ShowCancelButton = true;
			SendProgressForm.ShowProgressBar = true;
			SendProgressForm.Cancelled += new EventHandler(SendProgressForm_Cancelled);
#if DEBUG
			LastSendProgressForm = SendProgressForm;
#endif
			SendProgressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

		void BusinessEntity_CampaignSendEnd(object sender, EventArgs e)
		{
			CurrentDataItem.CampaignItemSchedule.ItemsDeleted = true;

			SendProgressForm.Close();
			SendProgressForm.Dispose();
		}

		#endregion

		#region Delivery Details

		void DeliveryDetailsButton_Click(object sender, EventArgs e)
		{
			if (SelectedCampaignItemInControllerFactory != null)
			{
				DeliveryDetailsPopupFormHelper helper = new DeliveryDetailsPopupFormHelper();
				helper.ShowDeliveryDetailsPopupForm(new EmailDeliveryDetailsProvider(SelectedItem), ParentForm);
			}
			else
			{
				Globals.Message.Show(Res.GetString("bca407dd-7673-4a0b-878e-e85ed54f07e3", "Please select a Sent Campaign to view it's delivery details."), Res.GetString("7614ac10-ece2-47a8-a0e6-4b0f80c629e9", "Cannot View"),
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		#endregion

		#region Event Handlers

		public void CampaignTrackingControl_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
		{
			if (e.IsError)
			{
				if (CurrentDataItem != null && CurrentDataItem.HasErrors)
				{
					((ISaveInitiator)FindForm()).ShowErrorsDialog();
				}
				else
				{
					Globals.Message.ShowError(e.Message, e.Summary);
				}
			}
			else
			{
				Globals.Message.ShowInformation(e.Message, e.Summary);
			}
		}

		public bool CampaignTrackingControl_ShouldContinueWithResending(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
		{
			ZString message = campaignToResend != null ? campaignToResend[0].ResendConfirmationMessage : ZString.Empty;
			return DialogResult.Yes == Globals.Message.Show(message, Res.GetString("61402958-a1b9-49a8-84e5-053d604d9044", "Resend Campaign"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		public void CampaignTrackingControl_ContactsNotSentCampaign(int numContactsSent, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.Load<GlbCompanyCampaign>(CurrentDataItem.PK);
			if (campaignInNewFactory != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new UpdateContactsForm(numContactsSent, contactsNotSentTo, new ContactsWithNonDeliveryReportsUpdater(newFactory, campaignInNewFactory)));
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (FilterItemModule != null)
				{
					DisposeFilterItemModuleDependentControls();
					FilterItemModule.Dispose();
					FilterItemModule = null;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void DisposeFilterItemModuleDependentControls()
		{
			Controls.Remove(salesRelationControl);
			Controls.Remove(CampaignItemContactCrossReferencesControl);
			Controls.Remove(CampaignItemContactEDocsControl);
			Controls.Remove(linkActivityUserControl);
			salesRelationControl.Dispose();
			CampaignItemContactCrossReferencesControl.Dispose();
			CampaignItemContactEDocsControl.Dispose();
			linkActivityUserControl.Dispose();
		}

		#endregion

#if DEBUG
		internal CampaignTrackingSalesRelationControl SalesRelationControlExposed => salesRelationControl;
#endif

		#region Implementation

		GlbCompanyCampaignItem SelectedItem
		{
			get
			{
				GlbCompanyCampaignItem result = null;
				if (FilterStripControl.FilteredGrid.ListManager != null)
				{
					var campaignItem = FilterStripControl.FilteredGrid.ListManager.GetCurrent();
					if (campaignItem != null)
					{
						result = (GlbCompanyCampaignItem)campaignItem;
					}
				}

				return result;
			}
		}

		new GlbCompanyCampaign CurrentDataItem
		{
			get { return (GlbCompanyCampaign)base.CurrentDataItem; }
		}

		#endregion
	}
}
