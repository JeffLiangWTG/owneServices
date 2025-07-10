using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder
{
	class NomenclatureEndDateXmlBuilder
	{
		public static void Write(string filePath, IEnumerable<Tuple<string, DateTime>> codes)
		{
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "IMP");
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_StartDate, false, new DateTime(1900, 01, 01));
			tariffConfig.IncludeColumn(x => x.ZZ1_EndDate, false, IsDataValue.True);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);

			var writer = new XmlWriter(writerConfiguration);
			writer.SetDataSource("EUN Tariffs End Dates");
			writer.SetPublicationTime(DateTime.Now);
			writer.SetUpdateType(UpdateType.Partial);

			foreach (var code in codes)
			{
				writer.PopulateData(new RefCusTariff { ZZ1_TariffCode = code.Item1, ZZ1_EndDate = code.Item2 });
			}

			writer.SaveXml(filePath);
		}
	}
}
