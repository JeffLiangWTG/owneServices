using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class PreferenceRulesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "PreferenceRules";

		protected override string TextFileName => "PRRPSNAP-P1-EDMAIN-2204200148.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_PreferenceRules.xml";

		protected override DateTime PublishedDate => new DateTime(2001, 02, 03, 04, 05, 06);

		protected override ICMRDataParser Parser => new PreferenceRulesParser_ForTest(Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2025, 03, 07, 01, 02, 03)));
	}

	sealed class PreferenceRulesParser_ForTest : PreferenceRulesParser
	{
		public PreferenceRulesParser_ForTest(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
		{
		}

		protected override IEnumerable<PreferenceRuleScheme> PreferenceRuleSchemes
		{
			get
			{
				var executingAssembly = Assembly.GetExecutingAssembly();
				var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", "PreferenceRules");
				using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, "PRSCHRUL-P1-EDMAIN-2411190201.txt")))
				using (var txtReader = new StreamReader(txtStream))
				{
					return PreferenceRuleSchemesParser.Parse(txtReader.ReadToEnd());
				}
			}
		}
	}
}
