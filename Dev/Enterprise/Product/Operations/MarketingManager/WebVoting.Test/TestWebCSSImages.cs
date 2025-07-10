using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	sealed class TestWebCSSImages : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmbeddedDefaultFiles()
		{
			var info = new WebCSSImages();
			AssertEquals(6, info.EmbeddedImagesForRegistryConfiguration().Count);

			var count = 0;
			var projectXml = new XmlDocument();
			projectXml.Load(BaseSourcePath + @"Enterprise\Product\Operations\MarketingManager\WebVoting\Enterprise.MarketingManager.WebVoting.csproj");
			var projectElement = projectXml.DocumentElement;
			foreach (XmlNode node in projectElement.ChildNodes)
			{
				if (node.Name.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase))
				{
					if
					(
						node.ChildNodes.Cast<XmlNode>().Any
						(
							x => x.Name == "EmbeddedResource" &&
								(x.Attributes["Include"] ?? x.Attributes["Update"]).InnerText.Equals("BaseStyle.css", StringComparison.OrdinalIgnoreCase)
						)
					)
					{
						count++;
					}
					foreach (var config in info.EmbeddedImagesForRegistryConfiguration())
					{
						if
						(
							node.ChildNodes.Cast<XmlNode>().Any
							(
								x => x.Name == "EmbeddedResource" &&
									(x.Attributes["Include"] ?? x.Attributes["Update"]).InnerText.Equals(@"Images\" + config, StringComparison.OrdinalIgnoreCase)
							)
						)
						{
							count++;
						}
					}
				}
			}
			AssertEquals(7, count);
		}
	}
}
