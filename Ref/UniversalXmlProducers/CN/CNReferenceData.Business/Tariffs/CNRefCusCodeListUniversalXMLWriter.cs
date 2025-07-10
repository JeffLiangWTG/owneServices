using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CNRefCusCodeListUniversalXMLWriter
	{
		readonly XmlWriter writer;

		AdditionalElementHelper AdditionalElementHelper { get; }

		public CNRefCusCodeListUniversalXMLWriter(DateTime publicationTime, AdditionalElementHelper additionalElementHelper)
		{
			writer = new XmlWriter(GetWriterConfiguration());
			writer.SetDataSource("CN Cus Code List");
			writer.SetPublicationTime(publicationTime);
			writer.SetUpdateType(UpdateType.Partial);
			AdditionalElementHelper = additionalElementHelper;
		}

		public void Write()
		{
			Write(Constants.FileNames.Xml_CN_RefCusCodeList);
		}

		public void Write(string fileName)
		{
			var outputFile = GlobalOption.Instance.Setting.GetFullOutputFileName(fileName);
			var refCusCodeLists = AdditionalElementHelper.CodeDescriptionDict.Where(x => x.Key.Length > 5).OrderBy(x => x.Value).Select(x => new RefCusCodeList
			{
				ZZD_Code = x.Key,
				ZZD_Description = x.Value,
				ZZD_ZZK_NKCodeType = "ADIEL"
			}).ToList();

			GlobalOption.Instance.Log.Info($"<======================> {refCusCodeLists.Count} Additional Elements in total <======================>");
			if (refCusCodeLists.Any())
			{
				GlobalOption.Instance.Log.Info($"Start to write '{outputFile}'");
				foreach (var refCusCodeList in refCusCodeLists)
				{
					writer.PopulateData(refCusCodeList);
				}
				writer.SaveXml(outputFile);

				GlobalOption.Instance.OutputFiles.Add(outputFile);
				GlobalOption.Instance.Log.Info($"Finish writing '{outputFile}'");
			}
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var cusCodeConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			cusCodeConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeConfiguration.IncludeColumn(x => x.ZZD_Description);
			cusCodeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 1, 1));
			cusCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CN");
			cusCodeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			cusCodeConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);

			var cusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttribute.IncludeColumn(x => x.ZZE_Value);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttribute);

			return writerConfiguration;
		}
	}
}
