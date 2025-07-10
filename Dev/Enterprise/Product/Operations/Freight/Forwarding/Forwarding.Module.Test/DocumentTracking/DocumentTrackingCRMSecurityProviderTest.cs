namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class DocumentTrackingCRMSecurityProviderTest : JobShipmentCRMSecurityProviderTest
	{
		public void TestOrgSecurityGroupSecurityFilterShouldNotBePresent()
		{
			AssertNotNull(ProviderForTest.CRMSecurity.IgnoreOSMG);
			ProviderForTest.CRMSecurity.IgnoreOSMG.IsAllowed = false;

			var filters = new DocumentTrackingFilterBusinessObject();
			AssertNull("Org. Security Group Security filter should not be present", filters["Org. Security Group Security"]);
		}
	}
}
