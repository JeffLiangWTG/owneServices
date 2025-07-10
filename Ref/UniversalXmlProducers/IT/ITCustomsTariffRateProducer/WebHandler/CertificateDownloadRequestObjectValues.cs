using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public class CertificateDownloadRequestObjectValues
	{
		[RequestObjectPropertyName("Misure.TipoCertificato")]
		public string CertificateType
		{
			get
			{
				return _certificateType;
			}
		}

		[RequestObjectPropertyName("Misure.NumeroCertificato")]
		public string CertificateNumber
		{
			get
			{
				return _certificateNumber;
			}
		}

		[RequestObjectPropertyName("CertificatoNaz.DataInizioValiditaDescrizione")]
		public string DateString
		{
			get
			{
				return _dateAsString;
			}
		}

		[RequestObjectPropertyName("SC")]
		public string StaticRequestParamSc
		{
			get
			{
				return _staticRequestParamSc;
			}
		}

		[RequestObjectPropertyName("ST")]
		public string StaticRequestParamSt
		{
			get
			{
				return _staticRequestParamSt;
			}
		}

		[RequestObjectPropertyName("UC")]
		public string StaticRequestParamUc
		{
			get
			{
				return _staticRequestParamUc;
			}
		}

		[RequestObjectPropertyName("Label")]
		public string StaticRequestParamLabel
		{
			get
			{
				return _staticRequestParamLabel;
			}
		}

		[RequestObjectPropertyName("RUOLO_R")]
		public static string StaticRequestParamRUOLO_R => "1";

		[RequestObjectPropertyName("PROG_R")]
		public static string StaticRequestParamPROG_R => "1";

		[RequestObjectPropertyName("NUMERO_ORDINE")]
		public static string StaticRequestParamNUMERO_ORDINE => "1";

		[RequestObjectPropertyName("NUM_R")]
		public static string StaticRequestParamNUM_R => "1";

		[RequestObjectPropertyName("MisureCodiceCadd")]
		public static string StaticRequestParamMisureCodiceCadd => "1";

		[RequestObjectPropertyName("Misure.TipoRegolamento")]
		public static string StaticRequestParamMisureTipoRegolamento => "1";

		[RequestObjectPropertyName("Misure.TipoNotaAss")]
		public static string StaticRequestParamMisureTipoNotaAss => "1";

		[RequestObjectPropertyName("Misure.TipoMisura")]
		public static string StaticRequestParamMisureTipoMisura => "1";

		[RequestObjectPropertyName("Misure.TipoCadd")]
		public static string StaticRequestParamMisureTipoCadd => "1";

		[RequestObjectPropertyName("Misure.SidMisura")]
		public static string StaticRequestParamMisureSidMisura => "1";

		[RequestObjectPropertyName("Misure.SidDesNotaAss")]
		public static string StaticRequestParamMisureSidDesNotaAss => "1";

		[RequestObjectPropertyName("Misure.SidCadd")]
		public static string StaticRequestParamMisureSidCadd => "1";

		[RequestObjectPropertyName("Misure.RuoloRegolamento")]
		public static string StaticRequestParamMisureRuoloRegolamento => "1";

		[RequestObjectPropertyName("Misure.InizioValDescrizioneNotaAss")]
		public static string StaticRequestParamMisureInizioValDescrizioneNotaAss => "1";

		[RequestObjectPropertyName("Misure.IniValCaddAss")]
		public static string StaticRequestParamMisureIniValCaddAss => "1";

		[RequestObjectPropertyName("Misure.DataIniValMisura")]
		public static string StaticRequestParamMisureDataIniValMisura => "1";

		[RequestObjectPropertyName("Misure.Condizione")]
		public static string StaticRequestParamMisureCondizione => "1";

		[RequestObjectPropertyName("Misure.CodiceRegolamento")]
		public static string StaticRequestParamMisureCodiceRegolamento => "1";

		[RequestObjectPropertyName("Misure.CodicePaese")]
		public static string StaticRequestParamMisureCodicePaese => "1";

		[RequestObjectPropertyName("Misure.CodiceNotaAss")]
		public static string StaticRequestParamMisureCodiceNotaAss => "1";

		[RequestObjectPropertyName("Misure.CodiceNomenclaturaTar")]
		public static string StaticRequestParamMisureCodiceNomenclaturaTar => "1";

		[RequestObjectPropertyName("Misure.CodiceNomenclaturaNC")]
		public static string StaticRequestParamMisureCodiceNomenclaturaNC => "1";

		[RequestObjectPropertyName("Misure.AnnoRegolamento")]
		public static string StaticRequestParamMisureAnnoRegolamento => "1";

		[RequestObjectPropertyName("DatiGenerali.PRG")]
		public static string StaticRequestParamDatiGeneraliPRG => "1";

		[RequestObjectPropertyName("DatiGenerali.CodPaeseRegGrp")]
		public static string StaticRequestParamDatiGeneraliCodPaeseRegGrp => "1";

		[RequestObjectPropertyName("DATA_RIFERIMENTO")]
		public static string StaticRequestParamDATA_RIFERIMENTO => "1";

		[RequestObjectPropertyName("COD_PAESE_GRUPPI")]
		public static string StaticRequestParamCOD_PAESE_GRUPPI => "1";

		[RequestObjectPropertyName("ANNO_R")]
		public static string StaticRequestParamANNO_R => "1";

		[RequestObjectPropertyName("$STACK$")]
		public static string StaticRequestParamStack { get; private set; }

		public CertificateDownloadRequestObjectValues(string parameterString, IDictionary<string, string> certificateDictionary, string tariffcode, IDateTimeProvider dateTimeProvider)
		{
			Argument.NotNullOrEmpty(parameterString, nameof(parameterString));
			Argument.NotNull(certificateDictionary, nameof(certificateDictionary));
			Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));

			var paramArray = parameterString.Split(','); //example parameterString: "5, 1, -2, 100, 'C', '678', '14/12/2019'"
			_staticRequestParamUc = paramArray[0].Replace("'", string.Empty).Trim();
			_staticRequestParamSc = paramArray[1].Replace("'", string.Empty).Trim();
			_staticRequestParamSt = paramArray[2].Replace("'", string.Empty).Trim();
			_staticRequestParamLabel = paramArray[3].Replace("'", string.Empty).Trim();
			_certificateType = paramArray[4].Replace("'", string.Empty).Trim();
			_certificateNumber = paramArray[5].Replace("'", string.Empty).Trim();
			_dateAsString = paramArray[6].Replace("'", string.Empty).Trim();
			fullTariffCode = tariffcode;
			certificateDictionary.TryGetValue("Misure.CodiceNomenclaturaNC", out _tariffCode);
			certificateDictionary.TryGetValue("Misure.DataIniValMisura", out stackDateString);
			certificateDictionary.TryGetValue("MisureCodiceCadd", out misureCodiceCadd);
			StaticRequestParamStack = GetStaticRequestParamStack(dateTimeProvider);
		}

		public IDictionary<string, string> AsDictionary()
		{
			var customAttributeType = typeof(RequestObjectPropertyNameAttribute);
			var dictionary = new Dictionary<string, string>();

			var properties = GetType().GetProperties();
			foreach (var currentProperyInfo in properties)
			{
				var customAttributes = currentProperyInfo.GetCustomAttributes(customAttributeType)?.ToList();

				if (customAttributes != null && customAttributes.OfType<RequestObjectPropertyNameAttribute>().Any())
				{
					var propertyNameAttribute = customAttributes.OfType<RequestObjectPropertyNameAttribute>().First(pn => !string.IsNullOrEmpty(pn.PropertyName));

					dictionary.Add(propertyNameAttribute.PropertyName, currentProperyInfo.GetValue(this, null)?.ToString());
				}
				else
				{
					dictionary.Add(currentProperyInfo.Name, currentProperyInfo.GetValue(this, null)?.ToString());
				}
			}

			return dictionary;
		}

		readonly string _certificateType;
		readonly string _certificateNumber;
		readonly string _dateAsString;
		readonly string _tariffCode;
		readonly string stackDateString;
		readonly string fullTariffCode;
		readonly string misureCodiceCadd;
		readonly string _staticRequestParamUc;
		readonly string _staticRequestParamSt;
		readonly string _staticRequestParamSc;
		readonly string _staticRequestParamLabel;

		#region StaticRequestParamStack

		string GetStaticRequestParamStack(IDateTimeProvider dateTimeProvider)
		{
			return
				"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87" +
				"%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87N" +
				"%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%3Econdizioni%C3%87java.lang.String%C3%87PK%C3%87" +
				"%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87java.lang.String%C3%86" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87Misure.PaeseGruppoRegione%C3%87java.lang.String%C3%86ALL+%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87java.lang.String%C3%86" +
				WebUtility.UrlEncode(fullTariffCode.Substring(0, 8)) +
				"%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureFinder%C3%86null%C3%94null%C3%94ALL+%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" +
				"null%C3%94null%C3%94" +
				WebUtility.UrlEncode(fullTariffCode.Substring(0, 8)) + "%C3%94" +
				WebUtility.UrlEncode(fullTariffCode.Substring(8, 2)) + "%C3%94null%C3%94P%C3%941%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) + "%C3%94false%C3%94null%C3%94%C3%87java.lang.String%C3%87MODE%C3%87" +
				"%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87java.lang.String%C3%86P%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA" +
				"%C3%87%C3%87java.lang.String%C3%87PK%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureObject%C3%86null%C3%94null%C3%94" +
				WebUtility.UrlEncode(misureCodiceCadd) + "%C3%94" +
				WebUtility.UrlEncode(_tariffCode) +
				"%C3%9400%C3%94null%C3%941000%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94T%C3%94null%C3%94CSA" +
				"%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%941%C3%94null%C3%94%C3%94%C3%94%C3%94" +
				WebUtility.UrlEncode(stackDateString) + "%C3%94" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) + "%C3%94%C3%94%C3%94%C3%94null%C3%94null%C3%94null%C3%94null%C3%94%C3%94null%C3%94null" +
				"%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87java.lang.String%C3%86" +
				WebUtility.UrlEncode(fullTariffCode.Substring(8, 2)) + "%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3AMisureServlet%3A30%3A1%3A3%3A%C3%B5";
		}

		#endregion
	}
}
