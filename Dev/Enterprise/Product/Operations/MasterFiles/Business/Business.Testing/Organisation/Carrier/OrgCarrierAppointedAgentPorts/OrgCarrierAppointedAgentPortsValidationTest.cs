using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class OrgCarrierAppointedAgentPortsValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestPortOrCountry()
		{
			CarrierPort.O5_PortOrCountry = "BLAT";
			AssertHasError(CarrierPort.O5_PortOrCountryInfo, "Enter a valid " + CarrierPort.O5_PortOrCountryInfo.Description + ".");

			CarrierPort.O5_PortOrCountry = "AUBNE";
			AssertNoErrors(CarrierPort.O5_PortOrCountryInfo);

			CarrierPort.O5_PortOrCountry = "";
			AssertHasError(CarrierPort.O5_PortOrCountryInfo, "Please enter a " + CarrierPort.O5_PortOrCountryInfo.Description + ".");
		}

		/// <summary>
		/// Multiple agents may share the same port provided they are of different types
		/// </summary>
		public void TestPortOrCountry_AllowsMultiplePortsWithDifferentAgentTypes()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var appointedAgent = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Appointed Agent should have no errors", appointedAgent.O5_PortOrCountryInfo);

			var airCTOAgent = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTOAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Air CTO Agent should have no errors", airCTOAgent.O5_PortOrCountryInfo);

			var containerYardParkAgent = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			containerYardParkAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Container Yard Park Agent should have no errors", containerYardParkAgent.O5_PortOrCountryInfo);

			var railHeadDepotAgent = carrier.CarrierAppointedAgentPorts_RailHeadDepot.AddNew();
			railHeadDepotAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Rail Head Depot Agent should have no errors", railHeadDepotAgent.O5_PortOrCountryInfo);

			var roadDepotShedAgent = carrier.CarrierAppointedAgentPorts_RoadDepotShed.AddNew();
			roadDepotShedAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Road depot Shed Agent should have no errors", roadDepotShedAgent.O5_PortOrCountryInfo);

			var stevedoreAgent = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			stevedoreAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors("Stevedore Agent should have no errors", stevedoreAgent.O5_PortOrCountryInfo);
		}

		public void TestCarrierPortsAddress()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var agency = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			agency.O5_PortOrCountry = "AUPER";

			agency.O5_OA_AgentOfficeAddress = ZGuid.Empty;
			AssertHasErrors("Address can not be empty", agency.O5_OA_AgentOfficeAddressInfo);

			agency.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			AssertNoErrors(agency.O5_OA_AgentOfficeAddressInfo);

			var roadDepot = carrier.CarrierAppointedAgentPorts_RoadDepotShed.AddNew();
			roadDepot.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			roadDepot.O5_PortOrCountry = "AUPER";

			AssertNoErrors("Different types of carrier ports should not conflict", agency.O5_OA_AgentOfficeAddressInfo);
			AssertNoErrors("Different types of carrier ports should not conflict", roadDepot.O5_OA_AgentOfficeAddressInfo);

			var newAgency = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			newAgency.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			newAgency.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			newAgency.O5_PortOrCountry = "AUPER";

			AssertHasErrors("Should have conflicting Agency carriers with the same port and org addresses", newAgency.O5_OA_AgentOfficeAddressInfo);

			newAgency.O5_OA_AgentOfficeAddress = NewFactory().NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNoErrors("Different agency addresses for same port and direction should not conflict", newAgency.O5_OA_AgentOfficeAddressInfo);
			AssertHasWarning("Different agency addresses for same port and direction should have warning", newAgency.O5_AgentDirectionInfo, "There is more than one record with port AUPER and direction BTH.");

			var newAddress = carrier.Addresses.AddNew();
			newAddress.FillWithValidTestData();
			newAgency.O5_OA_AgentOfficeAddress = newAddress.PK;
			AssertNoErrors(newAgency.O5_OA_AgentOfficeAddressInfo);

			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrierAgency = newCarrier.CarrierAppointedAgentPorts_Agency.AddNew();
			newCarrierAgency.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			newCarrierAgency.O5_PortOrCountry = "AUPER";
			AssertNoErrors("Carrier types with the same information but different orgs should not conflict", newCarrierAgency.O5_OA_AgentOfficeAddressInfo);
		}

		#region Implementation

		protected override Type ExpectedValidationType
		{
			get { return typeof(OrgCarrierAppointedAirCTOValidation); }
		}

		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_AirCTO;
		}

		protected override bool ExpectedMandatoryDirection => true;

		#endregion
	}

	public abstract class OrgCarrierAppointedAgentPortsValidationBaseTest : BusinessObjectValidationTestCase
	{
		public void TestUsingCorrectValidationType()
		{
			ZValidation validation = CarrierPort.Validation;
			AssertEquals("should be using the correct type of validation class.", ExpectedValidationType, validation == null ? null : validation.GetType());
		}

		public void TestAgentDirection()
		{
			CarrierPort.O5_AgentDirection = "IDK";
			CarrierPort.Validation.ValidateO5_AgentDirection();
			AssertEquals(ExpectedMandatoryDirection, CarrierPort.O5_AgentDirectionInfo.HasError("Enter a valid Direction."));

			CarrierPort.O5_AgentDirection = "";
			CarrierPort.Validation.ValidateO5_AgentDirection();
			AssertEquals(ExpectedMandatoryDirection, CarrierPort.O5_AgentDirectionInfo.HasError("Please enter a Direction."));
		}

		#region Implementation

		protected OrgCarrierAppointedAgentPorts CarrierPort
		{
			get { return carrierPorts ?? (carrierPorts = GetCarrierAppointedAgentsPortsCollection(Header).AddNew()); }
		}
		OrgCarrierAppointedAgentPorts carrierPorts;

		protected OrgHeader Header
		{
			get { return header ?? (header = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader header;

		#endregion

		protected abstract Type ExpectedValidationType { get; }
		protected abstract OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header);
		protected abstract bool ExpectedMandatoryDirection { get; }
	}
}
