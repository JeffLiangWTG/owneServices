using System;
using System.Web;
using CargoWise.RefDbRepo.Common.Argument;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public sealed class NationalRawAdditionalCode : IRawAdditionalCode
	{
		public NationalRawAdditionalCode(string rawHtml)
		{
			Argument.NotNullOrEmpty(rawHtml, nameof(rawHtml));
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(SanitizeHtml(rawHtml));
			documentNode = htmlDocument.DocumentNode;
		}

		readonly HtmlNode documentNode;

		string IRawAdditionalCode.Code => code ?? (code = GetCode());
		string code;

		string IRawAdditionalCode.Description => description ?? (description = SanitizeDescription(GetDescription()));
		string description;

		DateTime? IRawAdditionalCode.StartDate
		{
			get
			{
				if (!isStartDateInitialized)
				{
					startDate = GetStartDate();
					isStartDateInitialized = true;
				}
				return startDate;
			}
		}
		DateTime? startDate;
		bool isStartDateInitialized;

		DateTime? IRawAdditionalCode.EndDate
		{
			get
			{
				if (!isEndDateInitialized)
				{
					endDate = GetEndDate();
					isEndDateInitialized = true;
				}
				return endDate;
			}
		}
		DateTime? endDate;
		bool isEndDateInitialized;

		string GetCode() => documentNode.SelectSingleNode("//html//body//form[3]//table[1]//tbody[1]//tr[1]//td[1]")?.InnerText?.Replace("Codice Cadd:", "")?.Trim() ?? string.Empty;

		string GetDescription() => documentNode.SelectSingleNode("//html//body//form[3]//table[1]//tbody[1]//tr[6]//td[1]//textarea[1]")?.InnerText ?? string.Empty;

		DateTime? GetStartDate() => documentNode.SelectSingleNode("//html//body//form[3]//table[1]//tbody[1]//tr[3]//td[1]")?.InnerText?.Replace("Inizio validità:", "")?.Trim()?.ToDateTime();

		DateTime? GetEndDate() => documentNode.SelectSingleNode("//html//body//form[3]//table[1]//tbody[1]//tr[4]//td[1]")?.InnerText?.Replace("Fine validità:", "")?.Trim()?.ToDateTime();

		static string SanitizeHtml(string rawHtml) => HttpUtility.HtmlDecode(rawHtml);

		static string SanitizeDescription(string description) => description.Replace(Environment.NewLine, " ").Trim();
	}
}
