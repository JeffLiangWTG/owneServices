using System.Collections.Generic;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public class NationalAdditionalRequestObject
	{
		public NationalAdditionalRequestObject(IDateTimeProvider dateTimeProvider, INationalRawAdditionalCodeRequestObjectParameters parameters)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.parameters = Argument.NotNull(parameters, nameof(parameters));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly INationalRawAdditionalCodeRequestObjectParameters parameters;

		public Dictionary<string, string> Build()
		{
			var requestParameters = new Dictionary<string, string>();
			requestParameters["SC"] = parameters.SC;
			requestParameters["ST"] = parameters.ST;
			requestParameters["UC"] = parameters.UC;
			requestParameters["Label"] = parameters.Label;
			requestParameters["Cadd.Codice"] = parameters.AdditionalCodeSequentialNumber;
			requestParameters["Cadd.Tipo"] = parameters.AdditionalCodeType;
			requestParameters["Cadd.DataInizioValidita"] = parameters.ValidityStartDate;
			requestParameters["Input"] = "1";
			requestParameters["Cadd.SidCadd"] = parameters.SidCad;
			requestParameters["Cadd.CodiceNotaAssociata"] = "1";
			requestParameters["Cadd.TipoNotaAssociata"] = "1";
			requestParameters["Cadd.DataInizioValiditaDescrizioneNotaAssociata"] = "1";
			requestParameters["$STACK$"] = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String" +
				"%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String" +
				"%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String" +
				"%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Ecadd+nazionale%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action" +
				"%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%870%C3%87java.lang.String%C3%87FINDER" +
				"%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.CaddFinder%C3%86%C3%94%C3%94Q%C3%941%C3%94" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%94%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ACaddServlet" +
				"%3A10%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87MODE%C3%87java.lang.String%C3%860%C3%87%C3%B5";
			return requestParameters;
		}
	}
}
