using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class InvalidCodeRuleTest : TestCase
	{
		public void TestGetValidator()
		{
			var rule = new InvalidCodeRule();
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA 111");
			list.AddPair("B1", "BBB 111");
			list.AddPair("C1", "CCC 111");
			rule.List = list;

			Dictionary<string, object> values = new Dictionary<string, object>();
			CustomBusinessObject cusObj = new CustomBusinessObject(null, new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				})
			{
				{ typeof(ZString), "TestProp", rule.GetValidator(), rule.GetMetaData().ToArray() },
			});

			ZPropertyInfo propInfo = cusObj.FindPropertyInfo("TestProp");
			cusObj["TestProp"] = "A1";
			TestCaseWithFactory.AssertNoErrors(propInfo);
			cusObj["TestProp"] = "$$";
			TestCaseWithFactory.AssertHasError(propInfo, "Enter a valid selection.");
			cusObj["TestProp"] = "";
			TestCaseWithFactory.AssertNoErrors(propInfo);
		}

		public void TestGetMetaData()
		{
			var rule = new InvalidCodeRule();
			AssertEquals(0, rule.GetMetaData().Count());

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA 111");
			list.AddPair("B1", "BBB 111");
			list.AddPair("C1", "CCC 111");
			rule.List = list;

			var metadata = rule.GetMetaData();
			AssertEquals(2, metadata.Count());
			AssertEquals(list, metadata.ElementAt(0).Value);
			AssertEquals(2, metadata.ElementAt(1).Value);
		}

		public void TestToXml()
		{
			var rule = new InvalidCodeRule();
			var xml = new RulesFactory().ToXml(rule);
			AssertEquals("<sourceCode><rules><rule code=\"InvalidCode\" enabled=\"false\"><details /></rule></rules></sourceCode>", xml.ToString(SaveOptions.DisableFormatting));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA 111");
			list.AddPair("B1", "BBB 111");
			list.AddPair("C1", "CCC 111");
			rule.List = list;

			xml = new RulesFactory().ToXml(rule);
			AssertEquals("<sourceCode><rules><rule code=\"InvalidCode\" enabled=\"false\"><details><codeDescriptionList><codeDescription code=\"A1\" description=\"AAA 111\" /><codeDescription code=\"B1\" description=\"BBB 111\" /><codeDescription code=\"C1\" description=\"CCC 111\" /></codeDescriptionList></details></rule></rules></sourceCode>",
				xml.ToString(SaveOptions.DisableFormatting));
		}

		public void TestFromXml()
		{
			var xml =
				new XElement("sourceCode",
					new XElement("rules",
						new XElement("rule",
							new XAttribute("code", "InvalidCode"),
							new XElement("details",
								new XElement("codeDescriptionList",
									new XElement("codeDescription", new XAttribute("code", "A1"), new XAttribute("description", "AAA 111")),
									new XElement("codeDescription"),
									new XElement("codeDescription", new XAttribute("code", "B1"), new XAttribute("description", "BBB 111")),
									new XElement("codeDescription", new XAttribute("code", "C1"), new XAttribute("description", "CCC 111"))
				)))));

			var rule = (InvalidCodeRule)new RulesFactory().FromXml(xml).SingleOrDefault();
			var list = (CodeDescriptionPairList)rule.List;
			AssertEquals(3, list.Count);
			AssertEquals("A1", list[0].Code);
			AssertEquals("AAA 111", list[0].Description);
			AssertEquals("B1", list[1].Code);
			AssertEquals("BBB 111", list[1].Description);
			AssertEquals("C1", list[2].Code);
			AssertEquals("CCC 111", list[2].Description);
		}

		public void TestFromXmlEmptyDetails()
		{
			var xml =
				new XElement("sourceCode",
					new XElement("rules",
						new XElement("rule",
							new XAttribute("code", "InvalidCode")
				)));
			var rule = (InvalidCodeRule)new RulesFactory().FromXml(xml).SingleOrDefault();
			CodeDescriptionPairList list = (CodeDescriptionPairList)rule.List;
			AssertEquals(0, list.Count);
		}

		public void TestCanBeApplied()
		{
			var rule = new InvalidCodeRule();

			Assert(rule.CanBeApplied(typeof(ZString)));
			Assert(!rule.CanBeApplied(typeof(ZBool)));
			Assert(!rule.CanBeApplied(typeof(ZDateTime)));
		}
	}
}
