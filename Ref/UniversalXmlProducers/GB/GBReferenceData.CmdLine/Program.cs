using System;
using System.Globalization;
using CargoWise.RefDbRepo.GBReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			if (args.Length != 0)
			{
				var func = args[0].ToUpper(CultureInfo.CurrentCulture);
				GetProgramFunction(func)();
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}

		internal static Action GetProgramFunction(string func)
		{
			switch (func)
			{
				case Constants.ProgramFunctions.CDSStandingData:
					return CdsStandingDataProgram.Run;
				case Constants.ProgramFunctions.ChiefHarmonisedDeclarationCode:
					return ChiefHarmonisedDeclarationCodeProgram.Run;
				case Constants.ProgramFunctions.CDSTariffData:
					return CDSTariffDataProgram.Run;
				case Constants.ProgramFunctions.GvmsReferenceData:
					return GVMSReferenceDataProgram.Run;
				case Constants.ProgramFunctions.CDSPortData:
					return CDSPortDataProgram.Run;
				case Constants.ProgramFunctions.ExchangeRates:
					return ExchangeRateProgram.Run;
				case Constants.ProgramFunctions.CDSProcedureData:
					return CDSProcedureDataProgram.Run;
				case Constants.ProgramFunctions.UKOfficeCodes:
					return UKOfficeCodesProgram.Run;
				default:
					throw new ArgumentException(Invariant($"Invalid argument: {func}"));
			}
		}
	}
}
