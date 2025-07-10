using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DuplicationModelDetailUserControl : ZUserControl
	{
		readonly DuplicationModelDetailGroup modelDetailGroup;

		public DuplicationModelDetailUserControl(DuplicationModelDetailGroup modelDetailGroup, bool withConfidence)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				this.modelDetailGroup = modelDetailGroup;

				if (withConfidence)
				{
					ModelDetailConfidenceLabel.ForeColor = DeduplicationResultDetailHelper.GetConfidenceForeColor(this.modelDetailGroup.ConfidenceRating);
					ModelDetailScoreToolTip.SetToolTip(ModelDetailConfidenceLabel, this.modelDetailGroup.Score);
					SetupModelDetailGrid(this.modelDetailGroup.CandidateModels.Count, this.modelDetailGroup.CandidateColumns);
					BindingSource.SetBindingMember(ModelDetailGrid, nameof(this.modelDetailGroup.CandidateModels));
				}
				else
				{
					ModelDetailConfidenceLabel.Visible = false;
					SetupModelDetailGrid(this.modelDetailGroup.MasterModels.Count, this.modelDetailGroup.MasterColumns);
					BindingSource.SetBindingMember(ModelDetailGrid, nameof(this.modelDetailGroup.MasterModels));
				}

				SetDataBinding(this.modelDetailGroup, string.Empty);
			}

#if !WINZOR
			ModelDetailGrid.MouseWheel += (sender, args) =>
			{
				var scrollChangeStep = 75;
				var zGrid = sender as ZGrid;
				var candidateInformationControl = zGrid.GetParent<MasterCandidateInformationControl>();
				if (candidateInformationControl.Parent is ScrollableControl scrollableControl && scrollableControl.VerticalScroll.Visible)
				{
					SetOuterContainerScroll(scrollableControl, scrollChangeStep, args.Delta);
				}
				else
				{
					var mainContentTableLayoutContainer = ModelDetailGrid.GetTopLevelNonParentedControl().Controls.Find("mainContentTableLayoutContainer", true)[0];
					if (mainContentTableLayoutContainer is ScrollableControl mainScrollableControl)
					{
						SetOuterContainerScroll(mainScrollableControl, SystemInformation.VerticalScrollBarThumbHeight, args.Delta);
					}
				}

				base.OnMouseWheel(args);
			};
#endif
		}

		public int PreferredHeight { get; private set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (modelDetailGroup is null)
			{
				return;
			}

			base.SetDataBinding(modelDetailGroup, string.Empty);
		}

#if !WINZOR
		void SetOuterContainerScroll(ScrollableControl outerScrollableControl, int scrollChangeStep, int delta)
		{
			var minimum = outerScrollableControl.VerticalScroll.Minimum;
			var maximum = outerScrollableControl.VerticalScroll.Maximum;
			if (delta > 0)
			{
				if (minimum <= (outerScrollableControl.VerticalScroll.Value - scrollChangeStep))
				{
					outerScrollableControl.VerticalScroll.Value -= scrollChangeStep;

					if (minimum > outerScrollableControl.VerticalScroll.Value)
					{
						outerScrollableControl.VerticalScroll.Value = minimum;
					}
				}
				else
				{
					outerScrollableControl.VerticalScroll.Value = minimum;
				}
			}
			else
			{
				if (maximum >= (outerScrollableControl.VerticalScroll.Value + scrollChangeStep))
				{
					outerScrollableControl.VerticalScroll.Value += scrollChangeStep;

					if (maximum < outerScrollableControl.VerticalScroll.Value)
					{
						outerScrollableControl.VerticalScroll.Value = maximum;
					}
				}
				else
				{
					outerScrollableControl.VerticalScroll.Value = maximum;
				}
			}
		}
