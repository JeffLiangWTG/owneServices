using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing
{
	[TestedType(typeof(NOManifestTypes))]
	sealed class NOManifestTypesTest : TestCaseWithFactory
	{
		public void TestAllNOManifestTypes()
		{
			AssertEquals(1, new NOManifestTypes().All.Count);
		}

		public void TestManifestType_DMO()
		{
			var dmo = new NOManifestTypes().All.Single(x => x.Code == NOManifestTypes.Codes.DMO);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, dmo.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, dmo.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, dmo.ManifestNatures.CodesAsString);
			});
		}

		IEnumerable<string> TransportModes => new[] {
			Core.Constants.TransportModes.Air,
			Core.Constants.TransportModes.Rail,
			Core.Constants.TransportModes.Sea,
			Core.Constants.TransportModes.Road,
			Core.Constants.TransportModes.Mail,
			Core.Constants.TransportModes.FixedTransportInstallations,
			Core.Constants.TransportModes.InlandWaterwayTransport,
			Core.Constants.TransportModes.OwnPropulsion };
	}
}
