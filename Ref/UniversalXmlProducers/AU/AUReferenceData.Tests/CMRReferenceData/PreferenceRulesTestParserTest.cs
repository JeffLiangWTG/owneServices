using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class PreferenceRulesTestParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "PreferenceRulesTest";

		protected override string TextFileName => "PRRPSNAP-Q1-EDMAIN-2204210021.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_PreferenceRulesTest.xml";

		protected override DateTime PublishedDate => new DateTime(2001, 02, 03, 04, 05, 06);

		protected override ICMRDataParser Parser => new PreferenceRulesTestParser_ForTest(Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime ==  new DateTime(2025, 03, 07, 01, 02, 03)));
	}

	sealed class PreferenceRulesTestParser_ForTest : PreferenceRulesTestParser
	{
		public PreferenceRulesTestParser_ForTest(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
		{
		}

		protected override IEnumerable<PreferenceRuleScheme> PreferenceRuleSchemes
		{
			get
			{
				var executingAssembly = Assembly.GetExecutingAssembly();
				var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", "PreferenceRulesTest");
				using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, "PRSCHRUL-Q1-EDMAIN-2204210022.txt")))
				using (var txtReader = new StreamReader(txtStream))
				{
					return PreferenceRuleSchemesParser.Parse(txtReader.ReadToEnd());
				}
			}
		}
	}
}
