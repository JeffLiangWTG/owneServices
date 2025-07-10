using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public partial class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
	{
		readonly MenuItem reqdocRoot;

		public static class Constants
		{
			public const string RequestCustomsResendOfResponses = "Request Customs Resend of Responses";
		}

		public EntryHeaderFilterUserControl()
		{
			InitializeComponent();
			reqdocRoot = new ZMenuItem(ResString.GetMultilingualString("5C712EFC-342F-4142-B496-F150A9C5C073", $"{Constants.RequestCustomsResendOfResponses}"));
			FilteredGrid.ContextMenu.MenuItems.Add(reqdocRoot);
			FilteredGrid.ContextMenu.Popup += (object sender, EventArgs e) => RecDocHelper.RefreshReqdocList(FilteredGrid, reqdocRoot);

			ChangeGridColumnsCaption();
		}

		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			reqdocRoot = new ZMenuItem(ResString.GetMultilingualString("5C712EFC-342F-4142-B496-F150A9C5C073", $"{Constants.RequestCustomsResendOfResponses}"));
			FilteredGrid.ContextMenu.MenuItems.Add(reqdocRoot);
			FilteredGrid.ContextMenu.Popup += (object sender, EventArgs e) => RecDocHelper.RefreshReqdocList(FilteredGrid, reqdocRoot);
			ChangeGridColumnsCaption();
		}

		void ChangeGridColumnsCaption()
		{
			var referenceNumberColumnStyle = grid.GetColumnStyle(CusEntryHeaderSchema.Constants.CH_BGMReference);
			referenceNumberColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("4A413831-F312-4184-8D9E-F5218BF26D3A", "Reference Number (LRN)");

			var entryNumberColumnStyle = grid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber);
			entryNumberColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("DE68A414-F9BA-48E3-A6AB-C6F051C81276", "Entry Number (MRN)");
		}

		protected override ZFilterStrip NewZFilterStrip() => new EntryHeaderModuleStrip();

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("AC95FD99-C8CF-420A-8811-16C2E2AAF7B4", "VAT"),
					ColumnName = CusEntryHeader.Schema.ValueAddedTax,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("1721E4B0-19EE-4B98-8261-5F1067B39E22", "Duty"),
					ColumnName = CusEntryHeader.Schema.CustomsDuty,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("4A2609EB-9BF9-46CA-ACB3-D5536313B3E8", "Payment Method"),
					ColumnName = CusEntryHeader.Schema.CH_PaymentMethod,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("54741581-7D5E-4449-8092-505BC39A091D", "Packages"),
					ColumnName = CusEntryHeader.Schema.CH_Packages,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("821A5E1A-A015-46CF-B925-DC9D72C34942", "CPC"),
					ColumnName = CusEntryHeader.Schema.CustomsProcedureCode,
					IsMandatory = true,
					IsReadOnly = true,
					MaxLengthOverride = 8,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("89314B7D-7DC3-4D7D-BA74-F5A1A5C27383", "Instruction Desc.", "Procedure Instruction Description"),
					ColumnName = CusEntryHeader.Schema.CustomsProcedureInstructionDescription,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("1439855C-3860-4F6B-9D49-3D9D86E97F18", "Assessment Date"),
					ColumnName = CusEntryHeader.Schema.EntryInstructionAssessmentDate,
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("1FB5B7AB-7699-47CA-872E-935423B95CC8", "Declaration UCR"),
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryHeader.Schema.UniqueConsignmentReference,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("f55e67b2-3c7d-4538-b75c-d07ab32a04de", "Unique Consignment Reference (UCR)"),
					ColumnName = CusEntryHeader.Schema.CombinedUCREntryNumbers,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("98FE57AC-521D-4967-8D42-7101ECB929B3", "Customs Printed Release Required"),
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryHeader.Schema.CH_RelPrintInd,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_BondAcquittedDate,
					DateTimeFormat = ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_BondValidToDate,
					DateTimeFormat = ZDateTimePickerFormat.Short,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
			});

			grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
			grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 325, true);
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>
					{
						CusEntryHeader.Schema.EntryNumber,
						CusEntryHeader.Schema.DeclarationReference,
						CusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.CH_EntryStatus,
						CusEntryHeader.Schema.EntryHeaderStatusDescription,
						CusEntryHeader.Schema.CustomsProcedureCode,
						CusEntryHeader.Schema.CH_EntrySubmittedDate,
						CusEntryHeader.Schema.CH_EntryReleaseDate,
						CusEntryHeader.Schema.CH_Status,
						CusEntryHeader.Schema.MessageStatusDescription,
						CusEntryHeader.Schema.CH_MessageType,
						CusEntryHeader.Schema.CH_MessageTypeDescription,
						Schema.BranchName,
						Schema.ImporterName,
						Schema.SupplierName,
						Schema.AgentsReference,
						Schema.DateOfArrival,
						CusEntryHeader.Schema.CH_TotalPaid,
						CusEntryHeader.Schema.ValueAddedTax,
						CusEntryHeader.Schema.CustomsDuty,
						CusEntryHeader.Schema.CH_PaymentMethod,
						CusEntryHeader.Schema.CH_Packages,
						CusEntryHeader.Schema.CustomsProcedureInstructionDescription,
						CusEntryHeader.Schema.EntryInstructionAssessmentDate,
						CusEntryHeader.Schema.UniqueConsignmentReference,
						CusEntryHeader.Schema.CH_BondValidToDate,
						CusEntryHeader.Schema.CH_BondAcquittedDate,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
						CusEntryHeader.Schema.CH_RelPrintInd,
					};

					columnNamesInSortOrder = columns;
				}
				return columnNamesInSortOrder;
			}
		}
		List<string> columnNamesInSortOrder;
	}
}
