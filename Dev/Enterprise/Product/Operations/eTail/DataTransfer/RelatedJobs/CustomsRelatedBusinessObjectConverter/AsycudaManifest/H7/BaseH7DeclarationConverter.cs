using CargoWise.Application;
using Enterprise.Customs.ManifestBase;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class BaseH7DeclarationConverter : AsycudaManifestConverter
	{
		public BaseH7DeclarationConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		protected override string EntryCountryCode => Shipment.Destination.Country.Code;

		protected override RecipientRoleType PickupOrDeliveryCartageRole => UniversalDataBuss.Integration.RecipientRoleType.DCA;

		protected override bool IncludeAdditionalReferenceCollectionOverride => true;

		public override CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair
		{
			Code = ApplicationCodeTypeList.Codes.EuH7,
			Description = ApplicationCodeTypeList.Descriptions.EuH7,
		};

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair
		{
			Code = ObjectFactory.Get<Enterprise.Integration.Customs.EUH7.IEUH7ManifestTypes>().EH7CodeDescription.Code,
			Description = ObjectFactory.Get<Enterprise.Integration.Customs.EUH7.IEUH7ManifestTypes>().EH7CodeDescription.Description,
		};
	}
}
