using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public static class SupplierParser
	{
		const string DataSource = "NZ Supplier List";
		const string OutputFileName = "RefCusCodeListZZ_NZ_Supplier.xml";

		public static readonly string DataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + $@".Last{nameof(SupplierParser)}Data");

		public static IEnumerable<RefCusCodeList> Parse(string localFilePath, DateTime publicationDateTime)
		{
			var lines = File.ReadAllLines(localFilePath, Encoding.UTF8).Select(c => c.Trim()).Distinct();

			var lastData = File.Exists(DataFilePath)
				? new HashSet<string>(File.ReadAllLines(DataFilePath).Select(c => c.Trim()))
				: new HashSet<string>(0);

			var filterLines = lastData.Any() ? lines.Except(lastData) : lines;

			foreach (var line in filterLines)
			{
				var info = BuildSupplierInfo(line);

				if (info?.IsValid ?? false)
				{
					var code = info.Code;

					yield return new RefCusCodeList()
					{
						ZZD_Code = code.Substring(0, Math.Min(35, code.Length)),
						ZZD_Description = info.Name,
						ZZD_StartDate = publicationDateTime,

						RefCusCodeListAttributes = new[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = Constants.AttributeName.Country, ZZE_Value = info.CountryCode } }
					};
				}
			}
		}

		static SupplierInfo BuildSupplierInfo(string line)
		{
			SupplierInfo result = null;

			if (!string.IsNullOrWhiteSpace(line))
			{
				var array = line.Split(Constants.SupplierSplitChar) ?? Array.Empty<string>();

				if (array.Length == 3)
				{
					result = new SupplierInfo()
					{
						Code = array[0].Trim(),
						Name = array[1].Trim(),
						CountryCode = array[2].Trim()
					};
				}
			}

			return result;
		}

		public static void ExportToXMLFile(string localFilePath, string outputFolderPath, DateTime publicationDateTime)
		{
			var codes = Parse(localFilePath, publicationDateTime);
			var outputFileFullName = Path.Combine(outputFolderPath, OutputFileName);

			var writerConfiguration = GetWriterConfiguration();
			var writer = Helper.GenerateXmlWriter(writerConfiguration, UpdateType.Partial, DataSource, publicationDateTime, codes);

			Helper.ExportToXMLFile(writer, outputFileFullName);
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.CodeType.Supplier);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.DefaultStartDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultEndDateTime);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);

			return writerConfiguration;
		}
	}
}
