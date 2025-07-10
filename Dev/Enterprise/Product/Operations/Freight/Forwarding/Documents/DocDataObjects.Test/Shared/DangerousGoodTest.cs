using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DangerousGood))]
	sealed class DangerousGoodTest : NonPersistentBusinessObjectTestCase
	{
		#region Build

		public void TestBuild()
		{
			var substance_UN0004A = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			AssertEquals("prerequisite: 0004A is an IMO substance", UNDGSubstanceStandardTypes.IMO, substance_UN0004A.DG_Standard);

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance_UN0004A.PK;
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;

			void Assert()
			{
				var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
				AssertEquals(nameof(dangerousGood.Identifier), (ZGuid)dangerousGood.Identifier, undg.PK);
				AssertEquals(nameof(dangerousGood.Standard), dangerousGood.Standard, UNDGSubstanceStandardTypes.IMO);
				AssertEquals(nameof(dangerousGood.TransportMode), dangerousGood.TransportMode.Code, TransportModes.Sea);
				AssertEquals(nameof(dangerousGood.Quantity), dangerousGood.Quantity, 10);
				AssertEquals(nameof(dangerousGood.FlashPoint), dangerousGood.FlashPoint.Value, 1m);
				AssertEquals(nameof(dangerousGood.FlashPoint), dangerousGood.FlashPoint.Unit.Code, "C");
				AssertEquals(nameof(dangerousGood.ProperShippingName), dangerousGood.ProperShippingName, "AMMONIUM PICRATE");
				AssertEquals(nameof(dangerousGood.Contact), dangerousGood.Contact.FullName, "Deacon St John");
				AssertEquals(nameof(dangerousGood.Contact), dangerousGood.Contact.Phone, "1234567");
				AssertEquals(nameof(dangerousGood.PackedInLimitedQuantity), dangerousGood.PackedInLimitedQuantity, true);
			}

			CombineAssertions(Assert);
		}

		public void TestIdentifier()
		{
			var substance_UN0004A = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			AssertEquals("prerequisite: 0004A is an IMO substance", UNDGSubstanceStandardTypes.IMO, substance_UN0004A.DG_Standard);

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance_UN0004A.PK;
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;

			var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			AssertEquals(undg.PK, (ZGuid)dangerousGood.Identifier);

			var emptyDangerousGood = new DangerousGood();
			AssertEquals(emptyDangerousGood.PK, (ZGuid)emptyDangerousGood.Identifier);
		}

		#endregion

		#region IMO Standard

		public void TestIMOToString_UN0004A()
		{
			var substance_UN0004A = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			AssertEquals("prerequisite: 0004A is an IMO substance", UNDGSubstanceStandardTypes.IMO, substance_UN0004A.DG_Standard);

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance_UN0004A.PK;
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;

			var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);

			AssertEquals(@"UN0004, AMMONIUM PICRATE, class 1.1D, (1C c.c.), LTD QTY, contact Deacon St John 1234567", dangerousGood.ToString());
			Assert(dangerousGood.PackedInLimitedQuantity);

			undg.DI_IsLimitedQuantity = false;

			dangerousGood = new DangerousGoodBuilder().Build(undg, Context);

			AssertEquals(@"UN0004, AMMONIUM PICRATE, class 1.1D, (1C c.c.), contact Deacon St John 1234567", dangerousGood.ToString());
			Assert(!dangerousGood.PackedInLimitedQuantity);
		}

		public void TestIMOToString_UN3092()
		{
			var instance3092 = Factory.New<UNDGSubstance>();
			instance3092.DG_UNNO = "3092";
			instance3092.DG_Code = "3092";
			instance3092.DG_Standard = UNDGSubstanceStandardTypes.IMO;
			instance3092.DG_PSN = "1-Methoxy-2-propanol";
			instance3092.DG_Class = "3";
			instance3092.DG_IsNotOtherwiseSpecified = false;

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_F3_NKPackType = PkgUnit.Package;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DG = instance3092.PK;

			var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			AssertEquals("10 Package of UN3092, 1-Methoxy-2-propanol, class 3, (1C c.c.), contact Deacon St John 1234567", dangerousGood.ToString());

			undg.DI_F3_NKPackType = ZString.Empty;

			dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			AssertEquals("UN3092, 1-Methoxy-2-propanol, class 3, (1C c.c.), contact Deacon St John 1234567", dangerousGood.ToString());
		}

		#endregion

		#region IATA Standard

		public void TestIATAToString_UN3180()
		{
			var instance3180 = Factory.New<UNDGSubstance>();
			instance3180.DG_UNNO = "3180";
			instance3180.DG_Code = "3180b";
			instance3180.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			instance3180.DG_PSN = "Flammable solid, corrosive, inorganic";
			instance3180.DG_Class = "4.1";
			instance3180.DG_IsNotOtherwiseSpecified = true;

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_F3_NKPackType = PkgUnit.Package;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;

			undg.DI_DG = instance3180.PK;
			var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			AssertEquals("10 Package of UN3180, Flammable solid, corrosive, inorganic, class 4.1, (1C c.c.), contact Deacon St John 1234567", dangerousGood.ToString());

			undg.DI_F3_NKPackType = ZString.Empty;

			undg.DI_DG = instance3180.PK;
			dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			AssertEquals("UN3180, Flammable solid, corrosive, inorganic, class 4.1, (1C c.c.), contact Deacon St John 1234567", dangerousGood.ToString());
		}

		#endregion

		#region CFR Standard

		#region TestCFRToString

		public void TestCFRToString_CFRPSNComponent_SolidSubstance_Hot()
		{
			var undg = CreateCFRUN1353DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 300;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN1353, HOT - FABRICS IMPREGNATED WITH WEAKLY NITRATED NITROCELLULOSE, N.O.S. (technical name), class 4.1");
		}

		public void TestCFRToString_CFRPSNComponent_SolidSubstance_NotHot()
		{
			var undg = CreateCFRUN1353DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 160;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN1353, FABRICS IMPREGNATED WITH WEAKLY NITRATED NITROCELLULOSE, N.O.S. (technical name), class 4.1");
		}

		public void TestCFRToString_CFRPSNComponent_LiquidSubstance_Hot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 105;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN2031, HOT - NITRIC ACID (technical name), class 8, PG ||");
		}

		public void TestCFRToString_CFRPSNComponent_LiquidSubstance_NotHot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||");
		}

		public void TestCFRToString_CFRPSNComponent_LiquidSubstance_Flashpoint_Hot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			packline.UNDGs.Add(undg);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 39;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN2031, HOT - NITRIC ACID (technical name), class 8, PG ||, (39C c.c.)");
		}

		public void TestCFRToString_CFRPSNComponent_LiquidSubstance_Flashpoint_NotHot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			packline.UNDGs.Add(undg);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 39;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 37;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, (39C c.c.)");
		}

		public void TestCFRToString_CFRSpecialPermitNumber()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_SpecialPermitNumber = "1234";

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), DOT-SP 1234, class 8, PG ||");
		}

		public void TestCFRToString_CFRWasteCode()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_HazardousWasteCode = "1234";
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), 1234, class 8, PG ||");
		}

		public void TestCFRToString_CFRMaterialFormDescriptionD()
		{
			var undg = CreateCFRUN1993DataItem(false);
			undg.DI_MaterialFormDescription = "ABCD";
			AssertCFRSummary(undg, "UN1993, COMBUSTIBLE LIQUID, N.O.S. (technical name), ABCD");
		}

		public void TestCFRToString_CFRClassForUNNO1993D()
		{
			var undg = CreateCFRUN1993DataItem(false);

			AssertCFRSummary(undg, "UN1993, COMBUSTIBLE LIQUID, N.O.S. (technical name)");
		}

		public void TestCFRToString_CFRClassForNonUNNO1993D()
		{
			var undg = CreateCFRUN1993DataItem(false);

			AssertCFRSummary(undg, "UN1993, COMBUSTIBLE LIQUID, N.O.S. (technical name)");
		}

		public void TestCFRToString_CFRRadionuclideComponent_Curie()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 8m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, Ag-108m (a), 0.30 GBq (8 mCi), PG ||");
		}

		public void TestCFRToString_CFRRadionuclideComponent_Becquerel()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 300m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, Ag-108m (a), 0.3 GBq (8.11 mCi), PG ||");
		}

		public void TestCFRToString_CFRRadionuclideComponent_NoHyphen()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;
			undg.DI_RadioactiveMaximumActivity = 0.3m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Gigabecquerel;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 0.3 GBq (8.11 mCi), PG ||");
		}

		public void TestCFRToString_CFRRadionuclideComponent_Conversion()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;

			undg.DI_RadioactiveMaximumActivity = 30m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 30 MBq (810.81 uCi), PG ||");

			undg.DI_RadioactiveMaximumActivity = 300m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 0.3 GBq (8.11 mCi), PG ||");

			undg.DI_RadioactiveMaximumActivity = 300000m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 0.3 TBq (8.11 Ci), PG ||");

			undg.DI_RadioactiveMaximumActivity = 0.01m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 0.37 MBq (10 uCi), PG ||");

			undg.DI_RadioactiveMaximumActivity = 10m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 0.37 GBq (10 mCi), PG ||");

			undg.DI_RadioactiveMaximumActivity = 100m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, U (nat), 3.7 GBq (0.1 Ci), PG ||");
		}

		public void TestCFRToString_CFRRadioactiveLabelCategory()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.WhiteI;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, RADIOACTIVE WHITE-I LABEL, PG ||");
		}

		public void TestCFRToString_CFRRadioactiveTransportIndex()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadioactiveTransportIndex = 15.15;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, TI = 15.15, PG ||");
		}

		public void TestCFRToString_PackingGroupDescription()
		{
			var undg = CreateCFRUN2031DataItem(false);
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||");
		}

		public void TestCFRToString_UN2031_ReportableQuantityExceeded()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_DGWeight = 1000;

			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, RQ");
		}

		public void TestCFRToString_UN1955_PoisonInhalationHazard()
		{
			var undg = CreateCFRUN1955DataItem(false);
			AssertCFRSummary(undg, "UN1955, COMPRESSED GAS, TOXIC, N.O.S. (technical name), class 2.3, Poison-Inhalation Hazard Zone B");
		}

		public void TestCFRToString_FlashPointDescription()
		{
			var undg = CreateCFRUN1955DataItem(false);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 123;
			AssertCFRSummary(undg, "UN1955, COMPRESSED GAS, TOXIC, N.O.S. (technical name), class 2.3, Poison-Inhalation Hazard Zone B, (123C c.c.)");
		}

		public void TestCFRToString_UN2031_MarinePollutantWarning()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_MPMarinePollutant = "Y";
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, MARINE POLLUTANT");
		}

		public void TestCFRToString_UN2031_PSAGroupLabel()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";

			var undg = CreateCFRUN2031DataItem(false);

			var reference = Factory.New<MasterFiles.Business.UNDGCountryReference>();
			reference.DCR_HasFlashPointLower = false;
			reference.DCR_HasFlashPointUpper = true;
			reference.DCR_FlashPointUpperCentigrade = 50m;
			reference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference.DCR_Type = "PSA";
			reference.DCR_Code = "1S";
			undg.Substance.UNDGCountryReferences.Add(reference);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.Add(undg);

			Assert("prereq: PSA is applicable", undg.IsPSAGroupApplicable);
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, PSA Group: 1S");
		}

		public void TestCFRToString_UN2031_ExclusiveUse()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_IsExclusiveUse = true;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, Exclusive Use");
		}

		public void TestCFRToString_UN2031_LimitedQuantity()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_IsLimitedQuantity = true;
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||, LTD QTY");
		}

		public void TestCFRToString_ID8000_LimitedQuantity()
		{
			var undg = CreateCFRID8000DataItem(false);
			undg.DI_IsLimitedQuantity = true;
			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Limited Quantity");
		}

		public void TestCFRToString_ID8000_ResidueLastContained()
		{
			var undg = CreateCFRID8000DataItem(false);
			undg.DI_IsResidueLastContained = true;

			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Limited Quantity, RESIDUE: Last Contained * * *");
		}

		public void TestCFRToString_ID8000_HighwayRouteControlledQuantity()
		{
			var undg = CreateCFRID8000DataItem(false);
			undg.DI_IsHighwayRouteControlledQuantity = true;

			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-III LABEL, TI = 0.5, Limited Quantity, HRCQ");
		}

		public void TestCFRToString_UN3332_FissileExcepted()
		{
			var undg = CreateCFRUN3332DataItem(false);
			undg.DI_IsFissileExcepted = true;

			AssertCFRSummary(undg, "UN3332, RADIOACTIVE MATERIAL, TYPE A PACKAGE, SPECIAL FORM (technical name), class 7, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Fissile Excepted");
		}

		public void TestCFRToString_UN2031_NoContact()
		{
			var undg = CreateCFRUN2031DataItem(false);
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||");
		}

		public void TestCFRToString_UN2031_WithContact()
		{
			var undg = CreateCFRUN2031DataItem(true);
			AssertCFRSummary(undg, "UN2031, NITRIC ACID (technical name), class 8, PG ||", true);
		}

		public void TestCFRToString_UN3332_NoContact()
		{
			var undg = CreateCFRUN3332DataItem(false);
			AssertCFRSummary(undg, "UN3332, RADIOACTIVE MATERIAL, TYPE A PACKAGE, SPECIAL FORM (technical name), class 7, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5");
		}

		public void TestCFRToString_UN3332_WithContact()
		{
			var undg = CreateCFRUN3332DataItem(true);
			AssertCFRSummary(undg, "UN3332, RADIOACTIVE MATERIAL, TYPE A PACKAGE, SPECIAL FORM (technical name), class 7, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5", true);
		}

		public void TestCFRToString_ID8000_AIR_NoContact()
		{
			var undg = CreateCFRID8000DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Limited Quantity");
		}

		public void TestCFRToString_ID8000_AIR_WithContact()
		{
			var undg = CreateCFRID8000DataItem(true);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Limited Quantity", includeContact: true);
		}

		public void TestCFRToString_ID8000_SEA_NoContact()
		{
			var undg = CreateCFRID8000DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);
			AssertCFRSummary(undg, "ID8000, CONSUMER COMMODITY (technical name), class 9, Ag-108m (a), 0.30 GBq (8 mCi), RADIOACTIVE YELLOW-II LABEL, TI = 0.5, Limited Quantity");
		}

		void AssertCFRSummary(UNDGDataItem undg, string expectedSummary, bool includeContact = false)
		{
			var builder = new DangerousGoodBuilder();
			var dangerousGood = builder.Build(undg, Context);

			var assertMessage = undg.DI_OC_DGContact.IsValid
				? $"CFR substance {undg.UNDGSubstance?.DG_UNNO} (with contact) summary"
				: $"CFR substance {undg.UNDGSubstance?.DG_UNNO} (no contact) summary";

			expectedSummary = includeContact ? expectedSummary + ", contact Deacon St John (02) 8001 2200" : expectedSummary;
			AssertEquals(assertMessage, expectedSummary, dangerousGood.ToString());
		}

		#endregion

		#region MatchesUNDGSubstanceWrapperSummary

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_SolidSubstance_Hot()
		{
			var undg = CreateCFRUN1353DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 300;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_SolidSubstance_NotHot()
		{
			var undg = CreateCFRUN1353DataItem(false);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 160;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_LiquidSubstance_Hot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 105;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_LiquidSubstance_NotHot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(undg);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_LiquidSubstance_Flashpoint_Hot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			packline.UNDGs.Add(undg);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 39;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRPSNComponent_LiquidSubstance_Flashpoint_NotHot()
		{
			var undg = CreateCFRUN2031DataItem(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			packline.UNDGs.Add(undg);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 39;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 37;
			packline.JL_RequiredTemperatureUnit = "C";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRSpecialPermitNumber()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_SpecialPermitNumber = "1234";

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRWasteCode()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_HazardousWasteCode = "1234";
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRMaterialFormDescriptionForUNNO1993D()
		{
			var undg = CreateCFRUN1993DataItem(false);
			undg.DI_MaterialFormDescription = "ABCD";
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRMaterialFormDescriptionForNonUNNO1993D()
		{
			var undg = CreateCFRUN2031DataItem(false);

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRClassForUNNO1993D()
		{
			var undg = CreateCFRUN1993DataItem(false);

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRClassForNonUNNO1993D()
		{
			var undg = CreateCFRUN2031DataItem(false);

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadionuclideComponent_Curie()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 8m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadionuclideComponent_Becquerel()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 300m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadionuclideComponent_NoHyphen()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;
			undg.DI_RadioactiveMaximumActivity = 0.3m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Gigabecquerel;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadionuclideComponent_Conversion()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;

			undg.DI_RadioactiveMaximumActivity = 30m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);

			undg.DI_RadioactiveMaximumActivity = 300m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);

			undg.DI_RadioactiveMaximumActivity = 300000m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);

			undg.DI_RadioactiveMaximumActivity = 0.01m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);

			undg.DI_RadioactiveMaximumActivity = 10m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);

			undg.DI_RadioactiveMaximumActivity = 100m;
			undg.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadioactiveLabelCategory()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadioactiveLabelCategory = RadionuclideElementConstants.Codes.Uranium;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_CFRRadioactiveTransportIndex()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_RadioactiveTransportIndex = 15.15;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_PackingGroupDescription()
		{
			var undg = CreateCFRUN2031DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_FlashPointDescription()
		{
			var undg = CreateCFRUN1955DataItem(false);
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 123;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN2031_ReportableQuantityExceeded()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_DGWeight = 1000;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN1955_PoisonInhalationHazard()
		{
			var undg = CreateCFRUN1955DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN2031_NoContact()
		{
			var undg = CreateCFRUN2031DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN2448()
		{
			var undg = CreateCFRUN2448DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN3164_LimitedQuantity()
		{
			var undg = CreateCFRUN3164DataItem(false);
			undg.DI_IsLimitedQuantity = true;
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN3332_Radioactive()
		{
			var undg = CreateCFRUN3332DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN2031_MarinePollutantWarning()
		{
			var undg = CreateCFRUN2031DataItem(false);
			undg.DI_MPMarinePollutant = "Y";
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN2031_PSAGroupLabel()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";

			var undg = CreateCFRUN2031DataItem(false);

			var reference = Factory.New<MasterFiles.Business.UNDGCountryReference>();
			reference.DCR_HasFlashPointLower = false;
			reference.DCR_HasFlashPointUpper = true;
			reference.DCR_FlashPointUpperCentigrade = 50m;
			reference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference.DCR_Type = "PSA";
			reference.DCR_Code = "1S";
			undg.Substance.UNDGCountryReferences.Add(reference);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.Add(undg);

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_ID8000_SalvagePackaging()
		{
			var undg = CreateCFRID8000DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_ID8000_ResidueLastContained()
		{
			var undg = CreateCFRID8000DataItem(false);
			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_ID8000_HighwayRouteControlledQuantity()
		{
			var undg = CreateCFRID8000DataItem(false);
			undg.DI_IsHighwayRouteControlledQuantity = true;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		public void TestCFRToString_MatchesUNDGSubstanceWrapperSummary_UN3332_FissileExcepted()
		{
			var undg = CreateCFRUN3332DataItem(false);
			undg.DI_IsFissileExcepted = true;

			AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(undg);
		}

		void AssertCFRToString_MatchesUNDGSubstanceWrapperSummary(ForwardingUNDGDataItem undg)
		{
			var builder = new DangerousGoodBuilder();
			var dangerousGood = builder.Build(undg, Context);

			var wrapperSummaryProvider = ObjectFactory.Get<IUNDGSubstanceWrapperSummaryProviderForTest>();
			var undgWrapperSummary = wrapperSummaryProvider.GetSummary(undg, Factory);

			AssertEquals(
				$"CFR substance <strong>{undg.UNDGSubstance?.DG_UNNO}</strong> FormBuilder and DocBuilder summaries are the same",
				dangerousGood.ToString(),
				undgWrapperSummary,
				true);
		}

		#endregion

		public void TestCFRToString_ShouldHaveRadioactiveMaterialDescriptionAsLimitedQuantityDescription_WhenOneOtherClassIsSeven()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance.PK;
			undg.DI_IsLimitedQuantity = true;

			var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
			dangerousGood.Standard = UNDGSubstanceStandardTypes.CFR;
			dangerousGood.IMOClass = "6.1";
			dangerousGood.SecondaryClass = "7";
			dangerousGood.TertiaryClass = "8";

			var result = dangerousGood.ToString();

			AssertEquals("Radioactive material limited quantity", "UN, class 6.1, Limited quantity radioactive material", result);
		}

		#region UN1353

		ForwardingUNDGDataItem CreateCFRUN1353DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN1353();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN1353()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1353";
			substance.CFR_Variant = "b";
			substance.CFR_PSN = "FABRICS IMPREGNATED WITH WEAKLY NITRATED NITROCELLULOSE, N.O.S.";
			substance.CFR_PrimaryClass = "4.1";
			substance.CFR_ExceptedQuantity = "E1";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 0;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "003";
			substance.CFR_PassengerStowage = "001";
			substance.CFR_StowageCategory = "D";
			substance.CFR_BulkPackingInstructions = "240";
			substance.CFR_PackingExceptions = "None";
			substance.CFR_PackingInstructions = "213";
			substance.CFR_SpecialProvisions = "A1";
			substance.CFR_State = "S";
			substance.CFR_EmergencyResponseGuide = "133";
			substance.CFR_TechnicalName = "+";
			substance.CFR_PAXAirRailLimit = 25m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 100m;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN1955

		ForwardingUNDGDataItem CreateCFRUN1955DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN1955();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN1955()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1955";
			substance.CFR_Variant = "b";
			substance.CFR_PSN = "COMPRESSED GAS, TOXIC, N.O.S.";
			substance.CFR_Variation = "Inhalation Hazard Zone B.";
			substance.CFR_PrimaryClass = "2.3";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 1000;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "003";
			substance.CFR_PassengerStowage = "001";
			substance.CFR_StowageCategory = "D";
			substance.CFR_StowageCodes = "040";
			substance.CFR_StowageIMDGCodes = "032";
			substance.CFR_BulkPackingInstructions = "314 315";
			substance.CFR_BulkPackingProvisions = "B9 B14";
			substance.CFR_PackingExceptions = "None";
			substance.CFR_PackingInstructions = "302 305";
			substance.CFR_SpecialProvisions = "2";
			substance.CFR_PoisonInhalationHazard = "B";
			substance.CFR_State = "G";
			substance.CFR_EmergencyResponseGuide = "123";
			substance.CFR_TechnicalName = "*";
			substance.CFR_PAXAirRailLimit = 0m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 0m;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";
			substance.CFR_PAXAirRailLimitType = "FOB";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN1993

		ForwardingUNDGDataItem CreateCFRUN1993DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN1993();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN1993()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1993";
			substance.CFR_Variant = "d";
			substance.CFR_PSN = "COMBUSTIBLE LIQUID, N.O.S.";
			substance.CFR_PrimaryClass = "Comb";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 0;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "004";
			substance.CFR_PassengerStowage = "004";
			substance.CFR_StowageCategory = "A";
			substance.CFR_BulkPackingInstructions = "241";
			substance.CFR_PackingExceptions = "150";
			substance.CFR_PackingInstructions = "203";
			substance.CFR_SpecialProvisions = "148";
			substance.CFR_State = "L";
			substance.CFR_EmergencyResponseGuide = "128";
			substance.CFR_TechnicalName = "*";
			substance.CFR_PAXAirRailLimit = 60m;
			substance.CFR_PAXAirRailLimitUnit = "L";
			substance.CFR_CargoAirRailLimit = 220m;
			substance.CFR_CargoAirRailLimitUnit = "L";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN2031

		ForwardingUNDGDataItem CreateCFRUN2031DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN2031();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN2031()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "2031";
			substance.CFR_Variant = "b";
			substance.CFR_PSN = "NITRIC ACID";
			substance.CFR_Variation = "At least 65% but not more than 70% nitric acid.";
			substance.CFR_PrimaryClass = "8";
			substance.CFR_SecondaryClass = "5.1";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 1000;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "003";
			substance.CFR_PassengerStowage = "001";
			substance.CFR_StowageCategory = "D";
			substance.CFR_StowageCodes = "053 058 066 074 089 090";
			substance.CFR_StowageIMDGCodes = "237 264 265";
			substance.CFR_BulkPackingInstructions = "242";
			substance.CFR_BulkPackingProvisions = "B2 B47 B53";
			substance.CFR_IBCInstructions = "IB2";
			substance.CFR_IBCProvisions = "IP15";
			substance.CFR_PackingExceptions = "None";
			substance.CFR_PackingInstructions = "158";
			substance.CFR_PackingGroup = "||";
			substance.CFR_TankInstructions = "T8";
			substance.CFR_TankProvisions = "TP2";
			substance.CFR_State = "L";
			substance.CFR_EmergencyResponseGuide = "157";
			substance.CFR_TreatAs = "5.1";
			substance.CFR_PAXAirRailLimit = 0m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 30;
			substance.CFR_CargoAirRailLimitUnit = "L";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";
			substance.CFR_PAXAirRailLimitType = "FOB";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN2448

		ForwardingUNDGDataItem CreateCFRUN2448DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN2448();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN2448()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "2448";
			substance.CFR_Variant = "b";
			substance.CFR_Prefix = "na";
			substance.CFR_CVL = "3002";
			substance.CFR_PSN = "SULFUR, MOLTEN";
			substance.CFR_Variation = "Packing Group III.";
			substance.CFR_PrimaryClass = "9";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 0m;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "003";
			substance.CFR_PassengerStowage = "003";
			substance.CFR_StowageCategory = "C";
			substance.CFR_StowageCodes = "061";
			substance.CFR_BulkPackingInstructions = "247";
			substance.CFR_BulkPackingProvisions = "B13";
			substance.CFR_IBCInstructions = "IB3";
			substance.CFR_IBCProvisions = "IP15";
			substance.CFR_PackingExceptions = "None";
			substance.CFR_PackingInstructions = "213";
			substance.CFR_PackingGroup = "III";
			substance.CFR_SpecialProvisions = "30 R1";
			substance.CFR_TankInstructions = "T1";
			substance.CFR_TankProvisions = "TP3";
			substance.CFR_AppliesForDomesticTransport = true;
			substance.CFR_EmergencyResponseGuide = "133";
			substance.CFR_PAXAirRailLimit = 0m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 0m;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";
			substance.CFR_PAXAirRailLimitType = "FOB";
			substance.CFR_CargoAirRailLimitType = "FOB";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN3164

		ForwardingUNDGDataItem CreateCFRUN3164DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN3164();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN3164()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "3164";
			substance.CFR_Variant = "a";
			substance.CFR_PSN = "ARTICLES, PRESSURIZED PNEUMATIC";
			substance.CFR_PrimaryClass = "2.2";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = true;
			substance.CFR_ReportableQuantity = 0m;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "004";
			substance.CFR_PassengerStowage = "004";
			substance.CFR_StowageCategory = "A";
			substance.CFR_BulkPackingInstructions = "None";
			substance.CFR_PackingExceptions = "306";
			substance.CFR_PackingInstructions = "302 304";
			substance.CFR_SpecialProvisions = "371";
			substance.CFR_State = "G";
			substance.CFR_EmergencyResponseGuide = "126";
			substance.CFR_TechnicalName = "+";
			substance.CFR_PAXAirRailLimit = 0m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 0m;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 1m;
			substance.CFR_LQMaxAmtUQ = "L";
			substance.CFR_PAXAirRailLimitType = "NLM";
			substance.CFR_CargoAirRailLimitType = "NLM";

			Factory.Save();

			return substance;
		}

		#endregion

		#region UN3332

		ForwardingUNDGDataItem CreateCFRUN3332DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceUN3332();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 8m;
			undg.DI_RadioactiveMaximumActivityUnit = RadioactiveUnits.Millicurie;
			undg.DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.YellowII;
			undg.DI_RadioactiveTransportIndex = 0.5m;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceUN3332()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "3332";
			substance.CFR_Variant = "b";
			substance.CFR_CVL = "0001";
			substance.CFR_PSN = "RADIOACTIVE MATERIAL, TYPE A PACKAGE, SPECIAL FORM";
			substance.CFR_PrimaryClass = "7";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_LimitedQuantityPermitted = false;
			substance.CFR_ReportableQuantity = 0;
			substance.CFR_ReportableQuantityUnit = "lb";
			substance.CFR_GeneralStowage = "004";
			substance.CFR_PassengerStowage = "004";
			substance.CFR_StowageCategory = "A";
			substance.CFR_StowageCodes = "095";
			substance.CFR_StowageIMDGCodes = "070";
			substance.CFR_BulkPackingInstructions = "415 476";
			substance.CFR_PackingInstructions = "415 476";
			substance.CFR_SpecialProvisions = "A56 W7 W8";
			substance.CFR_EmergencyResponseGuide = "164";
			substance.CFR_PAXAirRailLimit = 0m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 0;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";

			Factory.Save();

			return substance;
		}

		#endregion

		#region ID8000

		ForwardingUNDGDataItem CreateCFRID8000DataItem(bool includeContact)
		{
			var substance = CreateCFRSubstanceID8000();

			var undg = Factory.New<ForwardingUNDGDataItem>();

			if (includeContact)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "Deacon St John";
				contact.OC_Phone = "(02) 8001 2200";

				undg.DI_OC_DGContact = contact.PK;
			}

			undg.DI_DG = substance.PK;

			undg.DI_TechnicalName = "technical name";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			undg.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			undg.DI_RadioactiveMaximumActivity = 8m;
			undg.DI_RadioactiveMaximumActivityUnit = RadioactiveUnits.Millicurie;
			undg.DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.YellowII;
			undg.DI_RadioactiveTransportIndex = 0.5m;
			undg.DI_DGWeight = 100;
			undg.DI_UnitOfWeight = Weight.Pounds;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Volume.CubicFeet;

			return undg;
		}

		UNDGSubstanceCFR CreateCFRSubstanceID8000()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "8000";
			substance.CFR_Prefix = "id";
			substance.CFR_PSN = "CONSUMER COMMODITY";
			substance.CFR_PrimaryClass = "9";
			substance.CFR_ExceptedQuantity = "E0";
			substance.CFR_BulkPackingInstructions = "None";
			substance.CFR_PackingInstructions = "167";
			substance.CFR_PackingExceptions = "167";
			substance.CFR_State = "S";
			substance.CFR_EmergencyResponseGuide = "171";
			substance.CFR_PAXAirRailLimit = 30m;
			substance.CFR_PAXAirRailLimitUnit = "kg";
			substance.CFR_CargoAirRailLimit = 30;
			substance.CFR_CargoAirRailLimitUnit = "kg";
			substance.CFR_IsActive = true;
			substance.CFR_LQMaxAmt = 0m;
			substance.CFR_LQMaxAmtUQ = "kg";

			Factory.Save();

			return substance;
		}

		#endregion

		#endregion

		#region UnnoPrefix

		public void TestUnnoPrefixInToString()
		{
			var context = new CommonContext(Factory);

			var dangerousGood = new DangerousGood()
			{
				Unno = "8000",
				Code = "8000",
				TransportMode = new CodeDescription(context.TransportModes)
				{
					Code = TransportModes.Air
				}
			};

			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is AIR", "ID8000", dangerousGood.ToString());

			dangerousGood.TransportMode.Code = TransportModes.Sea;
			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is SEA", "ID8000", dangerousGood.ToString());
		}

		public void TestUnnoPrefixInToStringGeneratedByBuilder()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "8000";
			undgSubstance.DG_Mode = TransportModes.Air;

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			var builder = new DangerousGoodBuilder();

			var dangerousGood = builder.Build(undgDataItem, Context);
			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is AIR", "ID8000", dangerousGood.ToString());

			undgSubstance.DG_Mode = TransportModes.Sea;
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			dangerousGood = builder.Build(undgDataItem, Context);
			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is SEA", "ID8000", dangerousGood.ToString());

			undgSubstance.DG_Mode = TransportModes.Air;
			undgSubstance.DG_UNNO = "6969";
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			dangerousGood = builder.Build(undgDataItem, Context);
			AssertEquals("DangerousGood should start with UN when UNNO is not 8000", "UN6969", dangerousGood.ToString());
		}

		#endregion

		#region Implementation

		IContext Context => context ?? (context = new CommonContext(Factory.GetCachedReadOnlyFactory()));
		IContext context;

		#endregion
	}
}
