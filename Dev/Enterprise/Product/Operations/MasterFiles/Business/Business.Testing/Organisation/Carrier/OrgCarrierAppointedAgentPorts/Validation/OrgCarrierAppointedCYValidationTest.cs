using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedCYValidationTest : OrgCarrierAppointedAgentPortsValidationBaseTest
	{
		public void TestOrganisationPK()
		{
			string errorText = "Selected Organization should be a container yard.";

			CarrierPort.OrganisationPK = CY.PK;
			AssertHasError(CarrierPort.OrganisationPKInfo, errorText);

			CY.OH_IsContainerYard = true;
			CarrierPort.Validation.ValidateOrganisationPK();
			AssertNoError(CarrierPort.OrganisationPKInfo, errorText);
		}

		#region Implementation

		OrgHeader CY
		{
			get { return cy ?? (cy = Factory.New<OrgHeader>()); }
		}
		OrgHeader cy;

		protected override Type ExpectedValidationType
		{
			get { return typeof(OrgCarrierAppointedCYValidation); }
		}
		protected override OrgCarrierAppointedAgentPortsDependentCollection GetCarrierAppointedAgentsPortsCollection(OrgHeader header)
		{
			return header.CarrierAppointedAgentPorts_ContainerYardPark;
		}

		protected override bool ExpectedMandatoryDirection => true;

		#endregion
	}
}
