using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public static class Extensions
	{
		public static string ToItalianShortDateString(this DateTime dateTime) => dateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
	}
}
