using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwadingUNDGDataItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeOfUNDGSubstanceCollection()
		{
			var lookups = new ForwardingUNDGDataItemLookups(Factory.New<ForwardingUNDGDataItem>());
			var collection = lookups.UNDGSubstances;

			AssertType<ForwardingUNDGSubstanceCollection>(collection);
		}

		public void TestUNDGSubstances_ShouldBeFilteredBasedOnForwardingShipmentTransportMode()
		{
			var expectedProperty = "Standard" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			var item = packLine.UNDGs.AddNew();
			var lookup = new ForwardingUNDGDataItemLookups(item as ForwardingUNDGDataItem);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(true, lookup.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(true, lookup.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(true, lookup.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));

			item = Factory.New<ForwardingUNDGDataItem>();
			lookup = new ForwardingUNDGDataItemLookups(item as ForwardingUNDGDataItem);
			AssertEquals(true, lookup.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
		}

		public void TestUNDGSubstances_ShouldBeFilteredBasedOnForwardingShipmentTransportMode_OnUpdateUNDG()
		{
			var expectedProperty = "Standard:Property";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItemForBinding = packline.UNDGs.FirstItemForBinding.AddNew();
			var lookup = new ForwardingUNDGDataItemLookups(undgDataItemForBinding as ForwardingUNDGDataItem);

			AssertEquals(UNDGSubstanceStandardTypes.IATA, lookup.UNDGSubstances.FilterBusinessObjectDefaults[expectedProperty].Value);
		}

		public void TestPackingInstructionSectionList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var dangerousGood = packline.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries;
			substance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries;
			dangerousGood.DI_DG = substance.PK;

			AssertCollectionNotContains(PackingInstructionSectionTypeList.Codes.SectionII, dangerousGood.Lookups.PackingInstructionSectionList.GetAllCodes());

			AssertContainsExactElementsInAnyOrder(dangerousGood.Lookups.PackingInstructionSectionList.GetAllCodes(), new string[] {
			PackingInstructionSectionTypeList.Codes.SectionIA ,
			PackingInstructionSectionTypeList.Codes.SectionIB });

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries;
			substance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries;
			dangerousGood.DI_DG = substance.PK;
			AssertContainsExactElementsInAnyOrder(dangerousGood.Lookups.PackingInstructionSectionList.GetAllCodes(), new string[] {
				PackingInstructionSectionTypeList.Codes.SectionI ,
				PackingInstructionSectionTypeList.Codes.SectionII });

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_Code = "1001";
			substance.DG_UNNO = "1001";
			dangerousGood.DI_DG = substance.PK;
			AssertEquals(0, dangerousGood.Lookups.PackingInstructionSectionList.Count);
		}

		public void TestIsCFRStandardSuitable()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "US3VA";
			shipment.JS_RL_NKDestination = "GBLON";

			var consolTransportLeg = consol.Transports.AddNew();
			consolTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Road;
			consolTransportLeg.JW_RL_NKLoadPort = "NZAKL";
			consolTransportLeg.JW_RL_NKDiscPort = "AUPER";

			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew() as ForwardingUNDGDataItem;

			AssertDefaultsStandard("Road shipment from/to the US with no transports should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			shipment.JS_RL_NKOrigin = "PRGUY";
			AssertDefaultsStandard("Road shipment from/to a US Territory with no transports should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertDoesntDefaultStandard("Non-road shipment from/to US or US Territory with no transports should not correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertDoesntDefaultStandard("Road shipment not from/to US or US Territory with no transports should not correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			var firstTransportLeg = shipment.Transports.AddNew();
			firstTransportLeg.JW_LegOrder = 1;
			firstTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			firstTransportLeg.JW_RL_NKLoadPort = "AUSYD";
			firstTransportLeg.JW_RL_NKDiscPort = "AUPER";

			shipment.JS_RL_NKOrigin = "US3VA";
			AssertDoesntDefaultStandard("If shipment has transports, then it should not correspond to CFR, even if shipment meets criteria.", dataItem, UNDGSubstanceStandardTypes.CFR);

			var secondTransportLeg = shipment.Transports.AddNew();
			secondTransportLeg.JW_LegOrder = 2;
			secondTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Road;
			secondTransportLeg.JW_RL_NKLoadPort = "USLAX";
			secondTransportLeg.JW_RL_NKDiscPort = "NZAKL";

			var thirdTransportLeg = shipment.Transports.AddNew();
			thirdTransportLeg.JW_LegOrder = 3;
			thirdTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			thirdTransportLeg.JW_RL_NKLoadPort = "NZAKL";
			thirdTransportLeg.JW_RL_NKDiscPort = "GBLON";

			AssertDefaultsStandard("Shipment with road leg from US should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			secondTransportLeg.JW_RL_NKDiscPort = "PRGUY";
			AssertDefaultsStandard("Shipment with road leg from US to US Territory should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			secondTransportLeg.JW_RL_NKLoadPort = "AUPER";
			AssertDefaultsStandard("Shipment with road leg to US Territory should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			secondTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertDoesntDefaultStandard("Shipment with no road legs should not correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			secondTransportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			secondTransportLeg.JW_RL_NKDiscPort = "NZAKL";
			AssertDoesntDefaultStandard("Shipment with no road legs from/to US or a US Territory should not correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			consolTransportLeg.JW_RL_NKLoadPort = "USJFK";
			AssertDefaultsStandard("Shipment with related consol with road leg from US should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			consolTransportLeg.JW_RL_NKLoadPort = "PRGUY";
			AssertDefaultsStandard("Shipment with related consol with road leg from US Territory should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);

			consolTransportLeg.JW_RL_NKDiscPort = "USJFK";
			AssertDefaultsStandard("Shipment with related consol with road leg from US Territory to US should correspond to CFR.", dataItem, UNDGSubstanceStandardTypes.CFR);
		}

		public void TestJTTStandardIsDefaulted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "CNDLC";
			transport.JW_RL_NKDiscPort = "AUBNE";

			var dataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew() as ForwardingUNDGDataItem;
			AssertDefaultsStandard("Shipment with China Road Leg should default JTT", dataItem, UNDGSubstanceStandardTypes.JTT);
		}

		public void TestJTTStandardIsDefaulted_ConsolHasChinaRoadLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consol = shipment.Consols.AddNew();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "CNDLC";
			transport.JW_RL_NKDiscPort = "AUBNE";

			var dataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew() as ForwardingUNDGDataItem;
			AssertDefaultsStandard("Shipment on Consol with China Road Leg should default JTT", dataItem, UNDGSubstanceStandardTypes.JTT);
		}

		public void TestJTTStandardIsDefaulted_ShipmentHasChinaTerritoryRoadLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "HKDLC";
			transport.JW_RL_NKDiscPort = "AUBNE";

			var dataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew() as ForwardingUNDGDataItem;
			AssertDefaultsStandard("Shipment with China Territory Road Leg should default JTT", dataItem, UNDGSubstanceStandardTypes.JTT);
		}

		public void TestJTTStandardIsDefaulted_ConsolHasChinaTerritoryRoadLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consol = shipment.Consols.AddNew();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "HKHAA";
			transport.JW_RL_NKDiscPort = "AUBNE";

			var dataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew() as ForwardingUNDGDataItem;
			AssertDefaultsStandard("Shipment on Consol with China Territory Road Leg should default JTT", dataItem, UNDGSubstanceStandardTypes.JTT);
		}

		public void TestRadioactiveLabelCategoryList()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var categoryList = ((ForwardingUNDGDataItemLookups)undgDataItem.Lookups).RadioactiveLabelCategoryList;

			CombineAssertions("All codes are present in the list", () =>
			{
				Assert(categoryList.ContainsCode(RadioactiveLabelCategoryList.Codes.WhiteI));
				Assert(categoryList.ContainsCode(RadioactiveLabelCategoryList.Codes.YellowII));
				Assert(categoryList.ContainsCode(RadioactiveLabelCategoryList.Codes.YellowIII));
			});
		}

		void AssertDefaultsStandard(string message, ForwardingUNDGDataItem dataItem, ZString standard)
		{
			var lookups = new ForwardingUNDGDataItemLookups(dataItem);
			var defaultStandardKey = "Standard" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			var defaultStandard = lookups.UNDGSubstances.FilterBusinessObjectDefaults[defaultStandardKey].Value;
			AssertEquals(message, standard, defaultStandard);
		}

		void AssertDoesntDefaultStandard(string message, ForwardingUNDGDataItem dataItem, ZString standard)
		{
			var lookups = new ForwardingUNDGDataItemLookups(dataItem);
			var defaultStandardKey = "Standard" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			var defaultStandard = lookups.UNDGSubstances.FilterBusinessObjectDefaults[defaultStandardKey].Value;
			AssertNotEquals(message, standard, defaultStandard);
		}

		public void TestRadionuclideElementList()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var categoryList = ((ForwardingUNDGDataItemLookups)undgDataItem.Lookups).RadionuclideElementList;

			CombineAssertions("Codes are present in the list", () =>
			{
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Actinium));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Germanium));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Hafnium));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Krypton));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Radium));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Tantalum));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Uranium));
				Assert(categoryList.ContainsCode(RadionuclideElementConstants.Codes.Xenon));
			});
		}

		public void TestRadionuclideElementSuffixList()
		{
			var actiniumUndgDataItem = Factory.New<ForwardingUNDGDataItem>();
			actiniumUndgDataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Actinium;
			var actiniumCategoryList = ((ForwardingUNDGDataItemLookups)actiniumUndgDataItem.Lookups).RadionuclideElementSuffixList;

			var actiniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium225a, "Actinium - 225 (a)");
			var actiniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium227a, "Actinium - 227 (a)");
			var actiniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Actinium228, "Actinium - 228");

			var germaniumUndgDataItem = Factory.New<ForwardingUNDGDataItem>();
			germaniumUndgDataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Germanium;
			var germaniumCategoryList = ((ForwardingUNDGDataItemLookups)germaniumUndgDataItem.Lookups).RadionuclideElementSuffixList;

			var germaniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium68a, "Germanium - 68 (a)");
			var germaniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium71, "Germanium - 71");
			var germaniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Germanium77, "Germanium - 77");

			var kryptonUndgDataItem = Factory.New<ForwardingUNDGDataItem>();
			kryptonUndgDataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Krypton;
			var kryptonCategoryList = ((ForwardingUNDGDataItemLookups)kryptonUndgDataItem.Lookups).RadionuclideElementSuffixList;

			var kryptonCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton79, "Krypton - 79");
			var kryptonCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton81, "Krypton - 81");
			var kryptonCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton85, "Krypton - 85");
			var kryptonCodePair4 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton85m, "Krypton - 85m");
			var kryptonCodePair5 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Krypton87, "Krypton - 87");

			var uraniumUndgDataItem = Factory.New<ForwardingUNDGDataItem>();
			uraniumUndgDataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			var uraniumCategoryList = ((ForwardingUNDGDataItemLookups)uraniumUndgDataItem.Lookups).RadionuclideElementSuffixList;

			var uraniumCodePair1 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium230FastLungAbsorptionad, "Uranium - 230 (fast lung absorption) (a)(d)");
			var uraniumCodePair2 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium232SlowLungAbsorptionf, "Uranium - 232 (slow lung absorption) (f)");
			var uraniumCodePair3 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium234FastLungAbsorptiond, "Uranium - 234 (fast lung absorption) (d)");
			var uraniumCodePair4 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.Uranium236FastLungAbsorptiond, "Uranium - 236 (fast lung absorption) (d)");
			var uraniumCodePair5 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.UraniumNat, "Uranium - (nat)");
			var uraniumCodePair6 = new CodeDescriptionPair(RadionuclideElementSuffixConstants.UraniumEnriched, "Uranium - (enriched to 20% or less)(g)");

			CombineAssertions("All codes are present in the list", () =>
			{
				AssertCollectionContains(actiniumCodePair1.ToString(), actiniumCodePair1, actiniumCategoryList);
				AssertCollectionContains(actiniumCodePair2.ToString(), actiniumCodePair2, actiniumCategoryList);
				AssertCollectionContains(actiniumCodePair3.ToString(), actiniumCodePair3, actiniumCategoryList);
				AssertCollectionNotContains(germaniumCodePair1.ToString(), germaniumCodePair1, actiniumCategoryList);

				AssertCollectionContains(germaniumCodePair1.ToString(), germaniumCodePair1, germaniumCategoryList);
				AssertCollectionContains(germaniumCodePair2.ToString(), germaniumCodePair2, germaniumCategoryList);
				AssertCollectionContains(germaniumCodePair3.ToString(), germaniumCodePair3, germaniumCategoryList);
				AssertCollectionNotContains(kryptonCodePair1.ToString(), kryptonCodePair1, germaniumCategoryList);

				AssertCollectionContains(kryptonCodePair1.ToString(), kryptonCodePair1, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair2.ToString(), kryptonCodePair2, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair3.ToString(), kryptonCodePair3, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair4.ToString(), kryptonCodePair4, kryptonCategoryList);
				AssertCollectionContains(kryptonCodePair5.ToString(), kryptonCodePair5, kryptonCategoryList);
				AssertCollectionNotContains(uraniumCodePair1.ToString(), uraniumCodePair1, kryptonCategoryList);

				AssertCollectionContains(uraniumCodePair1.ToString(), uraniumCodePair1, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair2.ToString(), uraniumCodePair2, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair3.ToString(), uraniumCodePair3, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair4.ToString(), uraniumCodePair4, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair5.ToString(), uraniumCodePair5, uraniumCategoryList);
				AssertCollectionContains(uraniumCodePair6.ToString(), uraniumCodePair6, uraniumCategoryList);
				AssertCollectionNotContains(actiniumCodePair1.ToString(), actiniumCodePair1, uraniumCategoryList);
			});
		}

		public void TestRadioactiveMaximumActivityUnitList()
		{
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var categoryList = ((ForwardingUNDGDataItemLookups)undgDataItem.Lookups).RadioactiveMaximumActivityUnitList;

			CombineAssertions("All codes are present in the list", () =>
			{
				Assert(categoryList.ContainsCode(RadioactiveUnits.Terabecquerel));
				Assert(categoryList.ContainsCode(RadioactiveUnits.Gigabecquerel));
				Assert(categoryList.ContainsCode(RadioactiveUnits.Megabecquerel));
				Assert(categoryList.ContainsCode(RadioactiveUnits.Curie));
				Assert(categoryList.ContainsCode(RadioactiveUnits.Millicurie));
				Assert(categoryList.ContainsCode(RadioactiveUnits.Microcurie));
			});
		}

		public void TestLabelCategory_Lookups()
		{
			var baseUndg = Factory.New<UNDGDataItem>();
			var forwardingUndg = Factory.New<ForwardingUNDGDataItem>();

			Factory.Save();

			Assert(baseUndg.Lookups.RadioactiveLabelCategoryList.Count == 0);
			Assert(forwardingUndg.Lookups.RadioactiveLabelCategoryList.Count != 0);
		}

		public void TestRadionuclide_Lookups()
		{
			var baseUndg = Factory.New<UNDGDataItem>();
			var forwardingUndg = Factory.New<ForwardingUNDGDataItem>();
			forwardingUndg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Actinium;

			Factory.Save();

			Assert(baseUndg.Lookups.RadionuclideElementList.Count == 0);
			Assert(forwardingUndg.Lookups.RadionuclideElementList.Count != 0);

			Assert(baseUndg.Lookups.RadionuclideElementSuffixList.Count == 0);
			Assert(forwardingUndg.Lookups.RadionuclideElementSuffixList.Count != 0);

			Assert(baseUndg.Lookups.RadioactiveMaximumActivityUnitList.Count == 0);
			Assert(forwardingUndg.Lookups.RadioactiveMaximumActivityUnitList.Count != 0);
		}
	}
}
