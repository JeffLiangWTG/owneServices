using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public class TariffInformationRequestObject
	{
		public TariffInformationRequestObject(IDateTimeProvider dateTimeProvider, string tariffCode)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.tariffCode = Argument.NotNull(tariffCode, nameof(tariffCode));
		}

		public Dictionary<string, string> Build()
		{
			var now = dateTimeProvider.Now;
			var stack = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String" +
				"%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87" +
				"%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87" +
				"%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+esportazione%C3%87java.lang.String" +
				"%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A" +
				"%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String" +
				"%C3%87Misure.PaeseGruppoRegione%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87%C3%87java.lang.String%C3%87FN%C3%87" +
				"%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String" +
				"%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA%C3%87" +
				"%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5";

			return new Dictionary<string, string>
			{
				{ "IS_ESPORTAZIONE", "1" },
				{ "Label", "102" },
				{ "AAAA-Misure.DataRiferimento", now.ToString("yyyy",CultureInfo.InvariantCulture) },
				{ "GG-Misure.DataRiferimento", now.ToString("dd", CultureInfo.InvariantCulture) },
				{ "MM-Misure.DataRiferimento", now.ToString("MM", CultureInfo.InvariantCulture) },
				{ "Misure.CodiceNomenclaturaNC", tariffCode },
				{ "Misure.CodiceNomenclaturaTar", "00" },
				{ "Misure.DataRiferimento", now.ToItalianShortDateString() },
				{ "Misure.PaeseGruppoRegione", "ALL" },
				{ "SC", "1" },
				{ "ST", "2" },
				{ "UC", "30" },
				{ "$STACK$", stack },
			};
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly string tariffCode;
	}
}
