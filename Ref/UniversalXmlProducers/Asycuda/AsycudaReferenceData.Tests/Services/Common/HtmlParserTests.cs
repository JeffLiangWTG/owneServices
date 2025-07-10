using System;
using System.Linq;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services;

[TestFixture]
public class HtmlParserTests
{
	[Test]
	public void FindNodes_ShouldReturnMatchingNodes()
	{
		// Arrange
		var html = "<html><body><a href='https://example.com/file1.pdf'>PDF1</a><a href='https://example.com/file2.pdf'>PDF2</a></body></html>";
		Predicate<HtmlNode> pattern = node => node.Name == "a" && node.GetAttributeValue("href", "").EndsWith(".pdf");

		// Act
		var nodes = HtmlParser.FindNodes(html, pattern).ToList();

		// Assert
		Assert.NotNull(nodes);
		Assert.That(nodes.Count, Is.EqualTo(2));
		Assert.That(nodes[0].GetAttributeValue("href", ""), Is.EqualTo("https://example.com/file1.pdf"));
		Assert.That(nodes[1].GetAttributeValue("href", ""), Is.EqualTo("https://example.com/file2.pdf"));
	}
}
