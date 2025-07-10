using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;

namespace CargoWise.RefDbRepo.AEReferenceData.CmdLine;

public static class DubaiRefDataExcelRunner
{
	public static void Run()
	{
		var file = ApplicationConfig.DubaiRefDataInputFile;
		if (File.Exists(file))
		{
			Parallel.Invoke(ProcessesToRun(file, ApplicationConfig.OutputFolder));
		}
		else
		{
			Console.Error.WriteLine($"File {file} not found.");
		}
	}

	static Action[] ProcessesToRun(string inputFile, string outputPath)
	{
		return new Action[]
		{
			() => RunProcessor(new ExitPointProcessor(outputPath, "RefExitCodesZZ_AED.xml")),
			() => RunProcessor(new CustomsResponseProcessor(outputPath, "RefCustomsResponsesZZ_AED.xml")),
			() => RunProcessor(new AmendCancelReasonProcessor(outputPath, "RefAmendCancelReasonZZ_AED.xml")),
			() => RunProcessor(new CusProcedureProcessor(outputPath, "RefCusProcedureZZ_AED.xml")),
			() => RunProcessor(new CustomsOfficesProcessor(outputPath, "RefCustomsOfficesCodesZZ_AED.xml")),
			() => RunProcessor(new VehicleTypeProcessor(outputPath, "RefVehicleTypesZZ_AED.xml")),
			() => RunProcessor(new VehicleBrandProcessor(outputPath, "RefVehicleBrandsZZ_AED.xml")),
			() => RunProcessor(new DeclarationPurposeProcessor(outputPath, "RefDeclerationPurposeCodesZZ_AED.xml")),
			() => RunProcessor(new InvoiceTypeProcessor(outputPath, "RefInvoiceTypeCodesZZ_AED.xml")),
		};

		void RunProcessor<T, TResult>(ExcelProcessor<T, TResult> processor)
		{
			var xls = ExcelHelper.LoadExcel(inputFile);
			processor.ProcessExcel(xls);
		}
	}
}
