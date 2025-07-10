using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.Module
{
	public class JobSeaSailingFilterControl : JobSailingFilterControl
	{
		/// <summary>
		/// Only for VS designer.
		/// </summary>
		public JobSeaSailingFilterControl()
		{
		}

		public JobSeaSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		protected override bool HideStowPlanColumn { get { return false; } }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_DepotCutOff].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_DepotReceivalCommences].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_DepotAvailabilityDate].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_DepotStorageDate].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_E_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_E_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_A_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_A_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_E_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_S_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_A_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JA_S_DEP].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			((ZDateEditColumnStyle)FilteredGrid.Columns[JobSailing.Schema.JX_JB_S_ARV].ColumnStyle).DateTimeFormat = ZDateTimePickerFormat.Short;
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_E_DEP).CaptionResourceString = Res.GetData("JobSeaSailingFilterControl|059ce742-d07a-4223-876e-191c56d33d4e", "Load Port ETD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_E_ARV).CaptionResourceString = Res.GetData("JobSeaSailingFilterControl|7055f071-824d-4540-b892-966f8e5cc879", "Disch. Port ETA");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_A_DEP).CaptionResourceString = Res.GetData("JobSeaSailingFilterControl|0f94d5ff-f54b-4923-b177-cf820bdc6aaa", "Load Port ATD");
			FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_A_ARV).CaptionResourceString = Res.GetData("JobSeaSailingFilterControl|e68c8ebf-0fdf-4f33-b3a1-d7e4451d2ded", "Disch. Port ATA");
		}
	}
}
