using System.Linq;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class DateTimeFormatRuleTest : TestCase
	{
		public void TestGetMetaData()
		{
			var rule = new DateTimeFormatRule();
			AssertEquals(KDateTimeFormat.Short, rule.GetMetaData().ElementAt(0).Value);
			rule.Format = KDateTimeFormat.Time;
			AssertEquals(KDateTimeFormat.Time, rule.GetMetaData().ElementAt(0).Value);
		}

		public void TestToXml()
		{
			var rule = new DateTimeFormatRule();
			var element = new RulesFactory().ToXml(rule);
			AssertEquals("<sourceCode><rules><rule code=\"DateTimeFormat\" enabled=\"false\"><details><format>Short</format></details></rule></rules></sourceCode>", element.ToString(SaveOptions.DisableFormatting));
		}

		public void TestFromXml()
		{
			var xml = XElement.Parse("<sourceCode><rules><rule code=\"DateTimeFormat\"><details><format>Long</format></details></rule></rules></sourceCode>");
			var rule = (DateTimeFormatRule)new RulesFactory().FromXml(xml).SingleOrDefault();
			AssertEquals(KDateTimeFormat.Long, rule.Format);

			xml = XElement.Parse("<sourceCode><rules><rule code=\"DateTimeFormat\"><details></details></rule></rules></sourceCode>");
			rule = (DateTimeFormatRule)new RulesFactory().FromXml(xml).SingleOrDefault();
			AssertEquals(KDateTimeFormat.Short, rule.Format);
		}

		public void TestCanBeApplied()
		{
			var rule = new DateTimeFormatRule();

			Assert(!rule.CanBeApplied(typeof(ZString)));
			Assert(!rule.CanBeApplied(typeof(ZBool)));
			Assert(rule.CanBeApplied(typeof(ZDateTime)));
		}

		public void TestGetObjectForBinding()
		{
			var rule = new DateTimeFormatRule();
			BusinessObject bizObj = rule.GetObjectForBinding();
			AssertEquals("SHORT", bizObj["DateTimeFormat"]);
			bizObj["DateTimeFormat"] = "TIME";
			AssertEquals(KDateTimeFormat.Time, rule.Format);
		}
	}
}
