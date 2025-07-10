using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class CTZCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<CartageZoneDistanceCalculator, Xsd.CTZCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore()
		{
			Xsd.CTZCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);
			var rateLine = RateLineForCTZCalculatorTest;

			#region Specified = false

			CalculatorGenerator.ImportFromValueObject(rateLine, calculatorXSD, context);

			AssertEquals("Calculator is a Cartage Distance Zone Calculator", typeof(CartageZoneDistanceCalculator), rateLine.Calculator.GetType());

			AssertEquals("Calculator's CartageZones Count", 2, CalculatorGenerated.CartageZones.Count);

			AssertEquals("Calculator's EquipmentType", "TT", CalculatorGenerated.EquipmentType);
			AssertEquals("First Zone's ZoneName", "", CalculatorGenerated.CartageZones[0].ZoneName);
			AssertEquals("Second Zone's ZoneName", "Zone2", CalculatorGenerated.CartageZones[1].ZoneName);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.CartageZones[0].ZoneRateLineItems.Count);
			AssertEquals("Second Zone RateLineItems Count", 0, CalculatorGenerated.CartageZones[1].ZoneRateLineItems.Count);

			#endregion

			#region Specified = true

			calculatorXSD = CalculatorXSDForTest(true);
			calculatorXSD.Zones[0].SimpleRate.PerUnitSpecified = false;

			CalculatorGenerated.CartageZones[0].ZoneRateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.CartageZones.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(rateLine, calculatorXSD, context);

			AssertEquals("Calculator's CartageZones Count", 2, CalculatorGenerated.CartageZones.Count);

			AssertEquals("Calculator's EquipmentType", "TT", CalculatorGenerated.EquipmentType);
			AssertEquals("First Zone's ZoneName", "", CalculatorGenerated.CartageZones[0].ZoneName);
			AssertEquals("Second Zone's ZoneName", "Zone2", CalculatorGenerated.CartageZones[1].ZoneName);

			AssertEquals("First Zone RateLineItems Count", 6, CalculatorGenerated.CartageZones[0].ZoneRateLineItems.Count);
			AssertEquals("Second Zone RateLineItems Count", 0, CalculatorGenerated.CartageZones[1].ZoneRateLineItems.Count);

			calculatorXSD.Zones[0].SimpleRate.PerUnitSpecified = true;
			calculatorXSD.Zones[0].RateItems = new Xsd.RateItemWithOperatorAndBreakCollection();

			CalculatorGenerated.CartageZones[0].ZoneRateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.CartageZones.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(rateLine, calculatorXSD, context);

			AssertEquals("Calculator's CartageZones Count", 2, CalculatorGenerated.CartageZones.Count);

			AssertEquals("Calculator's EquipmentType", "TT", CalculatorGenerated.EquipmentType);
			AssertEquals("First Zone's ZoneName", "", CalculatorGenerated.CartageZones[0].ZoneName);

			AssertEquals("First Zone RateLineItems Count", 4, CalculatorGenerated.CartageZones[0].ZoneRateLineItems.Count);

			#endregion
		}

		public void TestImportWithACIZones()
		{
			BusinessObjectFactory aciFactory = new BusinessObjectFactory();
			RefDomesticCartageZone aciZone = aciFactory.New<RefDomesticCartageZone>();
			aciZone.F1_Zone = "A";
			aciZone.F1_RL_NKLoco = "USLAX";
			aciFactory.Save();

			Xsd.CTZCalculator calculator = CalculatorXSDForTest(false);
			Xsd.CTZZone zone = calculator.Zones.AddNew();
			zone.Code = "A";
			zone.SimpleRate.BaseRate = 45m;

			Buffer = new NotificationBuffer();
			var rateLine = RateLineForCTZCalculatorTest;
			rateLine.RateLineItems.RemoveAndDeleteAll(); //Follow the logic in actual import procedure 
			CalculatorGenerator.ImportFromValueObject(rateLine, calculator, new ValueObjectImportContext(Factory, Buffer));
			AssertEquals("Invalid zone", true, Buffer.HasErrors);

			string originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			try
			{
				Buffer = new NotificationBuffer();
				rateLine.RateLineItems.RemoveAndDeleteAll(); //Follow the logic in actual import procedure
				CalculatorGenerator.ImportFromValueObject(rateLine, calculator, new ValueObjectImportContext(Factory, Buffer));
				AssertEquals("Valid zone", false, Buffer.HasErrors);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore()
		{
			Xsd.CTZCalculator calculatorXSD = new Xsd.CTZCalculator();
			var rateLineForCalculatorXSDTest = RateLineForCalculatorXSDTest;
			rateLineForCalculatorXSDTest.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			rateLineForCalculatorXSDTest.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			rateLineForCalculatorXSDTest.TL_WeightVolume = Core.Constants.Weight.Kilograms;
			rateLineForCalculatorXSDTest.TL_WeightVolumeMultiple = 2.34m;
			rateLineForCalculatorXSDTest.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			rateLineForCalculatorXSDTest.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;

			var calculator = rateLineForCalculatorXSDTest.GetCalculator<CartageZoneDistanceCalculator>();
			SetTestDataCalculatorForTest(calculator);

			CalculatorGenerator.ExportToValueObject(rateLineForCalculatorXSDTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator's CartageZones Count", 2, calculatorXSD.Zones.Count);

			AssertEquals("Calculator 's ZoneName", "Standard", calculatorXSD.Zones[0].Code);
			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.Zones[0].RateItems.Count);
			AssertEquals("First Zone has a Simple Rate", 1.2m, calculatorXSD.Zones[0].SimpleRate.Minimum);
			AssertEquals("First Zone has a Simple Rate", 1.3m, calculatorXSD.Zones[0].SimpleRate.BaseRate);
			AssertEquals("First Zone has a Simple Rate", 1.4m, calculatorXSD.Zones[0].SimpleRate.Maximum);
			AssertEquals("First Zone has a Simple Rate", 1.5m, calculatorXSD.Zones[0].SimpleRate.PerUnit);

			AssertEquals("Calculator 's ZoneName", "Zone2", calculatorXSD.Zones[1].Code);
			AssertEquals("Calculator RateLineItems Count", 2, calculatorXSD.Zones[1].RateItems.Count);
			AssertEquals("2nd Zone has no Simple Rate", 0m, calculatorXSD.Zones[1].SimpleRate.Minimum);
			AssertEquals("2nd Zone has no Simple Rate", 0m, calculatorXSD.Zones[1].SimpleRate.BaseRate);
			AssertEquals("2nd Zone has no Simple Rate", 0m, calculatorXSD.Zones[1].SimpleRate.Maximum);
			AssertEquals("2nd Zone has no Simple Rate", 0m, calculatorXSD.Zones[1].SimpleRate.PerUnit);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Helper.CreateRateTransportZoneSet(null, CountryCodes.UnitedStates, zoneNames: new ZString[] { "Zone2" });
			Factory.Save();
		}

		protected override ZString CalculatorType
		{
			get { return CartageZoneDistanceCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(CTZCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<CartageZoneDistanceCalculator, Xsd.CTZCalculator> CalculatorGenerator
		{
			get { return new CTZCalculatorGeneratorFromXSD(); }
		}

		protected override Xsd.CTZCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.CTZCalculator();

			calculatorXSD.Equipment = "TT";
			if (specified)
			{
				calculatorXSD.ConversionFactor = 12.3m;
			}

			Xsd.CTZZone zoneXSD = calculatorXSD.Zones.AddNew();
			zoneXSD.Code = "Standard";

			Xsd.RateItemWithOperatorAndBreak rateItemXSD = zoneXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("-");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = zoneXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = zoneXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			if (specified)
			{
				rateItemXSD.FlatAmount = 11.2m;
				rateItemXSD.BreakAmount = 6.3m;
				rateItemXSD.BreakMinimum = 5.2m;
				rateItemXSD.PerUnit = 21.3m;
			}

			rateItemXSD.Units = "HR";
			zoneXSD.RateItems.AddNew();

			if (specified)
			{
				zoneXSD.SimpleRate.Minimum = 1.2;
				zoneXSD.SimpleRate.BaseRate = 1.3;
				zoneXSD.SimpleRate.Maximum = 1.4;
				zoneXSD.SimpleRate.PerUnit = 1.5;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(CartageZoneDistanceCalculator calculator)
		{
			calculator.EquipmentType = "TT";

			CartageZone cartageZone = calculator.CartageZones[0];
			RateLineItem lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = new ZString("-");
			lineItem.TM_FlatAmount = 10.2m;
			lineItem.TM_Break = 5.3m;
			lineItem.TM_BreakMinimum = 4.2m;
			lineItem.TM_RelevantValue = 20.3m;
			lineItem.TM_BreakWeightVolume = "HR";

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = new ZString("+");
			lineItem.TM_FlatAmount = 10.2m;
			lineItem.TM_Break = 5.3m;
			lineItem.TM_BreakMinimum = 4.2m;
			lineItem.TM_RelevantValue = 20.3m;
			lineItem.TM_BreakWeightVolume = "HR";

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = new ZString("+");
			lineItem.TM_FlatAmount = 11.2m;
			lineItem.TM_Break = 6.3m;
			lineItem.TM_BreakMinimum = 5.2m;
			lineItem.TM_RelevantValue = 21.3m;
			lineItem.TM_BreakWeightVolume = "HR";

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = Calculator.Items.Operator.MIN;
			lineItem.TM_RelevantValue = 1.2m;

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = Calculator.Items.Operator.BAS;
			lineItem.TM_RelevantValue = 1.3m;

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = Calculator.Items.Operator.MAX;
			lineItem.TM_RelevantValue = 1.4m;

			lineItem = cartageZone.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = Calculator.Items.Operator.UNT;
			lineItem.TM_RelevantValue = 1.5m;

			CartageZone cartageZone2 = calculator.CartageZones[1];
			AssertEquals("Zone2", (string)cartageZone2.ZoneName);
			lineItem = cartageZone2.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = new ZString("-");
			lineItem.TM_Break = 100m;
			lineItem.TM_RelevantValue = 10m;
			lineItem.TM_BreakWeightVolume = "HR";

			lineItem = cartageZone2.ZoneRateLineItems.AddNew();
			lineItem.TM_Type = new ZString("+");
			lineItem.TM_Break = 100m;
			lineItem.TM_RelevantValue = 30m;
			lineItem.TM_BreakWeightVolume = "HR";
		}

		RateLine RateLineForCTZCalculatorTest
		{
			get
			{
				var rateLine = RateLineForCalculatorXSDTest;
				rateLine.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
				rateLine.TL_AC = Helper.ChargeCodes["DCART"].PK;
				rateLine.TL_RateCalculator = CalculatorType;

				return rateLine;
			}
		}

		#endregion
	}
}
