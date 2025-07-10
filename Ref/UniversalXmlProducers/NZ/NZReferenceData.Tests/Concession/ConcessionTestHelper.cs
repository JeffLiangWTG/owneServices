using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Business;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Concessions
{
	static class ConcessionTestHelper
	{
		public static IConcessionFileReader GetConcessionFileReaderForTest(string[] concessionToTariff, string[] concessionDetails, string[] concessionRates, string[] consolidatedListOfApprovalsJsonLines)
		{
			return new ConcessionFileReaderForTest(
				Concat(TestConstants.ConcessionToTariffHeader, concessionToTariff),
				Concat(TestConstants.ConcessionDetailsHeader, concessionDetails),
				Concat(TestConstants.ConcessionRatesHeader, concessionRates),
				consolidatedListOfApprovalsJsonLines);

			string[] Concat(string head, string[] content)
			{
				var concated = new string[content.Length + 1];
				concated[0] = head;
				content.CopyTo(concated, 1);
				return concated;
			}
		}

		static string assemblyDirectoryPath;
		static string AssemblyDirectoryPath
		{
			get
			{
				if (string.IsNullOrEmpty(assemblyDirectoryPath))
				{
					var assembly = Assembly.GetExecutingAssembly();
					assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
				}
				return assemblyDirectoryPath;
			}
		}

		static BuildersFilePath[] testBuildersFilePathFromTestFiles;
		public static BuildersFilePath[] TestBuildersFilePathFromTestFiles
		{
			get
			{
				if (testBuildersFilePathFromTestFiles == null)
				{
					var inputFolderPath = Path.Combine(AssemblyDirectoryPath, @"Concession\TestFiles\Input");
					testBuildersFilePathFromTestFiles = new BuildersFilePath[]
					{
						new BuildersFilePath(Path.Combine(inputFolderPath, "Concession", "Concession_Details.csv"), BuilderFilePathSymbol.ConcessionDetail),
						new BuildersFilePath(Path.Combine(inputFolderPath, "Concession", "Concession_Rates.csv"), BuilderFilePathSymbol.ConcessionRate),
						new BuildersFilePath(Path.Combine(inputFolderPath, "Concession", "Concession_to_Tariff.csv"), BuilderFilePathSymbol.ConcessionToTariff),
						new BuildersFilePath(Path.Combine(inputFolderPath, "Concession", "time_stamp.txt"), BuilderFilePathSymbol.PublicationTime),
						new BuildersFilePath(Path.Combine(inputFolderPath, "nz_en_concessions_paper.json"), BuilderFilePathSymbol.ConsolidatedListOfApprovalsJson),
						new BuildersFilePath(Path.Combine(inputFolderPath, "NZConcessionOverride.json"), BuilderFilePathSymbol.ConcessionOverride),
					};
				}
				return testBuildersFilePathFromTestFiles;
			}
		}

		public static string ReadExpectedOutput(string filename)
		{
			var outputFilePath = Path.Combine(AssemblyDirectoryPath, @"Concession\TestFiles\Output", filename);
			return File.ReadAllText(outputFilePath);
		}

		class ConcessionFileReaderForTest : IConcessionFileReader
		{
			public ConcessionFileReaderForTest(string[] concessionToTariff, string[] concessionDetails, string[] concessionRates, string[] consolidatedListOfApprovalsJsonLines)
			{
				ConcessionToTariff = concessionToTariff;
				ConcessionDetails = concessionDetails;
				ConcessionRates = concessionRates;
				ConsolidatedListOfApprovals = consolidatedListOfApprovalsJsonLines;
			}

			public string[] ConcessionToTariff { get; set; }
			public string[] ConcessionDetails { get; set; }
			public string[] ConcessionRates { get; set; }
			public string[] ConsolidatedListOfApprovals { get; set; }

			public string[] ReadAllLines(string path)
			{
				var result = Array.Empty<string>();
				if (path == "Concession_to_Tariff.txt")
				{
					result = ConcessionToTariff;
				}
				else if (path == "Concession_Details.txt")
				{
					result = ConcessionDetails;
				}
				else if (path == "Concession_Rates.txt")
				{
					result = ConcessionRates;
				}
				else if (path == "nz_en_concessions_paper.json")
				{
					result = ConsolidatedListOfApprovals;
				}
				return result;
			}
		}
	}
}
