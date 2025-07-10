using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI
{
	public partial class LandCostHistoryUserControl : ZUserControl
	{
		readonly Container components;

		public LandCostHistoryUserControl()
		{
			InitializeComponent();
		}

		public void SetBindTo(ZString bindTo)
		{
			LCHistoryGrid.BindTo = bindTo;
		}

		protected internal ZGrid LCHistoryGrid;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual ILandedCostHistoryMaster LCHistoryMaster
		{
			get { return fLCHistoryMaster; }
			set
			{
				fLCHistoryMaster = value;
				if (LCHistoryMaster != null)
				{
					CustomiseColumnsOfGrid(LCHistoryMaster);
				}
			}
		}
		ILandedCostHistoryMaster fLCHistoryMaster;

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void CustomiseColumnsOfGrid(ILandedCostHistoryMaster historyMaster)
		{
			SetLCHistoryGridReadonly(LandedCostHistorySchema.LH_LandedCostMarginPercent1.Name, historyMaster.ShouldMarginPercentagesReadOnly);
			SetLCHistoryGridReadonly(LandedCostHistorySchema.LH_LandedCostMarginPercent2.Name, historyMaster.ShouldMarginPercentagesReadOnly);
			SetLCHistoryGridReadonly(LandedCostHistorySchema.LH_LandedCostMarginPercent3.Name, historyMaster.ShouldMarginPercentagesReadOnly);

			AddColumnsFromCustomsChargeLCItemSettings();

			LCHistoryGrid.RefreshTableStyles();
		}

		void SetLCHistoryGridReadonly(string columnName, bool readOnly)
		{
			ZGridColumn column = LCHistoryGrid.Columns[columnName];
			if (column != null)
			{
				column.ColumnStyle.ReadOnly = readOnly;
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			if (LCHistoryMaster != null)
			{
				if (LCHistoryMaster.CountryCode == Core.Constants.CountryCodes.UnitedStates)
				{
					LCHistoryGrid.RemoveFromAvailableColumns(
						LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice1IncGST,
						LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice2IncGST,
						LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice3IncGST);
					LCHistoryGrid.SetColumnCaption(LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice1ExGST, Res.GetString("7d3511ef-46a6-4565-b9a8-9066514ba89e", "Sell Price 1"));
					LCHistoryGrid.SetColumnCaption(LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice2ExGST, Res.GetString("f59a33d2-3a74-4d81-9f75-957427bbdd40", "Sell Price 2"));
					LCHistoryGrid.SetColumnCaption(LandedCosting.Business.LandedCostHistory.Schema.RoundedSellPrice3ExGST, Res.GetString("d262b04c-0727-4209-9d21-802f3191dfde", "Sell Price 3"));
				}
				else if (LCHistoryMaster.CountryCode == Core.Constants.CountryCodes.SouthAfrica)
				{
					LCHistoryGrid.RemoveFromAvailableColumns(
						LandedCosting.Business.LandedCostHistory.Schema.LH_DutyPercent);
				}
			}
		}

		void AddColumnsFromCustomsChargeLCItemSettings()
		{
			if (LCHistoryMaster != null)
			{
				foreach (var setting in LCHistoryMaster.CustomsChargeLCItemSettings)
				{
					var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();

					var descriptor = new LandedLineCostItemAmountPropertyDescriptor(setting.CostType);
					zCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
					zCalcEditColumnStyleInfo.Caption = setting.Description;
					zCalcEditColumnStyleInfo.ColumnName = setting.CostType;
					zCalcEditColumnStyleInfo.Decimals = setting.NumberOfDecimals;
					zCalcEditColumnStyleInfo.IsReadOnly = descriptor.IsReadOnly;
					zCalcEditColumnStyleInfo.IsVisible = false;
					((IOverridablePropertyDescriptor)zCalcEditColumnStyleInfo).PropertyDescriptor = descriptor;
					TypeDescriptor.AddAttributes(zCalcEditColumnStyleInfo, new SuppressFormsLocalizedTestAttribute());

					this.LCHistoryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
				}
			}
		}

		#endregion
	}
}

