using System;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsMessageUserControl : Customs.GUI.ImportMessageUserControl
	{
		public delegate void ReprocessMessage();
		public ReprocessMessage OnReprocessMessage;

		public CustomsMessageUserControl()
			: base()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
			AddContextMenuItems();
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(CustomsMessagesTabUserControl);

		void AddContextMenuItems()
		{
			EntriesBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|SetEntryActive", "Set Entry 'Active'"), new EventHandler(SetEntryActiveMenu_Click)));
			EntryLineGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|ModifyInvoiceLineCodes", "Modify Invoice Line Codes"), new EventHandler(ModifyLinesMenu_Click)));
		}

		void SetEntryActiveMenu_Click(object sender, EventArgs e)
		{
			var entriesGrid = EntriesBoundGrid;
			if (entriesGrid.ListManager.Position > -1)
			{
				var entryHeaderSelected = (CusEntryHeader)entriesGrid.ListManager.GetCurrent();
				if (entryHeaderSelected.CH_IsActive)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|SetEntryActive_EntryIsAlreadyActive",
						"Cannot Activate - This Entry is already Active."));
				}
				else
				{
					var declaration = (JobDeclaration)JobDeclaration;
					var entryHeaderCurrentlyActive = declaration.CusEntryHeader;
					if (entryHeaderCurrentlyActive.CH_MessageType != entryHeaderSelected.CH_MessageType)
					{
						Globals.Message.ShowWarning(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|SetEntryActive_NotCurrentlyActiveType",
							"Cannot Activate - You can only activate an Entry of the currently active Type. ({0}-{1})",
							entryHeaderCurrentlyActive.CH_MessageType, entryHeaderCurrentlyActive.CH_MessageTypeDescription));
					}
					else
					{
						entryHeaderSelected.CH_IsActive = true;
						entryHeaderCurrentlyActive.CH_IsActive = false;
						entryHeaderSelected.SetDeclarationStatusesWhenSetToCurrent(declaration);
						Globals.Message.ShowInformation(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|SetEntryActive_NeedToGenerateEntry",
							"Entry Activated. You may need to Generate (Merge) this Entry to update the Entry Lines."));
					}
				}
			}
			else
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|SetEntryActive_NeedSelectAnEntry",
					"Cannot Activate - Please select an Entry first."));
			}
		}

		void ModifyLinesMenu_Click(object sender, EventArgs e)
		{
			if (EntryLineGrid.ListManager.Position > -1)
			{
				var entryLine = (CusEntryLine)base.EntryLineGrid.ListManager.GetCurrent();
				if (entryLine.RandomLine == null)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|ModifyLines_NeedSelectAnEntryLineWithInvoiceLines",
						"Please select an Entry Line with Invoice Lines attached."));
				}
				else
				{
					var syncroniser = new CusEntryLineSyncroniser(entryLine);
					var form = new CusEntryLineSyncrhoniserForm(syncroniser);
					ZFormModaliser.Show(form, FindForm());
				}
			}
			else
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("Enterprise.Customs.NZ.GUI.Declaration.CustomsMessageUserControl|ModifyLines_NeedSelectAnEntryLine",
					"Please select an Entry Line."));
			}
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.PackagesCount);
			var bgmReferenceColumnStyle = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference);
			bgmReferenceColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			EntriesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("FF55C23E-134B-4EDE-AA20-2A3EF3A95F76", "Created"),
				ColumnName = CusEntryHeader.Schema.CH_RecordAdded,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("71380DB5-EDBE-4124-AF6E-AA6C1E2F368F", "EDI Transmit Date"),
				ColumnName = CusEntryHeader.Schema.CH_EDITransmitDate,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("2C15D278-E265-4D2F-87FE-29F93753230D", "Active"),
				ColumnName = CusEntryHeader.Schema.CH_IsActive,
				IsMandatory = true,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52)
			});

			var messageTypeColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType);
			if (messageTypeColumn != null)
			{
				messageTypeColumn.CaptionResourceString = Res.GetData("479F2FED-D88E-4AAF-B190-77B31233C3C8", "Entry Type");
				messageTypeColumn.IsVisible = false;
				messageTypeColumn.GroupName = Res.GetData("0F808B84-4C77-49E0-8D5A-8B6455A966AE", "Entry Type");
				messageTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			}

			var messageTypeDescColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageTypeDescription);
			if (messageTypeDescColumn != null)
			{
				messageTypeDescColumn.CaptionResourceString = Res.GetData("7C8E113D-C229-4761-9BB3-7624F5409FB9", "Entry Type Description");
				messageTypeDescColumn.IsVisible = false;
				messageTypeDescColumn.GroupName = Res.GetData("0F808B84-4C77-49E0-8D5A-8B6455A966AE", "Entry Type");
				messageTypeDescColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			}

			EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("463833E8-E0A3-43C6-8F12-04A6400DB592", "Entry Status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				IsVisible = false,
				GroupName = Res.GetData("10B143BC-615A-4845-8E27-8DA0AC9C48E1", "Entry Status"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("AE15DA0F-ADCA-45AC-BD10-A9E4E0117A62", "Entry Status Description"),
				ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
				IsVisible = false,
				GroupName = Res.GetData("10B143BC-615A-4845-8E27-8DA0AC9C48E1", "Entry Status"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("BE75CEAE-D75B-4704-86CD-1B8F532CBFA8", "Total Duty"),
				ColumnName = CusEntryHeader.Schema.DutyTotalInMergedLines,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("7D13A29A-DD69-4A40-8278-5B238BF53FF8", "GST Amount"),
				ColumnName = CusEntryHeader.Schema.GSTAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("C73F6510-CECB-4C80-91AA-B37403715A48", "Levies"),
				ColumnName = CusEntryHeader.Schema.TotalMisc,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("1CEB3A13-56D7-48A1-84C0-3B7D0BFE9053", "Entry Fee"),
				ColumnName = CusEntryHeader.Schema.TotalEntryFeeAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("84A9DEBD-8AD6-41B4-8CCC-7FF66D2FB401", "Amount Payable"),
				ColumnName = CusEntryHeader.Schema.TotalAmountPayableIncludingEntryFee,
				IsMandatory = true,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("55266C98-D492-4441-86BF-916EDE0427B0", "Amount Returned"),
				ColumnName = CusEntryHeader.Schema.CH_TotalAmountReturned,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112)
			});
		}
	}
}
