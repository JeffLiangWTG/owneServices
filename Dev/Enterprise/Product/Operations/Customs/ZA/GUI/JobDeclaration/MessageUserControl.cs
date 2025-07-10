using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class MessageUserControl : Customs.GUI.ImportMessageUserControl
	{
		public MessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();

			reqdocRoot = new ZMenuItem("Request Customs Resend of Responses");
			var rsqMessagesQuery = new ZQuery(EDIMessageSchema.EM_MessageType, SARSEDIMessage.MessageTypes.CUSRES_REQDOC);
			rsqMessagesQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";

			regenLRN = new ZMenuItem("Regenerate new LRN");
			regenLRN.Click += RegenLRN_Click;

			EntriesBoundGrid.ContextMenu.MenuItems.Add(reqdocRoot);
			EntriesBoundGrid.ContextMenu.Popup += (object sender, EventArgs e) =>
			{
				RecDocHelper.RefreshReqdocList(EntriesBoundGrid, reqdocRoot);
				SetMenusEntryOnPopup();
			};

			EntriesBoundGrid.ContextMenu.MenuItems.Add(regenLRN);

			InitializeGridLayout();
			ExtendedInfoGroupBox.Visible = false;
		}

		void RegenLRN_Click(object sender, EventArgs e)
		{
			if (SelectedSingleEntry != null)
			{
				if (SelectedSingleEntry.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected)
				{
					SelectedSingleEntry.NeedsNewBGMReference = true;
					Globals.Message.Show("A new LRN will be generated when you save this form.");
					SelectedSingleEntry.HasChanges = true;
				}
				else
				{
					SelectedSingleEntry.NeedsNewBGMReference = false;
					Globals.Message.Show("A new LRN cannot be generated.");
				}
			}
		}

		public void SetMenusEntryOnPopup()
		{
			var clickedEntry = (CusEntryHeader)(EntriesBoundGrid.GetFirstSelectedRow() ?? EntriesBoundGrid.List?.Cast<BusinessObject>().FirstOrDefault());
			SelectedSingleEntry = clickedEntry;
		}

		#region Override Properties

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (Declaration != null)
			{
				Declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(this, null);
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				reqdocRoot.Visible = Declaration != null && !Declaration.IsImportByExternalBroker;
			}
		}

		public CusEntryHeader SelectedSingleEntry { get; set; }

		public JobDeclaration Declaration
		{
			get { return CurrentDataItem as JobDeclaration; }
		}

		protected override bool SupportWarehouseTransactionStatusColumns
		{
			get { return true; }
		}

		protected override Customs.GUI.BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			var entry = supporter as CusEntryHeader;
			if (entry == null)
			{
				ErrorReporter.ReportOnce("For ZA, supporter must be CusEntryHeader");
				return null;
			}
			else
			{
				return new Customs.GUI.EntryBondedWarehouseOperationDeterminer(entry);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.ReOrderColumns(EntryHeaderGridColumnNamesInsortOrder);
			}
		}

		string[] EntryHeaderGridColumnNamesInsortOrder => entryHeaderColumnNamesInSortOrder ?? (entryHeaderColumnNamesInSortOrder = new string[] { CusEntryHeader.Schema.CustomsProcedureCode, CusEntryHeader.Schema.CustomsProcedureInstructionDescription });
		string[] entryHeaderColumnNamesInSortOrder;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Declaration != null)
				{
					Declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		internal void RefreshReqdocList() => RecDocHelper.RefreshReqdocList(EntriesBoundGrid, reqdocRoot);

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var isImport = this.JobDeclaration?.IsImport ?? false;
			this.EntryLineAdditionalDataUserControl.ChangeControlsVisibility(isImport);

			var rooTypeColumnStyle = EntryLineGrid.GetColumnStyle("CalcPreference");
			if (rooTypeColumnStyle != null)
			{
				if (isImport)
				{
					rooTypeColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("28224A1E-6338-4503-AD92-FB20BEDFD420", "Preference");
				}
				else
				{
					rooTypeColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0C12DF65-3F46-409A-B597-351767F3571A", "ROO Type");
				}
			}
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.PackagesCount);
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.EntryNumber);

			var bgmReferenceColumnStyle = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference);
			bgmReferenceColumnStyle.CaptionResourceString = Res.GetData("8630ADA4-4802-464E-A78E-E4759A8C8715", "LRN");

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("10D9B8F9-A82C-4266-8613-C572E92BC91F", "VAT"),
				ColumnName = CusEntryHeader.Schema.ValueAddedTax,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(46)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("369C7936-551C-4DFB-87AE-B1D9460ADF2F", "Duty"),
				ColumnName = CusEntryHeader.Schema.CustomsDuty,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("FF02CA03-2365-4594-A574-1D475689C918", "Payment Method"),
				ColumnName = CusEntryHeader.Schema.CH_PaymentMethod,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("FFDEC47F-9599-4A2A-B442-65B06162A6D4", "Packages"),
				ColumnName = CusEntryHeader.Schema.CH_Packages,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("2E3D3679-65E0-4A46-AD52-5A5F5FC19DE0", "Entry Number"),
				ColumnName = CusEntryHeader.Schema.CH_EntryNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("884465BE-EBFB-4407-959B-A1BCD2DFC3B6", "Total Entries"),
				ColumnName = CusEntryHeader.Schema.CH_TotalEntries,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("45a1d5bd-f865-4a87-ab7f-1b0f6f05d439", "Entry Status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6bdde5f4-15f4-4916-bf94-d4bfc7ecafe5", "CPC"),
				ColumnName = CusEntryHeader.Schema.CustomsProcedureCode,
				IsMandatory = true,
				IsReadOnly = true,
				MaxLengthOverride = 8,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BEFC10F4-43C2-4503-AE4F-7DC1EFE513B5", "Procedure Instruction Desc."),
				ColumnName = CusEntryHeader.Schema.CustomsProcedureInstructionDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("e12d4c19-9dda-4c71-a60f-ddac06d0103a", "Release Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("81cc2903-8593-45ea-bd5f-bfc73fe19083", "MRN"),
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.MovementReferenceNumber,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("3bce0ad3-e485-4a4a-8a4c-b9fa042d05ea", "Declaration UCR"),
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.UniqueConsignmentReference,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6198f1c7-99c7-4313-8ae0-210c10b9b9f1", "Entry Submitted Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("3334efa6-b854-4d4e-8c3c-5a2471cd13a4", "Entry Status Desc."),
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("89d82247-bead-471d-8389-8acb7408c496", "Message Status"),
				ColumnName = CusEntryHeader.Schema.CH_Status,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7d5c1297-1769-42e5-b260-1d9016b4e6a4", "Message Status Desc."),
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.MessageStatusDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("d2181e67-6e41-4b23-8980-f7f9420dfcaa", "Acquit by Date"),
				ColumnName = CusEntryHeader.Schema.CH_BondValidToDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("f67a09ea-d61a-419f-877e-44090da6ebc8", "Acquittal Date"),
				ColumnName = CusEntryHeader.Schema.CH_BondAcquittedDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("8FC1D06E-9704-43C6-8A28-6B47F7687212", "Customs Printed Release Required"),
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = CusEntryHeader.Schema.CH_RelPrintInd,
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});
		}

		readonly MenuItem reqdocRoot;
		readonly MenuItem regenLRN;

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);
	}
}
