using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class TariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.EqualsIgnoringCase(TariffViewAsCodeDescription.NotApplicableCode) ? TariffViewAsCodeDescription.NotApplicableCode : unformattedTariff.KeepNumericCharacters().Left(10).ToString();
		}

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			if (unformattedTariff.EqualsIgnoringCase(TariffViewAsCodeDescription.NotApplicableCode))
			{
				return TariffViewAsCodeDescription.NotApplicableCode;
			}
			ZString newTariff = unformattedTariff.KeepNumericCharacters();
			ZString dottedTariff = newTariff.IsEmpty ? "" : newTariff.SubstringSafe(0, 4) + "." + newTariff.SubstringSafe(4, 2) + "." + newTariff.SubstringSafe(6, 2) + newTariff.SubstringSafe(8).Trim();
			return dottedTariff.Trim(new char[] { ' ', '.' });
		}
	}
}
