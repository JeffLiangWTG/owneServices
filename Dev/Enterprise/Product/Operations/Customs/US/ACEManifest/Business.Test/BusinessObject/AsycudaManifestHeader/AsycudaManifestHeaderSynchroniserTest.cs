using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestIsExpressCourier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("IsExpressCourier", false, manifestHeader.IsExpressCourier);

			consol.JK_AgentType = Core.Constants.AgentType.Courier;
			AssertEquals("IsExpressCourier", true, manifestHeader.IsExpressCourier);
		}
	}
}
