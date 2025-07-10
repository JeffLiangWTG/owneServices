using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	sealed class CustomTableNameToCodeTypeMappingTest
	{
		[Test]
		public void TestGetCodeType_Valid1091_ReturnsCorrectCodeTypes()
		{
			var element = new XElement("Table",
				new XElement("IsForManifest", "true"),
				new XElement("IsForDeclaration", "true"));

			var result = CustomTableNameToCodeTypeMapping.GetCodeType("1091", element);

			CollectionAssert.AreEquivalent(new[] { "MPKG", "PKG" }, result);
		}

		[Test]
		public void TestGetCodeType_Valid1091_OnlyManifest_ReturnsMPKG()
		{
			var element = new XElement("Table",
				new XElement("IsForManifest", "true"),
				new XElement("IsForDeclaration", "false"));

			var result = CustomTableNameToCodeTypeMapping.GetCodeType("1091", element);

			CollectionAssert.AreEquivalent(new[] { "MPKG" }, result);
		}

		[Test]
		public void TestGetCodeType_Defaults()
		{
			foreach (var item in tableNameToCodeTypeMap)
			{
				AssertCodeTypeMappingFor(item.Key, item.Value);
			}
		}

		[Test]
		public void TestGetCodeType_UnknownTable_ThrowsException()
		{
			var element = new XElement("Table");

			Assert.Throws<UnknownTableException>(() =>
				CustomTableNameToCodeTypeMapping.GetCodeType("9999", element).ToList()
				);
		}

		static void AssertCodeTypeMappingFor(string customTableName, string expectedCodeType)
		{
			var element = new XElement("Table");
			var result = CustomTableNameToCodeTypeMapping.GetCodeType(customTableName, element);

			CollectionAssert.AreEquivalent(new[] { expectedCodeType }, result);
		}

		readonly Dictionary<string, string> tableNameToCodeTypeMap = new Dictionary<string, string>()
		{
			{ "2012", "FAC" },
			{ "1307", "C1307" },
			{ "13", "ILDOC" },
			{ "1339", "C1339" },
			{ "2192", "DISCH" },
			{ "23774", "23774" },
		};
	}
}
