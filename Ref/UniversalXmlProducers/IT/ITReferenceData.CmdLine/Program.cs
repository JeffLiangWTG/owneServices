using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.CmdLine.AdditionalCodes;
using CargoWise.RefDbRepo.ITReferenceData.CmdLine.ExchangeRates;
using CargoWise.RefDbRepo.ITReferenceData.CmdLine.ExportMeasures;
using CargoWise.RefDbRepo.ITReferenceData.CmdLine.SupportingDocuments;
using CargoWise.RefDbRepo.ITReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.ITReferenceData.CmdLine
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			await ProduceXmlAsync(args);
			return (int)ProducerStatus.Success;
		}

		static async Task ProduceXmlAsync(string[] args)
		{
			if (args.Length == 0)
			{
				throw new ArgumentException("No arguments entered.");
			}
			else
			{
				var functionName = args[0].ToUpperInvariant();
				ProgramFunction function = TryGetProgramFunction(functionName);
				if (function != null)
				{
					function.Invoke(ApplicationConfig.OutputDirectory);
					return;
				}

				AsyncProgramFunction asyncFunction = TryGetAsyncProgramFunction(functionName);
				if (asyncFunction != null)
				{
					await asyncFunction.Invoke(ApplicationConfig.OutputDirectory);
					return;
				}

				throw new ArgumentException($"Invalid argument entered: {functionName}");
			}
		}

		delegate void ProgramFunction(string outputDirectory);

		delegate Task AsyncProgramFunction(string outputDirectory);

		static ProgramFunction TryGetProgramFunction(string function) => function switch
		{
			"EXCHANGERATES" => ExchangeRatesProgram.Run,
			"SUPPORTINGDOCUMENTS" => SupportingDocumentsProgram.Run,
			"ADDITIONALCODES" => AdditionalCodesProgram.Run,
			_ => null,
		};

		static AsyncProgramFunction TryGetAsyncProgramFunction(string function) => function switch
		{
			"EXPORTMEASURES" => ExportMeasuresProgram.RunAsync,
			_ => null,
		};
	}
}
