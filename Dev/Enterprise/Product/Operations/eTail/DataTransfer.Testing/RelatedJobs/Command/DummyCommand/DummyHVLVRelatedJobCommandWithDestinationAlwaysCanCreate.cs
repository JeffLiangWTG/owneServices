using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup(Testing)")]
	[ApplicableLoginCountry(new[] { CountryCodes.Australia })]
	[ShipmentTransportMode(new[] { TransportModes.Sea })]
	[ShipmentDestinationCountry(CountryCodes.Australia)]
	public class DummyHVLVRelatedJobCommandWithDestinationAlwaysCanCreate : BaseHVLVRelatedJobCommand
	{
		public DummyHVLVRelatedJobCommandWithDestinationAlwaysCanCreate(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => (NoResString)"Always Can Create Job (With Destination)";

		public override CustomsRelatedBusinessObjectConverter Converter => throw new NotImplementedException();

		public override string UsageCode => "DM2";

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);
	}
}
