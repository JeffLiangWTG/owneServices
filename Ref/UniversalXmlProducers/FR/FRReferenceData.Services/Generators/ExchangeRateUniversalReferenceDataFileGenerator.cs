using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class ExchangeRateUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefExchangeRateZZ>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.CurrenciesFileName;
				yield return ApplicationConfig.Instance.CurrencyPricesFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRExchangeOutputFile;

		protected override List<RefExchangeRateZZ> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefExchangeRateZZ>();

			var currencyDictionary = GetCurrencyDictionary(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.CurrenciesFileName));

			XmlDocument sourceXmlDocument = new XmlDocument();
			sourceXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.CurrencyPricesFileName));
			XmlNodeList nodeList = sourceXmlDocument.GetElementsByTagName("ligne");

			foreach (XmlNode node in nodeList)
			{
				var id = UniversalDataHelper.GetTagValue(node, "CHAMP1");
				var startDate = UniversalDataHelper.GetStartDateFromTag(node, "CHAMP3");
				var maximumDateTime = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
				var endDate = UniversalDataHelper.GetEndDateFromTag(node, "CHAMP4", maximumDateTime);
				
				var rate = decimal.Parse(UniversalDataHelper.GetTagValue(node, "CHAMP2").Replace(".", ","), new CultureInfo("fr-FR"));

				if (currencyDictionary.TryGetValue(id, out var currencyCode))
				{
					DateTime firstDataDate = new DateTime(publicationDate.Year, publicationDate.Month, 1);

					if (startDate >= firstDataDate && UniversalDataHelper.CheckDatesAreValid(startDate, endDate) && UniversalDataHelper.CheckRateIsValid(rate))
					{
						result.Add(new RefExchangeRateZZ()
						{
							ZZN_RX_NKExCurrency = currencyCode,
							ZZN_StartDate = startDate,
							ZZN_EndDate = endDate,
							ZZN_Rate = rate
						});
					}
				}
			}

			return result.ToList();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var rate = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			rate.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			rate.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, UniversalDataHelper.Constants.France);
			rate.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			rate.IncludeColumn(x => x.ZZN_EndDate, false);
			rate.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			rate.IncludeColumn(x => x.ZZN_Rate, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(rate);

			return xmlWriterConfiguration;
		}

		static Dictionary<string, string> GetCurrencyDictionary(string xmlSource)
		{
			var result = new Dictionary<string, string>();

			XmlDocument sourceXmlDocument = new XmlDocument();
			sourceXmlDocument.Load(xmlSource);
			XmlNodeList nodeList = sourceXmlDocument.GetElementsByTagName("ligne");

			foreach (XmlNode node in nodeList)
			{
				var id = UniversalDataHelper.GetTagValue(node, "CHAMP1");
				if (!result.ContainsKey(id))
				{
					result.Add(id, HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, "CHAMP2")));
				}
			}

			return result;
		}

		public override string DataSource => "FR Exchange Rate";
	}
}
