using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class BaseTariff
	{
		protected static bool ExitProgram(Errors error)
		{
			Environment.Exit((int)error);
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception will be treated in a report.")]
		protected static void GenerateURDFiles(List<string> tariffListToProcess, UpdateType updateType, DateTime updateDay)
		{
			int createdFilesCount = 0;

			Console.WriteLine("Generating TARIC tariff output files...");
			var taricGenerator = new TARICTariffUniversalReferenceDataFileGenerator();
			try
			{
				createdFilesCount = taricGenerator.GenerateURDFiles(tariffListToProcess.ToArray(), updateType, updateDay);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}

			Console.WriteLine("Generating common tariff output files...");
			var commonTariffGenerator = new CommonTariffUniversalReferenceDataFileGenerator();
			try
			{
				createdFilesCount += commonTariffGenerator.GenerateURDFiles(tariffListToProcess.ToArray(), updateType, updateDay);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}

			Console.WriteLine("Generating National Rate Code Usage output file...");
			var nationalCodeGenerator = new NationalRateCodeUsageUniversalReferenceDataFileGenerator();
			try
			{
				createdFilesCount += nationalCodeGenerator.GenerateURDFiles(tariffListToProcess.ToArray(), updateType, updateDay);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}

			Console.WriteLine("{0} Xml files created, end of process.", createdFilesCount);
		}

		protected static string GetTariffListFile()
		{
			var tariffListPath = Path.Combine(Path.GetTempPath(), "CompleteTariffList.txt");

			if (Resumer.ParseStatus() == Resumer.DoNotResume)
			{
				if (File.Exists(tariffListPath))
				{
					File.Delete(tariffListPath);
				}
			}

			return tariffListPath;
		}
	}
}
