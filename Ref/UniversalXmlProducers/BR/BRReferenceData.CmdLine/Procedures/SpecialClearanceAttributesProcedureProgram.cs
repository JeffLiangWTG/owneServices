using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class SpecialClearanceAttributesProcedureProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = new SpecialClearanceAttributesProcedureDownloader().Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Special Clearance Attributes Procedure";

		protected override string LogFileSuffix => Constants.SpecialClearanceAttributesProcedureLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusProcedure_BR_SpecialClearanceAttributesProcedure.xml");
			using (var stream = new MemoryStream(bFile))
			{
				new SpecialClearanceAttributesProcedureParser("BR Export Special Clearance Attributes Procedure").ExportToXMLFile(stream, outputFileName, DateTime.Now);
			}
		}
	}
}
