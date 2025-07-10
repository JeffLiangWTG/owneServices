using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public static class OnDemandCollectorProgram
	{
		public static void Run()
		{
			var error = Errors.No;

			if (!Directory.Exists(ApplicationConfig.Instance.DropDirectory))
			{
				Console.Write($"{ApplicationConfig.Instance.DropDirectory} was not found.");
				throw new DropFileException($"Drop Directory {ApplicationConfig.Instance.DropDirectory} was not found. Please check App.config DropDirectory value.");
			}
			else
			{
				var droppedFile = Directory.GetFiles(ApplicationConfig.Instance.DropDirectory, "*.zip").FirstOrDefault() ?? string.Empty;

				if (!string.IsNullOrEmpty(droppedFile))
				{
					Console.Write($"{droppedFile} found. ");
					ApplicationConfig.Instance.DeltaGBaseUrl = droppedFile;
					new AdditionalCodesCollectorProgram().ExecuteTasks(ref error);
					new DeltaIECodesCollectorProgram().ExecuteTasks(ref error);
					new DeltaGCustomsDestinationCodesCollectorProgram().ExecuteTasks(ref error);
					new AirportIATACollectorProgram().ExecuteTasks(ref error);
					new DeltaGCustomsProcedureCollectorProgram().ExecuteTasks(ref error);
					new ExchangeRateCollectorProgram().ExecuteTasks(ref error);
					new MonthlyDataCollectorProgram().ExecuteTasks(ref error);
					new RateTypeCollectorProgram().ExecuteTasks(ref error);
					new TradeGroupCollectorProgram().ExecuteTasks(ref error);
					Console.Write("File processed. ");
					File.Delete(droppedFile);
					Console.WriteLine("File deleted.");
				}
			}

			Environment.Exit((int)error);
		}
	}
}
