using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.Organisation;
		}

		public void TestOrganisationConsumerType()
		{
			AssertEquals(typeof(OrganisationConsumerType), JobInvoicingConsumerTypes.Organisation.GetType());
			AssertEquals("ORG", JobInvoicingConsumerTypes.Organisation.Code);
			AssertEquals("Organization", JobInvoicingConsumerTypes.Organisation.Description);
			AssertEquals(ControllerIDs.Organisation, JobInvoicingConsumerTypes.Organisation.ControllerID);
			AssertEquals(typeof(OrgHeader), JobInvoicingConsumerTypes.Organisation.BizoType);
		}
	}
}
