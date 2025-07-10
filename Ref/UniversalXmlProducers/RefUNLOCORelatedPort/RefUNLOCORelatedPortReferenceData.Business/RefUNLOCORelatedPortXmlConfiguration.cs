using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business
{
	public static class RefUNLOCORelatedPortXmlConfiguration
	{
		public static IXmlWriterConfiguration GetConfiguration()
		{
			var relatedPortConfiguration = new EntityTypeConfiguration<RefUNLOCORelatedPort>(true);
			relatedPortConfiguration.IncludeColumn(x => x.RLR_GroupNumber, isKeyColumn: true);
			relatedPortConfiguration.IncludeColumn(x => x.RLR_RL_NKRelatedPort, isKeyColumn: true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(relatedPortConfiguration);

			return writerConfiguration;
		}
	}
}
