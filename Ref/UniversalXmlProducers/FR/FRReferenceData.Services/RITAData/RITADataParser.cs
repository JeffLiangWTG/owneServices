using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RITADataParser
	{
		static string ResponseDirectory => ApplicationConfig.Instance.DownloadDirectory;

		public static List<RefCusConditionType> GetFRConditionTypesFromHTML(string content)
		{
			var result = new List<RefCusConditionType>();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(content);

			var conditionTypeNodes = htmlDocument.DocumentNode.SelectNodes("//table[starts-with(@id, 'tabMesure')]//tbody//tr" );
			if (conditionTypeNodes != null && conditionTypeNodes.Count != 0)
			{
				foreach (var conditionTypeNode in conditionTypeNodes)
				{
					var conditionType = conditionTypeNode.ChildNodes[1].InnerText;
					var conditionTypeDescription = conditionTypeNode.ChildNodes[3].InnerText;
					if (!int.TryParse(conditionType, NumberStyles.None, CultureInfo.InvariantCulture, out _))
					{
						result.Add(new RefCusConditionType()
						{
							ZX2_ConditionType = conditionType,
							ZX2_Description = HttpUtility.HtmlDecode(conditionTypeDescription),
						});
					}
				}
			}

			return result;
		}

		public static List<RefCusTradeGroup> GetFRTradeGroupsFromHTML(RITADataProvider provider, string content)
		{
			var result = new List<RefCusTradeGroup>();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(content);

			var tradeGroupNodes = htmlDocument.DocumentNode.SelectNodes("//table[starts-with(@id, 'tabPaysRegion')]//tbody//tr");
			if (tradeGroupNodes != null && tradeGroupNodes.Count != 0)
			{
				foreach (var tradeGroupNode in tradeGroupNodes)
				{
					var tradeGroupCode = tradeGroupNode.ChildNodes[1].InnerText;
					var tradeGroupDescription = tradeGroupNode.ChildNodes[3].InnerText;
					var tradeGroupStartdate = tradeGroupNode.ChildNodes[7].InnerText;

					var tradeGroupCountryList = provider.GetFRTradeGroupCountries(tradeGroupCode);
					result.Add(new RefCusTradeGroup()
					{
						ZZA_TradeGroup = tradeGroupCode,
						ZZA_Description = HttpUtility.HtmlDecode(tradeGroupDescription),
						ZZA_StartDate = DateTime.ParseExact(tradeGroupStartdate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
						RefCusTradeGroupCountries = tradeGroupCountryList.ToArray()
					});
				}
			}

			return result;
		}

		public static List<RefCusTradeGroupCountry> GetFRTradeGroupCountriesFromHTML(string content)
		{
			var result = new List<RefCusTradeGroupCountry>();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(content);

			var tradeGroupCountryNodes = htmlDocument.DocumentNode.SelectNodes("//table[starts-with(@id, 'tabPaysRegion')]//tbody//tr");
			if (tradeGroupCountryNodes != null && tradeGroupCountryNodes.Count != 0)
			{
				foreach (var tradeGroupCountryNode in tradeGroupCountryNodes)
				{
					var tradeGroupCountryCode = tradeGroupCountryNode.ChildNodes[1].InnerText;
					var tradeGroupCountryDescription = tradeGroupCountryNode.ChildNodes[3].InnerText;
					var tradeGroupCountryStartdate = tradeGroupCountryNode.ChildNodes[7].InnerText;
					result.Add(new RefCusTradeGroupCountry()
					{
						ZZB_RN_NKTradeGroupCountryCode = tradeGroupCountryCode,
						ZZB_Description = HttpUtility.HtmlDecode(tradeGroupCountryDescription),
						ZZB_StartDate = DateTime.ParseExact(tradeGroupCountryStartdate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
					});
				}
			}
			return result;
		}

		public static List<RefCusTradeGroup> GetCountriesFromHTML(string content)
		{
			var result = new List<RefCusTradeGroup>();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(content);

			var tradeGroupCountryNodes = htmlDocument.DocumentNode.SelectNodes("//table[starts-with(@id, 'tabPaysRegion')]//tbody//tr");
			if (tradeGroupCountryNodes != null && tradeGroupCountryNodes.Count != 0)
			{
				foreach (var tradeGroupCountryNode in tradeGroupCountryNodes)
				{
					var tradeGroupCode = tradeGroupCountryNode.ChildNodes[1].InnerText;
					var tradeGroupDescription = tradeGroupCountryNode.ChildNodes[3].InnerText;
					var tradeGroupStartdate = tradeGroupCountryNode.ChildNodes[7].InnerText;
					var tradeGroupCountry = new RefCusTradeGroupCountry()
					{
						ZZB_RN_NKTradeGroupCountryCode = tradeGroupCode,
						ZZB_Description = HttpUtility.HtmlDecode(tradeGroupDescription),
						ZZB_StartDate = DateTime.ParseExact(tradeGroupStartdate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
					};
					var tradeGroupCountryList = new List<RefCusTradeGroupCountry>();
					tradeGroupCountryList.Add(tradeGroupCountry);
					result.Add(new RefCusTradeGroup()
					{
						ZZA_TradeGroup = tradeGroupCode,
						ZZA_Description = HttpUtility.HtmlDecode(tradeGroupDescription),
						ZZA_StartDate = DateTime.ParseExact(tradeGroupStartdate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
						RefCusTradeGroupCountries = tradeGroupCountryList.ToArray()
					});
				}
			}
			return result;
		}

		public static List<string> GetFRTariffsFromHTML(string content)
		{
			var result = new List<string>();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(content);

			var tariffNodes = htmlDocument.DocumentNode.SelectNodes("//table[starts-with(@id, 'resultatSuiviMesures')]//tbody//tr");
			if (tariffNodes != null && tariffNodes.Count != 0)
			{
				foreach (var tariffNode in tariffNodes)
				{
					result.Add(tariffNode.ChildNodes[1].InnerText.Trim().Substring(0, 10));
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The Exception are wrote in a console, which is a way to throw the real exception type.")]
		public List<Measure> GetMeasuresAndConditions(string tariff, string direction)
		{
			try
			{
				xmlMeasuresDocument.Load(Path.Combine(ResponseDirectory, tariff + "_M" + direction + ".XML"));
				xmlConditionsDocument.Load(Path.Combine(ResponseDirectory, tariff + "_C" + direction + ".XML"));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			FillRITADictionary(xmlMeasuresDocument, true);
			FillRITADictionary(xmlConditionsDocument, false);

			var tariffMeasureList = GetMeasures(xmlMeasuresDocument, direction);

			return tariffMeasureList;
		}

		void FillRITADictionary(XmlDocument xMLDocument, bool shouldClear)
		{
			if (shouldClear)
			{
				RitaDictionary.Clear();
			}
			foreach (string type in codeTypes)
			{
				XmlNodeList nodeList = xMLDocument.GetElementsByTagName(type);
				foreach (XmlNode node in nodeList)
				{
					//RITA error needs fixing
					if (type == "ED" && node.Attributes["code"].Value == "01")
					{
						node.InnerText = "% du montant";
					}

					node.InnerText = node.InnerText.Replace("'", "'");

					var dictionaryEntry = new RITADictionaryEntry(node.Attributes["code"].Value.Trim(), node.InnerText, type);
					RitaDictionary.Add(dictionaryEntry);
				}
			}
		}

		List<Measure> GetMeasures(XmlDocument xMLMeasuresDocument, string direction)
		{
			List<Measure> measuresWholeList = new List<Measure>();

			XmlNodeList measuresGroupList = xMLMeasuresDocument.GetElementsByTagName("MESURE");
			foreach (XmlNode measureGroup in measuresGroupList)
			{
				string measureType = measureGroup.Attributes["typ_mesu"].Value;

				XmlNodeList measuresList = measureGroup.ChildNodes;
				foreach (XmlNode measure in measuresList)
				{
					var sid = GetTagValue(measure, "SID");
					var tradeGroup = GetTagValue(measure, "GEO");
					List<string> excludedTradeGroups = GetTagValuesToList(measure, "GEO_EXCLU");
					var applicationTerritory = GetTagValue(measure, "TERR_APPLI");
					var taxCode = GetTagValue(measure, "COD_CPTA");
					var taxCodeDescription = GetDescription(taxCode, "CC");
					var regulation = GetTagValue(measure, "REGL");
					var startDate = UniversalDataHelper.GetStartDateFromTag(measure, "DEB");
					var endDate = UniversalDataHelper.GetEndDateFromTag(measure, "FIN");
					var quotaNumber = GetTagValue(measure, "CONTINGENT");
					var supplementaryCode = GetTagValue(measure, "COD_ADD");
					var supplementaryCodeDescription = GetDescription(supplementaryCode, "CA");
					var nomenclature = GetTagValue(measure, "NOMENC");
					var renvois = GetTagValuesToList(measure, "RENVOI");
					var preferences = GetTagValuesToList(measure, "PREF");
					var components = GetComponents(measure);
					var conditions = GetConditions(sid);
					var measureClass = MeasureHelper.GetMeasureClass(measureType);

					measuresWholeList.Add(new Measure(sid, measureType, measureClass, tradeGroup, excludedTradeGroups, applicationTerritory, taxCode, taxCodeDescription, regulation
														, startDate, endDate, quotaNumber, supplementaryCode, supplementaryCodeDescription, nomenclature, direction, renvois, preferences
														, components, conditions));
				}
			}
			return measuresWholeList;
		}

		List<Component> GetComponents(XmlNode node)
		{
			List<Component> result = new List<Component>();
			XmlNodeList nodeList = node.SelectNodes("COMPOSANT");
			foreach (XmlNode composantNode in nodeList)
			{
				var dutyExpression = GetTagValue(composantNode, "EXPR_DROIT");
				decimal? amount = GetDecimalValue(composantNode, "MONT_DROIT");
				string currency = GetTagValue(composantNode, "COD_MONET");
				string measurementCode = GetTagValue(composantNode, "COD_MESA");
				string measurementCodeDescription = GetDescription(measurementCode, "CMS");
				string qualifier = GetTagValue(composantNode, "QUALIF_COD_MESA");
				string qualifierDescription = GetDescription(qualifier, "QCM");
				Component newComponent = new Component(dutyExpression, amount, currency, measurementCode, measurementCodeDescription, qualifier, qualifierDescription);
				result.Add(newComponent);
			}
			return result;
		}

		List<Condition> GetConditions(string sid)
		{
			List<Condition> measureConditionList = new List<Condition>();

			XmlNodeList measuresList = xmlConditionsDocument.GetElementsByTagName("MESURE");
			foreach (XmlNode measure in measuresList)
			{
				if (measure.Attributes["sid"].Value != sid)
				{
					continue;
				}

				XmlNodeList conditionsList = measure.ChildNodes;

				foreach (XmlNode condition in conditionsList)
				{
					string code = GetTagValue(condition, "COND_COD").Trim();
					string taxCode = GetTagValue(condition, "COD_CPTA");
					int? sequenceNumber = GetIntValue(condition, "NUM_SEQ");
					string description = GetDescription(code, "CD");
					string documentCode = GetTagValue(condition, "DOC_CODE");
					string documentType = GetTagValue(condition, "STATUT_DOC") == "0" ? Condition.supDocType : Condition.dtpDocType;
					string action = GetTagValue(condition, "ACT_CODE").Trim();
					decimal? amount = GetDecimalValue(condition, "MONT_DROIT");
					string measurementCode = GetTagValue(condition, "COD_MESA");
					string measurementCodeDescription = GetDescription(measurementCode, "CMS");
					string qualifier = GetTagValue(condition, "QUALIF_COD_MESA");
					string qualifierDescription = GetDescription(qualifier, "QCM");

					List<Component> components = GetComponents(condition);
					if (!(code == "B" && action == "07"))
					{
						measureConditionList.Add(new Condition(code, taxCode, sequenceNumber, description, documentCode, documentType, action, amount, measurementCode, measurementCodeDescription, qualifier, qualifierDescription, components));
					}
				}
			}

			foreach (var condition in measureConditionList)
			{
				condition.IsRateFormula = MeasureHelper.IsRateFormula(measureConditionList, condition.Code);
			}

			return measureConditionList;
		}

		public static string GetErrorFromWebServiceResponse(string responseContent)
		{
			var result = string.Empty;

			responseContent = responseContent.Replace("+apos;", "&apos;");
			responseContent = HttpUtility.HtmlDecode(responseContent);
			var descriptionTag = responseContent.Contains("<Description>") ? "<Description>" : "<ltDescription>";
			var descriptionClosingTag = responseContent.Contains("</Description>") ? "</Description>" : "</ltDescription>";
			var startPosition = responseContent.IndexOf(descriptionTag, StringComparison.InvariantCultureIgnoreCase) + descriptionTag.Length;
			var endPosition = responseContent.IndexOf(descriptionClosingTag, StringComparison.InvariantCultureIgnoreCase);
			result = responseContent.Substring(startPosition, endPosition - startPosition);

			return result;
		}

		static string GetTagValue(XmlNode node, string tag)
		{
			XmlNode tagNode = node.SelectSingleNode(tag);
			if (tagNode == null)
			{
				return "";
			}
			else
			{
				return tagNode.InnerText;
			}
		}

		public static decimal? GetDecimalValue(XmlNode node, string tag)
		{
			decimal? result = null;

			var style = NumberStyles.AllowDecimalPoint;
			var provider = new CultureInfo("fr-FR");

			var tagValue = GetNullableTagValue(node, tag);
			if (tagValue != null)
			{
				result = decimal.Parse(tagValue, style, provider);
			}

			return result;
		}

		static int? GetIntValue(XmlNode node, string tag)
		{
			int? result = null;

			var tagValue = GetNullableTagValue(node, tag);
			if (tagValue != null)
			{
				result = int.Parse(tagValue, CultureInfo.InvariantCulture);
			}

			return result;
		}

		static string GetNullableTagValue(XmlNode node, string tag)
		{
			XmlNode tagNode = node.SelectSingleNode(tag);
			return tagNode != null ? tagNode.InnerText : null;
		}

		static List<string> GetTagValuesToList(XmlNode node, string tag)
		{
			List<string> result = new List<string>();
			XmlNodeList nodeList = node.SelectNodes(tag);
			foreach (XmlNode tagNode in nodeList)
			{
				result.Add(tagNode.InnerText);
			}
			return result;
		}

		public string GetDescription(string code, string type)
		{
			return RitaDictionary.FirstOrDefault(c => (c.Code == code && c.Type == type))?.Description ?? "";
		}

		readonly XmlDocument xmlMeasuresDocument = new XmlDocument();
		readonly XmlDocument xmlConditionsDocument = new XmlDocument();
		readonly List<string> codeTypes = new List<string> { "ED", "CMS", "CMT", "CO", "CC", "CA", "R", "G", "P", "TM", "CD", "D", "A", "TD", "QCM" };

		public List<RITADictionaryEntry> RitaDictionary => ritaDictionary ?? (ritaDictionary = new List<RITADictionaryEntry>());
		List<RITADictionaryEntry> ritaDictionary;
	}
}
