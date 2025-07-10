using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class TaiwanTariffFormatter : Customs.Business.TariffFormatter
	{
		public ZString DottedFormatWithPrintLength(ZString unformattedTariff, ZString tariffPrintLength)
		{
			var result = ZString.Empty;
			switch (tariffPrintLength)
			{
				case TariffPrintLengthList.Codes.One:
					result = DottedFormatWithFormattedTariff(Format(unformattedTariff).Left(6));
					break;
				case TariffPrintLengthList.Codes.TWO:
					result = DottedFormatWithFormattedTariff(Format(unformattedTariff).Left(8));
					break;
				case TariffPrintLengthList.Codes.THREE:
					result = DottedFormatWithFormattedTariff(Format(unformattedTariff).Left(11));
					break;
			}
			return result;
		}

		public override ZString Format(ZString unformattedTariff) => unformattedTariff.KeepNumericCharacters();

		protected override ZString DottedFormat(ZString unformattedTariff) => DottedFormatWithFormattedTariff(Format(unformattedTariff));

		ZString DottedFormatWithFormattedTariff(ZString formattedTariff)
		{
			var dottedTariff = formattedTariff.IsEmpty ? string.Empty : $"{formattedTariff.SubstringSafe(0, 4)}.{formattedTariff.SubstringSafe(4, 2)}.{formattedTariff.SubstringSafe(6, 2)}.{formattedTariff.SubstringSafe(8, 2)}-{formattedTariff.SubstringSafe(10).Trim()}";
			return dottedTariff.Trim(new char[] { '-', '.' });
		}
	}
}
