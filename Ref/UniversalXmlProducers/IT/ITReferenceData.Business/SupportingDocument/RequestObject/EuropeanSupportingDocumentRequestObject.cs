using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public class EuropeanSupportingDocumentRequestObject
	{
		public EuropeanSupportingDocumentRequestObject(IDateTimeProvider dateTimeProvider, IEuropeanRawSupportingDocumentRequestObjectParameters parameters)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.parameters = Argument.NotNull(parameters, nameof(parameters));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IEuropeanRawSupportingDocumentRequestObjectParameters parameters;

		public Dictionary<string, string> Build()
		{
			var requestParameters = new Dictionary<string, string>();
			requestParameters["SC"] = parameters.SC;
			requestParameters["ST"] = parameters.ST;
			requestParameters["UC"] = parameters.UC;
			requestParameters["Label"] = parameters.Label;
			requestParameters["DatiGenerali.TipoCertificato"] = parameters.Suffix;
			requestParameters["DatiGenerali.NumeroCertificato"] = parameters.ProgressiveNumber;
			requestParameters["DatiGenerali.DataIniValDesCertificato"] = parameters.DescriptionValidityStartDate;
			requestParameters["DatiGenerali.CodPaeseRegGrp"] = parameters.RegGrpCountryCode;
			requestParameters["DATA_PROVENIENZA_MISURE"] = dateTimeProvider.Now.ToItalianShortDateString();
			requestParameters["PROVENIENZA_MISURE"] = "FALSE";
			requestParameters["$STACK$"] = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87" +
				"%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87" +
				"%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87" +
				"%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Ecertificato%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action" +
				"%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%870%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87FLAG_RIC" +
				"%C3%87java.lang.String%C3%86true%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.DatiGeneraliFinder%C3%86null%C3%94A%C3%94%C3%941%C3%94null%C3%94" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%94%C3%94%C3%94%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ADatiGeneraliServlet%3A6%3A1%3A-1%3A%C3%B5";
			return requestParameters;
		}
	}
}
