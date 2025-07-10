using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	class eManifestJobInvoicingConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestOverrides()
		{
			AssertEquals("Code", "MAN", ConsumerType.Code);
			AssertEquals("Code", "e-Manifest", ConsumerType.Description);
			AssertEquals("ControllerID", ControllerIDs.Customs.US.eManifest, ConsumerType.ControllerID);
			AssertEquals("BizoType", ObjectFactory.GetType<Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader>(), ConsumerType.BizoType);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.eManifest;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
