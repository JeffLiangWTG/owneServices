using System;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class ESH7DeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "Low Value (H7)";

		protected override Type ExpectedRelatedJobConverterType => typeof(ESH7DeclarationConverter);

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Spain };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road };

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "H7D";

		protected override string ExpectedUsageCategory => "LVD";

		protected override string ExpectedRequiredFeatureControlCode => LicenceFeatureCodeList.Codes.EcommerceH7Feature;

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<Enterprise.Integration.Customs.ESH7.IAsycudaManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new ESH7DeclarationCommand(shipment);
		}
	}
}
