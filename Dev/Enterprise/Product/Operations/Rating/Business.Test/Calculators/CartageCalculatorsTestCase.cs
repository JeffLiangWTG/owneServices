using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class CartageCalculatorsTestCase : BaseCombinedCalculatorTest<CartageCalculator>
	{
		protected override int NumberOfRateLineItemsAfterInitialization { get { return 5; } }

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));

			base.TestCheckOrCreateItems();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));

			AssertEquals(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_TextInfo, TestCalculator.String1Info);
			AssertEquals(Line.TL_ConversionFactorStringInfo, TestCalculator.String2Info);
		}

		public override void TestMapping()
		{
			base.TestMapping();

			TestMapping(CartageCalculator.Items.EquipmentType, "String1");
		}

		public void TestString2Mapping()
		{
			Line.TL_WeightVolume = "KG";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			AssertEquals("String2", "250 KG/M3", TestCalculator.String2);

			TestCalculator.String2 = "330 CC/LB";
			AssertEquals("Factor", 330m, Line.ConversionFactor.Factor);
			AssertEquals("Numerator", "CC", Line.ConversionFactor.NumeratorUnit);
			AssertEquals("DenominatorUnit", "LB", Line.ConversionFactor.DenominatorUnit);
		}

		public override void TestList1()
		{
			AssertEquals("List1", typeof(CodeDescriptionPairList), TestCalculator.List1.GetType());
		}

		public override void TestList2()
		{
			AssertEquals("List2", typeof(ConversionFactorList), TestCalculator.List2.GetType());
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public void TestEquipmentType()
		{
			Assert(TestCalculator.ShowEquipmentType);
			Assert(!TestCalculator.String1Info.ReadOnly);

			var whsLine = Helper.NewClientRate(NewClient).AddRateEntry("WHS").AddRateLine("ODOC");
			whsLine.TL_RateCalculator = CalculatorCode;
			Assert(!whsLine.Calculator.ShowEquipmentType);
			Assert(whsLine.Calculator.String1Info.ReadOnly);
		}

		public void TestValidateEquipmentType()
		{
			var cartage = Factory.New<AccChargeCode>();
			cartage.AC_RateCalculator = CartageCalculator.Code;

			var testRate = Factory.New<ClientRate>();
			var oRGEntry = testRate.AddRateEntry("ORG", "FCL", "AUSYD", "");

			var cartageLine1 = oRGEntry.RateLines.AddNew();
			cartageLine1.TL_AC = cartage.PK;
			var calculator1 = (CartageCalculator)cartageLine1.Calculator;
			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;

			var cartageLine2 = oRGEntry.RateLines.AddNew();
			cartageLine2.TL_AC = cartage.PK;
			var calculator2 = (CartageCalculator)cartageLine2.Calculator;
			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;

			AssertHasError(calculator2.String1Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;
			AssertNoErrors(calculator2.String1Info);

			calculator1.EquipmentType = "";
			AssertHasError(calculator1.String1Info, "Please enter a value.");

			calculator2.EquipmentType = "";
			AssertHasError(calculator2.String1Info, "Please enter a value.");

			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertNoErrors(calculator1.String1Info);

			calculator2.EquipmentType = "SDL";
			AssertNoErrors(calculator2.String1Info);

			calculator1.EquipmentType = "ZZZ";
			AssertHasError(calculator1.String1Info, "Enter a valid selection.");

			oRGEntry.TI_Mode = Core.Constants.RateMode.AIR;

			calculator1.EquipmentType = "";
			AssertHasError(calculator1.String1Info, "Please enter a value.");
		}

		public void TestValidateEquipmentType_AllowsDuplicateForDifferentPackTypeOrCondition()
		{
			var cartageCharge = Factory.New<AccChargeCode>();
			cartageCharge.AC_RateCalculator = CartageCalculator.Code;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "");

			var line1 = rateEntry.RateLines.AddNew();
			line1.TL_AC = cartageCharge.PK;
			line1.TL_WeightVolume = "KG";
			line1.TL_Condition = RateLineConditions.OwnCFS;
			var calculator1 = (CartageCalculator)line1.Calculator;
			calculator1.EquipmentType = Core.Constants.EquipmentNeeded.Any;

			var line2 = rateEntry.RateLines.AddNew();
			line2.TL_AC = cartageCharge.PK;
			line2.TL_WeightVolume = "KG";
			line2.TL_Condition = RateLineConditions.OwnCFS;
			var calculator2 = (CartageCalculator)line2.Calculator;
			calculator2.EquipmentType = Core.Constants.EquipmentNeeded.Any;

			AssertHasError(calculator2.String1Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			line1.TL_WeightVolume = "L";
			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertHasError(calculator2.String1Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			calculator1.EquipmentType = Core.Constants.EquipmentNeeded.Any;
			calculator2.EquipmentType = Core.Constants.EquipmentNeeded.Any;
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors as the TL_WeightVolume is different", calculator2.String1Info);

			line1.TL_WeightVolume = "KG";
			line1.TL_Condition = RateLineConditions.HandOver;
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors as the TL_Condition is different", calculator2.String1Info);

			line1.TL_Condition = RateLineConditions.UserDefined;
			line1.TL_ConditionalExpression = "Test";
			line2.TL_Condition = RateLineConditions.UserDefined;
			line2.TL_ConditionalExpression = "Test";
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertHasError(calculator2.String1Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			line2.TL_ConditionalExpression = "Test Another";
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors as the TL_ConditionalExpression is different", calculator2.String1Info);
		}

		public void TestValidateEquipmentType_AllowsDuplicateForDifferentConditionsWhenEqualEquipmentTypes()
		{
			var cartageCharge = Factory.New<AccChargeCode>();
			cartageCharge.AC_RateCalculator = CartageCalculator.Code;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("ORG", "FCL", "AUSYD");

			var line1 = rateEntry.RateLines.AddNew();
			line1.TL_AC = cartageCharge.PK;
			line1.TL_WeightVolume = "CN";
			var calculator1 = (CartageCalculator)line1.Calculator;

			var line2 = rateEntry.RateLines.AddNew();
			line2.TL_AC = cartageCharge.PK;
			line2.TL_WeightVolume = "CN";
			var calculator2 = (CartageCalculator)line2.Calculator;

			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;
			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;

			line1.TL_Condition = ZString.Empty;
			line2.TL_Condition = ZString.Empty;
			AssertHasError(calculator2.String1Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			line1.TL_Condition = ZString.Empty;
			line2.TL_Condition = RateLineConditions.HandOver;
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors when TL_Condition in one line", calculator2.String1Info);

			line1.TL_Condition = RateLineConditions.OwnCFS;
			line2.TL_Condition = RateLineConditions.OwnGateway;
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors as the TL_Condition is different", calculator2.String1Info);

			line1.TL_Condition = RateLineConditions.UserDefined;
			line1.TL_ConditionalExpression = "Expr 1";
			line2.TL_Condition = RateLineConditions.UserDefined;
			line2.TL_ConditionalExpression = "Expr 2";
			line2.RateLineItems[0].Validation.ValidateTM_Text();
			AssertNoErrors("Has NO errors when user defined condition expression is different", calculator2.String1Info);
		}

		#region Implementation

		protected AutoRatingCalculatorParameters GetParameters(ZDecimal originalWeight, ZDecimal originalVolume)
		{
			if (Criteria.FreightMode == FreightMode.UKN)
			{
				Criteria.FreightMode = FreightMode.AIR;
			}
			Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, originalWeight, "KG");
			Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, originalVolume, "M3");

			return new AutoRatingCalculatorParametersForTesting(Criteria);
		}

		#endregion
	}

	sealed class CartageCalculatorTest : CartageCalculatorsTestCase
	{
		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 100, max: 109, flat: 101, perUnit: 102, equipmentType: Constants.FCLEquipmentNeeded.SideLoader, ("-5", 103), ("+5", 104));
			var rateLine11 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 110, max: 119, flat: 111, perUnit: 112, equipmentType: Constants.FCLEquipmentNeeded.SideLoader, ("-5", 113), ("+5", 114), ("+6", 115));
			var rateLine12 = AddCartageRateLine(rateEntry, "FRT", "AUD", "M3", min: 120, max: 129, flat: 121, perUnit: 122, equipmentType: Constants.FCLEquipmentNeeded.SideLoader, ("-5", 123), ("+5", 124));
			var rateLine13 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 130, max: 139, flat: 131, perUnit: 132, equipmentType: Constants.FCLEquipmentNeeded.Trailer, ("-5", 133), ("+5", 134));

			var rateLine20 = AddCartageRateLine(rateEntry, "FRT", "USD", "KG", min: 200, max: 209, flat: 201, perUnit: 112, equipmentType: Constants.FCLEquipmentNeeded.SideLoader, ("-5", 203), ("+5", 204));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine13.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"|AUD|464.00|",
					"Maximum|AUD|139.00|",
					"|AUD|346.00|KG",
					"Drop Container with Sideloader - Less than 5 Kilogram(s)|AUD|216.00|per KG",
					"Drop Container with Sideloader - 5 Kilogram(s) and above|AUD|104.00|per KG",
					"Drop Container with Sideloader - 5 Kilogram(s) to less than 6 Kilogram(s)|AUD|114.00|per KG",
					"Drop Container with Sideloader - 6 Kilogram(s) and above|AUD|115.00|per KG",
					"|AUD|122.00|M3",
					"Drop Container with Sideloader - Less than 5 Cubic Meter(s)|AUD|123.00|per M3",
					"Drop Container with Sideloader - 5 Cubic Meter(s) and above|AUD|124.00|per M3",
					"Drop Trailer - Less than 5 Kilogram(s)|AUD|133.00|per KG",
					"Drop Trailer - 5 Kilogram(s) and above|AUD|134.00|per KG",
					"Minimum|USD|200.00|",
					"|USD|201.00|",
					"Maximum|USD|209.00|",
					"|USD|112.00|KG",
					"Drop Container with Sideloader - Less than 5 Kilogram(s)|USD|203.00|per KG",
					"Drop Container with Sideloader - 5 Kilogram(s) and above|USD|204.00|per KG"
				}
			);
		}

		static RateLine AddCartageRateLine(RateEntry rateEntry, ZString chargeCode, string currency, string unit, decimal min, decimal max, decimal flat, decimal perUnit, string equipmentType, params (string breakValue, decimal breakRate)[] breaks)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<CartageCalculator>();
			calculator.Minimum = min;
			calculator.Maximum = max;
			calculator.BaseRate = flat;
			calculator.PerUnit = perUnit;
			calculator.EquipmentType = equipmentType;

			foreach (var currentBreak in breaks)
			{
				calculator[currentBreak.breakValue] = (ZDecimal)currentBreak.breakRate;
			}

			return rateLine;
		}

		public void TestQuotationLines_Container()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FCL, "AUSYD", "USLAX");

			((RateEntry)Line.ParentRateEntry).TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator["-10"] = (ZDecimal)10m;
			TestCalculator["+10"] = (ZDecimal)20m;
			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertMultilineASCIIEquals
			(
				@"Test Rate|||
Less than 10 Container(s)|USD|10.00|per 20GP Container
10 Container(s) and above|USD|20.00|per 20GP Container",
				string.Join("\r\n", quotationLines.Select(x => x.ToString()))
			);
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR);

			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.PerUnit = 45m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per M3 / 250 KG", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|AUD|45.00|per M3 / 250 KG", quotationLines[0].ToString());

			Line.UseOnlyActualWeightMeasure = true;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|AUD|45.00|per M3", quotationLines[2].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			TestCalculator["-5"] = (ZDecimal)60m;
			var minusItem = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+5"] = (ZDecimal)55m;
			TestCalculator["+10"] = (ZDecimal)52m;
			var plus10Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 5 M3|AUD|60.00|per M3", quotationLines[2].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|55.00|per M3", quotationLines[3].ToString());
			AssertEquals("10 M3 and above|AUD|52.00|per M3", quotationLines[4].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 5 M3|AUD|60.00|per M3", quotationLines[2].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|55.00|per M3", quotationLines[3].ToString());
			AssertEquals("10 M3 and above|AUD|52.00|per M3", quotationLines[4].ToString());

			Line.UseOnlyActualWeightMeasure = false;
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.Minimum = 0m;
			minusItem.TM_BreakMinimum = 100m;
			plus10Item.TM_BreakMinimum = 900m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|AUD|60.00|per M3 / 1000 KG", quotationLines[1].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[2].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|55.00|per M3 / 1000 KG", quotationLines[3].ToString());
			AssertEquals("10 M3 and above|AUD|52.00|per M3 / 1000 KG", quotationLines[4].ToString());
			AssertEquals("Minimum|AUD|900.00|", quotationLines[5].ToString());

			minusItem.TM_RelevantValue = 0m;
			minusItem.TM_BreakMinimum = 0m;
			minusItem.TM_FlatAmount = 60m;
			TestCalculator["+5"] = (ZDecimal)55m;
			plus10Item.TM_RelevantValue = 0m;
			plus10Item.TM_BreakMinimum = 0m;
			plus10Item.TM_FlatAmount = 52m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|AUD|60.00|", quotationLines[1].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|55.00|per M3 / 1000 KG", quotationLines[2].ToString());
			AssertEquals("10 M3 and above|AUD|52.00|", quotationLines[3].ToString());

			Line.TL_WeightVolume = "KG";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 KG|AUD|60.00|(1 M3 = 250 KG)", quotationLines[1].ToString());
			AssertEquals("5 KG to less than 10 KG|AUD|55.00|per KG (1 M3 = 250 KG)", quotationLines[2].ToString());
			AssertEquals("10 KG and above|AUD|52.00|(1 M3 = 250 KG)", quotationLines[3].ToString());

			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			Line.TL_WeightVolume = "M3";

			Line.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.PerUnit = 10m;
			Line.Parent.TI_CartagePickupAddressPostCode = "2229";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2229|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.TI_OH_Consignor = NewClient.PK;
			Line.Parent.TI_CartagePickupAddressPostCode = "2229";
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2229 for Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.TI_RateCategory = "DST";
			Line.Parent.TI_OH_Consignor = ZGuid.Empty;
			Line.Parent.TI_CartagePickupAddressPostCode = ZString.Empty;
			Line.Parent.TI_OH_Consignee = NewClient.PK;
			Line.Parent.TI_CartageDeliveryAddressPostCode = "2229";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  To Postcode 2229 for Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.TI_CartageDeliveryAddressPostCode = "";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  To Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			var address1 = NewClient.Addresses.AddNew();
			address1.OA_Address1 = "Address1";
			address1.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			address1.OA_City = "Alexandria";

			var address2 = NewClient.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			address2.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			address2.OA_City = "Randwick";
			address2.OA_State = "NSW";

			Line.Parent.TI_RateCategory = "ORG";
			Line.Parent.TI_OH_Consignee = ZGuid.Empty;
			Line.Parent.TI_OH_Consignor = NewClient.PK;
			Line.Parent.TI_OA_CartagePickupAddressOverride = address1.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Alexandria for Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.TI_OA_CartagePickupAddressOverride = address2.PK;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW for Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			address2.OA_PostCode = "2020";
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW Postcode 2020 for Test Client #1|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.Parent.TH_OH = NewClient.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW Postcode 2020|AUD|10.00|per M3 / 1000 KG", quotationLines[0].ToString());

			Line.Parent.TI_Mode = "FCL";
			Line.Parent.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			Line.Parent.TI_CartagePickupAddressPostCode = "2021";
			TestCalculator.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Drop Container with Sideloader
