using System;
using System.Globalization;
using CargoWise.RefDbRepo.PLReferenceData.Business;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

class Program
{
	static int Main(string[] args)
	{
		UniversalXMLProducers.Common.CommonApplicationConfig.AddJsonFile(ApplicationConfig.JsonConfigFile);
		UniversalXMLProducers.EUNTariffDataProducer.ApplicationConfig.ConfigEnvironment();
		return (int)ProduceXml(args);
	}

	static ProducerStatus ProduceXml(string[] args)
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

		if (args.Length == 0)
		{
			throw new ArgumentException("No arguments entered.");
		}

		var func = args[0].ToUpper(CultureInfo.CurrentCulture);

		if (ProcessRequiresToBeX64(func) && !Environment.Is64BitProcess)
		{
			throw new ArgumentException($"This must be run on 64 bit process - '{func}' cannot be executed on 32 bit as it will be out of memory during execution.");
		}

		var result = func switch
		{
			Constants.ProgramFunctions.CusExchangeRates => CusProgram.Run(),
			Constants.ProgramFunctions.CudExchangeRates => CudProgram.Run(),
			Constants.ProgramFunctions.Dictionaries => DictionariesProgram.Run(),
			Constants.ProgramFunctions.Tariffs => TariffProgram.Run(),
			Constants.ProgramFunctions.Quota => QuotaProgram.Run(),
			Constants.ProgramFunctions.Normalize => NormalizeTaricProgram.Run(), // currently only for local usage - !!!!! requires x64 !!!!!
			Constants.ProgramFunctions.TaricUpdate => TaricUpdateProgram.Run(),
			Constants.ProgramFunctions.DownloadIsztar4Base => Taric4BaseProgram.Run(), // uses FTP available only for UAT and PROD - !!!!! requires x64 !!!!!
			Constants.ProgramFunctions.CusProcedure => CusProcedureProgram.Run(),
			_ => throw new ArgumentException($"Invalid argument entered - {func}."),
		};
		return result ? ProducerStatus.Success : ProducerStatus.Failure;
	}

	static bool ProcessRequiresToBeX64(string processArg) =>
		processArg == Constants.ProgramFunctions.Normalize
		|| processArg == Constants.ProgramFunctions.DownloadIsztar4Base;
}
