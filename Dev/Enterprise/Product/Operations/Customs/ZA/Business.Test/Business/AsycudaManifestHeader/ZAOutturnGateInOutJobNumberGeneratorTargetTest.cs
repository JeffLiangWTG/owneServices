using System;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Customs.ZA.DataRegistry.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAOutturnGateInOutJobNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var customisation = new ZAOutturnGateInOutJobNumberCustomisation();
			ZAOutturnGateInOutManifestJobNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, false, "OGM");
			ZACustomsRegistry.Instance.ZAOutturnGateInOutJobNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var target = new ZAOutturnGateInOutJobNumberGeneratorTarget { Context = new NumberGeneratorContext() };
			AssertCustomisation("Should find the customisation", "OGM", target.NumberCustomisation);
			AssertLocation(ZACustomsRegistry.Instance.ZAOutturnGateInOutJobNumberCustomization, target.NumberCustomisationLocation);
			AssertEquals(AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, target.MaxLength);
			AssertEquals("Outturn & Gate In/Out job number", target.Name);
		}
	}
}
