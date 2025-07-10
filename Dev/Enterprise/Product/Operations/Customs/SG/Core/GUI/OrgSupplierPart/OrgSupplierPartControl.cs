using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class OrgSupplierPartControl : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartControl()
		{
			InitializeComponent();
			DisableUnneededPivotColumns();
		}

		#region overrides

		protected override ZBaseFindBoxColumnStyleInfo CreateTariffColumn()
		{
			var tariffColumnStyleInfo = new TariffColumnStyleInfo();
			tariffColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SG.V4.OrgSupplierPartControl|E46172C6-AB5C-4280-9824-C272C4E78408", "Tariff", "Tariff Code");
			tariffColumnStyleInfo.ColumnName = TariffColumnName;
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo.GetCountryCode = GetCustomsCountryCode;
			tariffColumnStyleInfo.GetDataGrouping = GetDataGroupingForUniversalTariff;
			tariffColumnStyleInfo.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			return tariffColumnStyleInfo;
		}

		protected override string TariffColumnNameCore => Business.CusClassPartPivot.Schema.CI_TariffNum;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Singapore;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Singapore;

		#endregion
	}
}
