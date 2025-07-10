using CargoWise.Application;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.eTail.DataTransfer
{
	public class IEH7DeclarationConverter : BaseH7DeclarationConverter
	{
		public IEH7DeclarationConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair
		{
			Code = ApplicationCodeTypeList.Codes.EuH7V1,
			Description = ApplicationCodeTypeList.Descriptions.EuH7V1,
		};

		protected override void PopulateDeclarantType(ForwardingConsol consolBO, Shipment dataObject)
		{
			dataObject.DeclarantType = new CodeDescriptionPair()
			{
				Code = ObjectFactory.Get<Enterprise.Integration.Customs.EU.IRepresentationTypeList>()._2DirectCode,
				Description = ObjectFactory.Get<Enterprise.Integration.Customs.EU.IRepresentationTypeList>()._2DirectDescription,
			};
		}
	}
}
