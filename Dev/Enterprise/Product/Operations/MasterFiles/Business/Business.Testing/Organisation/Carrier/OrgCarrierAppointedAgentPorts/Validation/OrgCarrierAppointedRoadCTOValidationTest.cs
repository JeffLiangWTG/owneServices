using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedCTOValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestOrganisationPK()
		{
			string errorText = "Selected Organization should be Road Depot/Transit Shed";

			CarrierPort.OrganisationPK = CTO.PK;
			AssertHasError(CarrierPort.OrganisationPKInfo, errorText);

			CTO.OH_IsRoadFreightDepot = true;
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
			get { return typeof(OrgCarrierAppointedRoadCTOValidation); }
		}
		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_RoadDepotShed;
		}

		protected override bool ExpectedMandatoryDirection => false;

		#endregion
	}
}
