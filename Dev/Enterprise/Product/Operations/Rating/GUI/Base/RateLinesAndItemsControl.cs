using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI
{
	public partial class RateLinesAndItemsControl : BaseRateLinesAndItemsControl
	{
		public RateLinesAndItemsControl()
		{
			InitializeComponent();
			RateLinesGrid.ContextMenu.Popup += ContextMenu_Popup;
			RateLinesGrid.DoubleClick += RateLinesGrid_DoubleClick;

			AddOverrideMenuItems();
		}

		void AddOverrideMenuItems()
		{
			RateLinesGrid.GetDeleteMenuVisibleMethod = () => NeedDeleteMenuItem;

			deleteOverrideItem = new ZMenuItem(DeleteOverrideText, ContextMenu_DeleteOverride);
			RateLinesGrid.ContextMenu.MenuItems.Add(deleteOverrideItem);

			overrideItem = new ZMenuItem(OverrideText, ContextMenu_Override);
			RateLinesGrid.ContextMenu.MenuItems.Add(overrideItem);
		}

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!firstBound)
			{
				if (BindTo.StartsWith("WHSRateEntriesForBinding", StringComparison.OrdinalIgnoreCase))
				{
					AddCustomColumn(GetWarehouseProductNumberColumn());
					AddCustomColumn(GetWarehouseIsOnPalletsColumn());
					AddCustomColumn(GetJobLevelColumn());
				}
				else if (BindTo.StartsWith("TRWRateEntriesForBinding", StringComparison.OrdinalIgnoreCase))
				{
					AddConditionColumns();
					AddCustomColumn(GetJobLevelColumn());
				}
				else if (BindTo.StartsWith("TWURateEntriesForBinding", StringComparison.OrdinalIgnoreCase))
				{
					AddConditionColumns();
					AddCustomColumn(GetWarehouseIsOnPalletsColumn());
					AddCustomColumn(GetJobLevelColumn());
				}
				else if (!RatingHelper.RateLineGridHidesConversionFactorColumn(Category))
				{
					AddCustomColumn(GetConversionFactorColumn());
					SetConversionFactorChangedHandlers(false);
				}

				if (IsBindingToForwardingRateEntries()
					|| IsBindingToShippingNonDetentionRateEntries()
					|| IsBindingToTransportRateEntries()
					|| BindTo.StartsWith("PreviewEntries", StringComparison.OrdinalIgnoreCase))
				{
					AddConditionColumns();
					AddFeesAndChargesColumns();

					if (!IsBindingToTransportRateEntries())
					{
						AddCustomColumn(GetContainerOwnershipColumn());

						var isJobLevelApplicable = (dataSource is Costing || dataSource is IntercompanyTariff)
							&& !IsBindingToShippingNonDetentionRateEntries();
						if (isJobLevelApplicable)
						{
							AddCustomColumn(GetJobLevelColumn());
						}
					}
				}

				var isWiseRates = BindTo.StartsWith("WiseEntryViews", StringComparison.OrdinalIgnoreCase);
				if (isWiseRates)
				{
					AddWiseRatesSpecificColumns();
				}
				else
				{
					AddRateLineSpecificColumns(dataSource, Category);
				}

				CalculatorPanelAgentRatesCheckBoxVisible = !isWiseRates;

				RateLinesGrid.ColourDeciding += RateLinesGrid_ColourDeciding;

				firstBound = true;
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void RateLinesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var line = e.ObjectAtRow as RateLine;
			if (line != null && !line.IsDeleted)
			{
				if (line.IsExpired())
				{
					e.Colour = Color.PaleGoldenrod;
				}
			}
		}

		void AddConditionColumns()
		{
			var groupName = Res.GetData("572cdb0c-d0b6-4d86-b92c-4f986d26dbf1", "Condition");

			var conditionColumn = new ZDropEditColumnStyleInfo();
			conditionColumn.Caption = groupName.Caption;
			conditionColumn.ColumnName = RateLine.Schema.TL_Condition;
			conditionColumn.GroupName = groupName;
			ControlDpiScalingHelper.SetWidth(ref conditionColumn, 30, true);

			var expressionDescriptionColumn = new ZTextBoxColumnStyleInfo();
			expressionDescriptionColumn.Caption = Res.GetString("5fb8c8b2-389b-4c98-b258-22f7537c0da4", "Expression Description");
			expressionDescriptionColumn.ColumnName = RateLine.Schema.TL_ConditionalExpressionDescription;
			expressionDescriptionColumn.GroupName = groupName;
			ControlDpiScalingHelper.SetWidth(ref expressionDescriptionColumn, 160, true);

			var conditionExpressionColumn = new ZMacrosFindBoxColumnStyleInfo();
			conditionExpressionColumn.Caption = Res.GetString("3e9450cb-7371-458c-bd1f-02e711b1016b", "Expression");
			conditionExpressionColumn.ColumnName = RateLine.Schema.TL_ConditionalExpression;
			ControlDpiScalingHelper.SetWidth(ref conditionExpressionColumn, 300, true);
			conditionExpressionColumn.AllowMultipleMacroses = true;
			conditionExpressionColumn.UsePredefinedRoots = true;
			conditionExpressionColumn.IsUsedForExpressions = true;
			conditionExpressionColumn.GroupName = groupName;
			conditionExpressionColumn.RootTypes = new[] { GenericWrapperLoader.GetFromDataContext(Constants.DataContext.GenericFreightJob).GetWrapperType() };

			AddCustomColumn(conditionColumn);
			AddCustomColumn(expressionDescriptionColumn);
			AddCustomColumn(conditionExpressionColumn);
		}

		void AddFeesAndChargesColumns()
		{
			var groupName = Res.GetData("807ad76d-c1e1-42c1-b9fd-f22c3f70dfc5", "Fees And Charges");

			var feeChargeTypeColumn = new ZDropEditColumnStyleInfo();
			feeChargeTypeColumn.CaptionResourceString = Res.GetData("e425ea4e-70b3-4f42-ba91-a943ae2655cb", "F/C. Type", "Fees/Charges Type", "Fees and Charges Type", "");
			feeChargeTypeColumn.ColumnName = RateLine.Schema.TL_FeeChargeType;
			feeChargeTypeColumn.GroupName = groupName;
			feeChargeTypeColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref feeChargeTypeColumn, 60, true);

			var feeChargeLevelColumn = new ZDropEditColumnStyleInfo();
			feeChargeLevelColumn.CaptionResourceString = Res.GetData("fe8f6711-c619-49cd-88e7-c2a2534f93ae", "F/C. Level", "Fees/Charges Level", "Fees and Charges Level", "");
			feeChargeLevelColumn.ColumnName = RateLine.Schema.TL_FeeChargeLevel;
			feeChargeLevelColumn.GroupName = groupName;
			feeChargeLevelColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref feeChargeLevelColumn, 60, true);

			AddCustomColumn(feeChargeTypeColumn);
			AddCustomColumn(feeChargeLevelColumn);
		}

		void AddRateLineSpecificColumns(object dataSource, string category)
		{
			var overrideChargeDescriptionColumn = new ZCheckBoxColumnStyleInfo("OverrideChargeDescription", 50);
			var unitMultipleAsStringColumn = new ZDropEditColumnStyleInfo("UnitMultipleAsString", 40);
			var useOnlyActualWeightMeasureColumn = new ZCheckBoxColumnStyleInfo("UseOnlyActualWeightMeasure", 50);
			var chargeInformationNoteColumn =
				new ZMultiLineTextBoxColumnInfo("ChargeInformationNoteText", 300)
				{
					IsVisible = false,
					MinimumEditControlWidth = 300
				};

			var chargeInternalNoteColumn =
				new ZMultiLineTextBoxColumnInfo("ChargeInternalNoteText", 300)
				{
					IsVisible = false,
					MinimumEditControlWidth = 300
				};

			AddCustomColumn(overrideChargeDescriptionColumn);
			AddCustomColumn(unitMultipleAsStringColumn);
			AddCustomColumn(useOnlyActualWeightMeasureColumn);
			AddCustomColumn(chargeInformationNoteColumn);
			AddCustomColumn(chargeInternalNoteColumn);

			var unitFactorColumn = new ZDropEditColumnStyleInfo("TL_UnitFactor", 80)
			{
				GroupName = Res.GetData("C616875C-9D98-4EA1-9092-40DB543DE7AF", "Unit Factor"),
				IsVisible = new[]
				{
					RatingConstants.RateCategory.AIR,
					RatingConstants.RateCategory.FCL,
					RatingConstants.RateCategory.DST,

					RatingConstants.RateCategory.WHS,

					// Customs
					RatingConstants.RateCategory.CAI,
					RatingConstants.RateCategory.CFC,
					RatingConstants.RateCategory.CDS,
				}.Contains(category)
			};
			AddCustomColumn(unitFactorColumn);
		}

		void AddWiseRatesSpecificColumns()
		{
			var commentColumn = new ZTextBoxColumnStyleInfo
			{
				ColumnName = (NoResString)"Comment", // Hard-coded constant
				Width = 100,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("0ef9f455-ff77-4850-b244-560cc7d1fca5", "Comment", "Charge Comment", "Comment related to Charge", "")
			};

			AddCustomColumn(commentColumn);

			var universlcChargeColumn = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "UniversalChargeCodes", // Hard-coded constant
				Width = 110,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("9b981c13-3c77-4219-8dae-97fa661ffc6b", "Univ.Ch.Code", "Universal Charge Codes", "Universal Charge Codes mapped to this Rate Line's Charge Code", "")
			};

			AddCustomColumn(universlcChargeColumn);

			var carrierChargeCodeColumn = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "CarrierChargeCode", // Hard-coded constant
				Width = 130,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("3199e836-36ff-4524-aeac-bf1afc8e0b62", "Carr.Ch.Code", "Carrier Charge Code", "Equivalent Charge Code used by the Carrier who supplied this rate", "")
			};

			AddCustomColumn(carrierChargeCodeColumn);

			var conversionFactorColumn = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "ConversionFactorForBinding.ConversionFactorString", // Hard-coded constant
				Width = 130,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("ceecad65-903e-4542-b7e9-c4432d73e6dc", "Factor", "Conversion Factor", "Charge Conversion Factor", "")
			};

			AddCustomColumn(conversionFactorColumn);

			var unitMultipleAsStringColumn = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "UnitMultipleAsString", // Hard-coded constant
				Width = 130,
				CharacterCasing = CharacterCasing.Normal,
				CaptionResourceString = Res.GetData("b9f7e51e-d9da-4371-8799-51bd394a45cf", "Multi.", "Unit Multiple", "Charge Unit Multiple", "")
			};

			AddCustomColumn(unitMultipleAsStringColumn);
		}

		static ZDropEditColumnStyleInfo GetContainerOwnershipColumn()
		{
			var containerOwnershipColumn = new ZDropEditColumnStyleInfo();
			containerOwnershipColumn.ColumnName = RateLine.Schema.TL_ContainerOwnership;
			ControlDpiScalingHelper.SetWidth(ref containerOwnershipColumn, 40, true);

			return containerOwnershipColumn;
		}

		bool firstBound;

		bool IsBindingToShippingNonDetentionRateEntries()
		{
			return BindTo.StartsWith("SCORateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("SNCRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("SDERateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("SORRateEntriesForBinding", StringComparison.OrdinalIgnoreCase);
		}

		bool IsBindingToForwardingRateEntries()
		{
			return BindTo.StartsWith("AIRRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("FCLRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("LCLRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("ORGRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("DSTRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				// Customs
				|| BindTo.StartsWith("CAIRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("CFCRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("CLCRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("CORRateEntriesForBinding", StringComparison.OrdinalIgnoreCase)
				|| BindTo.StartsWith("CDSRateEntriesForBinding", StringComparison.OrdinalIgnoreCase);
		}

		bool IsBindingToTransportRateEntries()
		{
			return BindTo.StartsWith("TRNRateEntriesForBinding", StringComparison.OrdinalIgnoreCase) || BindTo.StartsWith("TBCRateEntriesForBinding", StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Conversion Factor

		static ZDropEditColumnStyleInfo GetConversionFactorColumn()
		{
			var convFactorColumn = new DropEditColumnWithEventStyleInfo();
			convFactorColumn.ColumnName = "ConversionFactorForBinding.ConversionFactorString";
			convFactorColumn.GroupName = Res.GetData("e630c9d0-4fb4-4a82-8d45-fc3c38467493", "Conversion Factor");
			ControlDpiScalingHelper.SetWidth(ref convFactorColumn, 70, true);
			convFactorColumn.IsVisible = false;
			return convFactorColumn;
		}

		#endregion

		#region Custom Conversion Factor

		void SetConversionFactorChangedHandlers(bool remove)
		{
			if (areDropDownHandlersSet == remove)
			{
				foreach (object style in RateLinesGrid.ColumnStyles)
				{
					var info = style as DropEditColumnWithEventStyleInfo;
					if (info != null)
					{
						if (remove)
						{
							info.ColumnTextBoxChanged -= ConversionFactorAsString_ColumnTextBoxChanged;
						}
						else
						{
							info.ColumnTextBoxChanged -= ConversionFactorAsString_ColumnTextBoxChanged;
							info.ColumnTextBoxChanged += ConversionFactorAsString_ColumnTextBoxChanged;
						}
					}
				}

				areDropDownHandlersSet = !remove;
			}
		}

		bool areDropDownHandlersSet;

		void ConversionFactorAsString_ColumnTextBoxChanged(object sender, EventArgs e)
		{
			var dropCodeBox = sender as ZDropCodeBox;
			if (dropCodeBox != null && dropCodeBox.Text == ConversionFactorList.Codes.Custom)
			{
				var line = (RateLine)RateLinesGrid.GetCurrent();

				using (var form = new CustomConversionFactorForm(line))
				{
					Point location = dropCodeBox.Parent.PointToScreen(dropCodeBox.Location);
					ControlDpiScalingHelper.SetTop(form, location.Y + dropCodeBox.Size.Height, false);
					ControlDpiScalingHelper.SetLeft(form, location.X, false);
					if (form.ShowDialog(this) == DialogResult.OK)
					{
						dropCodeBox.Text = form.ConversionFactor;
					}
					else
					{
						dropCodeBox.Text = line.ConversionFactorForBinding.ConversionFactorString;
					}
				}
			}
		}

		#endregion

		#region WHSRate

		static ZGuidFindBoxColumnStyleInfo GetWarehouseProductNumberColumn()
		{
			var prodNo = new ZGuidFindBoxColumnStyleInfo();
			prodNo.ColumnName = RateLine.Schema.TL_OP_ProductNumber;
			ControlDpiScalingHelper.SetWidth(ref prodNo, 100, true);

			return prodNo;
		}

		static ZCheckBoxColumnStyleInfo GetWarehouseIsOnPalletsColumn()
		{
			var isOnPalletsColumn = new ZCheckBoxColumnStyleInfo();
			isOnPalletsColumn.ColumnName = RateLine.Schema.TL_IsOnPallets;
			ControlDpiScalingHelper.SetWidth(ref isOnPalletsColumn, 40, true);

			return isOnPalletsColumn;
		}

		static ZCheckBoxColumnStyleInfo GetJobLevelColumn()
		{
			var isProductLevelCharge = new ZCheckBoxColumnStyleInfo();
			isProductLevelCharge.ColumnName = RateLine.Schema.TL_IsWhsJobLevelCharge;
			ControlDpiScalingHelper.SetWidth(ref isProductLevelCharge, 50, true);

			return isProductLevelCharge;
		}

		#endregion

		#region Context Menu Items Overrides

		MenuItem deleteOverrideItem;
		MenuItem overrideItem;
		MenuItem insertNewChargeLineBreak;
		MenuItem insertNewChargeItem;

		bool CanOverride
		{
			get
			{
				bool result = false;
				var form = ParentForm as RatingForm;
				if (form != null)
				{
					BaseTabControl baseTabControl = form.BaseTabControl;
					result = baseTabControl.InEditMode
						&& !baseTabControl.TopLevelTabControl.SelectedTab.Name.StartsWith((NoResString)"Summary", StringComparison.OrdinalIgnoreCase) // Hard-coded menu item constant
						&& RateLinesCollection?.Master?.ParentRatingHeader.IsAdditionalTariff() == ZBool.True;
				}

				return result;
			}
		}

		RateLine CurrentRateLine
			=> RateLinesGrid.GetCurrent() as RateLine;

		IEnumerable<RateLine> SelectedOrCurrentRows
		{
			get
			{
				var selected = RateLinesGrid.SelectedElements;
				if (selected.Length > 0)
				{
					return selected.Cast<RateLine>();
				}
				else
				{
					var current = CurrentRateLine;
					if (current != null)
					{
						return new[] { current };
					}
				}
				return Enumerable.Empty<RateLine>();
			}
		}

		bool NeedOverrideMenuItem
			=> CanOverride && SelectedOrCurrentRows.Any(x => x.IsTariffLineInherited);

		bool NeedDeleteMenuItem
		{
			get { return MasterEntry == null || MasterEntry.Parent == null || !MasterEntry.Parent.IsTariff() || MasterEntry.Parent.IsLevelOneTariff(); }
		}

		bool NeedDeleteOverrideMenuItem
			=> CanOverride && SelectedOrCurrentRows.Any(x => !x.IsTariffLineInherited);

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			deleteOverrideItem.Visible = NeedDeleteOverrideMenuItem;
			overrideItem.Visible = NeedOverrideMenuItem;
			AddInsertNewChargeMenuItem();
		}

		public MultilingualString DeleteOverrideText
		{
			get
			{
				return ResString.GetMultilingualString("debe1620-b7b3-4d75-834b-d77388b58b34", "Delete Override");
			}
		}

		public MultilingualString OverrideText
		{
			get
			{
				return ResString.GetMultilingualString("301961ec-3c18-4ddb-b4f8-863296780acf", "Override");
			}
		}

		public MultilingualString InsertNewCharge
		{
			get { return ResString.GetMultilingualString("dfb0954b-177f-48e5-9c8a-75b178f958d3", "Insert New Charge"); }
		}

		void ContextMenu_DeleteOverride(object sender, EventArgs e)
		{
			if (RateLinesGrid.DeleteMenuItem != null)
			{
				RateLinesGrid.DeleteMenuItem.PerformClick();
			}
		}

		void AddInsertNewChargeMenuItem()
		{
			if (insertNewChargeLineBreak == null)
			{
				insertNewChargeLineBreak = RateLinesGrid.ContextMenu.MenuItems.Add("-");
			}
			if (insertNewChargeItem == null)
			{
				insertNewChargeItem = RateLinesGrid.ContextMenu.MenuItems.Add(InsertNewCharge, ContextMenu_InsertRateLine);
			}

			var canShowInsertRateLineMenuItem = MasterEntry != null && !MasterEntry.IsReadOnlyDueToGlobalPublisher;
			insertNewChargeLineBreak.Visible = canShowInsertRateLineMenuItem;
			insertNewChargeItem.Visible = canShowInsertRateLineMenuItem;
		}

		void ContextMenu_InsertRateLine(object sender, EventArgs e)
		{
			if (RateLinesCollection != null)
			{
				try
				{
					RateLinesCollection.InsertNew(RateLinesGrid.ListManager.Position);
				}
				catch (ArgumentOutOfRangeException)
				{
					RateLinesGrid.ListManager.Refresh();
					RateLinesCollection.InsertNew(RateLinesGrid.ListManager.Position);
				}
			}
		}

		void ContextMenu_Override(object sender, EventArgs e)
		{
			int indexOfFirstNewLine = RateLinesCollection.OverrideTariffLines(SelectedOrCurrentRows);

			if (indexOfFirstNewLine >= 0)
			{
				RateLinesGrid.CurrentRowIndex = indexOfFirstNewLine;
			}
		}

		void RateLinesGrid_DoubleClick(object sender, EventArgs e)
		{
			if (CanOverride)
			{
				var current = CurrentRateLine;
				if (current != null && current.IsTariffLineInherited)
				{
					int indexOfFirstNewLine = RateLinesCollection.OverrideTariffLines(new RateLine[] { current });
					if (indexOfFirstNewLine >= 0)
					{
						RateLinesGrid.CurrentRowIndex = indexOfFirstNewLine;
					}
				}
			}
		}

		#endregion

		#region AddCustomColumn

		void AddCustomColumn(object newColumn)
		{
			RateLinesGrid.ColumnStyles.Add(newColumn);
		}

		#endregion

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RateLinesGrid.ColourDeciding -= RateLinesGrid_ColourDeciding;
				SetConversionFactorChangedHandlers(true);

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region ForTest

#if DEBUG
		internal ZGrid RateLinesGrid_ForTest => RateLinesGrid;

		public void RateLinesGrid_ColourDeciding_ForTest(ColourDecidingEventArgs e)
		{
			RateLinesGrid_ColourDeciding(null, e);
		}
#endif

		#endregion
	}
}
