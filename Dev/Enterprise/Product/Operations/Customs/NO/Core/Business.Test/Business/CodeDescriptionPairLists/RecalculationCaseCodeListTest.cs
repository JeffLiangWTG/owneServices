using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(RecalculationCaseCodeList))]
	sealed class RecalculationCaseCodeListTest : TestCaseWithFactory
	{
		public void TestCodeDescriptionsPairs()
		{
			using var context = Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.Norwegian);
			var recalcCodeList = new RecalculationCaseCodeList();
			CombineAssertions(() =>
			{
				var expectedCodes = ExpectedCodes.ToList();
				AssertEquals("Count", expectedCodes.Count, recalcCodeList.Count);
				foreach (var (code, description) in expectedCodes)
				{
					AssertEquals($"Code '{code}'", description, recalcCodeList.GetDescriptionFromCode(code));
				}
			});
		}

		IEnumerable<(string code, string description)> ExpectedCodes
		{
			get
			{
				yield return ("0019", "Særavgift");
				yield return ("0020", "Avgiftskode-/Avgiftssats");
				yield return ("0021", "Fastsatt dobbelt (utbetaling)");
				yield return ("0022", "Feil eier / kunde");
				yield return ("0023", "Klassifisering");
				yield return ("0024", "Mengde / grunnlag");
				yield return ("0028", "Preferanse / opprinnelse");
				yield return ("0030", "Toll og avgiftsfritak");
				yield return ("0031", "Unntaksdokument");
				yield return ("0033", "Refusjon tross rett vedtak");
				yield return ("0038", "Deklarasjonen utgår");
				yield return ("0039", "Prosedyreendring");
				yield return ("0040", "Feil/manglende faktura");
				yield return ("0041", "Gjenutførsel");
				yield return ("0050", "Feilliste");
				yield return ("0051", "Tollnedsettelser og tollkvoter");
				yield return ("0052", "Tolln. og tollkv. i ettertid");
				yield return ("0088", "Man. beregning av Tolletaten");
				yield return ("0097", "Statistisk oppretting EB/RE");
				yield return ("0098", "Statistiske opprettinger, SO");
			}
		}
	}
}
