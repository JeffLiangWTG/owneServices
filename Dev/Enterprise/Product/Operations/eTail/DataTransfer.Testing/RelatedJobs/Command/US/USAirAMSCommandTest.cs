using System;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class USAirAMSCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "US Air AMS (Import)";

		protected override Type ExpectedRelatedJobConverterType => typeof(USAirAMSConverter);

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air };

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.UnitedStates };

		protected override bool ExpectedShouldValidateWaybill => true;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => false;

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.UnitedStates;

		protected override string ExpectedAllowLoginToDifferentCountryRegistry => HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Name;

		protected override string ExpectedUsageCode => "UAM";

		protected override string ExpectedUsageCategory => "SEC";

		protected override Type ExpectedCustomsJobsType => typeof(Customs.US.ACEManifest.Business.AsycudaManifestHeader);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GetOrCreateHVLVConsignmentHeader();
			return new USAirAMSCommand(shipment);
		}
	}
}
