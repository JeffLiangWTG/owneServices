using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
{
	public void TestManifestVehicleRegistration() => CombineAssertions(() =>
	{
		var sourceConsol = Factory.New<ForwardingConsol>();
		var transport = sourceConsol.Transports.MostInterestingTransport;
		transport.JW_VoyageFlightForBinding = "ABC1234";

		var header = Factory.New<AsycudaManifestHeader>();
		header.SetParent(sourceConsol);
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();
		AssertEquals("Manifest AMA_VehicleRegistration should be inherited from consol", "ABC1234", header.AMA_VehicleRegistration);
	});
}
