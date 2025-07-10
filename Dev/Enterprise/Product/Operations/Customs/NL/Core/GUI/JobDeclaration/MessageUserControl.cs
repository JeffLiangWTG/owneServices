using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.GUI;

public partial class MessageUserControl : EU.GUI.MessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
	}

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();
		SetUpEntryHeaderColumns();
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		SetUpAmendmentReasonVisibility();
	}

	void SetUpAmendmentReasonVisibility()
	{
		AmendmentReasonTextBox.Visible = false;
		AmendmentReasonLabel.Visible = false;
	}
	void SetUpEntryHeaderColumns()
	{
		var columnStyle_IssueDate = EntriesBoundGrid.GetColumnStyle(nameof(CusEntryHeader.CusEntryNumber) + "+" + nameof(CusEntryNumber.CE_IssueDate));
		EntriesBoundGrid.ColumnStyles.Remove(columnStyle_IssueDate);

		var columnStyle_CusEntryNumberIssueDate = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CusEntryNumberIssueDate);
		EntriesBoundGrid.ColumnStyles.Remove(columnStyle_CusEntryNumberIssueDate);
		EntriesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
		{
			ColumnName = CusEntryHeader.Schema.CusEntryNumberIssueDate,
			IsMandatory = false,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			DateTimeFormat = ZDateTimePickerFormat.Short
		});

		var columnStyle_ExpiryDate = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumberExpiryDate);
		EntriesBoundGrid.ColumnStyles.Remove(columnStyle_ExpiryDate);
		EntriesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("419C942E-F717-4E87-B97C-3511EB5AB12C", "Expiry Date"),
			ColumnName = CusEntryHeader.Schema.MovementReferenceNumberExpiryDate,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long
		});

		var columnStyle_EntrySubmittedDate = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntrySubmittedDate);
		columnStyle_EntrySubmittedDate.CaptionResourceString = Res.GetData("28B12060-903C-4C5A-97DB-F118DC7E2FC3", "Issue Date");
		columnStyle_EntrySubmittedDate.IsVisible = true;
		EntriesBoundGrid.ColumnStyles.Remove(columnStyle_EntrySubmittedDate);
		EntriesBoundGrid.ColumnStyles.Add(columnStyle_EntrySubmittedDate);

		var columnStyle_MovementReferenceNumber = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumber);
		EntriesBoundGrid.ColumnStyles.Remove(columnStyle_MovementReferenceNumber);

		if (JobDeclaration.IsExport)
		{
			var columnStyle_Vat = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.VAT);
			var columnStyle_Duty = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.Duty);
			EntriesBoundGrid.ColumnStyles.Remove(columnStyle_Duty);
			EntriesBoundGrid.ColumnStyles.Remove(columnStyle_Vat);

			var columnStyle_ExitDate = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitDate);
			EntriesBoundGrid.ColumnStyles.Remove(columnStyle_ExitDate);
			EntriesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_ExitDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long
			});

			var columnStyle_PhaseStatus = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_PhaseStatus);
			EntriesBoundGrid.ColumnStyles.Remove(columnStyle_PhaseStatus);
			EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_PhaseStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			var columnStyle_PhaseStatusDescription = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_PhaseStatusDescription);
			EntriesBoundGrid.ColumnStyles.Remove(columnStyle_PhaseStatusDescription);
			EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_PhaseStatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			if (JobDeclaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Any(x => x.HasRelatedExitControl))
			{
				var columnStyle_ExitedStatus = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus);
				if (columnStyle_ExitedStatus != null)
				{
					EntriesBoundGrid.ColumnStyles.Remove(columnStyle_ExitedStatus);
				}
				EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("{596FA5E5-AE94-42D8-A0D3-FF7AEC0C558C}", "Exit Status"),
					ColumnName = CusEntryHeader.Schema.CH_ExitedStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				});
			}
			else
			{
				var columnStyle_ExitedStatus = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus);
				if (columnStyle_ExitedStatus != null)
				{
					EntriesBoundGrid.ColumnStyles.Remove(columnStyle_ExitedStatus);
				}
			}
			EntriesBoundGrid.ReOrderColumns(orderedColumnsExport);
		}
		else
		{
			EntriesBoundGrid.ReOrderColumns(orderedColumnsImport);
		}
	}

	protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

	readonly string[] orderedColumnsImport =
	{
		CusEntryHeader.Schema.EntryNumber,
		CusEntryHeader.Schema.CH_BGMReference,
		CusEntryHeader.Schema.PackagesCount,
		CusEntryHeader.Schema.Duty,
		CusEntryHeader.Schema.VAT,
		CusEntryHeader.Schema.CH_EntrySubmittedDate,
		CusEntryHeader.Schema.CusEntryNumberIssueDate,
		CusEntryHeader.Schema.MovementReferenceNumberExpiryDate,
		CusEntryHeader.Schema.CH_EntryReleaseDate,
		CusEntryHeader.Schema.CH_MessageType,
		CusEntryHeader.Schema.CH_MessageTypeDescription,
		CusEntryHeader.Schema.EntryHeaderStatusDescription,
		CusEntryHeader.Schema.DeclarationUCR,
		CusEntryHeader.Schema.EntryTypeFriendlyName,
		CusEntryHeader.Schema.CH_TotalPaid
	};

	readonly string[] orderedColumnsExport =
	{
		CusEntryHeader.Schema.EntryNumber,
		CusEntryHeader.Schema.CH_BGMReference,
		CusEntryHeader.Schema.PackagesCount,
		CusEntryHeader.Schema.MovementReferenceNumberExpiryDate,
		CusEntryHeader.Schema.EntryTypeFriendlyName,
		CusEntryHeader.Schema.EntryHeaderStatusDescription,
		CusEntryHeader.Schema.CH_MessageType,
		CusEntryHeader.Schema.CH_MessageTypeDescription,
		CusEntryHeader.Schema.CH_PhaseStatus,
		CusEntryHeader.Schema.CH_PhaseStatusDescription,
		CusEntryHeader.Schema.CH_EntrySubmittedDate,
		CusEntryHeader.Schema.CusEntryNumberIssueDate,
		CusEntryHeader.Schema.CH_EntryReleaseDate,
		CusEntryHeader.Schema.CH_ExitDate,
		CusEntryHeader.Schema.CH_ExitedStatus,
		CusEntryHeader.Schema.DeclarationUCR
	};

	protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EntryLineAdditionalDataUserControl();
}
