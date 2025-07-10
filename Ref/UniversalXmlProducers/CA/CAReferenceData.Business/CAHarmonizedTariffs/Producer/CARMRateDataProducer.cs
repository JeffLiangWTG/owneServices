using System;
using System.Globalization;
using System.IO;
using System.Text;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMRateDataProducer : IProducer
	{
		public CARMRateDataProducer(IWebServiceCaller serviceCaller, StringBuilder errorBuilder)
		{
			_serviceCaller = serviceCaller;
			_errorBuilder = errorBuilder;
		}
		readonly IWebServiceCaller _serviceCaller;
		readonly StringBuilder _errorBuilder;

		public string OutPutFilePath => _outPutFilePath ?? (_outPutFilePath = Path.Combine(ApplicationConfig.OutputPath, string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string _outPutFilePath;

		public string FunctionCode => Constants.ProgramFunctions.CARate;

		public string WorkingFileOrDirectory => _workingFolder ?? (_workingFolder = Path.Combine(Path.GetTempPath(), FunctionCode));

		public DateTime PublicationTime => publicationTime.HasValue ? publicationTime.Value : DateTime.Today;
		DateTime? publicationTime;

		string _workingFolder;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CA rate data");
			var preProcessChecker = new PreProcessChecker(FunctionCode);
			try
			{
				publicationTime = _serviceCaller.GetLatestUpdateOnDate(CARMAPIQueryTypes.ExciseTaxCodesQueryType);
				if (preProcessChecker.UpdateLastPublishDate(publicationTime))
				{
					Directory.CreateDirectory(WorkingFileOrDirectory);
					try
					{
						Console.WriteLine("CA rate data: Downloading files from webservice...");
						_serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.ExciseTaxCodesQueryType, new string[] { nameof(CARMExciseTaxCode.ExciseTaxCode) }, new string[] { nameof(CARMExciseTaxCode.ExciseTaxCode), nameof(CARMExciseTaxCode.ExciseTaxRateValue), nameof(CARMExciseTaxCode.UnitOfMeasureCode), nameof(CARMExciseTaxCode.Description) });
						Console.WriteLine("CA rate data: All required files downloaded, start parsing...");
						new CARMRateParser(WorkingFileOrDirectory).ParseXMLfilesIntoXML(OutPutFilePath, publicationTime.Value);
					}
					finally
					{
						Directory.Delete(WorkingFileOrDirectory, true);
					}
				}
				else
				{
					Console.WriteLine($"CA rate data: Nothing new published since last process. Skip processing this time.");
				}
			}
			catch (Exception ex)
			{
				_errorBuilder.AppendLine($"Error processing in '{WorkingFileOrDirectory}':{System.Environment.NewLine}{ex.Message}");
				preProcessChecker.MarkAsProcessRequired();
			}

			Console.WriteLine("End processing CA rate data");
		}
	}
}
