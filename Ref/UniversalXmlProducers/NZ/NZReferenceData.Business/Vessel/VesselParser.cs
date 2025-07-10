using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public static class VesselParser
	{
		const string DataSource = "NZ Vessels List";
		const string OutputFileName = "RefCusCodeListZZ_NZ_Vessel.xml";

		public static IEnumerable<RefCusCodeList> Parse(string localFilePath)
		{
			var codes = File.ReadAllLines(localFilePath, Encoding.UTF8).Select(c => c.Trim()).Distinct();

			foreach (var code in codes)
			{
				if (!string.IsNullOrWhiteSpace(code))
				{
					yield return new RefCusCodeList() { ZZD_Code = code.Substring(0, Math.Min(35, code.Length)), ZZD_Description = code };
				}
			}
		}

		public static void ExportToXMLFile(string localFilePath, string outputFolderPath, DateTime publicationDateTime)
		{
			var codes = Parse(localFilePath);
			var outputFileFullName = Path.Combine(outputFolderPath, OutputFileName);

			var writerConfiguration = GetWriterConfiguration();
			var writer = Helper.GenerateXmlWriter(writerConfiguration, UpdateType.Full, DataSource, publicationDateTime, codes);

			Helper.ExportToXMLFile(writer, outputFileFullName);
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeList>(true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.CodeType.Vessel);
			configuration.IncludeColumn(x => x.ZZD_Code, true);
			configuration.IncludeColumn(x => x.ZZD_Description, false);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.DefaultStartDateTime);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultEndDateTime);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(configuration);

			return writerConfiguration;
		}
	}
}