#endif
		void SetupModelDetailGrid(int rowsCount, IEnumerable<string> columns)
		{
			SetupColumns();
			AddColumnsIntoGrid(columns);

			var compensateHeight = 6;
			var headerAndHorizontalScrollbarCount = 2;
			var preferredGridHeight = GetRowHeightWithBorder(ModelDetailGrid.PreferredRowHeight) * (rowsCount + headerAndHorizontalScrollbarCount) + compensateHeight;
			PreferredHeight = Margin.Top + ModelDetailLayoutPanel.Controls[0].Height + preferredGridHeight + Margin.Bottom;
		}

		/// <summary>
		/// Scale	| DPI	| PreferRowHeight	| ActualRowHeight	| Border
		/// 100%	| 96 	| 16 				| 16				| 1
		/// 125%	| 120 	| 18 				| 20				| 1
		/// 150%	| 144 	| 22 				| 23				| 1
		/// 175%	| 168 	| 25 				| 26				| 1
		/// 200%	| 192 	| 27 				| 29				| 1
		/// 225%	| 216 	| 31 				| 32				| 1
		/// 250%	| 240 	| 34 				| 36				| 1
		/// 300%	| 288 	| 40 				| 42				| 1
		/// 350%	| 336 	| 45 				| 49				| 1
		/// </summary>
		/// <param name="preferredRowHeight">PreferredRowHeight of DataGrid</param>
		/// <returns>Actual row height with border</returns>
		int GetRowHeightWithBorder(int preferredRowHeight)
		{
			int rowHeight;

			if (preferredRowHeight == 16)
			{
				rowHeight = preferredRowHeight;
			}
			else if (preferredRowHeight == 22 || preferredRowHeight == 25 || preferredRowHeight == 31)
			{
				rowHeight = preferredRowHeight + 1;
			}
			else if (preferredRowHeight <= 40)
			{
				rowHeight = preferredRowHeight + 2;
			}
			else
			{
				rowHeight = preferredRowHeight + 4;
			}

			return rowHeight + 1;
		}

		void AddColumnsIntoGrid(IEnumerable<string> columns)
		{
			columns.ForEach(column =>
			{
				FieldInfo fieldInfo;
				if (column is null || (fieldInfo = typeof(DuplicationModelDetailUserControl).GetField(column + "Column", BindingFlags.NonPublic | BindingFlags.Instance)) is null)
				{
					return;
				}

				var columnStyle = fieldInfo.GetValue(this);
				ModelDetailGrid.ColumnStyles.Add(columnStyle);
			});
		}

		void SetupColumns()
		{
			NameColumn.ColumnName = "Name";
			NameColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			NameColumn.CaptionResourceString = Res.GetData("5ac0a82d-c753-45cd-af51-4430af59e389", "Name");
			NameColumn.IsSortable = false;
			BrandColumn.ColumnName = "Brand";
			BrandColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			BrandColumn.CaptionResourceString = Res.GetData("e48661c5-0d87-4ae3-ac15-db975f2ffdde", "Brand");
			BrandColumn.IsSortable = false;
			ContactPhoneColumn.ColumnName = "ContactPhone";
			ContactPhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ContactPhoneColumn.CaptionResourceString = Res.GetData("10e60143-0d28-4e14-975d-c54d70310276", "Contact Phone");
			ContactPhoneColumn.IsSortable = false;
			AddressPhoneColumn.ColumnName = "AddressPhone";
			AddressPhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			AddressPhoneColumn.CaptionResourceString = Res.GetData("b236e4c1-f1f2-4484-99cb-ad6221d79ac1", "Address Phone");
			AddressPhoneColumn.IsSortable = false;
			ContactMobileColumn.ColumnName = "ContactMobile";
			ContactMobileColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ContactMobileColumn.CaptionResourceString = Res.GetData("d9382d4d-9a63-4378-88a4-4ac2452819bc", "Contact Mobile");
			ContactMobileColumn.IsSortable = false;
			AddressMobileColumn.ColumnName = "AddressMobile";
			AddressMobileColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			AddressMobileColumn.CaptionResourceString = Res.GetData("a10c21f2-57d2-46fd-b106-2e82c9c7fbe8", "Address Mobile");
			AddressMobileColumn.IsSortable = false;
			EmailColumn.ColumnName = "Email";
			EmailColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			EmailColumn.CaptionResourceString = Res.GetData("dc4e2684-a709-4350-b429-ba1b8fac37b9", "E-mail");
			EmailColumn.IsSortable = false;
			BirthdayColumn.ColumnName = "Birthday";
			BirthdayColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			BirthdayColumn.CaptionResourceString = Res.GetData("fbad0d09-f5d5-4e48-bb57-06e8bb218744", "Birthday");
			BirthdayColumn.IsSortable = false;
			OtherPhoneColumn.ColumnName = "OtherPhone";
			OtherPhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			OtherPhoneColumn.CaptionResourceString = Res.GetData("49f96cdc-a83f-416a-8715-63fdea7bb0e5", "Other Ph");
			OtherPhoneColumn.IsSortable = false;
			HomePhoneColumn.ColumnName = "HomePhone";
			HomePhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			HomePhoneColumn.CaptionResourceString = Res.GetData("24b770eb-182f-42e4-910f-621a08bcc1a8", "Home Ph");
			HomePhoneColumn.IsSortable = false;
			WebsiteColumn.ColumnName = "Website";
			WebsiteColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			WebsiteColumn.CaptionResourceString = Res.GetData("90f84c63-7a0a-4df3-93ab-2c28f3dffe1d", "Website");
			WebsiteColumn.IsSortable = false;
			ContactFaxColumn.ColumnName = "ContactFax";
			ContactFaxColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ContactFaxColumn.CaptionResourceString = Res.GetData("4f9f8a20-6058-41de-8748-7b92d3eb061c", "Contact Fax");
			ContactFaxColumn.IsSortable = false;
			AddressFaxColumn.ColumnName = "AddressFax";
			AddressFaxColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			AddressFaxColumn.CaptionResourceString = Res.GetData("d374cd0e-f954-47f4-84ac-c0f80f53f482", "Address Fax");
			AddressFaxColumn.IsSortable = false;
			TypeColumn.ColumnName = "Type";
			TypeColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			TypeColumn.CaptionResourceString = Res.GetData("6bb5a24e-b508-4cd7-b70b-b24789477c0a", "Type");
			TypeColumn.IsSortable = false;
			CountryColumn.ColumnName = "Country";
			CountryColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CountryColumn.CaptionResourceString = Res.GetData("96f07a09-8517-48f0-ab46-f157749cc949", "Country/Region");
			CountryColumn.IsSortable = false;
			CusCodeCustomsRegNoColumn.ColumnName = "CusCodeCustomsRegNo";
			CusCodeCustomsRegNoColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CusCodeCustomsRegNoColumn.CaptionResourceString = Res.GetData("fff0b764-9663-4f8a-9776-cf005cd8d4c1", "Reg No");
			CusCodeCustomsRegNoColumn.IsSortable = false;
			CoordinatesColumn.ColumnName = "Coordinates";
			CoordinatesColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CoordinatesColumn.CaptionResourceString = Res.GetData("4747c650-7859-4af2-b1ba-3df519d3de37", "Coordinates");
			CoordinatesColumn.IsSortable = false;
			AddressColumn.ColumnName = "Address";
			AddressColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			AddressColumn.CaptionResourceString = Res.GetData("9e40279c-5fea-4626-a9b8-ffa403828894", "Address");
			AddressColumn.IsSortable = false;
			CodeColumn.ColumnName = "Code";
			CodeColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CodeColumn.CaptionResourceString = Res.GetData("2b3bd0d3-23bb-4a3e-aa67-2717dbb462a9", "Code");
			CodeColumn.IsSortable = false;
			DomainColumn.ColumnName = "Domain";
			DomainColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			DomainColumn.CaptionResourceString = Res.GetData("873b49e8-b722-4df4-bf7a-9c03671f7add", "Domain");
			DomainColumn.IsSortable = false;
			PersonPhoneColumn.ColumnName = "PersonPhone";
			PersonPhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PersonPhoneColumn.CaptionResourceString = Res.GetData("0c89bf75-2ade-4218-8885-edf1da64a82d", "Person Phone");
			PersonPhoneColumn.IsSortable = false;
			PersonWorkPhoneColumn.ColumnName = "PersonWorkPhone";
			PersonWorkPhoneColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PersonWorkPhoneColumn.CaptionResourceString = Res.GetData("eb81429e-ad78-4d5b-bfbc-6526a2666228", "Person Work Phone");
			PersonWorkPhoneColumn.IsSortable = false;
			PersonMobileColumn.ColumnName = "PersonMobile";
			PersonMobileColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PersonMobileColumn.CaptionResourceString = Res.GetData("398b8baf-eb0f-4fe9-aee5-445f276cd8a1", "Person Mobile");
			PersonMobileColumn.IsSortable = false;
			PersonFaxColumn.ColumnName = "PersonFax";
			PersonFaxColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PersonFaxColumn.CaptionResourceString = Res.GetData("65349b6c-e5ef-4ed8-b131-1e2ade32ddc7", "Person Fax");
			PersonFaxColumn.IsSortable = false;
			SimilarityColumn.ColumnName = "Similarity";
			SimilarityColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			SimilarityColumn.CaptionResourceString = Res.GetData("72eeadd8-149b-44e9-8d1d-4faa101cc353", "Similarity");
			SimilarityColumn.IsSortable = false;
			ConfidenceScoreColumn.ColumnName = "ConfidenceScore";
			ConfidenceScoreColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ConfidenceScoreColumn.CaptionResourceString = Res.GetData("ef393699-5fa1-40e4-83eb-41eae15e49cb", "Score");
			ConfidenceScoreColumn.IsSortable = false;
			NumberColumn.ColumnName = "Number";
			NumberColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			NumberColumn.CaptionResourceString = Res.GetData("8af08d3e-506b-43bf-9482-74f19597e92a", "Number");
			NumberColumn.IsSortable = false;
			SourceColumn.ColumnName = "Source";
			SourceColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			SourceColumn.CaptionResourceString = Res.GetData("14392ad7-7c27-4d16-b6e0-4301fef0ccf4", "Source");
			SourceColumn.IsSortable = false;
			PrimaryWorkplaceColumn.ColumnName = "PrimaryWorkplace";
			PrimaryWorkplaceColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PrimaryWorkplaceColumn.CaptionResourceString = Res.GetData("534fe809-0288-479e-8b35-f7544813d4e7", "Primary Workplace");
			PrimaryWorkplaceColumn.IsSortable = false;
			UNLOCOColumn.ColumnName = "UNLOCO";
			UNLOCOColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			UNLOCOColumn.CaptionResourceString = Res.GetData("acd7a6a3-75ee-426a-b8b3-778fe7db2803", "UNLOCO");
			UNLOCOColumn.IsSortable = false;
			CityColumn.ColumnName = "City";
			CityColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CityColumn.CaptionResourceString = Res.GetData("725e49c6-7dc1-49dd-80f8-7bd78cfc2301", "City");
			CityColumn.IsSortable = false;
			StateColumn.ColumnName = "State";
			StateColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			StateColumn.CaptionResourceString = Res.GetData("d8e92f84-dba5-41d3-b81a-a3209cd53483", "State");
			StateColumn.IsSortable = false;
			ActiveColumn.ColumnName = "Active";
			ActiveColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ActiveColumn.CaptionResourceString = Res.GetData("b5b44e97-9ecb-4e1b-a799-53460e769036", "Active");
			ActiveColumn.IsSortable = false;
		}

		readonly ZTextBoxColumnStyleInfo NameColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo BrandColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo ContactPhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo AddressPhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo ContactMobileColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo AddressMobileColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo EmailColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo BirthdayColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo OtherPhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo HomePhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo WebsiteColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo ContactFaxColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo AddressFaxColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo TypeColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo CountryColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo CusCodeCustomsRegNoColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo CoordinatesColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo AddressColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo CodeColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo DomainColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo PersonPhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo PersonWorkPhoneColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo PersonMobileColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo PersonFaxColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo SimilarityColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo ConfidenceScoreColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo NumberColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo SourceColumn = new ZTextBoxColumnStyleInfo();
		readonly ZCheckBoxColumnStyleInfo PrimaryWorkplaceColumn = new ZCheckBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo UNLOCOColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo CityColumn = new ZTextBoxColumnStyleInfo();
		readonly ZTextBoxColumnStyleInfo StateColumn = new ZTextBoxColumnStyleInfo();
		readonly ZCheckBoxColumnStyleInfo ActiveColumn = new ZCheckBoxColumnStyleInfo();
	}
}
