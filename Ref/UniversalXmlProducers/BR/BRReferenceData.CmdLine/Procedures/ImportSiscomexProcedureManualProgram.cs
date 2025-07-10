using System;
using System.Diagnostics.Contracts;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class ImportSiscomexProcedureManualProgram : BaseProgram
	{
		public ImportSiscomexProcedureManualProgram(string inputFilePath)
		{
			Contract.Assume(!string.IsNullOrEmpty(inputFilePath));
			this.inputFilePath = inputFilePath;
		}
		readonly string inputFilePath;

		protected override void RunCore()
		{
			using (var inputFile = new StreamReader(inputFilePath))
			{
				var publicationTime = DateTime.Now;

				var outputFileName = GetOutputFilePath($"RefCusProcedure_BR_{Constants.ShipmentTypes.ImportSiscomex}_DeclarationType.xml");

				new ImportSiscomexProcedureManualParser("BR Import Siscomex Procedure Declaration Type").ExportToXMLFile(inputFile.BaseStream, outputFileName, publicationTime);
				Console.WriteLine($"RefCusProcedure records generated to {outputFileName}");
			}
		}
	}
}
