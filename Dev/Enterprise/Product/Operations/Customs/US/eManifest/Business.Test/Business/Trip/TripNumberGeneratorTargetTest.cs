using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var customisation = new TripNumberCustomisation();
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, false, "TST");
			USeManifestDataRegistry.Instance.NumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var target = new TripNumberGeneratorTarget { Context = new NumberGeneratorContext() };
			AssertCustomisation("Should find the customisation", "TST", target.NumberCustomisation);
			AssertLocation(USeManifestDataRegistry.Instance.NumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(CusInBondHeaderSchema.BH_JobReference.MaxLength, target.MaxLength);
			AssertEquals("e-Manifest number", target.Name);
		}
	}
}
