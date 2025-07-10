using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ContentRecordModifiedDateComparer : IComparer<HtmlNode>
	{
		public int Compare(HtmlNode x, HtmlNode y)
		{
			var modifiedDateX = GetModifiedDateFromNode(x);
			var modifiedDateY = GetModifiedDateFromNode(y);

			return modifiedDateX.CompareTo(modifiedDateY) * -1;
		}

		static DateTime GetModifiedDateFromNode(HtmlNode node)
		{
			Argument.NotNull(node, nameof(node));
			Argument.IsTrue(node.ParentNode?.ParentNode?.ParentNode != null, "node.ParentNode?.ParentNode?.ParentNode != null");

			var row = node.ParentNode?.ParentNode?.ParentNode;
			var modifiedDateNode = row.SelectSingleNode("td[contains(@class, 'cell-last-modification')]");
			if (modifiedDateNode != null)
			{
				return DateTime.Parse(modifiedDateNode.InnerText.Trim(), CultureInfo.InvariantCulture);
			}
			else
			{
				throw new ArgumentException("Cannot find last modification datetime.");
			}
		}
	}
}
