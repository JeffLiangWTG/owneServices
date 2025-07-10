using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public class SGIFTRINMessageProcessor : IFTRINMessageProcessor
	{
		public SGIFTRINMessageProcessor(IStagingRepository staging, string outputPath)
			: base(staging, DataSourceConstants.Country.Singapore, outputPath)
		{
		}

		protected override BaseMessageProcessor GetProcessor(string contentType)
		{
			return contentType == DataSourceConstants.ContentType.Edifact
				? (BaseMessageProcessor)new SGEDIFACTMessageProcessor(outputPath)
				: new SGXMLMessageProcessor(outputPath);
		}
	}
}
