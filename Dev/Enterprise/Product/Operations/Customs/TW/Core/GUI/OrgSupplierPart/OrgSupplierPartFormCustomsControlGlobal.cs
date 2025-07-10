using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializeComponent();
			ResetCustomsPivotGridColumns();
		}

		static ResourceStringData ChineseDescriptionString => DataBoundResourceStrings.GetDataForProperty(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_NDescription));
		static ResourceStringData PermitString => Res.GetData("4591861B-CEEF-4F18-96D4-6E6582CC04E7", "Permit {0}");
		static ResourceStringData PermitNoString => Res.GetData("3FEA9E3A-B460-46EF-8B1D-032AB01F5BE3", "Permit No {0}");
		static ResourceStringData PermitLineNoString => Res.GetData("B73D71FC-AAA4-4F71-8FBB-691C6958797F", "Permit Line No {0}");
		static ResourceStringData AssignedNumberString => Res.GetData("C06596C1-A1AB-496E-B02D-BBBC35230CB1", "Assigned Number {0}");

		void ResetCustomsPivotGridColumns()
		{
			PivotGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = ChineseDescriptionString,
				ColumnName = CusClassPartPivotSchema.Constants.CI_NDescription,
				ToolTip = ChineseDescriptionString.Caption,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
				IsVisible = false,
			});

			AddAssignedNumberColumnStyleToPivotGrid("1", CusClassPartPivot.Schema.AssignedNumber1);
			AddAssignedNumberColumnStyleToPivotGrid("2", CusClassPartPivot.Schema.AssignedNumber2);
			AddAssignedNumberColumnStyleToPivotGrid("3", CusClassPartPivot.Schema.AssignedNumber3);
			AddAssignedNumberColumnStyleToPivotGrid("4", CusClassPartPivot.Schema.AssignedNumber4);
			AddAssignedNumberColumnStyleToPivotGrid("5", CusClassPartPivot.Schema.AssignedNumber5);
			AddAssignedNumberColumnStyleToPivotGrid("6", CusClassPartPivot.Schema.AssignedNumber6);
			AddAssignedNumberColumnStyleToPivotGrid("7", CusClassPartPivot.Schema.AssignedNumber7);
			AddAssignedNumberColumnStyleToPivotGrid("8", CusClassPartPivot.Schema.AssignedNumber8);
			AddAssignedNumberColumnStyleToPivotGrid("9", CusClassPartPivot.Schema.AssignedNumber9);
			AddAssignedNumberColumnStyleToPivotGrid("10", CusClassPartPivot.Schema.AssignedNumber10);

			AddPermitNoColumnStyleToPivotGrid("1", CusClassPartPivot.Schema.PermitCusSupportingNo1);
			AddPermitLineNoColumnStyleToPivotGrid("1", CusClassPartPivot.Schema.PermitCusSupportingLineNo1);

			AddPermitNoColumnStyleToPivotGrid("2", CusClassPartPivot.Schema.PermitCusSupportingNo2);
			AddPermitLineNoColumnStyleToPivotGrid("2", CusClassPartPivot.Schema.PermitCusSupportingLineNo2);

			AddPermitNoColumnStyleToPivotGrid("3", CusClassPartPivot.Schema.PermitCusSupportingNo3);
			AddPermitLineNoColumnStyleToPivotGrid("3", CusClassPartPivot.Schema.PermitCusSupportingLineNo3);

			AddPermitNoColumnStyleToPivotGrid("4", CusClassPartPivot.Schema.PermitCusSupportingNo4);
			AddPermitLineNoColumnStyleToPivotGrid("4", CusClassPartPivot.Schema.PermitCusSupportingLineNo4);

			AddPermitNoColumnStyleToPivotGrid("5", CusClassPartPivot.Schema.PermitCusSupportingNo5);
			AddPermitLineNoColumnStyleToPivotGrid("5", CusClassPartPivot.Schema.PermitCusSupportingLineNo5);
		}

		void AddAssignedNumberColumnStyleToPivotGrid(string index, string columnName)
		{
			PivotGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = AssignedNumberString.Format(index),
				ColumnName = columnName,
				ToolTip = AssignedNumberString.Format(index).Caption,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				IsVisible = false,
			});
		}

		void AddPermitLineNoColumnStyleToPivotGrid(string index, string columnName)
		{
			var groupName = PermitString.Format(index);
			PivotGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = PermitLineNoString.Format(index),
				ColumnName = columnName,
				ToolTip = PermitLineNoString.Format(index).Caption,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				IsVisible = false,
			});
		}

		void AddPermitNoColumnStyleToPivotGrid(string index, string columnName)
		{
			var groupName = PermitString.Format(index);
			PivotGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = PermitNoString.Format(index),
				ColumnName = columnName,
				ToolTip = PermitNoString.Format(index).Caption,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsVisible = false,
			});
		}

		protected new CusClassPartPivot currentPartPivot => base.currentPartPivot as CusClassPartPivot;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			if (currentPartPivot != null)
			{
				CI_ModeOfStatisticsDropEdit.Visible = currentPartPivot.IsExportClassification;
				CI_DutyTreatmentDropEdit.Visible = currentPartPivot.IsImportClassification;
				EnvironmentalProtectionTariffsGroupBox.Visible = currentPartPivot.ShouldAllowEnvironmentalProtectionTariff;
				CarInfoTabPage.TabVisible = currentPartPivot?.IsCarRelatedTariff ?? false;
			}
		}

		protected override string TariffColumnNameCore => Customs.Business.BaseCusClassPartPivot.Schema.CI_FormattedTariffNum;

		protected override void InitializeForm()
		{
			base.InitializeForm();
			var tariffColumn = PivotGrid.GetColumnStyle(TariffColumnName) as Universal.GUI.TariffColumnStyleInfo;
			if (tariffColumn != null)
			{
				tariffColumn.Width = 95;
			}
		}
	}
}
