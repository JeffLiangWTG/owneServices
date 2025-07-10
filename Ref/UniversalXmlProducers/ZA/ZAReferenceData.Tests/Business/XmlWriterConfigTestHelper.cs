using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business
{
	internal static class XmlWriterConfigTestHelper
	{
		internal static void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();

			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}
	}
}
