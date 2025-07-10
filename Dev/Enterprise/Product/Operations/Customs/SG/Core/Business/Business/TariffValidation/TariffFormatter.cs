using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class TariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters();
		}
	}
}
