using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class NumberOnlyTariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters();
		}
	}
}