-  From Postcode 2021|AUD|10.00|per M3", quotationLines[0].ToString());

			TestCalculator.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2021|AUD|10.00|per M3", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowEquipmentType, parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Wait for Pack/Unpack
-  From Postcode 2021|AUD|10.00|per M3", quotationLines[0].ToString());

			var line2 = Line.Parent.RateLines.AddNew();
			line2.TL_AC = Line.TL_AC;
			line2.TL_RateCalculator = CartageCalculator.Code;
			((CartageCalculator)line2.Calculator).EquipmentType = Constants.FCLEquipmentNeeded.Trailer;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Wait for Pack/Unpack
-  From Postcode 2021|AUD|10.00|per M3", quotationLines[0].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.PerUnit = 45m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per W/M", quotationLines[0].ToString());

			Line.UseOnlyActualWeightMeasure = true;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|AUD|45.00|per W/M", quotationLines[2].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			TestCalculator["-5"] = (ZDecimal)60m;
			var minusItem = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+5"] = (ZDecimal)55m;
			TestCalculator["+10"] = (ZDecimal)52m;
			var plus10Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 5 W/M|AUD|60.00|per W/M", quotationLines[2].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|55.00|per W/M", quotationLines[3].ToString());
			AssertEquals("10 W/M and above|AUD|52.00|per W/M", quotationLines[4].ToString());

			Line.UseOnlyActualWeightMeasure = false;
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.Minimum = 0m;
			minusItem.TM_BreakMinimum = 100m;
			plus10Item.TM_BreakMinimum = 900m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|AUD|60.00|per W/M", quotationLines[1].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[2].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|55.00|per W/M", quotationLines[3].ToString());
			AssertEquals("10 W/M and above|AUD|52.00|per W/M", quotationLines[4].ToString());
			AssertEquals("Minimum|AUD|900.00|", quotationLines[5].ToString());

			minusItem.TM_RelevantValue = 0m;
			minusItem.TM_BreakMinimum = 0m;
			minusItem.TM_FlatAmount = 60m;
			TestCalculator["+5"] = (ZDecimal)55m;
			plus10Item.TM_RelevantValue = 0m;
			plus10Item.TM_BreakMinimum = 0m;
			plus10Item.TM_FlatAmount = 52m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|AUD|60.00|", quotationLines[1].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|55.00|per W/M", quotationLines[2].ToString());
			AssertEquals("10 W/M and above|AUD|52.00|", quotationLines[3].ToString());

			Line.TL_WeightVolume = "KG";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|AUD|60.00|(1 M3 = 250 KG)", quotationLines[1].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|55.00|per W/M", quotationLines[2].ToString());
			AssertEquals("10 W/M and above|AUD|52.00|(1 M3 = 250 KG)", quotationLines[3].ToString());
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			Line.TL_WeightVolume = "M3";

			Line.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.PerUnit = 10m;
			Line.Parent.TI_CartagePickupAddressPostCode = "2229";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2229|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.TI_OH_Consignor = NewClient.PK;
			Line.Parent.TI_CartagePickupAddressPostCode = "2229";
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2229 for Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.TI_RateCategory = "DST";
			Line.Parent.TI_OH_Consignor = ZGuid.Empty;
			Line.Parent.TI_CartagePickupAddressPostCode = ZString.Empty;
			Line.Parent.TI_OH_Consignee = NewClient.PK;
			Line.Parent.TI_CartageDeliveryAddressPostCode = "2229";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  To Postcode 2229 for Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.TI_CartageDeliveryAddressPostCode = "";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  To Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			var address1 = NewClient.Addresses.AddNew();
			address1.OA_Address1 = "Address1";
			address1.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			address1.OA_City = "Alexandria";

			var address2 = NewClient.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			address2.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			address2.OA_City = "Randwick";
			address2.OA_State = "NSW";

			Line.Parent.TI_RateCategory = "ORG";
			Line.Parent.TI_OH_Consignee = ZGuid.Empty;
			Line.Parent.TI_OH_Consignor = NewClient.PK;
			Line.Parent.TI_OA_CartagePickupAddressOverride = address1.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Alexandria for Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.TI_OA_CartagePickupAddressOverride = address2.PK;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW for Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			address2.OA_PostCode = "2020";
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW Postcode 2020 for Test Client #1|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.Parent.TH_OH = NewClient.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);

			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Randwick, NSW Postcode 2020|AUD|10.00|per W/M", quotationLines[0].ToString());

			Line.Parent.TI_Mode = "FCL";
			Line.Parent.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			Line.Parent.TI_CartagePickupAddressPostCode = "2021";
			TestCalculator.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Drop Container with Sideloader
