using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public class TariffDownloadRequestObjectValues
	{
		[RequestObjectPropertyName("Misure.CodiceNomenclaturaNC")]
		public string CodeNc
		{
			get
			{
				return _rawCusTariffCode.Substring(0, 8);
			}
		}

		[RequestObjectPropertyName("Misure.CodiceNomenclaturaTar")]
		public string CodeTar
		{
			get
			{
				return _rawCusTariffCode.Substring(8, 2);
			}
		}

		[RequestObjectPropertyName("Misure.PaeseGruppoRegione")]
		public static string Region => "ALL";

		[RequestObjectPropertyName("SC")]
		public static string StaticRequestParamSc => "1";

		[RequestObjectPropertyName("ST")]
		public static string StaticRequestParamSt => "2";

		[RequestObjectPropertyName("UC")]
		public static string StaticRequestParamUc => "30";

		[RequestObjectPropertyName("Label")]
		public static string StaticRequestParamLabel => "102";

		[RequestObjectPropertyName("$STACK$")]
		public string StaticRequestParamStack { get; }

		public TariffDownloadRequestObjectValues(string cusTariffCode, IDateTimeProvider dateTimeProvider)
		{
			Argument.NotNullOrEmpty(cusTariffCode, nameof(cusTariffCode));
			Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));

			_rawCusTariffCode = cusTariffCode;
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

		readonly string _rawCusTariffCode;

		#region StaticRequestParamStack

		static string GetStaticRequestParamStack(IDateTimeProvider dateTimeProvider)
		{
			return
				"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" +
				WebUtility.UrlEncode(dateTimeProvider.Now.ToItalianShortDateString()) +
				"%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String" +
				"%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String" +
				"%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea" +
				"%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String" +
				"%C3%87Misure.PaeseGruppoRegione%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87MODE%C3%87" +
				"%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA%C3%87%C3%87java.lang.String" +
				"%C3%87PK%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5";
		}

		#endregion
	}
}
