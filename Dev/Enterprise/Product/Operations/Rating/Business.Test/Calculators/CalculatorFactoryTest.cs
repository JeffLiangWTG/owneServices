using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class CalculatorFactoryTest : TestCaseWithFactory
	{
		public void TestFactory()
		{
			var assembly = typeof(Calculator).Assembly;
			var assemblyTypes = assembly.GetTypes();
			var calculatorTypes = new List<Type>();

			foreach (var type in assemblyTypes)
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(Calculator)) && type.IsPublic
					&& type.GetCustomAttributes(typeof(TestOnlyCalculatorAttribute), true).Length == 0)
				{
					calculatorTypes.Add(type);
				}
			}

			var calculatorCodes = new List<string>();
			foreach (var calculatorType in calculatorTypes)
			{
				FieldInfo[] fieldInfos;
				if (calculatorType == typeof(CompanyTariffOrCostBasedCalculator))
				{
					fieldInfos = new FieldInfo[] { calculatorType.GetField("CompanyTariffBasedCode", BindingFlags.Static | BindingFlags.Public), calculatorType.GetField("CostBasedCode", BindingFlags.Static | BindingFlags.Public) };
				}
				else
				{
					fieldInfos = new FieldInfo[] { calculatorType.GetField("Code", BindingFlags.Static | BindingFlags.Public) };
				}

				foreach (var info in fieldInfos)
				{
					AssertNotNull(calculatorType.ToString(), info);
					var code = (string)info.GetValue(null);

					var clientRate = Factory.New<ClientRate>();
					var rateEntry = clientRate.AddRateEntry("AIR");
					var testLine = rateEntry.RateLines.AddNew();

					testLine.TL_RateCalculator = code;
					AssertEquals(calculatorType + "." + info.Name, calculatorType, testLine.Calculator.GetType());

					if (calculatorType != typeof(NullCalculator))
					{
						calculatorCodes.Add(code);
					}
				}
			}

			var coreCalculators = new CodeDescriptionPairList(OLookUpEditType.RateCalculators);
			foreach (var code1 in calculatorCodes)
			{
				if (code1 != "EQH")
				{
					Assert(code1 + " exists in Core Calculators", coreCalculators.ContainsCode(code1));
				}
			}

			foreach (CodeDescriptionPair descriptionPair in coreCalculators)
			{
				Assert(descriptionPair.Code + " exists in Rating Calculators", calculatorCodes.Contains(descriptionPair.Code));
			}

			var testChargeCode = Factory.New<AccChargeCode>();
			AssertEquals("Empty Calculator Description", "", testChargeCode.AC_RateCalculatorDesc);
			foreach (var code1 in calculatorCodes)
			{
				if (code1 != "EQH")
				{
					testChargeCode.AC_RateCalculator = code1;
					AssertEquals(code1 + " Calculator Description", RateCalculatorDescriptions[code1],
						testChargeCode.AC_RateCalculatorDesc);
				}
			}
		}

		public void TestCalculatorCreatedNotUsingFactory()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			AssertExceptionThrown<InvalidOperationException>("Calculator has to be created using CalculatorFactory", () => new FlatCalculator(rateLine));
		}

		#region Implementation

		Dictionary<string, string> RateCalculatorDescriptions
		{
			get
			{
				if (fRateCalculatorDescriptions == null)
				{
					fRateCalculatorDescriptions = new Dictionary<string, string>();
					fRateCalculatorDescriptions.Add(AgencyCalculator.Code, "Used to calculate customs agency charges and handle associated lines and tariff information.");
					fRateCalculatorDescriptions.Add(CombinedCalculator.Code, "Allows for Base, Minimum, Per Unit and sliding charges (with possible accumulation) to be specified.");
					fRateCalculatorDescriptions.Add(CombinedBreaksWithIncrementCalculator.Code, "Used to calculate Maintenance and Repair charges to be specified.");
					fRateCalculatorDescriptions.Add(CompanyTariffOrCostBasedCalculator.CostBasedCode, "Used to specify charges that are based on a matching Costing.");
					fRateCalculatorDescriptions.Add(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "Used to specify charges that are based on a matching Company Tariff.");
					fRateCalculatorDescriptions.Add(CartageCalculator.Code, "Used to handle various Port Transport weight breaks and varying volumetric conversion factors.");
					fRateCalculatorDescriptions.Add(CartageZoneDistanceCalculator.Code, "Used to specify Port Transport rates that are based on Port Transport Zones (distance ranges).");
					fRateCalculatorDescriptions.Add(DisbursementInterestCalculator.Code, "Used to specify an interest charge on disbursement amounts. This can be based on the customer's credit terms, or on the number of outstanding days.");
					fRateCalculatorDescriptions.Add(EquipmentHireCalculator.Code, "Used to calculate a charge for special equipment required on a job.");
					fRateCalculatorDescriptions.Add(ExcludeCompanyTariffsCalculator.Code, "");
					fRateCalculatorDescriptions.Add(FlatCalculator.Code, "Used to handle fixed charges.");
					fRateCalculatorDescriptions.Add(FirstPlusAdditionalCalculator.Code, "Used to calculate a first unit charge and then a thereafter charge per unit.");
					fRateCalculatorDescriptions.Add(FlatPlusPerUnitCalculator.Code, "Used to handle a base charge as well as an additional per unit charge.");
					fRateCalculatorDescriptions.Add(FreightInclusiveCalculator.Code, "Used to specify charges to be listed as an Inclusive Charge for Freight instead of creating a separate charge.");
					fRateCalculatorDescriptions.Add(HighestChargeCalculator.Code, "Used to calculate a charge which is the highest amount of a set of charges.");
					fRateCalculatorDescriptions.Add(HighestRateCalculator.Code, "Used to calculate charges based on the highest rate of the weight, volume or minimum.");
					fRateCalculatorDescriptions.Add(HousebillReleaseTypeCalculator.Code, "Used to calculate a charge for each House bill Release Type.");
					fRateCalculatorDescriptions.Add(PackageCountCalculator.Code, "Used to calculate a charge taking into consideration the number of outer packages and weights - e.g. Italian Airport Tax.");
					fRateCalculatorDescriptions.Add(ValueRangeCalculator.Code, "Used to calculate charges based on the Value of Goods. Most commonly used for the Italian export customs formalities charge.");
					fRateCalculatorDescriptions.Add(MinimumCalculator.Code, "Used to calculate rates based on minimum per Job or Charge code.");
					fRateCalculatorDescriptions.Add(MinimumOrPerUnitCalculator.Code, "Used to calculate per unit charges and apply them if greater than the minimum.");
					fRateCalculatorDescriptions.Add(NoteCalculator.Code, "Used to specify free-text charges that cannot be automatically calculated.");
					fRateCalculatorDescriptions.Add(PercentageBreaksCalculator.Code, "Used to calculate a charge which is a percentage of a group of charges, another charge or specific job values, where a different percentage is required based on a break value.");
					fRateCalculatorDescriptions.Add(PercentageCalculator.Code, "Used to calculate a charge which is a percentage of a group of charges, another charge or specific job values.");
					fRateCalculatorDescriptions.Add(ProfitShareRebateCalculator.Code, "Used to calculate Profit Share /Rebate against the total Profit or Loss of a job.");
					fRateCalculatorDescriptions.Add(SplitMonthBillingCalculator.Code, "Used to calculate charges based on the day of month. Most commonly used for the Warehousing.");
					fRateCalculatorDescriptions.Add(TimeCalculator.Code, "Used to specify accumulated sliding charges based on a time period and units.");
					fRateCalculatorDescriptions.Add(UnitCalculator.Code, "Used to calculate a charge per unit.");
					fRateCalculatorDescriptions.Add(EqualizationCalculator.Code, "Used to calculate a pivot weight break and apply the volume equalized discount rate.");
					fRateCalculatorDescriptions.Add(WarehouseLocationTypeCalculator.Code, "Used to calculate storage rates based on the location type.");
					fRateCalculatorDescriptions.Add(WarehousePackCalculator.Code, "Used to calculate warehouse package rates for all inner pack types.");
				}

				return fRateCalculatorDescriptions;
			}
		}

		Dictionary<string, string> fRateCalculatorDescriptions;

		#endregion
	}
}
