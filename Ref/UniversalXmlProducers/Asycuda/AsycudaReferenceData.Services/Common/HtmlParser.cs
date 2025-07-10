using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public static class HtmlParser
{
	public static IEnumerable<HtmlNode> FindNodes(string html, Predicate<HtmlNode> predicate)
	{
		var htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(html);
		return htmlDoc.DocumentNode.Descendants().Where(node => predicate(node));
	}
}
