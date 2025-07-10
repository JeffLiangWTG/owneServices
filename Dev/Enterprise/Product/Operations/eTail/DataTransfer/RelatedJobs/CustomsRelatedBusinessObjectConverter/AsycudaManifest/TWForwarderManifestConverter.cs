using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.eTail.DataTransfer
{
	public class TWForwarderManifestConverter : AsycudaManifestConverter
	{
		public TWForwarderManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair
		{
			Code = TWManifestTypes.Codes.MAN,
			Description = TWManifestTypes.Descriptions.MAN,
		};
	}
}
