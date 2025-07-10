using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TouchScheduleUserControl : ZUserControl
	{
		public TouchScheduleUserControl()
		{
			InitializeComponent();
		}

		public TouchScheduleUserControl(bool isHRCampaign) : this()
		{
			IsHRCampaign = isHRCampaign;
		}

		readonly bool IsHRCampaign;

		GlbCompanyCampaignSendSettings Settings
		{
			get { return (GlbCompanyCampaignSendSettings)CurrentDataItem; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateControlVisibility();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var settings = Settings;
			if (settings != null)
			{
				settings.GSC_ScheduleTypeInfo.ValueChanged -= ScheduleTypeInfo_ValueChanged;
				settings.IsUseCurrentTimeInfo.ValueChanged -= ScheduleTypeInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var settings = Settings;
			if (settings != null)
			{
				settings.GSC_ScheduleTypeInfo.ValueChanged += ScheduleTypeInfo_ValueChanged;
				settings.IsUseCurrentTimeInfo.ValueChanged += ScheduleTypeInfo_ValueChanged;
				UpdateControlVisibility();
			}
		}

		void ScheduleTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlVisibility();
		}

		void UpdateControlVisibility()
		{
			SuspendLayout();
			try
			{
				var settings = Settings;
				if (settings != null)
				{
					groupBoxOptions.Visible = settings.IsDelayed || settings.IsFixedDate || settings.IsBatchSchedule;
					panelDaysOffset.Visible = settings.IsImmediate || settings.IsDelayed;

					var isUsingCurrentTime = settings.IsUseCurrentTime && settings.IsDelayed;

					ScheduleSendTimeDateEdit.Visible = !settings.IsBatchSchedule && !isUsingCurrentTime;
					ScheduleSendTimeDateEdit.DateTimeFormat = settings.IsDelayed ? ZDateTimePickerFormat.Time : ZDateTimePickerFormat.Long;
					zNumericUpDownHoursOffset.Visible = !settings.IsBatchSchedule && isUsingCurrentTime;

					checkBoxUseCurrentTime.Visible = settings.IsDelayed;
					nextScheduledPrintRunTimeLocalDateEdit.Visible = settings.IsBatchSchedule;
					panelScheduleDateTime.Visible = settings.IsDelayed || settings.IsFixedDate || settings.IsBatchSchedule;
					panelZones.Visible = (settings.IsDelayed && !settings.IsUseCurrentTime) || settings.IsFixedDate || settings.IsBatchSchedule;
					groupBoxContactLimitPerOrganization.Visible = settings.IsBatchSchedule && !IsHRCampaign;
					recurrenceControl.Visible = settings.IsBatchSchedule;
					panelBatchControlRules.Visible = settings.IsBatchSchedule;
					IsContactLimitPerOrganizationEachBatchUsedCheckBox.Visible = !IsHRCampaign;
					GSC_ContactLimitPerOrganizationEachBatchCalcEdit.Visible = !IsHRCampaign;
				}
				else
				{
					HideControls();
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void HideControls()
		{
			radioBatchRecurrencePattern.Visible = false;
			groupBoxOptions.Visible = false;
			panelDaysOffset.Visible = false;
			checkBoxUseCurrentTime.Visible = false;
			panelScheduleDateTime.Visible = false;
			panelZones.Visible = false;
			groupBoxContactLimitPerOrganization.Visible = false;
			recurrenceControl.Visible = false;
			panelBatchControlRules.Visible = false;
		}
	}
}
