using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibility()
		{
			if (currentPartPivot != null)
			{
				var isImport = currentPartPivot.CI_ChildType == ClassificationTypeList.Codes.HTI;
				var isExport = currentPartPivot.CI_ChildType == ClassificationTypeList.Codes.HTE;
				PreferenceDropEdit.Visible = isImport;
				tradeAgreementLabel.Visible = isImport;
				ROOTypeDropEdit.Visible = isExport;
				RulesOfOriginCertificateTextBox.Visible = isImport || isExport;
				PositionFields(currentPartPivot.CI_ChildType == ClassificationTypeList.Codes.HTB);
			}
		}

		protected override string TariffColumnNameCore => CusClassPartPivotSchema.Constants.CI_TariffNum;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.SouthAfrica;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.SouthAfrica;

		protected override string UniversalTariffType => Enterprise.Customs.ZA.Business.UniversalReferenceConstants.CusTariffCode.Schedule1Part1;

		void PositionFields(bool isBoth)
		{
			SetFieldLocation(GoodsTypeDropEdit, 96 + (isBoth ? -44 : 0));
			SetFieldLocation(EngineCapacityTextBox, 118 + (isBoth ? -44 : 0));
			SetFieldLocation(VehicleFormatDropEdit, 140 + (isBoth ? -44 : 0));
			SetFieldLocation(VehicleTypeDropEdit, 162 + (isBoth ? -44 : 0));
			SetFieldLocation(VehicleColorTextBox, 184 + (isBoth ? -44 : 0));
			SetFieldLocation(ClassificationDescriptionTextBox, 206 + (isBoth ? -44 : 0));
		}

		void SetFieldLocation(System.Windows.Forms.Control control, int value)
		{
			var currentX = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.Location.X);
			control.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(currentX, value);
		}

		public ZArchitecture.GUI.ZCodeFindBox GoodsOriginBoundFindBox;
		public ZArchitecture.GUI.ZDropEdit PreferenceDropEdit;
		public ZArchitecture.GUI.ZDropEdit ROOTypeDropEdit;
		public ZArchitecture.ZTextBox RulesOfOriginCertificateTextBox;
		public ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		public ZArchitecture.ZTextBox EngineCapacityTextBox;
		public ZArchitecture.GUI.ZDropEdit VehicleFormatDropEdit;
		public ZArchitecture.GUI.ZDropEdit VehicleTypeDropEdit;
		public ZArchitecture.ZTextBox VehicleColorTextBox;
		ZArchitecture.GUI.ZGroupBox additionalDutiesGroupBox;
		ZArchitecture.ZGrid additionalDutiesGrid;
		ZArchitecture.ZLabel tradeAgreementLabel;
	}
}

