using System;
using System.Net;
using HtmlAgilityPack;

namespace FsisEstNumbersCrawler.Services
{
	public interface IHtmlParser
	{
		HtmlNode FindNode(string url, Func<HtmlNode, bool> predicate, WebClient webClient);
		HtmlNode FindNode(HtmlNode node, Func<HtmlNode, bool> predicate);

		string GetAttributeValue(HtmlNode node, string attributeName);
	}
}
