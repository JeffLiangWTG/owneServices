using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public class ScrappedRecord : IScrappedRecord
	{
		public MeasureData Measure { get; }
		public IEnumerable<RequirementData> Requirement { get; }
		public string RequirementHtml { get; }

		public ScrappedRecord(string measureHtml, string requirementHtml)
		{
			Argument.NotNullOrEmpty(measureHtml, nameof(measureHtml));
			Argument.NotNullOrEmpty(requirementHtml, nameof(requirementHtml));

			Measure = GetMeasureData(CleanHtml(measureHtml));
			Requirement = GetRequirementData(CleanHtml(requirementHtml));
			RequirementHtml = measureHtml;
		}

		public static string CleanHtml(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			var pattern = new Regex("[\t\r\n]|&nbsp;");
			var output = pattern.Replace(input, string.Empty);
			return output.Trim();
		}

		static MeasureData GetMeasureData(string measureHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(measureHtml);

			var cleanMeasureHtml = htmlDoc.DocumentNode.InnerText;

			var descriptionPattern = new Regex("^[^\\(]+");
			var tradeGroupPattern = new Regex("(?<=\\()(.*?)(?=\\))");
			var formulaPattern = new Regex("(?<=:)(.+)");

			if (!TryGetValuesFromRegEx(cleanMeasureHtml, descriptionPattern, out List<string> descriptionValues)
			 || !TryGetValuesFromRegEx(cleanMeasureHtml, tradeGroupPattern, out List<string> tradeGroupValues))
			{
				return null;
			}

			var hasFormula = TryGetValuesFromRegEx(cleanMeasureHtml, formulaPattern, out List<string> formulaValues);

			var description = descriptionValues.First();

			string formula = null;
			if (hasFormula)
			{
				formula = formulaValues.First();
			}

			return new MeasureData(description, tradeGroupValues.ToArray(), formula);
		}

		static IEnumerable<RequirementData> GetRequirementData(string requirementHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(requirementHtml);

			var descirptionNodes = htmlDoc.DocumentNode?.SelectNodes("//a");

			if (descirptionNodes == null)
			{
				return null;
			}

			var requirements = from descirptionNode in descirptionNodes
							   where !string.IsNullOrEmpty(descirptionNode.InnerText) &&
								   descirptionNode.PreviousSibling != null &&
								   descirptionNode.PreviousSibling.NodeType == HtmlNodeType.Text &&
								   !string.IsNullOrEmpty(descirptionNode.PreviousSibling.InnerText)
							   select new RequirementData(descirptionNode.PreviousSibling.InnerText, descirptionNode.InnerText);

			return requirements;
		}

		static bool TryGetValuesFromRegEx(string input, Regex expression, out List<string> values)
		{
			Argument.NotNullOrEmpty(input, nameof(input));
			Argument.NotNull(expression, nameof(expression));

			var matches = expression.Matches(input);
			if (matches.Count <= 0)
			{
				values = null;
				return false;
			}

			values = (from Match match in matches select match.Value).ToList();
			return true;
		}
	}
}
