using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WF = CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomAddOnRule))]
	sealed class GenCustomAddOnRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetRules()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.SetRules(new CheckEnteredRule(), new DateTimeFormatRule { Format = KDateTimeFormat.Time });

			AssertEquals("<sourceCode><rules><rule code=\"CheckEntered\" enabled=\"false\"><details /></rule><rule code=\"DateTimeFormat\" enabled=\"false\"><details><format>Time</format></details></rule></rules></sourceCode>", addOnRule.XR_SourceCode);
		}

		public void TestGetRules()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();

			ICustomAddOnRule[] rules = addOnRule.GetRules();
			AssertEquals(0, rules.Length);

			addOnRule.XR_SourceCode = "<sourceCode><rules><rule code=\"CheckEntered\" enabled=\"false\"><details /></rule><rule code=\"DateTimeFormat\" enabled=\"false\"><details><format>Time</format></details></rule></rules></sourceCode>";
			rules = addOnRule.GetRules();
			AssertEquals(2, rules.Length);
			AssertEquals(WF.CustomAddOnRuleTypes.CheckEntered, rules[0].Code);
			AssertEquals(WF.CustomAddOnRuleTypes.DateTimeFormat, rules[1].Code);
			AssertEquals(KDateTimeFormat.Time, ((DateTimeFormatRule)rules[1]).Format);
		}

		public void TestGetRules_WorksWithNoAttributes()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();

			ICustomAddOnRule[] rules = addOnRule.GetRules();
			AssertEquals(0, rules.Length);

			addOnRule.XR_SourceCode = "<sourceCode><rules><rule code=\"CheckEntered\"><details /></rule><rule code=\"DateTimeFormat\"><details><format>Time</format></details></rule></rules></sourceCode>";
			rules = addOnRule.GetRules();
			AssertEquals(2, rules.Length);
			AssertEquals(WF.CustomAddOnRuleTypes.CheckEntered, rules[0].Code);
			AssertEquals(WF.CustomAddOnRuleTypes.DateTimeFormat, rules[1].Code);
			AssertEquals(KDateTimeFormat.Time, ((DateTimeFormatRule)rules[1]).Format);
		}

		public void TestAllRulesHasChanges()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			Assert(!addOnRule.HasChanges);
			Assert(addOnRule.AllRules.Count > 0);
			addOnRule.AllRules[0].IsEnabled = true;
			Assert(addOnRule.HasChanges);
		}

		public void TestAllRules()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.SetRules(new DateTimeFormatRule { IsEnabled = true });

			Assert(addOnRule.AllRules.Count > 0);
			foreach (AvailableRule availableRule in addOnRule.AllRules)
			{
				AssertEquals(availableRule.Rule is DateTimeFormatRule, availableRule.IsEnabled);
			}

			addOnRule.AllRules.Cast<AvailableRule>().First(rule => rule.Rule is CheckEnteredRule).IsEnabled = true;
			addOnRule.Factory.Save();

			var updateRules = addOnRule.GetRules();
			Assert(Array.Exists(updateRules, rule => rule is DateTimeFormatRule));
			Assert(Array.Exists(updateRules, rule => rule is CheckEnteredRule));
		}

		public void TestGetRulesLoadsLargeXML()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();

			ICustomAddOnRule[] rules = addOnRule.GetRules();
			AssertEquals(0, rules.Length);

			addOnRule.XR_SourceCode = "<sourceCode><rules><rule code=\"InvalidCode\"><details><codeDescriptionList><codeDescription code=\"HN292\" description=\"????\" /><codeDescription code=\"HNCBT\" description=\"???????\" /><codeDescription code=\"HNDCT\" description=\"YUSEN LOGITEC\" /><codeDescription code=\"HNHLC\" description=\"HIRANO LOGISTICS\" /><codeDescription code=\"HNHND\" description=\"HANEDA UNYU\" /><codeDescription code=\"HNKAS\" description=\"KANTOH AIR SERVICE\" /><codeDescription code=\"HNKLG\" description=\"KOKUNAI LOGISTICS\" /><codeDescription code=\"HNKSK\" description=\"KOKUSAIKUYU\" /><codeDescription code=\"HNKYO\" description=\"KYOWA TSUSYOU\" /><codeDescription code=\"HNNSU\" description=\"NISSHO UNYU\" /><codeDescription code=\"HNP/U\" description=\"PICK UP BY CUSTOMER\" /><codeDescription code=\"HNQCG\" description=\"Q-CARGO\" /><codeDescription code=\"HNSBS\" description=\"SBS LOGITEMU\" /><codeDescription code=\"HNSGW\" description=\"SAGAWA EXPRESS\" /><codeDescription code=\"HNSTT\" description=\"SHUTTLE\" /><codeDescription code=\"HNTEX\" description=\"TOEI EXPRESS\" /><codeDescription code=\"HNTHI\" description=\"TOHAI\" /><codeDescription code=\"HNTOE\" description=\"TOEI HOLDINGS\" /><codeDescription code=\"HNWNG\" description=\"WING EXPRESS\" /><codeDescription code=\"HNYBC\" description=\"YAMATO BOX CHARTER\" /><codeDescription code=\"HNYEX\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"HNYGX\" description=\"YAMATO GLOBAL EXPRESS\" /><codeDescription code=\"HNYTQ\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"HNZZZ\" description=\"SHITATE  SHARYO AIZUMI\" /><codeDescription code=\"KXACH\" description=\"ACHIHA CO.,LTD.\" /><codeDescription code=\"KXANA\" description=\"ANA LOGISTIC SERVICE CO.,LTD.\" /><codeDescription code=\"KXFUK\" description=\"FUKUI KAMOTSU JIDOSHA\" /><codeDescription code=\"KXHTB\" description=\"HITACHI BUTSURYU\" /><codeDescription code=\"KXKKU\" description=\"KOKUSAI KUYU\" /><codeDescription code=\"KXKML\" description=\"K M LINE\" /><codeDescription code=\"KXKTL\" description=\"KTL CO.,LTD.\" /><codeDescription code=\"KXKTS\" description=\"KTL CO.,LTD.\" /><codeDescription code=\"KXMEI\" description=\"MEITETSU KYUHAI\" /><codeDescription code=\"KXP/U\" description=\"PICK UP BY CUSTOMER\" /><codeDescription code=\"KXSAI\" description=\"SANAI\" /><codeDescription code=\"KXSAN\" description=\"SANYO JIDOSHA\" /><codeDescription code=\"KXSEI\" description=\"YAMATO SEIBANBIN\" /><codeDescription code=\"KXSGW\" description=\"SAGAWA KYUBIN\" /><codeDescription code=\"KXSHU\" description=\"KOHKU SHUHAI SERVICE CO.,LTD.\" /><codeDescription code=\"KXSIB\" description=\"SEIBU UNYU\" /><codeDescription code=\"KXSSC\" description=\"ESURAINGIFU\" /><codeDescription code=\"KXWEX\" description=\"WING EXPRESS\" /><codeDescription code=\"KXWIN\" description=\"WING EXPRESS\" /><codeDescription code=\"KXYAS\" description=\"YUSEN JISHA-BIN\" /><codeDescription code=\"KXYEX\" description=\"YAMATO GLOBAL EXPRESS CO.,LTD.\" /><codeDescription code=\"KXYGX\" description=\"YAMATO GLOBAL EXPRESS CO.,LTD.\" /><codeDescription code=\"KXYTC\" description=\"YAMATO TRANSPORT\" /><codeDescription code=\"KXYTQ\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"NGAPS\" description=\"APS\" /><codeDescription code=\"NGARK\" description=\"AICHI RIKUUN\" /><codeDescription code=\"NGETC\" description=\"EZAKI UNYU\" /><codeDescription code=\"NGFUJ\" description=\"FUJI UNYU\" /><codeDescription code=\"NGMTQ\" description=\"MEITETSU KYUHAI\" /><codeDescription code=\"NGNGO\" description=\"NAGOYA RIKUSO\" /><codeDescription code=\"NGP/U\" description=\"PICK UP BY CUSTOMER\" /><codeDescription code=\"NGSEB\" description=\"SEIBU UNYU\" /><codeDescription code=\"NGSGW\" description=\"SAGAWA EXPRESS\" /><codeDescription code=\"NGSKY\" description=\"BLUESKY LOGISTICS\" /><codeDescription code=\"NGS-L\" description=\"S-LINE GIFU\" /><codeDescription code=\"NGTYO\" description=\"TOYOOKA KYODO YUSO\" /><codeDescription code=\"NGYEX\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"NGYLN\" description=\"YUSEN LOGITEC\" /><codeDescription code=\"NGYTQ\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"NR292\" description=\"FUKUYAMA TSUUN\" /><codeDescription code=\"NRABT\" description=\"ASAHI BUTSURYU\" /><codeDescription code=\"NRADS\" description=\"AIR DELIVERY SERVICE\" /><codeDescription code=\"NRAMG\" description=\"AMAGI\" /><codeDescription code=\"NRANA\" description=\"ANA LOGISTIC SERVICE\" /><codeDescription code=\"NRAPS\" description=\"ALPS LOGISTICS\" /><codeDescription code=\"NRARK\" description=\"AICHI RIKUUN\" /><codeDescription code=\"NRAST\" description=\"ASSIST TOHKEI\" /><codeDescription code=\"NRBST\" description=\"BEST TRANSPORT\" /><codeDescription code=\"NRCBT\" description=\"CHIBA TSUSHO\" /><codeDescription code=\"NRCML\" description=\"CAMEL TRANSPORT\" /><codeDescription code=\"NRDCT\" description=\"YUSEN LOGITEC\" /><codeDescription code=\"NRDIK\" description=\"DAIICHI KAMOTSU\" /><codeDescription code=\"NRDKY\" description=\"DOKYUU UNYU\" /><codeDescription code=\"NRHKY\" description=\"HAKUYO UNYU\" /><codeDescription code=\"NRHLC\" description=\"HIRANO LOGISTICS\" /><codeDescription code=\"NRHND\" description=\"HANEDA UNYU\" /><codeDescription code=\"NRICT\" description=\"INTERNATIONAL AIR CARGO TERMINAL\" /><codeDescription code=\"NRISU\" description=\"ISU CARGO\" /><codeDescription code=\"NRJTS\" description=\"JAL CARGO SERVICE\" /><codeDescription code=\"NRKAI\" description=\"KANTOH AIR SERVICE\" /><codeDescription code=\"NRKAS\" description=\"KANTOH AIR SERVICE\" /><codeDescription code=\"NRKKU\" description=\"KOSHIKAWA UNYU\" /><codeDescription code=\"NRKNK\" description=\"KOKUNAI KOGYO\" /><codeDescription code=\"NRKSK\" description=\"KOKUSAIKUYU K.K.\" /><codeDescription code=\"NRKSS\" description=\"KOKUSHUHAI SERVICE\" /><codeDescription code=\"NRKYO\" description=\"KYOWA TSUSYO\" /><codeDescription code=\"NRMOM\" description=\"MATSUOKAMAN UNYU\" /><codeDescription code=\"NRMRS\" description=\"MARUSHIN SHOJI\" /><codeDescription code=\"NRNGM\" description=\"NAGUMO SANGYO\" /><codeDescription code=\"NRNIG\" description=\"NIIGATA UNYU\" /><codeDescription code=\"NRNSK\" description=\"NANSHIN KAMOTSU JIDOSYA\" /><codeDescription code=\"NRNSU\" description=\"NISSHO UNYU\" /><codeDescription code=\"NRP/U\" description=\"PICK UP BY CUSTOMER\" /><codeDescription code=\"NRQCG\" description=\"Q-CARGO\" /><codeDescription code=\"NRSBS\" description=\"SBS LOGITEMU\" /><codeDescription code=\"NRSEW\" description=\"SEIWA EXPRESS\" /><codeDescription code=\"NRSGW\" description=\"SAGAWA EXPRESS\" /><codeDescription code=\"NRS-L\" description=\"S-LINE GIFU\" /><codeDescription code=\"NRSRS\" description=\"KYOUSHIN SHOJI\" /><codeDescription code=\"NRSTO\" description=\"SATOU UNSOU\" /><codeDescription code=\"NRSTS\" description=\"SHINKAI TRANSPORT SYSTEMS\" /><codeDescription code=\"NRSTT\" description=\"SHUTTLE\" /><codeDescription code=\"NRTEX\" description=\"TOEI EXPRESS\" /><codeDescription code=\"NRTLL\" description=\"TL LOGICOM\" /><codeDescription code=\"NRTUS\" description=\"TAKARATSUKA UNKO SERVICE\" /><codeDescription code=\"NRWNG\" description=\"WING EXPRESS\" /><codeDescription code=\"NRYBC\" description=\"YAMATO BOX CHARTER\" /><codeDescription code=\"NRYEX\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"NRYMH\" description=\"YAMAHIRO UNYU\" /><codeDescription code=\"NRYTQ\" description=\"YAMATO TRANSPORT CO.,LTD.\" /><codeDescription code=\"NRZZZ\" description=\"SHITATE  SHARYO AIZUMI\" /><codeDescription code=\"P/U\" description=\"Consignee PickUp\" /></codeDescriptionList></details></rule></rules></sourceCode>";
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			GenCustomAddOnRule ruleLoadedIntoAnotherFactory = anotherFactory.Load<GenCustomAddOnRule>(addOnRule.PK);

			rules = ruleLoadedIntoAnotherFactory.GetRules();
			AssertEquals(1, rules.Length);
			AssertEquals(WF.CustomAddOnRuleTypes.InvalidCode, rules[0].Code);
			AssertEquals(116, (rules[0] as InvalidCodeRule).List.Count);
		}

		public void TestGetRulesForGLOWAddOnRule()
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			AssertEquals(0, rule.GetRules().Length);

			rule.XR_SourceCode = @"<rule xmlns=""http://cargowise.com/glow/2011/01/27/rule.xsd"" returnType=""CargoWise.Glow.Infrastructure.Domain.RuleValidationResult, CargoWise.Glow.Infrastructure.Domain"">
	<parameters>
		<parameter name=""chargeCodePK"" type=""System.Guid"" />
		<parameter name=""chargePK"" type=""System.Guid"" />
		<parameter name=""isCostPosted"" type=""System.Boolean"" />
		<parameter name=""isRevenuePosted"" type=""System.Boolean"" />
		<parameter name=""isApportioned"" type=""System.Boolean"" />
	</parameters>
	<compiledCode assembly=""CargoWise.Glow.Business.Accounting"" class=""CargoWise.Glow.Business.Accounting.JobChargeReadOnly"" method=""ChargeCodeReadOnly"" />
</rule>";
			AssertEquals(0, rule.GetRules().Length);
		}
	}
}
