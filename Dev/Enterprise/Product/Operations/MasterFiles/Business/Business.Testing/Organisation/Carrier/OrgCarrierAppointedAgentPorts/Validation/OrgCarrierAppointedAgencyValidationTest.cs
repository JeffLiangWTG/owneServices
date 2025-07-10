using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedAgencyValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		#region Implementation

		protected override Type ExpectedValidationType
		{
			get { return typeof(OrgCarrierAppointedAgencyValidation); }
		}
		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_Agency;
		}

		protected override bool ExpectedMandatoryDirection => false;

		public void TestPortOrCountry_CheckAgencyPerPortOrCountry()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var appointedAgent = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgent.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			appointedAgent.O5_PortOrCountry = "AUSYD";
			AssertNoErrors(appointedAgent.O5_PortOrCountryInfo);

			var appointedAgent1 = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgent1.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			appointedAgent1.O5_PortOrCountry = "NZAKL";
			AssertNoErrors(appointedAgent1.O5_PortOrCountryInfo);

			var appointedAgent2 = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgent2.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Agency;
			appointedAgent2.O5_PortOrCountry = "NZAKL";
			AssertHasErrors(appointedAgent2.O5_PortOrCountryInfo);

			appointedAgent2.O5_PortOrCountry = "NZ";
			AssertNoErrors(appointedAgent2.O5_PortOrCountryInfo);
		}
		#endregion
	}
}
