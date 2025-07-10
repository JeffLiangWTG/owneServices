using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public class CertificateLinkDownloadRequestObjectValues
	{
		[RequestObjectPropertyName("Misure.CodiceNomenclaturaNC")]
		public string CodeNc
		{
			get
			{
				return _cusTariffCode;
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

		[RequestObjectPropertyName("Misure.TipoRegolamento")]
		public static string StaticRequestParamTipoRegolamento => "1";

		[RequestObjectPropertyName("Misure.TipoNotaAss")]
		public static string StaticRequestParamTipoNotaAss => "1";

		[RequestObjectPropertyName("Misure.TipoCertificato")]
		public static string StaticRequestParamTipoCertificato => "1";

		[RequestObjectPropertyName("Misure.SidMisura")]
		public static string StaticRequestParamSidMisura => "1";

		[RequestObjectPropertyName("Misure.SidDesNotaAss")]
		public static string StaticRequestParamSidDesNotaAss => "1";

		[RequestObjectPropertyName("Misure.SidCadd")]
		public static string StaticRequestParamSidCadd => "1";

		[RequestObjectPropertyName("Misure.RuoloRegolamento")]
		public static string StaticRequestParamRuoloRegolamento => "1";

		[RequestObjectPropertyName("Misure.NumeroCertificato")]
		public static string StaticRequestParamNumeroCertificato => "1";

		[RequestObjectPropertyName("Misure.InizioValDescrizioneNotaAss")]
		public static string StaticRequestParamInizioValDescrizioneNotaAss => "1";

		[RequestObjectPropertyName("Misure.IniValCaddAss")]
		public static string StaticRequestParamIniValCaddAss => "1";

		[RequestObjectPropertyName("Misure.DataIniValMisura")]
		public string StaticRequestParamDataIniValMisura
		{
			get
			{
				return _dateString;
			}
		}

		[RequestObjectPropertyName("Misure.Condizione")]
		public static string StaticRequestParamCondizione => "1";

		[RequestObjectPropertyName("Misure.CodiceRegolamento")]
		public static string StaticRequestParamCodiceRegolamento => "1";

		[RequestObjectPropertyName("Misure.CodiceNotaAss")]
		public static string StaticRequestParamCodiceNotaAss => "1";

		[RequestObjectPropertyName("Misure.CodiceNomenclaturaTar")]
		public string StaticRequestParamCodiceNomenclaturaTar
		{
			get
			{
				return _staticRequestParamCodiceNomenclaturaTar;
			}
		}

		[RequestObjectPropertyName("Misure.AnnoRegolamento")]
		public static string StaticRequestParamAnnoRegolamento => "1";

		[RequestObjectPropertyName("DatiGenerali.PRG")]
		public static string StaticRequestParamDatiGeneraliPRG => "1";

		[RequestObjectPropertyName("DatiGenerali.CodPaeseRegGrp")]
		public static string StaticRequestParamDatiGeneraliCodPaeseRegGrp => "1";

		[RequestObjectPropertyName("DATA_RIFERIMENTO")]
		public static string StaticRequestParamDATA_RIFERIMENTO => "1";

		[RequestObjectPropertyName("COD_PAESE_GRUPPI")]
		public static string StaticRequestParamCOD_PAESE_GRUPPI => "1";

		[RequestObjectPropertyName("CertificatoNaz.DataInizioValiditaDescrizione")]
		public static string StaticRequestParamCertificatoNazDataInizioValiditaDescrizione => "1";

		[RequestObjectPropertyName("ANNO_R")]
		public static string StaticRequestParamANNO_R => "1";

		[RequestObjectPropertyName("$STACK$")]
		public string StaticRequestParamStack { get; }

		[RequestObjectPropertyName("Misure.TipoMisura")]
		public string Type
		{
			get
			{
				return _type;
			}
		}

		[RequestObjectPropertyName("Misure.CodicePaese")]
		public string StaticRequestParamCodicePaese
		{
			get
			{
				return _staticRequestParamCodicePaese;
			}
		}

		[RequestObjectPropertyName("MisureCodiceCadd")]
		public string StaticRequestParamCertificateCode
		{
			get
			{
				return _certificateCode;
			}
		}

		[RequestObjectPropertyName("Misure.TipoCadd")]
		public string StaticRequestParamCodiceCertificateType
		{
			get
			{
				return _certificateType;
			}
		}

		public CertificateLinkDownloadRequestObjectValues(string requirementHtml, IDateTimeProvider dateTimeProvider)
		{
			Argument.NotNull(requirementHtml, nameof(requirementHtml));
			Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));

			var certificateLink = Regex.Match(requirementHtml, certificateLinkRegex);
			if (certificateLink.Groups.Count == 4)
			{
				var paramArray = certificateLink.Groups[2].Value.Split(','); //example regex match: 30,1,-2,6,'CSA','1000','07020000','00','T','010','29/01/2013'
				_staticRequestParamUc = paramArray[0].Replace("'", string.Empty).Trim();
				_staticRequestParamSc = paramArray[1].Replace("'", string.Empty).Trim();
				_staticRequestParamSt = paramArray[2].Replace("'", string.Empty).Trim();
				_staticRequestParamLabel = paramArray[3].Replace("'", string.Empty).Trim();
				_type = paramArray[4].Replace("'", string.Empty).Trim();
				_staticRequestParamCodicePaese = paramArray[5].Replace("'", string.Empty).Trim();
				_cusTariffCode = paramArray[6].Replace("'", string.Empty).Trim();
				_staticRequestParamCodiceNomenclaturaTar = paramArray[7].Replace("'", string.Empty).Trim();
				_certificateType = paramArray[8].Replace("'", string.Empty).Trim();
				_certificateCode = paramArray[9].Replace("'", string.Empty).Trim();
				_dateString = paramArray[10].Replace("'", string.Empty).Trim();
				StaticRequestParamStack = GetStaticRequestParamStack(dateTimeProvider);
			}
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

		readonly string _cusTariffCode;
		readonly string _certificateType;
		readonly string _certificateCode;
		readonly string _type;
		readonly string _dateString;
		const string certificateLinkRegex = "(<a href=\"javascript:linkToPostKeyBill\\('MisureServlet',)(.*)(\\)\">Certificato</a>)";
		readonly string _staticRequestParamUc;
		readonly string _staticRequestParamSt;
		readonly string _staticRequestParamSc;
		readonly string _staticRequestParamLabel;
		readonly string _staticRequestParamCodicePaese;
		readonly string _staticRequestParamCodiceNomenclaturaTar;

		#region StaticRequestParamStack

		string GetStaticRequestParamStack(IDateTimeProvider dateTimeProvider)
		{
			return
				"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String" +
				"%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String" +
				"%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%C3%87java.lang.String%C3%87GlobalArea%C3%871" +
				"%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87java.lang.String%C3%86" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87Misure.PaeseGruppoRegione%C3%87java.lang.String%C3%86ALL+%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87java.lang.String%C3%86" +
				WebUtility.UrlEncode(_cusTariffCode) + "%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureFinder%C3%86null%C3%94null%C3%94ALL+" +
				"%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" +
				WebUtility.UrlEncode(_cusTariffCode) + "%C3%9499%C3%94null%C3%94P%C3%941%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) + "%C3%94false%C3%94null%C3%94%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String" +
				"%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PRG%C3%87java.lang.String%C3%86P%C3%87java.lang.String%C3%87PKNOTA%C3%87%C3%87java.lang.String%C3%87PK%C3%87" +
				"%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87java.lang.String%C3%8699%C3%87%C3%B5";
		}

		#endregion
	}
}
