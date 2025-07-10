using System;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class FRH7DeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "Low Value (H7)";

		protected override Type ExpectedRelatedJobConverterType => typeof(FRH7DeclarationConverter);

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.France };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road };

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => UsageCodes.EUH7Declaration;

		protected override string ExpectedUsageCategory => UsageCategories.LowValueDeclaration;

		protected override string ExpectedRequiredFeatureControlCode => LicenceFeatureCodeList.Codes.EcommerceH7Feature;

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<Enterprise.Integration.Customs.FRH7.IH7ManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);

			return new FRH7DeclarationCommand(shipment);
		}
	}
}
