using System;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public abstract class BaseEntityConfigurationParser
	{
		protected BaseEntityConfigurationParser(string configFilePath, string dataFilePath)
		{
			ConfigFile = configFilePath;
			DataFile = dataFilePath;
		}

		protected string ConfigFile { get; }
		protected string DataFile { get; }

		public void ConvertToXMLFile(string outputFilePath, DateTime publicationDate, string suffix = "")
		{
			var (configuration, workbook) = GetEntityConfigurationAndWorkbook(DataFile, ConfigFile);
			if (configuration == null || workbook == null)
			{
				return;
			}
			ExportToXmlFilesCore(outputFilePath, publicationDate, configuration, workbook, suffix);
		}
		public static (EntityConfiguration, IWorkbook) GetEntityConfigurationAndWorkbook(string dataFile, string configFile)
		{
			if (!File.Exists(configFile) || Path.GetExtension(configFile) != Constants.FileExtensions.XML)
			{
				throw new ArgumentException("No config file(.xml) exists.");
			}
			if (!File.Exists(dataFile))
			{
				throw new ArgumentException("No data file exists.");
			}

			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFile);
			if (configuration.DataFileExtensions == null)
			{
				throw new ArgumentException("No file extensions are defined in the config file.");
			}
			var fileExtensions = configuration.DataFileExtensions.Split(',').Select(x => x.Trim());

			IWorkbook workbook;
			if (Path.GetExtension(dataFile) == Constants.FileExtensions.XLSX)
			{
				workbook = new XSSFWorkbook(dataFile);
			}
			else if (Path.GetExtension(dataFile) == Constants.FileExtensions.XLS)
			{
				using (var file = new FileStream(dataFile, FileMode.Open, FileAccess.Read))
				{
					workbook = new HSSFWorkbook(file);
				}
			}
			else
			{
				throw new ArgumentException($"Unexpected file format: {dataFile}");
			}
			return (configuration, workbook);
		}
		abstract protected void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix);
	}
}
