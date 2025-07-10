using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.Argument;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax
{
	public static class WebSurTaxParser
	{
		public static IEnumerable<WebSurTaxText> Parse(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));

			var htmlDoc = new HtmlDocument();
			htmlDoc.Load(stream);
			foreach (var table in htmlDoc.DocumentNode.SelectNodes(@"//div[@id='mainContent']/article/table"))
			{
				var result = new WebSurTaxText();
				result.Table = table.SelectSingleNode("caption").InnerText.Trim(new[] { '\t', '\r', '\n', ' ' });
				var regex = new Regex(@"[0-9]{4}\.[0-9]{2}\.[0-9]{2}");
				var matches = regex.Matches(table.InnerText);
				result.Classifications = matches.Cast<Match>().Select(x => x.Value).ToArray();
				yield return result;
			}
		}
	}
}
