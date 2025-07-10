using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class TradeGroupProgram
	{
		public static void Run(string outputPath)
		{
			new RefCusTradeGroupParser(ApplicationConfig.TradeGroupConfigFilePath, ApplicationConfig.TradeGroupDataFilePath).ConvertToXMLFile(Path.Combine(outputPath, Constants.OutputFileName.TradeGroup), DateTime.Today);
		}
	}
}
