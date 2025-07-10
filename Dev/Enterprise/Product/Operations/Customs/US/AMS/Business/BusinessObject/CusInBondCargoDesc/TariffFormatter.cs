using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class TariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters().Left(10);
		}

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			var newTariff = unformattedTariff.KeepNumericCharacters();
			ZString dottedTariff = newTariff.IsEmpty ? "" : newTariff.SubstringSafe(0, 4) + "." + newTariff.SubstringSafe(4, 2) + "." + newTariff.SubstringSafe(6, 2) + newTariff.SubstringSafe(8).Trim();
			return dottedTariff.Trim(new char[] { ' ', '.' });
		}
	}
}
