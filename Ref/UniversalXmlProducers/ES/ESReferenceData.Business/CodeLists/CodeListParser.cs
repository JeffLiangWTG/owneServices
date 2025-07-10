using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CsvHelper;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public abstract class CodeListParser<T,TMap> : CommonParser
		where TMap : ClassMap
	{
		protected CodeListParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		public string ConvertRecordsToXMLFile(string inputCsvFile, string outPutFileWithPath)
		{
			ErrorBuilder.Clear();
			var result = GetRefCodeListToExport(inputCsvFile);
			if (result.Count > 0)
			{
				Helper.ExportToXMLFile(DataSource, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine(ErrorMessage);
			}

			return ErrorBuilder.ToString();
		}
		protected abstract string DataSource { get; }
		protected abstract string ErrorMessage { get; }
		protected abstract List<RefCusCodeList> GetRefCodeListToExport(string inputCsvFile);

		protected List<T> GetRecordsFromCsv(string inputCsvFile)
		{
			var itemList = new List<T>();
			var reader = File.OpenText(inputCsvFile);
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.Delimiter = ";";
				csvReader.Configuration.MissingFieldFound = null;
				csvReader.Configuration.RegisterClassMap<TMap>();
				while (csvReader.Read())
				{
					var record = csvReader.GetRecord<T>();
					if (record != null)
					{
						itemList.Add(record);
					}
				}
			}
			return itemList;
		}
	}
}
