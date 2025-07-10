using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public class RefAccessorialXmlProducer(IHttpWebHelper<ResponseResult<AccessorialInfo[]>> httpWebHelper, IAccessTokenProvider tokenProvider) : IXmlProducer
	{
		private readonly IHttpWebHelper<ResponseResult<AccessorialInfo[]>> _httpWebHelper = httpWebHelper;
		private readonly IAccessTokenProvider _tokenProvider = tokenProvider;

		public async Task ProduceXmlAsync(string outputPath, DateTime publicationDate)
		{
			var exportFilePath = Path.Combine(outputPath, "RefAccessorialList.xml");
			var refAccessorialService = new ResponseResultService<AccessorialInfo>(_httpWebHelper, _tokenProvider);
			var refAccessorialList = await refAccessorialService.GetRefAccessorialListAsync(AppConfigurationProvider.AppConfiguration).ConfigureAwait(false);
			var refAccessorials = AccessorialInfoParser.Convert(refAccessorialList);

			var refAccessorialConfiguration = new EntityTypeConfiguration<RefAccessorial>(true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Code, isKeyColumn: true);
			refAccessorialConfiguration.IncludeColumn(x => x.ASI_Description);

			var producer = new XmlProducer<RefAccessorial>(
				refAccessorialConfiguration,
				publicationDate,
				"Accessorial List",
				refAccessorials,
				exportFilePath
			);
			producer.ExportXml();
		}
	}
}
