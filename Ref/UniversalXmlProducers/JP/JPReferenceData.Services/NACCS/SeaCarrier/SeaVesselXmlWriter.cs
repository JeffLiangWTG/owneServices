using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class SeaVesselXmlWriter
	{
		static UpdateType UpdateType => UpdateType.Full;

		public static string DataSource => "Japan Sea Vessel";

		static string FileNameWithoutExtension => "JPSeaVessel";

		static XmlWriterConfiguration Config
		{
			get
			{
				var writerConfig = new XmlWriterConfiguration();

				var vesselZZ = new EntityTypeConfiguration<RefVesselZZ>(true);
				vesselZZ.IncludeColumn(x => x.ZZO_Code, true);
				vesselZZ.IncludeColumn(x => x.ZZO_RadioCallSign, true);
				vesselZZ.IncludeColumnWithConstantValue(x => x.ZZO_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
				writerConfig.IncludeEntityTypeConfiguration(vesselZZ);

				return writerConfig;
			}
		}

		public static void ParseAndSaveXml(IEnumerable<string> records, DateTime publicationDate)
		{
			var isParsed = SeaVesselParser.TryParse(records, out var refVesselCodeList);
			if (isParsed)
			{
				var xmlWriter = new XmlWriterHelper(Config, DataSource, publicationDate, UpdateType, FileNameWithoutExtension);
				xmlWriter.PopulateAndSave(refVesselCodeList);
			}
		}
	}
}
