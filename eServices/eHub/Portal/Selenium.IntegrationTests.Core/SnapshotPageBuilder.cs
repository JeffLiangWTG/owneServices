using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace CargoWise.eHub.Selenium.IntegrationTests.Core
{
	public class SnapshotPageBuilder
	{
		readonly string[] linkPaths;
		readonly string[] preservedNodesXpaths;
		readonly string referencePath;

		public SnapshotPageBuilder(string referencePath, string[] linkPaths, string[] preservedNodesXpaths)
		{
			this.linkPaths = linkPaths;
			this.preservedNodesXpaths = preservedNodesXpaths;
			this.referencePath = referencePath;
		}

		public HtmlDocument SaveSnapShotOfHtml(string html)
		{
			HtmlDocument result = new HtmlDocument();
			result.OptionFixNestedTags = true;
			result.LoadHtml(html);
			ModifyHtmlLink(result);

			var headPreserve = GetHeadLinkAndMeta(result);
			KeepOnlyPreservations(result.DocumentNode.SelectSingleNode(".//head"), headPreserve);

			var bodyPerserve = GetPreservedHtmlNodes(result);
			KeepOnlyPreservations(result.DocumentNode.SelectSingleNode(".//body"), bodyPerserve);
			return result;
		}

		void KeepOnlyPreservations(HtmlNode htmlNode, List<HtmlNodeCollection> preservations)
		{
			htmlNode.RemoveAllChildren();
			foreach (var collection in preservations)
			{
				foreach (var node in collection)
				{
					htmlNode.ChildNodes.Add(node);
				}
			}
		}

		List<HtmlNodeCollection> GetPreservedHtmlNodes(HtmlDocument htmlDoc)
		{
			List<HtmlNodeCollection> result = new List<HtmlNodeCollection>();
			foreach (var xpath in preservedNodesXpaths)
			{
				var nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
				if (nodes != null)
				{
					result.Add(nodes);
				}
			}
			return result;
		}

		List<HtmlNodeCollection> GetHeadLinkAndMeta(HtmlDocument xml)
		{
			List<HtmlNodeCollection> result = new List<HtmlNodeCollection>(2);
			result.Add(xml.DocumentNode.SelectNodes(".//html/head/link"));
			result.Add(xml.DocumentNode.SelectNodes(".//html/head/meta"));
			return result;
		}

		void ModifyHtmlLink(HtmlDocument htmlDoc)
		{
			var links = htmlDoc.DocumentNode.SelectNodes(".//html/head/link");
			if (links.Count != linkPaths.Length)
				throw new ArgumentException("Link count in the input is not equal to link count on the page");

			for (int i = 0; i < links.Count; i++)
			{
				links[i].SetAttributeValue("href", referencePath + linkPaths[i]);
			}
		}

	}
}
