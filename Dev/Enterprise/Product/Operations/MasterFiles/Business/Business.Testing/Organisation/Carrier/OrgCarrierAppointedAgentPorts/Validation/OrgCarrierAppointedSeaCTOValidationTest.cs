using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedSeaCTOValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestTerminalType()
		{
			CarrierPort.O5_TerminalType = "XXX";
			AssertHasError(CarrierPort.O5_TerminalTypeInfo, "Enter a valid " + CarrierPort.O5_TerminalTypeInfo.Description + ".");

			CarrierPort.O5_TerminalType = StevedoreTerminalType.Codes.ContainerTerminal;
			AssertNoNotifications(CarrierPort.O5_TerminalTypeInfo);

			CarrierPort.O5_TerminalType = "";
			AssertHasError(CarrierPort.O5_TerminalTypeInfo, "Please enter a " + CarrierPort.O5_TerminalTypeInfo.Description + ".");
		}

		public void TestOrganisationPK()
		{
			string errorText = "Selected Organization should be Sea CTO/Stevedore";

			CarrierPort.OrganisationPK = CTO.PK;
			AssertHasError(CarrierPort.OrganisationPKInfo, errorText);

			CTO.OH_IsSeaCTO = true;
			CarrierPort.Validation.ValidateOrganisationPK();
			AssertNoError(CarrierPort.OrganisationPKInfo, errorText);
		}

		public void TestSeaCarrierPortsAddress()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var stevedoreForRORO = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			stevedoreForRORO.O5_TerminalType = StevedoreTerminalType.Codes.ROROTerminal;
			stevedoreForRORO.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			stevedoreForRORO.O5_PortOrCountry = "AUPER";

			AssertNoErrors(stevedoreForRORO.O5_OA_AgentOfficeAddressInfo);

			var stevedoreForBulk = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			stevedoreForBulk.O5_TerminalType = StevedoreTerminalType.Codes.BulkTerminal;
			stevedoreForBulk.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			stevedoreForBulk.O5_PortOrCountry = "AUPER";

			AssertNoErrors("RORO Stevedore has same address & location but different terminal type", stevedoreForRORO.O5_OA_AgentOfficeAddressInfo);
			AssertNoErrors("Bulk Stevedore has same address & location but different terminal type", stevedoreForBulk.O5_OA_AgentOfficeAddressInfo);

			var stevedoreForRORO2 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			stevedoreForRORO2.O5_TerminalType = StevedoreTerminalType.Codes.ROROTerminal;
			stevedoreForRORO2.O5_OA_AgentOfficeAddress = carrier.MainAddress.PK;
			stevedoreForRORO2.O5_PortOrCountry = "AUPER";

			AssertHasErrors("Should have conflict with first stevedore for RORO", stevedoreForRORO2.O5_OA_AgentOfficeAddressInfo);

			var newAddress = carrier.Addresses.AddNew();
			newAddress.FillWithValidTestData();
			stevedoreForRORO2.O5_OA_AgentOfficeAddress = newAddress.PK;

			AssertNoErrors("Stevedore 2 should have no error now it has a different address", stevedoreForRORO2.O5_OA_AgentOfficeAddressInfo);
			AssertHasWarning("Different Stevedores for same port, direction and terminal type", stevedoreForRORO2.O5_AgentDirectionInfo, "There is more than one record with port AUPER, direction BTH and terminal type ROR.");
		}

		#region Implementation

		OrgHeader CTO
		{
			get { return cto ?? (cto = Factory.New<OrgHeader>()); }
		}
		OrgHeader cto;

		protected override Type ExpectedValidationType
		{
			get { return typeof(OrgCarrierAppointedSeaCTOValidation); }
		}

		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_Stevedore;
		}

		protected override bool ExpectedMandatoryDirection => true;

		#endregion
	}
}
