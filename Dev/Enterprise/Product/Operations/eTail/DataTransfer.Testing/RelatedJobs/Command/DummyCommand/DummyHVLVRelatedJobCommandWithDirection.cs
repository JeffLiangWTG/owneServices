using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup(Testing)")]
	[ApplicableLoginCountry(new[] { CountryCodes.UnitedStates, CountryCodes.NewZealand }, allowLoginToDifferentCountryRegistry: nameof(HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies))]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export, Directions.Domestic })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road })]
	public class DummyHVLVRelatedJobCommandWithDirection : BaseHVLVRelatedJobCommand
	{
		public DummyHVLVRelatedJobCommandWithDirection(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => (NoResString)"Always Can Create Job (With Direction)";

		public override CustomsRelatedBusinessObjectConverter Converter => throw new NotImplementedException();

		public override string UsageCode => "DM1";

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);
	}
}
