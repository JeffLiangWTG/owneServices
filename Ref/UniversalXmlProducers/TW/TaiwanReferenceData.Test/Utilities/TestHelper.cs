using System;
using NUnit.Framework;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	public static class TestHelper
	{
		public static void AssertXMLEquals(string expectedXML, string actualXML, string errorMessage = "")
		{
			var builder = DiffBuilder.Compare(expectedXML)
					.WithTest(actualXML)
					.IgnoreWhitespace()
					.WithNodeMatcher(new DefaultNodeMatcher(ElementSelectors.ByNameAndText))
					.Build();
			Assert.IsFalse(builder.HasDifferences(), $"{errorMessage}. Differences: {string.Join(Environment.NewLine, builder.Differences)}");
		}
	}
}
