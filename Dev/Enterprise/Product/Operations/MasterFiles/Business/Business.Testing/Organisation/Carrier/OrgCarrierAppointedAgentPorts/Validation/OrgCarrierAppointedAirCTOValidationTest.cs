using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedAirCTOValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestOrganisationPK()
		{
			string errorText = "Selected Organization should be Air CTO/Depot";

			CarrierPort.OrganisationPK = CTO.PK;
			AssertHasError(CarrierPort.OrganisationPKInfo, errorText);

			CTO.OH_IsAirCTO = true;
			CarrierPort.Validation.ValidateOrganisationPK();
			AssertNoError(CarrierPort.OrganisationPKInfo, errorText);
		}

		#region Implementation

		OrgHeader CTO
		{
			get { return cto ?? (cto = Factory.New<OrgHeader>()); }
		}

		OrgHeader cto;

		protected override Type ExpectedValidationType
		{
			get { return typeof(OrgCarrierAppointedAirCTOValidation); }
		}

		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(
			OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_AirCTO;
		}

		protected override bool ExpectedMandatoryDirection => true;

		#endregion
	}
}

