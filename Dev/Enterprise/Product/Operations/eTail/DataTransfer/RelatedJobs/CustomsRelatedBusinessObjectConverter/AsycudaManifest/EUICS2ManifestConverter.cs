using CargoWise.Application;
using Enterprise.Customs.ManifestBase;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class EUICS2ManifestConverter : AsycudaManifestConverter
	{
		public EUICS2ManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		protected override string EntryCountryCode => Shipment.Destination.Country.Code;

		protected override RecipientRoleType PickupOrDeliveryCartageRole => UniversalDataBuss.Integration.RecipientRoleType.DCA;

		protected override bool ShouldStripNonWesternEuropeanCharacters => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersEUICS2Manifest.Value;

		public override CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair
		{
			Code = ApplicationCodeTypeList.Codes.Consolidator,
			Description = ApplicationCodeTypeList.Descriptions.Consolidator,
		};

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair
		{
			Code = ObjectFactory.Get<Enterprise.Integration.Customs.EUICS2.IEUICS2ManifestTypes>().ENSCodeDescription.Code,
			Description = ObjectFactory.Get<Enterprise.Integration.Customs.EUICS2.IEUICS2ManifestTypes>().ENSCodeDescription.Description,
		};
	}
}
