using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
{
	public class RefUNLOCOUtcOffsetXmlProducer
	{
		readonly XmlWriter xmlWriter;
		readonly DateTime publishTime;
		public RefUNLOCOUtcOffsetXmlProducer(DateTime publishTime)
		{
			this.publishTime = publishTime;
			xmlWriter = new XmlWriter(GetWriterConfiguration());
		}

		public void ExportXML(IEnumerable<RefUNLOCOUtcOffset> refUNLOCOUtcOffsets)
		{
			InitializeWriter();
			foreach (var refUNLOCOUtcOffset in refUNLOCOUtcOffsets)
			{
				xmlWriter.PopulateData(refUNLOCOUtcOffset);
			}
			xmlWriter.SaveXml(Path.Combine(ApplicationConfig.OutputFilePath, ApplicationConfig.OutputFilename));
		}

		void InitializeWriter()
		{
			xmlWriter.SetDataSource(ApplicationConfig.DataSourceName);
			xmlWriter.SetPublicationTime(publishTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var unlocoOffsetConfiguration = new EntityTypeConfiguration<RefUNLOCOUtcOffset>(true);
			unlocoOffsetConfiguration.IncludeColumn(x => x.RLO_RL_NKCode, true);
			unlocoOffsetConfiguration.IncludeColumn(x => x.RLO_StartTimeUtc, true);
			unlocoOffsetConfiguration.IncludeColumn(x => x.RLO_EndTimeUtc, false);
			unlocoOffsetConfiguration.IncludeColumn(x => x.RLO_OffsetMinutesFromUtc, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(unlocoOffsetConfiguration);

			return writerConfiguration;
		}
	}
}
