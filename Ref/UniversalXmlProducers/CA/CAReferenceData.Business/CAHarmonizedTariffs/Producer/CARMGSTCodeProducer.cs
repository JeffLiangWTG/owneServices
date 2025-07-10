using System;
using System.Globalization;
using System.IO;
using System.Text;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMGSTCodeProducer : IProducer
	{
		public CARMGSTCodeProducer(IWebServiceCaller serviceCaller, StringBuilder errorBuilder)
		{
			this.serviceCaller = serviceCaller;
			this.errorBuilder = errorBuilder;
		}
		readonly IWebServiceCaller serviceCaller;
		readonly StringBuilder errorBuilder;

		public string OutPutFilePath => outPutFilePath ?? (outPutFilePath = Path.Combine(ApplicationConfig.OutputPath, string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string outPutFilePath;

		public string FunctionCode => Constants.ProgramFunctions.CAGSTCode;

		public string WorkingFileOrDirectory => workingFolder ?? (workingFolder = Path.Combine(Path.GetTempPath(), FunctionCode));
		string workingFolder;

		public DateTime PublicationTime => publicationTime.HasValue ? publicationTime.Value : (publicationTime = DateTime.UtcNow.AddHours(-5).Date).Value;
		DateTime? publicationTime;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CARM GST Codes");
			var preProcessChecker = new PreProcessChecker(FunctionCode);
			try
			{
				Directory.CreateDirectory(WorkingFileOrDirectory);
				publicationTime = serviceCaller.GetLatestUpdateOnDate(CARMAPIQueryTypes.GSTCodesQueryType);
				if (preProcessChecker.IsLastPublishDateExpired(publicationTime).Expired)
				{
					Console.WriteLine("CARM GST Codes: Downloading files from webservice...");
					serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.GSTCodesQueryType, new string[] { nameof(CARMGSTCode.GSTCode) }, new string[] { nameof(CARMGSTCode.GSTCode), nameof(CARMGSTCode.GSTCodeValidStartDate), nameof(CARMGSTCode.GSTCodeValidEndDate), nameof(CARMGSTCode.ExciseTaxRateCheckIndicator),
							nameof(CARMGSTCode.GSTCheckGroup), nameof(CARMGSTCode.GSTRateType), nameof(CARMGSTCode.Description)});
					Console.WriteLine("CARM GST Codes: All required files downloaded, start parsing...");
					new CARMGSTCodeParser(WorkingFileOrDirectory).ParseXMLfilesIntoXML(OutPutFilePath, publicationTime.Value);
					preProcessChecker.SavePublishDate(PublicationTime);
				}
				else
				{
					Console.WriteLine($"CARM GST Codes: Nothing new published since last process. Skip processing this time.");
				}
			}
			catch (Exception ex)
			{
				errorBuilder.AppendLine($"Error processing in '{WorkingFileOrDirectory}':{System.Environment.NewLine}{ex.Message}");
				preProcessChecker.MarkAsProcessRequired();
			}
			finally
			{
				Directory.Delete(WorkingFileOrDirectory, true);
			}
			Console.WriteLine("End processing CARM GST Codes");
		}
	}
}
