using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using NUnit.Framework;
using NUnit.Framework.Internal;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.Utils
{
	[TestFixture]
	internal class StaticPageCrawlerTest
	{

		[Test]
		[Explicit("Developer Test for local integration")]
		public void TestGIP83()
		{
			using (var httpClient = new HttpClient())
			using (var stream = httpClient.GetStreamAsync(new Uri("https://www.cbsa-asfc.gc.ca/publications/cn-ad/cn19-20-eng.html"))?.Result)
			{
				var htmlDoc = new HtmlDocument();
				htmlDoc.Load(stream);
				var pNode = htmlDoc.DocumentNode.Descendants("p").FirstOrDefault(x => x.InnerText.Contains("The below aluminum products are subject to the aluminum import monitoring program. For a description of the commodity, please refer to the CBSA"));
				if (pNode != null)
				{
					var tableNode = pNode.SelectSingleNode("following-sibling::table");
					if (tableNode != null)
					{
						using (var file = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\StaticGIP83.txt")))
						{
							foreach (var tdNode in tableNode.Descendants("td"))
							{
								var pInsideTd = tdNode.SelectSingleNode("p");
								if (pInsideTd != null)
								{
									var tariffNumber = pInsideTd.InnerText.Trim();
									if (Regex.IsMatch(tariffNumber, @"^\d+(\.\d+)*\d$"))
									{
										tariffNumber = tariffNumber?.Replace(".", "") ?? string.Empty;
										tariffNumber = string.IsNullOrEmpty(tariffNumber) ? string.Empty : tariffNumber.PadRight(CARMConstants.TariffNumberLength, '0');
										file.WriteLine($"{tariffNumber},PGA,GAC,ALL,GIP83");
									}
								}
							}
						}
					}
				}
			}
		}

	}
}
