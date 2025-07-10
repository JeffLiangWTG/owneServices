using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.NZ.GUI.MasterFiles
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializeComponent();
			SetupTariffFindBox();
			SetControlVisibility();
			DisableUnneededPivotColumns();
		}

		CusClassPartPivot CurrentPivot => currentPartPivot as CusClassPartPivot;

		void SetupTariffFindBox()
		{
			TariffNumFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			TariffNumFindBox.GetCountryCode = () => Core.Constants.CountryCodes.NewZealand;
			TariffNumFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			TariffNumFindBox.GetEffectiveDate = () => (CurrentPivot as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;

			PartsOfClassificationFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			PartsOfClassificationFindBox.GetCountryCode = () => Core.Constants.CountryCodes.NewZealand;
			PartsOfClassificationFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			PartsOfClassificationFindBox.GetEffectiveDate = () => (CurrentPivot as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;
		}

		void SetControlVisibility()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var useRefDB = UniversalTariffHelper.UseRefDatabaseData;
				TariffCodeFindBox.Visible = !useRefDB;
				PartsOfClassificationNZCClassFindBox.Visible = !useRefDB;
				ConcessionCodeCodeFindBox.Visible = !useRefDB;

				TariffNumFindBox.Visible = useRefDB;
				PartsOfClassificationFindBox.Visible = useRefDB;
				ConcessionCodeDropEdit.Visible = useRefDB;
			}
		}

		protected override ZBaseFindBoxColumnStyleInfo CreateTariffColumn()
		{
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				var tariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
				tariffColumnStyleInfo.ColumnName = TariffColumnName;
				tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				tariffColumnStyleInfo.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
				tariffColumnStyleInfo.GetCountryCode = GetCustomsCountryCode;
				tariffColumnStyleInfo.GetDataGrouping = GetDataGroupingForUniversalTariff;
				tariffColumnStyleInfo.GetEffectiveDate = () => (CurrentPivot as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;
				return tariffColumnStyleInfo;
			}
			else
			{
				var tariffColumnStyleInfo = new NZCClassColumnStyleInfo();
				tariffColumnStyleInfo.ColumnName = TariffColumnName;
				tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				return tariffColumnStyleInfo;
			}
		}

		protected override string TariffColumnNameCore => CusClassPartPivot.Schema.CI_TariffNum;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.NewZealand;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.NewZealand;
	}
}
