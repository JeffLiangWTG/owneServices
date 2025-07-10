using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffRateProgram : BaseProgram
	{
		public TariffRateProgram(string filepath, string fileType)
		{
			this.filepath = filepath;
			this.fileType = fileType;
		}

		readonly string filepath;
		readonly string fileType;

		protected override void RunCore()
		{
			if (string.IsNullOrEmpty(filepath))
			{
				throw new ArgumentException("Please add a second parameter with the file path");
			}

			if (string.IsNullOrEmpty(fileType))
			{
				throw new ArgumentException("Please add a third parameter with the file type");
			}
			else if (fileType != Constants.HSNTariffDutyRateFileType.XLS_RATES && fileType != Constants.HSNTariffDutyRateFileType.XLS_TEC)
			{
				throw new ArgumentException("Invalid file type");
			}

			var result = File.ReadAllBytes(filepath);

			if (result == null)
			{
				Console.WriteLine("File not found");
				return;
			}

			var outputFolder = ConfigurationProvider.Configuration.GetSection("OutputFolder").Value;
			if (string.IsNullOrEmpty(outputFolder))
			{
				Console.WriteLine("AppSetting OutputFolder need to be set in config file");
				return;
			}

			var outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);

			var outputFileName = GetOutputFilePath($"RefCusRate_BR_HSN.xml");
			using (var streamFile = new MemoryStream(result))
			{
				var parser = new HSNTariffDutyRateParser("BR HSN Tariff II_IPI_PIS_COF Rates");
				parser.ExportToXMLFile(streamFile, fileType, outputFileName, GetPublicationTime(filepath));
				Console.WriteLine($"RefCusRate records generated to {outputFileName}");
			}
		}

		public static DateTime GetPublicationTime(string filepath)
		{
			if (Regex.IsMatch(filepath, @"\d{1,3}_\d{4}(?=\.pdf$|\.xls$|\.xlsx$)"))
			{
				var dayOfYear = int.Parse(Regex.Match(filepath, @"\d{1,3}(?=_\d{4}\.pdf$|_\d{4}\.xls$|_\d{4}\.xlsx$)").Value, CultureInfo.CurrentCulture);
				var year = int.Parse(Regex.Match(filepath, @"\d{4}(?=\.pdf$|\.xls$|\.xlsx$)").Value, CultureInfo.CurrentCulture);
				if (dayOfYear <= new DateTime(year, 12, 31).DayOfYear)
				{
					return new DateTime(year, 1, 1, 0, 0, 0).AddDays(dayOfYear - 1);
				}
			}
			else if (Regex.IsMatch(filepath, @"\d{8}(?=\.pdf$|\.xls$|\.xlsx$)"))
			{
				return DateTime.ParseExact(Regex.Match(filepath, @"\d{8}(?=\.pdf$|\.xls$|\.xlsx$)").Value, "yyyyMMdd", CultureInfo.InvariantCulture);
			}
			return DateTime.Now;
		}
	}
}
