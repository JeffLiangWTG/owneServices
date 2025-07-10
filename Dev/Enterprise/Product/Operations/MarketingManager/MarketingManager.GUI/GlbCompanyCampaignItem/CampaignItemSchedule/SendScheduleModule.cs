using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class SendScheduleModule : ZFilterGridModule, IGlbCompanyCampaignItemModule
	{
		SendScheduleFilterControl FilterControl { get; set; }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItemSchedule);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SendScheduleFilterBusinessObject((GlbCompanyCampaign)Campaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			FilterControl = new SendScheduleFilterControl(GridCollection, (SendScheduleFilterBusinessObject)FilterBusinessObject);
			return FilterControl;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			GlbCompanyCampaign campaign = (GlbCompanyCampaign)Campaign;
			return new GlbCompanyCampaignItemScheduleItemsCollection(campaign);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaignItemSchedule; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CampaignManagement; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override bool IsModuleAllowAsync => false;

		protected override void OnCustomGridLoad(IBusinessObjectCollection gridCollection, PerformSearchResult searchResult)
		{
			((BusinessObjectCollection)gridCollection).Load(searchResult.Query);
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return PerformSearchResult.CustomGridLoad(factory, query);
		}

		#region Menu Item Event Handlers

		protected override void HandleViewClickCore(object sender, EventArgs e)
		{
			var scheduledItems = SelectedBusinessObjects.Cast<ScheduleCampaignItems>();

			if (!scheduledItems.Any())
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var campaign = (GlbCompanyCampaign)Campaign;
				ViewEditScheduleCampaignItems(campaign, scheduledItems.ToList(), setReadOnly: true);
			}
		}

		protected override void HandleEditClickCore(object sender, EventArgs e)
		{
			var scheduledItems = SelectedBusinessObjects.Cast<ScheduleCampaignItems>();

			if (!scheduledItems.Any())
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var campaign = (GlbCompanyCampaign)Campaign;
				ViewEditScheduleCampaignItems(campaign, scheduledItems.ToList(), setReadOnly: false);
			}
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			var scheduledItems = SelectedBusinessObjects.Cast<ScheduleCampaignItems>();

			if (!scheduledItems.Any())
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var items = scheduledItems.Where(s => s.Status == TrackingStatusCodes.Codes.QUE).ToArray();
				if (items.Any())
				{
					Requeue((GlbCompanyCampaign)Campaign, items);
					if (FilterControl != null)
					{
						FilterControl.FirePerformSearch(false);
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("3a4f46b9-ade9-45ad-b218-a82111c2d769", "Only Queued Schedules can be deleted."), Res.GetString("30ef2218-707e-43a3-8f1f-62f6620c06f2", "Delete"));
				}
			}
		}

		void ViewEditScheduleCampaignItems(GlbCompanyCampaign campaign, List<ScheduleCampaignItems> selectedItems, bool setReadOnly)
		{
			var newFactory = new BusinessObjectFactory();

			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(campaign);
			var scheduler = campaignInNewFactory.CampaignItemSchedule;
			var loader = new ScheduleItemDataLoader(scheduler);

			foreach (var schItem in selectedItems)
			{
				schItem.SetReadOnlyIncludingChildren(false);
				scheduler.SelectedScheduleItems.UnionWith(GetSelectedItemsForSchedule(campaign, loader, schItem));
			}

			var scheduleForm = new CampaignItemScheduleForm(campaignInNewFactory, scheduler.SelectedScheduleItems);
			if (setReadOnly)
			{
				scheduleForm.DisplayMode = ODisplayMode.ReadOnly;
				scheduleForm.SetReadOnlyIncludingChildren();
			}

			ZFormModaliser.Show(scheduleForm, FilterControl.ParentForm);
		}

		void Requeue(GlbCompanyCampaign campaign, ScheduleCampaignItems[] selectedItems)
		{
			var loader = new ScheduleItemDataLoader(campaign.CampaignItemSchedule);
			var selectedCampaignItems = selectedItems.SelectMany(schItem => GetSelectedItemsForSchedule(campaign, loader, schItem)).Distinct().ToArray();

			if (selectedCampaignItems.Any())
			{
				GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign, selectedCampaignItems);
				campaignSender.ShouldContinueWithSending += CampaignSender_ShouldContinueWithDeleting;
				try
				{
					campaignSender.Requeue(selectedCampaignItems);
				}
				finally
				{
					campaignSender.ShouldContinueWithSending -= CampaignSender_ShouldContinueWithDeleting;
				}
			}
		}

		IEnumerable<GlbCompanyCampaignItem> GetSelectedItemsForSchedule(GlbCompanyCampaign campaign, ScheduleItemDataLoader loader, ScheduleCampaignItems schedule)
		{
			var items = campaign.CampaignItemSchedule.SelectedScheduleItems.Cast<GlbCompanyCampaignItem>();
			return items.Where(item =>
				item.RecipientFromView != null
				&& schedule.UtcOffset == loader.OffsetFromUtcTimeZone(item.RecipientFromView.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(item.RecipientFromView.RelatedPortCodeForScheduling, schedule.ScheduleSendTimeUTC))
				&& schedule.StandardTimeZoneCode == loader.CivilianTimeZoneCodeTimeZone(item.RecipientFromView.RelatedPortCodeForScheduling, loader.GetUtcFromUnlocoTime(item.RecipientFromView.RelatedPortCodeForScheduling, schedule.ScheduleSendTimeUTC))
				&& schedule.ScheduleSendTimeUTC == item.G8_ScheduleTimeUtc);
		}

		bool CampaignSender_ShouldContinueWithDeleting(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
		{
			var message = campaignToResend != null ? campaignToResend[0].RequeueConfirmationMessage : ZString.Empty;
			return DialogResult.Yes == Globals.Message.Show(message, Res.GetString("06882fca-ce22-45e2-9c2f-f9f84ce40495", "Delete Campaign"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (FilterControl != null && !FilterControl.IsDisposed)
			{
				FilterControl.Dispose();
				FilterControl = null;
			}
			base.Dispose(isDisposing);
		}

		#endregion

		#region IGlbCompanyCampaignItemModule Members

		public MasterFiles.Integration.IGlbCompanyCampaign Campaign
		{
			get;
			set;
		}

		#endregion

	}
}
