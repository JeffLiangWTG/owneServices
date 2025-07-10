using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using FlexCel.XlsAdapter;
using static CargoWise.RefDbRepo.BEReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class UCCImportCodeListProcessManager
	{
		public UCCImportCodeListProcessManager()
		{
			ErrorCollector = new StringBuilder();
		}

		public UCCImportCodeListProcessManager(StringBuilder errorCollector)
		{
			ErrorCollector = errorCollector;
		}

		public void RunProducers()
		{
			var xlsFile = GetXlsFile();

			if (xlsFile != null)
			{
				new ImportAdditionalInformationProducer().RunProcess(ApplicationConfig.OutputDirectory, xlsFile, Constants.UccCodeListNames.AdditionalInformationCodes, ErrorCollector);
				new ImportAdditionalReferenceProducer().RunProcess(ApplicationConfig.OutputDirectory, xlsFile, Constants.UccCodeListNames.AdditionalReferenceCodes, ErrorCollector);
				new ImportPreviousDocumentTypeProducer().RunProcess(ApplicationConfig.OutputDirectory, xlsFile, Constants.UccCodeListNames.PreviousDocumentCodesCL214, ErrorCollector);
				new ImportSupportingDocumentTypeProducer().RunProcess(ApplicationConfig.OutputDirectory, xlsFile, Constants.UccCodeListNames.SupportingDocumentCodes, ErrorCollector);
				new ImportTransportDocumentTypeProducer().RunProcess(ApplicationConfig.OutputDirectory, xlsFile, Constants.UccCodeListNames.TransportDocumentCodes, ErrorCollector);
			}
			else
			{
				ErrorCollector.AppendLine("No XLS or XLSX file was found in folder " + contentFolder);
			}
		}

		public XlsFile GetXlsFile()
		{
			XlsFile xlsFile = null;

			if (Directory.Exists(contentFolder))
			{
				var filenames = Directory.GetFiles(contentFolder, "*.xls*");
				if (filenames.Any())
				{
					xlsFile = new XlsFile(filenames.First(), false);
				}
			}
			return xlsFile;
		}

		public virtual string contentFolder => Path.Combine(ApplicationConfig.ServiceDir, FolderNames.CodeListData);
		readonly StringBuilder ErrorCollector;
	}
}