-  From Postcode 2021|AUD|10.00|per W/M", quotationLines[0].ToString());

			TestCalculator.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  From Postcode 2021|AUD|10.00|per W/M", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowEquipmentType, parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Wait for Pack/Unpack
-  From Postcode 2021|AUD|10.00|per W/M", quotationLines[0].ToString());

			var line2 = Line.Parent.RateLines.AddNew();
			line2.TL_AC = Line.TL_AC;
			line2.TL_RateCalculator = CartageCalculator.Code;
			((CartageCalculator)line2.Calculator).EquipmentType = Constants.FCLEquipmentNeeded.Trailer;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			Factory.Save();
			AssertEquals(1, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Wait for Pack/Unpack
-  From Postcode 2021|AUD|10.00|per W/M", quotationLines[0].ToString());
		}

		public void TestQuotationLines_ConversionFactor()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			parentEntry.TI_RH_NKCommodityCode = "GEN";

			TestCalculator.PerUnit = 45m;
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "T";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(200m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per T (1 M3 = 200 KG)", quotationLines[0].ToString());

			Line.TL_WeightVolume = "M3";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per M3 / 200 KG", quotationLines[0].ToString());
		}

		public void TestQuotationLinesAlternativeFomat()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			TestCalculator.PerUnit = 2.95m;

			var testContainer = Factory.New<RefContainer>();
			testContainer.RC_Code = "11AA";

			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			Line.TL_WeightVolume = RatingConstants.Units.CN;
			Line.Parent.TI_RC = testContainer.PK;

			var alternativeFormat = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				var quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.AlternativeFormat, parentEntry);

				AssertEquals(1, quotationLines.Count);
				AssertEquals(ZString.Format("Test Rate|AUD|2.95|per {0} Container", testContainer.RC_Code), quotationLines[0].ToString());
			}
			finally
			{
				DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, alternativeFormat);
			}
		}

		public void TestCalculation()
		{
			Entry.TI_Mode = Core.Constants.RateMode.LSE;
			Line.TL_WeightVolume = Constants.Weight.Kilograms;
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			AssertCalculation(GetParameters(1m, 0.1m), 0m, "25 Kilogram(s) @ AUD 0.00/KG");

			TestCalculator.EquipmentType = "STD";
			TestCalculator.Minimum = 8m;
			TestCalculator["-50"] = (ZDecimal)0.3m;
			TestCalculator["+50"] = (ZDecimal)0.25m;
			TestCalculator["+100"] = (ZDecimal)0.2m;

			AssertCalculation(GetParameters(1m, 0.1m), 8m, "Minimum AUD 8.00");
			AssertCalculation(GetParameters(30m, 0.1m), 9m, "30 Kilogram(s) @ AUD 0.30/KG");
			AssertCalculation(GetParameters(35m, 0.1m), 10.5m, "35 Kilogram(s) @ AUD 0.30/KG");
			AssertCalculation(GetParameters(50m, 0.1m), 12.5m, "50 Kilogram(s) @ AUD 0.25/KG");
			AssertCalculation(GetParameters(51m, 0.2m), 12.75m, "51 Kilogram(s) @ AUD 0.25/KG");
			AssertCalculation(GetParameters(75m, 0.4m), 20m, "100 Kilogram(s) @ AUD 0.20/KG");
			AssertCalculation(GetParameters(100m, 0.6m), 30m, "150 Kilogram(s) @ AUD 0.20/KG");
			AssertCalculation(GetParameters(101m, 0.4m), 20.2m, "101 Kilogram(s) @ AUD 0.20/KG");
			AssertCalculation(GetParameters(200m, 1m), 50m, "250 Kilogram(s) @ AUD 0.20/KG");

			Line.RateLineItems[8].TM_BreakMinimum = 10m;
			AssertCalculation(GetParameters(100m, 0.6m), 30m, "150 Kilogram(s) @ AUD 0.20/KG");

			Line.RateLineItems[8].TM_BreakMinimum = 100m;
			AssertCalculation(GetParameters(100m, 0.6m), 100m, "Minimum AUD 100.00");

			Line.RateLineItems[8].TM_BreakMinimum = 0m;
			Line.RateLineItems[8].TM_RelevantValue = 0m;
			Line.RateLineItems[8].TM_FlatAmount = 20m;
			AssertCalculation(GetParameters(100m, 0.6m), 20m, "Base Rate AUD 20.00 for 150 KG");

			TestCalculator.BaseRate = 5m;
			AssertCalculation(GetParameters(100m, 0.6m), 25m, "Base Rate AUD 20.00 for 150 KG + Base Rate AUD 5.00");

			Line.RateLineItems[8].TM_RelevantValue = 0.2m;
			AssertCalculation(GetParameters(100m, 0.6m), 55m, "Base Rate AUD 20.00 + Base Rate AUD 5.00 + 150 Kilogram(s) @ AUD 0.20/KG");
		}

		public void TestCalculationPerContainer()
		{
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";
			Line.UseOnlyActualWeightMeasure = true;

			var measures = Criteria.RateableMeasures;
			new TestContainers(Factory, "20GP", 3).PopulateContainerList(measures);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.Minimum = 50m;
			TestCalculator["-5"] = (ZDecimal)100m;
			TestCalculator["+5"] = (ZDecimal)120m;

			AssertCalculation(parameters, 300m, "3 Container(s) @ AUD 100.00/Container");

			AssertEquals(Calculator.Items.Operator.Minus, Line.RateLineItems[6].TM_Type);
			Line.RateLineItems[6].TM_BreakWeightVolume = QuantityUnit.M3;
			AssertCalculation(parameters, "Weight / Volume / Packages information was not specified for Pack Lines on this job. Rating based on container weight / volume / packages cannot be performed.");

			measures.RemoveContainerList();
			new TestContainers(Factory, "20GP",
				new MeasureInfo.ContainerInfo[]
				{
					new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 10m, Constants.Volume.CubicMetres, 25, 0, ZString.Empty),
					new MeasureInfo.ContainerInfo(4000m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 5, 0, ZString.Empty),
					new MeasureInfo.ContainerInfo(3000m, Constants.Weight.Kilograms, 5m, Constants.Volume.CubicMetres, 10, 0, ZString.Empty)
				}).PopulateContainerList(measures);
			AssertCalculation(parameters, 340m, "2 Container(s) @ AUD 120.00/Container + 1 Container(s) @ AUD 100.00/Container");

			Line.RateLineItems[6].TM_BreakWeightVolume = "T";
			AssertCalculation(parameters, 320m, "1 Container(s) @ AUD 120.00/Container + 2 Container(s) @ AUD 100.00/Container");
		}

		public void TestValidateRateOperator()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = CartageCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("ORG");

			var oRGRateLine = testEntry.RateLines.AddNew();
			oRGRateLine.TL_AC = dummyChargeCode.PK;

			var line0 = oRGRateLine.RateLineItems.AddNew();
			line0.TM_Type = CartageCalculator.Items.EquipmentType;
			line0.TM_Text = "STD";

			var line1 = oRGRateLine.RateLineItems.AddNew();
			line1.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			var line2 = oRGRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			line2.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has no Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			var line3 = oRGRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.UNT;
			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line3.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("It's okay to have BAS after Minus", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.BAS;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			var line4 = oRGRateLine.RateLineItems.AddNew();

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line4.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line4.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("It's okay to have BAS after Minus", false, line4.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			line2.TM_Type = Calculator.Items.Operator.Minus;
			line3.TM_Type = Calculator.Items.Operator.Plus;
			line4.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line4.TM_TypeInfo.HasErrors());
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Minimum = 50m;
			TestCalculator.PerUnit = 5m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.Minimum = 100m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(150m, clonedLine.GetCalculator<CartageCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<CartageCalculator>().PerUnit);
			AssertEquals(0m, clonedLine.GetCalculator<CartageCalculator>().BaseRate);

			TestCalculator.BaseRate = 25m;
			source.Minimum = 10m;
			source.BaseRate = 80m;
			source.PerUnit = 0m;
			source.PerUnitPercent = 20m;
			source.Percent = 20m;
			source.PerUnitPercent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals("(50 * 120 / 100) + 10 = 70", 70m, clonedLine.GetCalculator<CartageCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<CartageCalculator>().PerUnit);
			AssertEquals(110m, clonedLine.GetCalculator<CartageCalculator>().BaseRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals("(50 + 10) * 120 / 100 ", 72m, clonedLine.GetCalculator<CartageCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<CartageCalculator>().PerUnit);
			AssertEquals(126m, clonedLine.GetCalculator<CartageCalculator>().BaseRate);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);
			TestCalculator["-45"] = (ZDecimal)6m;
			TestCalculator["+45"] = (ZDecimal)5m;
			TestCalculator["+100"] = (ZDecimal)4m;
			TestCalculator["+250"] = (ZDecimal)3m;
			TestCalculator.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source.BaseRate = 70m;
			source["-100"] = (ZDecimal)2m;
			source["+100"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(70m, clonedLine.GetCalculator<CartageCalculator>().BaseRate);
			AssertEquals(8m, clonedLine.Calculator["-45"]);
			AssertEquals(7m, clonedLine.Calculator["+45"]);
			AssertEquals(5m, clonedLine.Calculator["+100"]);
			AssertEquals(4m, clonedLine.Calculator["+250"]);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, clonedLine.GetCalculator<CartageCalculator>().EquipmentType);
		}

		public void TestEquipmentForOriginEntryWithOrgForAir()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = CNR.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Special AIR Equipment should have been found on CNR", "RPA", testCalculator.DetermineDefaultEquipment_Exposed());

			CNRPickupAddress.AddressCapability.DisableAllCapabilities();
			CNRPickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForOriginEntryWithOrgForFCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = CNR.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Special AIR Equipment should have been found on CNR", "RPF", testCalculator.DetermineDefaultEquipment_Exposed());

			CNRPickupAddress.AddressCapability.DisableAllCapabilities();
			CNRPickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "WUP", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForOriginEntryWithOrgForLCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = CNR.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Special AIR Equipment should have been found on CNR", "RPL", testCalculator.DetermineDefaultEquipment_Exposed());

			CNRPickupAddress.AddressCapability.DisableAllCapabilities();
			CNRPickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForOriginEntryWithoutOrgForAir()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "OPA", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgPickupAddr.AddressCapability.DisableAllCapabilities();
			OrgPickupAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForOriginEntryWithoutOrgForFCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "OPF", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgPickupAddr.AddressCapability.DisableAllCapabilities();
			OrgPickupAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "WUP", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForOriginEntryWithoutOrgForLCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OH_Consignor = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "OPL", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgPickupAddr.AddressCapability.DisableAllCapabilities();
			OrgPickupAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithOrgForAir()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = CNE.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Special AIR Equipment should have been found on CNE", "EDA", testCalculator.DetermineDefaultEquipment_Exposed());

			CNEDeliveryAddress.AddressCapability.DisableAllCapabilities();
			CNEDeliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithOrgForFCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = CNE.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Special AIR Equipment should have been found on CNE", "EDF", testCalculator.DetermineDefaultEquipment_Exposed());

			CNEDeliveryAddress.AddressCapability.DisableAllCapabilities();
			CNEDeliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "WUP", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithOrgForLCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = CNE.PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Special AIR Equipment should have been found on CNE", "EDL", testCalculator.DetermineDefaultEquipment_Exposed());

			CNEDeliveryAddress.AddressCapability.DisableAllCapabilities();
			CNEDeliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithoutOrgForAir()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "ODA", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgDeliveryAddr.AddressCapability.DisableAllCapabilities();
			OrgDeliveryAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithoutOrgForFCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "ODF", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgDeliveryAddr.AddressCapability.DisableAllCapabilities();
			OrgDeliveryAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "WUP", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		public void TestEquipmentForDestinationEntryWithoutOrgForLCL()
		{
			SetupTestData();
			Line.TL_RateCalculator = CartageCalculatorWithOverride.Code;
			var testCalculator = (CartageCalculatorWithOverride)Line.Calculator;

			Line.Parent.TI_RateCategory = RatingConstants.RateCategory.DST;
			Line.Parent.TI_OH_Consignee = ZGuid.Empty;
			Line.Parent.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Special AIR Equipment should have been found on Main Header", "ODL", testCalculator.DetermineDefaultEquipment_Exposed());

			OrgDeliveryAddr.AddressCapability.DisableAllCapabilities();
			OrgDeliveryAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Registry Default AIR Equipment should have been found", "PSL", testCalculator.DetermineDefaultEquipment_Exposed());
		}

		#region Implementation

		void SetupTestData()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Line.Parent.Parent.TH_OH = Org.PK;

			OrgPickupAddr = Line.Parent.Parent.Header.Addresses.AddNew();
			OrgPickupAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			OrgPickupAddr.OA_AIREquipmentNeeded = "OPA";
			OrgPickupAddr.OA_FCLEquipmentNeeded = "OPF";
			OrgPickupAddr.OA_LCLEquipmentNeeded = "OPL";

			OrgDeliveryAddr = Line.Parent.Parent.Header.Addresses.AddNew();
			OrgDeliveryAddr.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			OrgDeliveryAddr.OA_AIREquipmentNeeded = "ODA";
			OrgDeliveryAddr.OA_FCLEquipmentNeeded = "ODF";
			OrgDeliveryAddr.OA_LCLEquipmentNeeded = "ODL";

			CNE = Factory.NewWithValidTestData<OrgHeader>();
			CNEDeliveryAddress = CNE.Addresses.AddNew();
			CNEDeliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			CNEDeliveryAddress.OA_AIREquipmentNeeded = "EDA";
			CNEDeliveryAddress.OA_FCLEquipmentNeeded = "EDF";
			CNEDeliveryAddress.OA_LCLEquipmentNeeded = "EDL";

			CNR = Factory.NewWithValidTestData<OrgHeader>();
			CNRPickupAddress = CNR.Addresses.AddNew();
			CNRPickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			CNRPickupAddress.OA_AIREquipmentNeeded = "RPA";
			CNRPickupAddress.OA_FCLEquipmentNeeded = "RPF";
			CNRPickupAddress.OA_LCLEquipmentNeeded = "RPL";
		}

		OrgHeader CNE;
		OrgAddress CNEDeliveryAddress;

		OrgHeader CNR;
		OrgAddress CNRPickupAddress;

		OrgHeader Org;
		OrgAddress OrgPickupAddr;
		OrgAddress OrgDeliveryAddr;

		protected override Type CalculatorType
		{
			get { return typeof(CartageCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return CartageCalculator.Code; }
		}

		new CartageCalculator TestCalculator
		{
			get { return (CartageCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
