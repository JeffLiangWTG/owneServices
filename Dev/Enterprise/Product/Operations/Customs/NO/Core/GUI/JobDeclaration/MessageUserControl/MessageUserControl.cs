using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public partial class MessageUserControl : Customs.GUI.ImportMessageUserControl
	{
		public MessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
			SetupEntryLineColumns();
		}

		protected override string MessagesUserControlBindingPath => "CustomsEntryHeaders.Messages";

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.PackagesCount);
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.CH_MessageTypeDescription);

			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType).CaptionResourceString = Res.GetData("8C911D6F-CD7F-BEBB-4C1B-1C09B5C4D818", "IMP/EXP");
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType).Width = 55;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference).CaptionResourceString = Res.GetData("783F3532-3F0B-8091-4176-A3ADB99D3EEE", "Ref. No.");
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference).Width = 65;

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F7DCE699-7D6B-B1AB-4DCE-5DC5F9B089E9", "Type"),
				ColumnName = CusEntryHeader.Schema.Style,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("CCB592FF-24D0-7EA8-4247-DD5EA7A001EF", "Description"),
				ColumnName = CusEntryHeader.Schema.Description,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7F33290E-8BC8-39BC-4DC3-FD38053A29AC", "Procedure"),
				ColumnName = CusEntryHeader.Schema.Procedure,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A9DB614E-065A-97BC-41D2-F10818B81808", "Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("43B84D4E-4E07-758D-43A1-C92F5EC1A533", "Customs status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("C00E0C09-F759-B880-4485-12E80F536CBE", "Customs status description"),
				ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("9BECA9A6-31D6-5BAE-4F23-CEEAD7198B41", "Phase Status"),
				ColumnName = CusEntryHeader.Schema.CH_PhaseStatus,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("33738732-1834-2393-4395-A48D195C167C", "Phase Status Description"),
				ColumnName = CusEntryHeader.Schema.PhaseStatusDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7904D6BC-9137-4CAD-445A-EE1C62A84B7B", "Approval ID"),
				ColumnName = CusEntryHeader.Schema.MovementReferenceNumber,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("0A4E6031-DC59-6FA3-4DEB-8C795BF026C7", "CIF"),
				ColumnName = CusEntryHeader.Schema.CIFAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("FE39CD0F-13C6-9B80-46C7-60655937F1C5", "Pay"),
				ColumnName = CusEntryHeader.Schema.CH_PaymentMethod,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("C690B986-ED25-A498-4DA4-FB92248A3B86", "Customs Duty"),
				ColumnName = CusEntryHeader.Schema.TotalDutyAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("E3E4CA5A-FFDA-98BF-43BC-961A60ADDF15", "Excise duties"),
				ColumnName = CusEntryHeader.Schema.ExciseDutyAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("146F8C25-932C-BCA4-4F7B-40BCE08D902F", "VAT"),
				ColumnName = CusEntryHeader.Schema.VatAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("93e86af3-8788-4069-a5eb-fa663c362d07", "Total"),
				ColumnName = CusEntryHeader.Schema.TotalAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ReOrderColumns(EntryHeaderColumnsInOrder);
		}

		void SetupEntryLineColumns()
		{
			EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_LineNumber).CaptionResourceString = Res.GetData("99c7cb05-c25f-4779-bc50-42af208619bd", "Line No.");
			EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_LineNumber).Width = 80;

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("978DBC74-4D7D-EEA2-4368-3EAB2B721767", "Tariff No"),
				ColumnName = nameof(CusEntryLine.Schema.CL_AdValoremTariff),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("14588B95-A6E8-1EA8-432F-5D69BBD738EC", "Ctry of Origin"),
				ColumnName = nameof(CusEntryLine.Schema.CtryOfOrigin),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("663AFAD1-80C7-5A9D-4F70-EAE90E92CBB6", "Procedure"),
				ColumnName = nameof(CusEntryLine.Schema.Procedure),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("2BB30AF4-B1B1-1BBA-415F-5A06920F077B", "Pref.code"),
				ColumnName = nameof(CusEntryLine.Schema.Preference),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F5B8BB41-0161-2B80-4369-200AF1FE1832", "Reduced customs flag"),
				ColumnName = nameof(CusEntryLine.Schema.ReducedCustomsFlag),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("9C7A573D-5971-E9B5-43BB-93E489B46AC7", "Customs Duty"),
				ColumnName = nameof(CusEntryLine.Schema.DutyAmount),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("f5b7dfd7-3bf5-4d95-b061-af8edec01e07", "Excise duties"),
				ColumnName = nameof(CusEntryLine.Schema.ExciseDutyAmount),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("fa9c3717-a832-47be-b996-9a0ff1ea88cf", "VAT amount"),
				ColumnName = nameof(CusEntryLine.Schema.VatAmount),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6D48227F-C3A9-1EAC-474C-C04CF08CF80B", "VAT code"),
				ColumnName = nameof(CusEntryLine.Schema.VatCode),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("18292459-b506-4a73-abf7-a462d59d2eac", "Total"),
				ColumnName = nameof(CusEntryLine.Schema.TotalAmount),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
			});

			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("50394D13-3FBF-4B89-4B51-7EEBEC118FAA", "Valuation Method"),
				ColumnName = nameof(CusEntryLine.Schema.ValuationCode),
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97)
			});

			EntryLineGrid.ReOrderColumns(EntryLineColumnsInOrder);
		}

		string[] EntryHeaderColumnsInOrder
		{
			get
			{
				if (entryHeaderColumnsInOrder == null)
				{
					entryHeaderColumnsInOrder = new[]
					{
						CusEntryHeader.Schema.EntryNumber,
						CusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.CH_MessageType,
						CusEntryHeader.Schema.Style,
						CusEntryHeader.Schema.Description,
						CusEntryHeader.Schema.Procedure,
						CusEntryHeader.Schema.CH_EntryReleaseDate,
						CusEntryHeader.Schema.CH_EntryStatus,
						CusEntryHeader.Schema.EntryHeaderStatusDescription,
						CusEntryHeader.Schema.CH_PhaseStatus,
						CusEntryHeader.Schema.PhaseStatusDescription,
						CusEntryHeader.Schema.MovementReferenceNumber,
						CusEntryHeader.Schema.CH_PaymentMethod,
						CusEntryHeader.Schema.CIFAmount,
						CusEntryHeader.Schema.TotalDutyAmount,
						CusEntryHeader.Schema.ExciseDutyAmount,
						CusEntryHeader.Schema.VatAmount,
						CusEntryHeader.Schema.TotalAmount,
					};
				}
				return entryHeaderColumnsInOrder;
			}
		}
		string[] entryHeaderColumnsInOrder;

		string[] EntryLineColumnsInOrder
		{
			get
			{
				if (entryLineColumnsInOrder == null)
				{
					entryLineColumnsInOrder = new[]
					{
						CusEntryLine.Schema.CL_LineNumber,
						CusEntryLine.Schema.CL_AdValoremTariff,
						CusEntryLine.Schema.CtryOfOrigin,
						CusEntryLine.Schema.Procedure,
						CusEntryLine.Schema.Preference,
						CusEntryLine.Schema.ReducedCustomsFlag,
						CusEntryLine.Schema.DutyAmount,
						CusEntryLine.Schema.ExciseDutyAmount,
						CusEntryLine.Schema.VatAmount,
						CusEntryLine.Schema.VatCode,
						CusEntryLine.Schema.TotalAmount,
						CusEntryLine.Schema.ValuationCode,
					};
				}
				return entryLineColumnsInOrder;
			}
		}
		string[] entryLineColumnsInOrder;
	}
}
