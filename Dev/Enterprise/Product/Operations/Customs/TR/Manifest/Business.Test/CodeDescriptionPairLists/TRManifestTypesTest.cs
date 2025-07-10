using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class TRManifestTypesTest : TestCaseWithFactory
	{
		public void TestAllTRManifestTypes()
		{
			AssertEquals(17, TRManifestTypes.All.Count);
		}

		public void TestManifestType_ATAITH()
		{
			var ataita = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.ATAITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, ataita.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, ataita.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, ataita.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_ATAIHR()
		{
			var ataihr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.ATAIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, ataihr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, ataihr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, ataihr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_CIKONC()
		{
			var cikonc = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.CIKONC);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, cikonc.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, cikonc.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, cikonc.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DEMITH()
		{
			var demith = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DEMITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Rail }, demith.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, demith.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, demith.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DEMIHR()
		{
			var demihr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DEMIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Rail }, demihr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, demihr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, demihr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DENITH()
		{
			var denith = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DENITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Sea }, denith.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, denith.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, denith.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DENIHR()
		{
			var denitr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DENIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Sea }, denitr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, denitr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, denitr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DIGITH()
		{
			var digith = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DIGITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, digith.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, digith.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, digith.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_DIGIHR()
		{
			var digihr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.DIGIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, digihr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, digihr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, digihr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_GRUPAJ()
		{
			var grupaj = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.GRUPAJ);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, grupaj.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, grupaj.ApplicableManifestStyles);
				AssertContainsExactElementsInAnyOrder("Manifest Natures", new[] { ShipmentTypeList.Codes.Export22, ShipmentTypeList.Codes.Import23 }, grupaj.ManifestNatures.GetAllCodes());
			});
		}

		public void TestManifestType_HAVIHR()
		{
			var havihr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.HAVIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Air }, havihr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine }, havihr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, havihr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_HAVITH()
		{
			var havith = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.HAVITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Air }, havith.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, havith.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, havith.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_TESLIM()
		{
			var teslim = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.TESLIM);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, teslim.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator }, teslim.ApplicableManifestStyles);
				AssertContainsExactElementsInAnyOrder("Manifest Natures", new[] { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Export22 }, teslim.ManifestNatures.GetAllCodes());
			});
		}

		public void TestManifestType_TIRIHR()
		{
			var tirihr = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.TIRIHR);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Road }, tirihr.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, tirihr.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Export22, tirihr.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_TIRITH()
		{
			var tirith = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.TIRITH);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Road }, tirith.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, tirith.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, tirith.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_VARONC()
		{
			var varonc = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.VARONC);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", TransportModes, varonc.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, varonc.ApplicableManifestStyles);
				AssertEquals("Manifest Natures", ShipmentTypeList.Codes.Import23, varonc.ManifestNatures.CodesAsString);
			});
		}

		public void TestManifestType_EMANIF()
		{
			var emanif = TRManifestTypes.All.Single(x => x.Code == TRManifestTypes.Codes.EMANIF);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Transport Modes", new[] { Core.Constants.TransportModes.Sea }, emanif.ApplicableTransportModes);
				AssertContainsExactElementsInAnyOrder("Manifest Styles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, emanif.ApplicableManifestStyles);
				AssertContainsExactElementsInAnyOrder("Manifest Natures", new[] { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Export22 }, emanif.ManifestNatures.GetAllCodes());
			});
		}

		public void TestIsManifestTypesRelatedToSea()
		{
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.CIKONC));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.DENIHR));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.DENITH));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.HAVIHR));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.HAVITH));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.VARONC));
			Assert(TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.EMANIF));
			Assert(!TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.ATAIHR));
			Assert(!TRManifestTypes.IsManifestTypesRelatedToSea(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.EMANIF));
		}

		public void TestIsManifestTypesRelatedToAir()
		{
			Assert(TRManifestTypes.IsManifestTypesRelatedToAir(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.VARONC));
			Assert(TRManifestTypes.IsManifestTypesRelatedToAir(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.HAVITH));
			Assert(!TRManifestTypes.IsManifestTypesRelatedToAir(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.ATAIHR));
			Assert(!TRManifestTypes.IsManifestTypesRelatedToAir(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.EMANIF));
		}

		public void TestIsNeedToDefaultTransportModesAndManifesTypes()
		{
			Assert(TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.VARONC));
			Assert(TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.CIKONC));
			Assert(TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.VARONC));
			Assert(TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.CIKONC));
			Assert(!TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Sea, TRManifestTypes.Codes.TIRITH));
			Assert(!TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Air, TRManifestTypes.Codes.TIRITH));
			Assert(!TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Road, TRManifestTypes.Codes.VARONC));
			Assert(!TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Core.Constants.TransportModes.Road, TRManifestTypes.Codes.CIKONC));
		}

		IEnumerable<string> TransportModes => new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.InlandWaterwayTransport, Core.Constants.TransportModes.OwnPropulsion };
	}
}
