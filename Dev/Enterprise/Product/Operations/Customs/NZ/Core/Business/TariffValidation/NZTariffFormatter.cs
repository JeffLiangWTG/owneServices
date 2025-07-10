using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.TariffValidation
{
	public class NZTariffFormatter : Customs.Business.TariffFormatter
	{
		public NZTariffFormatter()
		{
		}

		public override ZString Format(ZString unformattedTariff)
		{
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				return DottedFormat(unformattedTariff).Replace(".", ZString.Empty);
			}
			else
			{
				return DottedFormat(unformattedTariff);
			}
		}

		public ZString FormatDotted(ZString unformattedTariff) => DottedFormat(unformattedTariff);

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			var newTariff = unformattedTariff.KeepChars("0123456789");
			var trailingCheckCharacter = unformattedTariff.Trim().Right(1).KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			var dottedTariff = ZString.Empty;
			if (IsExciseTariff(newTariff))
			{
				newTariff = newTariff.PadRight(6);
				dottedTariff = newTariff.Substring(0, 2) + "." + newTariff.Substring(2, 2) + "." + newTariff.Substring(4, 2);
			}
			else if (!newTariff.IsEmpty)
			{
				newTariff = newTariff.PadRight(10);
				dottedTariff = newTariff.Substring(0, 4) + "." + newTariff.Substring(4, 2) + "." + newTariff.Substring(6, 2) + "." + newTariff.Substring(8, 2);
			}

			dottedTariff = dottedTariff.Trim(' ', '.');

			if (!dottedTariff.IsEmpty && dottedTariff.Substring(dottedTariff.LastIndexOf('.') + 1).Length == 2 && !trailingCheckCharacter.IsEmpty)
			{
				dottedTariff = dottedTariff + trailingCheckCharacter;
			}

			return dottedTariff;
		}

		bool IsExciseTariff(ZString tariffNumber)
		{
			return !tariffNumber.IsEmpty && tariffNumber.StartsWith("99") && tariffNumber.Length < 7;
		}
	}
}
