using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GUI
{
	public partial class GeneralCountryClassificationUserControl : BaseClassificationUserControl
	{
		public GeneralCountryClassificationUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				CC_TariffNumFindBox.GetCountryCode = GetCustomsCountryCode;
				CC_TariffNumFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
				CC_TariffNumFindBox.TariffType = UniversalTariffType;
			}
		}

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

		protected virtual ZString UniversalTariffType
		{
			get { return Universal.Constants.TariffTypes.HarmonizedSystem; }
		}

		protected virtual ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GetCustomsCountryCode());

		protected virtual string GetCustomsCountryCode()
		{
			string result = null;
			if (!DesignModeFinder.IsDesigning)
			{
				var classification = (CurrentDataItem as BaseCusClassification);
				result = classification == null || classification.IsDeleted ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : classification.CC_RN_NKCountryCode;
			}
			return result;
		}
	}
}
