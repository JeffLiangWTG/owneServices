using CargoWise.Types;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI
{
	public static class TariffFindHelper
	{
		public static void AddDefaultPropertyToTariffControl(TariffColumnStyleInfo control, CommonShipment shipment)
		{
			if (control != null)
			{
				control.GetDataGrouping = () => Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
				control.TariffType = Customs.Universal.Constants.TariffTypes.HarmonizedSystem;
				control.GetEffectiveDate = () => TariffHelper.GetHSCodeEffectiveDate(shipment);
			}
		}

		public static void AddDefaultPropertyToTariffControlWithEffectiveDate(TariffColumnStyleInfo control, ZDateTime effectiveDate)
		{
			if (control != null)
			{
				control.GetDataGrouping = () => Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
				control.TariffType = Customs.Universal.Constants.TariffTypes.HarmonizedSystem;
				control.GetEffectiveDate = () => effectiveDate;
			}
		}

		public static void AddDefaultPropertyToTariffControlWithEffectiveDate(TariffFindBox control, ZDateTime effectiveDate)
		{
			if (control != null)
			{
				control.GetDataGrouping = () => Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
				control.TariffType = Customs.Universal.Constants.TariffTypes.HarmonizedSystem;
				control.GetEffectiveDate = () => effectiveDate;
			}
		}

		public static void AddDefaultPropertyToTariffControl(TariffFindBox control, CommonShipment shipment)
		{
			if (control != null)
			{
				control.GetDataGrouping = () => Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
				control.TariffType = Customs.Universal.Constants.TariffTypes.HarmonizedSystem;
				control.GetEffectiveDate = () => TariffHelper.GetHSCodeEffectiveDate(shipment);
			}
		}
	}
}
