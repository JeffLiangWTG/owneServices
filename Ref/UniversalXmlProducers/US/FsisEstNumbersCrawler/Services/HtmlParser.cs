using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace FsisEstNumbersCrawler.Services
{
	public class HtmlParser : IHtmlParser
	{
		public HtmlNode FindNode(string url, Func<HtmlNode, bool> predicate, WebClient webClient)
		{
			var html = webClient.DownloadString(url);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			return doc.DocumentNode.Descendants().FirstOrDefault(predicate);
		}

		public HtmlNode FindNode(HtmlNode node, Func<HtmlNode, bool> predicate)
		{
			return node.ParentNode.Descendants().FirstOrDefault(predicate);
		}

		public static IEnumerable<HtmlNode> FindNodes(string url, Func<HtmlNode, bool> predicate)
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var html = webClient.DownloadString(url);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				return doc.DocumentNode.Descendants().Where(predicate);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}

		public string GetAttributeValue(HtmlNode node, string attributeName)
		{
			var attributeValue = node.GetAttributeValue(attributeName, string.Empty);
			return string.IsNullOrEmpty(attributeValue) ? null : attributeValue;
		}
	}
}
