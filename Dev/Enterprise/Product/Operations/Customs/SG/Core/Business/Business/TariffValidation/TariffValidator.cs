using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public static class TariffValidator
	{
		public static void ValidateTariff(BusinessObjectFactory factory, TariffView tariff, ZPropertyInfo propInfo)
		{
			var tariffCode = (ZString)propInfo.Value;

			if (tariff == null && !tariffCode.IsEmpty)
			{
				var invalidTariff = UniversalReferenceDataHelper.LoadLatestTariff(factory, tariffCode);
				if (invalidTariff != null)
				{
					if (invalidTariff.ZZ1_EndDate < ZDateTime.Today)
					{
						propInfo.AddMessageError(TariffNoLongerValid);
					}
					else if (invalidTariff.ZZ1_StartDate > ZDateTime.Today)
					{
						propInfo.AddMessageError(TariffNotYetValid(invalidTariff.ZZ1_StartDate));
					}
					else
					{
						propInfo.AddMessageError(TariffNotTradenet);
					}
				}
				else if (!propInfo.HasNotifications())
				{
					propInfo.AddMessageError(TariffDoesNotExist);
				}
			}
		}

		public static ResourceString TariffNoLongerValid => ResString.GetMultilingualString("B0EBA40A-FCC4-4FF4-91B7-CEA5594E3D13", "This Tariff is no longer valid for use as it has expired.");
		public static ResourceString TariffNotYetValid(ZDateTime startDate) => ResString.GetMultilingualString("EBF1A281-FA88-4EAE-B210-D94F878F4E7B", "This Tariff is not yet valid for use. It does not become active until {0}.", startDate.ToLongTimeString());
		public static ResourceString TariffNotTradenet => ResString.GetMultilingualString("62C423FE-8269-4666-9A07-B28676B97116", "Tariff is not valid for use in this TradeNet version.");
		public static ResourceString TariffDoesNotExist => ResString.GetMultilingualString("EABCAF0B-A27E-4EE7-B187-1A18EC00F3A7", "The tariff code cannot be found in the customs tariff code list.");
	}
}
