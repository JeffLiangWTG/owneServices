using System.Linq;
using HtmlAgilityPack;

namespace Enterprise.MarketingManager.GUI.Testing
{
	internal static class HtmlTestHelper
	{
		internal static HtmlNode LoadHtml(string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			return doc.DocumentNode;
		}

		internal static bool HtmlDocumentsAreEqual(HtmlNode node1, HtmlNode node2)
		{
			if (node1 == null || node2 == null)
			{
				return node1 == node2;
			}

			if (node1.Name != node2.Name)
			{
				return false;
			}

			if (node1.InnerText.Trim() != node2.InnerText.Trim())
			{
				return false;
			}

			var attrs1 = node1.Attributes.OrderBy(a => a.Name).ToList();
			var attrs2 = node2.Attributes.OrderBy(a => a.Name).ToList();

			if (attrs1.Count != attrs2.Count)
			{
				return false;
			}

			for (var i = 0; i < attrs1.Count; i++)
			{
				if (attrs1[i].Name != attrs2[i].Name ||
					attrs1[i].Value != attrs2[i].Value)
				{
					return false;
				}
			}

			if (node1.ChildNodes.Count != node2.ChildNodes.Count)
			{
				return false;
			}

			for (var i = 0; i < node1.ChildNodes.Count; i++)
			{
				if (!HtmlDocumentsAreEqual(node1.ChildNodes[i], node2.ChildNodes[i]))
				{
					return false;
				}
			}

			return true;
		}
	}
}
