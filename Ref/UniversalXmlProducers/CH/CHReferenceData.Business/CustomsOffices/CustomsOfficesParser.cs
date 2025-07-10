using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CustomsOffices
{
	public class CustomsOfficesParser
	{
		readonly DownloadResult download;

		public CustomsOfficesParser(DownloadResult download)
		{
			this.download = download;
		}

		public void ConvertToRefXML(string outputFilePath, string dataSource, DateTime actualDate = default)
		{
			var writerConfiguration = GetRefCodeListConfiguration();
			var inputDoc = Helper.DeserializeXML<customsOffices>(download.Content);
			var outputCodeList = new List<RefCusCodeList>();

			foreach (var officeGroup in from office in inputDoc.customsOffice
											 group office by office.number into officeGroup
											 select officeGroup)
			{
				var validOffice = (from office in officeGroup
									where office.validFrom <= actualDate && office.validTo >= actualDate
									orderby office.validTo descending
									select office).FirstOrDefault();
				if (validOffice == null)
				{
					validOffice = (from office in officeGroup
										orderby office.validTo descending
										select office).First();
				}

				outputCodeList.Add(new RefCusCodeList
				{
					ZZD_Code = validOffice.number,
					ZZD_Description = validOffice.name,
					ZZD_StartDate = validOffice.validFrom.Truncate(),
					ZZD_EndDate = validOffice.validTo.Truncate().EndOfDay(),
					RefCusCodeListAttributes = new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute
						{
							ZZE_ZXE_NKName = "CITY",
							ZZE_Value = validOffice.city
						},
						new RefCusCodeListAttribute
						{
							ZZE_ZXE_NKName = "Street",
							ZZE_Value = validOffice.address
						}
					}
				});
			}

			var published = inputDoc.created;

			Helper.ExportToXMLFile(dataSource, outputFilePath, writerConfiguration, published, outputCodeList);
		}

		static XmlWriterConfiguration GetRefCodeListConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "CUSCH");
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CH");
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_EndDate, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);

			return writerConfiguration;
		}
	}
}
