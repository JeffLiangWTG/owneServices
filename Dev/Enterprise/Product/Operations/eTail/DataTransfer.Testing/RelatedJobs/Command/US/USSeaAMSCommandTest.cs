using System;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class USSeaAMSCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "US Sea AMS (Import)";

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.UnitedStates;

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.UnitedStates };

		protected override Type ExpectedRelatedJobConverterType => typeof(USSeaAMSConverter);

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Sea };

		protected override bool ExpectedShouldValidateWaybill => true;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => false;

		protected override string ExpectedAllowLoginToDifferentCountryRegistry => HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Name;

		protected override string ExpectedUsageCode => "USM";

		protected override string ExpectedUsageCategory => "SEC";

		protected override Type ExpectedCustomsJobsType => typeof(CusInBondHeader);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GetOrCreateHVLVConsignmentHeader();
			return new USSeaAMSCommand(shipment);
		}
	}
}
