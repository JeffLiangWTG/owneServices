using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedRailCTOValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestOrganisationPK()
		{
			string errorText = "Selected Organization should be Rail Head/Depot";

			CarrierPort.OrganisationPK = CTO.PK;
			AssertHasError(CarrierPort.OrganisationPKInfo, errorText);

			CTO.OH_IsRailHead = true;
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
			get { return typeof(OrgCarrierAppointedRailCTOValidation); }
		}

		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_RailHeadDepot;
		}

		protected override bool ExpectedMandatoryDirection => false;

		#endregion
	}
}
