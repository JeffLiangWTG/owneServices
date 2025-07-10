using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public abstract class RefCusCodeTypeProducer
	{
		protected RefCusCodeTypeProducer()
		{
		}

		public void GenerateFile(string outputPath)
		{
			var dataCollection = GetDataCollection();
			var xmlWriterConfiguration = GetXmlWriterConfiguration();
			var outputFilePath = Path.Combine(outputPath, $"RefCusCodeTypeZZ_EUN_{CodeType}.xml");
			XmlWriterHelper.ExportToXMLFile(DataSource, outputFilePath, xmlWriterConfiguration, PublicationDate, dataCollection, UpdateType);
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeType = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeType.IncludeColumn(x => x.ZZK_CodeType, true);
			codeType.IncludeColumn(x => x.ZZK_Description, false);
			codeType.IncludeColumn(x => x.ZZK_IsReadonly, false);
			codeType.IncludeColumn(x => x.ZZK_MaxLength, false);
			codeType.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, DataGrouping);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeType);

			return xmlWriterConfiguration;
		}

		RefCusCodeType[] GetDataCollection()
		{
			return new RefCusCodeType[]
			{
				new RefCusCodeType()
				{
					ZZK_CodeType = CodeType,
					ZZK_Description = Description,
					ZZK_IsReadonly = ReadOnly,
					ZZK_MaxLength = MaxLength,
				}
			};
		}

		static UpdateType UpdateType => UpdateType.Full;

		public string DataSource => $"{DataGrouping} {Description} Code Type";

		static string DataGrouping => "EUN";
		
		protected abstract string CodeType { get; }

		protected abstract string Description { get; }

		protected abstract bool ReadOnly { get; }

		protected abstract byte MaxLength { get; }

		public abstract DateTime PublicationDate { get; }
	}
}
