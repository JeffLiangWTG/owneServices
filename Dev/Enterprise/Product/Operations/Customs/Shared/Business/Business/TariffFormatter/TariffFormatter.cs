namespace Enterprise.Customs.Business
{
	using CargoWise.Types;
	using Enterprise.Customs.Common;

	public class TariffFormatter : ITariffFormatter
	{
		public virtual ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff;
		}

		public ZString DisplayFormat(ZString unformattedTariff)
		{
			return DottedFormat(unformattedTariff);
		}

		protected virtual ZString DottedFormat(ZString unformattedTariff)
		{
			ZString newTariff = unformattedTariff.KeepNumericCharacters();
			ZString dottedTariff = newTariff.IsEmpty ? "" : newTariff.SubstringSafe(0, 4) + "." + newTariff.SubstringSafe(4, 2) + "." + newTariff.SubstringSafe(6, 2) + " " + newTariff.SubstringSafe(8).Trim();
			return dottedTariff.Trim(new char[] { ' ', '.' });
		}
	}
}
