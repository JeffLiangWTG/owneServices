using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Module
{
	public class JobRailSailingFilterControl : JobSailingFilterControl
	{
		/// <summary>
		/// Only for VS designer.
		/// </summary>
		public JobRailSailingFilterControl()
		{
		}

		public JobRailSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_NKVessel).Caption = Res.GetString("JobSailingFilterControl|JourneyName", "Journey Name");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_VoyageFlight).Caption = Res.GetString("JobSailingFilterControl|JourneyNo", "Journey No.");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_E_DEP).CaptionResourceString = Res.GetData("JobRailSailingFilterControl|059ce742-d07a-4223-876e-191c56d33d4e", "ETD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_E_ARV).CaptionResourceString = Res.GetData("JobRailSailingFilterControl|7055f071-824d-4540-b892-966f8e5cc879", "ETA");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_A_DEP).CaptionResourceString = Res.GetData("JobRailSailingFilterControl|0f94d5ff-f54b-4923-b177-cf820bdc6aaa", "ATD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_A_ARV).CaptionResourceString = Res.GetData("JobRailSailingFilterControl|e68c8ebf-0fdf-4f33-b3a1-d7e4451d2ded", "ATA");

			base.OnLoad(e);

			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_E_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_S_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_A_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_E_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_S_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_A_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_E_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_S_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_A_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
		}
	}
}
