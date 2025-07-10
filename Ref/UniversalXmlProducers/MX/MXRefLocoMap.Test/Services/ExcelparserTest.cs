using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business.Test
{
	[TestFixture]
	public class ExcelParserTest
	{
		static readonly IExcelParser Parser = new ExcelParser();

		[Test]
		public void ParseShouldWork()
		{
			var config = new ExcelParserConfiguration
			{
				SheetIndex = 1,
				HeaderRow = 1,
				StartingRow = 12,
				LastRow = 14
			};
			var filePath = Utilities.CurrentFolder() + "\\Resources\\RefLocoMap.xlsx";
			var locomap = Parser.Parse<RefLocoMap>(filePath, config, RefLocoMap.BackupMapper);

			Assert.AreEqual(3, locomap.Count);
			Assert.That(locomap.Any(x =>
				x.RY_LocalPortCode == "801"
				&& x.RY_RL_NKLocoPort == "MXCOA"
				&& x.RY_SystemUsage == "CUS"
				&& x.RY_RN_NKCountryCode == "MX"
			));
			Assert.That(locomap.Any(x =>
				x.RY_LocalPortCode == "802"
				&& x.RY_RL_NKLocoPort == "MXCOA"
				&& x.RY_SystemUsage == "CUS"
				&& x.RY_RN_NKCountryCode == "MX"
			));

			Assert.That(locomap.Any(x =>
				x.RY_LocalPortCode == "5300"
				&& x.RY_RL_NKLocoPort == "MXCUN"
				&& x.RY_SystemUsage == "CUS"
				&& x.RY_RN_NKCountryCode == "MX"
			));
		}
	}
}
