using System;
using System.Linq;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ContentRecordModifiedDateComparerFixture
	{
		[Test]
		public void TestComparer()
		{
			var comparer = new ContentRecordModifiedDateComparer();

			var oldHtmlNode = GetNode(new DateTime(2021, 12, 29, 13, 54, 00));
			var newHtmlNode = GetNode(new DateTime(2021, 12, 30, 17, 49, 00));
			Assert.DoesNotThrow(() => comparer.Compare(oldHtmlNode, newHtmlNode));
			var ex = Assert.Throws<ArgumentException>(() => comparer.Compare(oldHtmlNode.FirstChild, newHtmlNode));
			Assert.That(ex.Message.StartsWith("Cannot find last modification datetime."));

			Assert.Multiple(() =>
			{
				Assert.That(comparer.Compare(oldHtmlNode, newHtmlNode), Is.EqualTo(1));
				Assert.That(comparer.Compare(newHtmlNode, oldHtmlNode), Is.EqualTo(-1));
				Assert.That(comparer.Compare(oldHtmlNode, oldHtmlNode), Is.EqualTo(0));
			});
		}

		[Test]
		public void TestListComparer()
		{
			var nowNode = GetNode(DateTime.Now);
			var futureNode = GetNode(DateTime.Now.AddDays(1));
			var pastNode = GetNode(DateTime.Now.AddMinutes(-1));

			var orderedList = new[] { nowNode, futureNode, pastNode }.OrderBy(x => x, new ContentRecordModifiedDateComparer());

			Assert.That(orderedList, Is.EqualTo(new[] { futureNode, nowNode, pastNode }));
		}

		HtmlNode GetNode(DateTime dateTime)
		{
			var rawHtml = $@"<tr class=""row"">
  <td class=""cell-left-border""></td>
  <td class=""cell-checkbox""></td>
  <td class=""cell-icon col-hidable cell-icon--file"">
    <div class=""icon""></div>
  </td>
  <td class=""cell-file-name"">
    <div class=""file-name"">
      <a>Declarable codes.xlsx</a>
    </div>
  </td>
  <ul class=""actions"">
    <li><a href= ""/"" >Download</a></li>
    <li><a id=""copyContentId"">Copy</a></li>
    <li><a id= ""DetailContentId"" href = ""/details""> Details </a></li>
    <li></li>
  </ul>
  <td class=""cell - title"">
  </td>
  <td class=""cell-last-modification"">
    <span class=""date"">{dateTime:yyyy MM dd, HH:mm}</span>
  </td>
  <td class=""col-hidable"">1.2</td>
  <td class=""col-hidable"">763.46 KB</td>  
</tr>";
			return HtmlNode.CreateNode(rawHtml).SelectSingleNode("td[contains(@class, 'cell-file-name')]").ChildNodes[1].ChildNodes[1];
		}
	}
}
