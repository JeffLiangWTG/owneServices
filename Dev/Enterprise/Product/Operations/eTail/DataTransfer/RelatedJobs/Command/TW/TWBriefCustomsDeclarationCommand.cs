using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Taiwan })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea })]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export })]
	public class TWBriefCustomsDeclarationCommand : CustomsJobCommand
	{
		public TWBriefCustomsDeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("934CFF4E-05B2-433E-A016-77C11F3760CA", "Taiwan Brief Customs Declaration {0}",
			Shipment.JobDirection == Directions.Import
			? ResString.GetMultilingualString("E80F6BFE-D73C-49EC-B0B6-3322CE849652", "Import")
			: ResString.GetMultilingualString("F284CACD-D962-4443-8A79-167DECD6D5A4", "Export"));

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new TWBriefCustomsDeclarationConverter(this);

		public override string UsageCode => UsageCodes.TWBriefCustomsDeclaration;
	}
}
