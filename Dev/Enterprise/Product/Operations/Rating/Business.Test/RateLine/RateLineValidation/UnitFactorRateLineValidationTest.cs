using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class UnitFactorRateLineValidationTest : RatingTestCase
	{
		public void TestUnitFactor()
		{
			var ratingHeaderTypes = new[] { RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RatingHeaderTypes.Quote };
			foreach (var ratingHeaderType in ratingHeaderTypes)
			{
				var ratingHeader = Helper.NewRatingHeader(ratingHeaderType, Helper.NewOrgHeader());
				var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);
				var rateLine = rateEntry.RateLines.AddNew();

				foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
				{
					foreach (var calculator in Helper.CalcCodeList)
					{
						rateEntry.TI_RateCategory = rateCategory;
						rateLine.TL_RateCalculator = calculator;
						rateLine.TL_ActualPercentage = 100;
						rateLine.TL_WeightVolume = QuantityUnit.KG;

						foreach (var unitFactor in new[] { UnitFactorList.Codes.PacksWeight, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine })
						{
							foreach (var enableWarehouseUnitFactorRatesDevelopment in new[] { true, false })
							{
								using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableWarehouseUnitFactorRatesDevelopment))
								{
									var testCase = UnitFactorTestCase.Find(UnitFactorTestCases, enableWarehouseUnitFactorRatesDevelopment, ratingHeaderType, rateCategory, calculator, unitFactor);
									rateLine.TL_UnitFactor = unitFactor;
									if (string.IsNullOrEmpty(testCase.ExpectedErrorMessage))
									{
										AssertNoErrors
										(
											$"EnableRegistry: '{enableWarehouseUnitFactorRatesDevelopment}', RatingHeader: '{ratingHeaderType}', category: '{rateCategory}', calculator: '{calculator}', unitFactor: '{unitFactor}': calculator should be supported",
											rateLine.TL_UnitFactorInfo
										);
									}
									else
									{
										AssertHasError
										(
											$"EnableRegistry: '{enableWarehouseUnitFactorRatesDevelopment}', RatingHeader: '{ratingHeaderType}', category: '{rateCategory}', calculator: '{calculator}', unitFactor: '{unitFactor}': should have error '{testCase.ExpectedErrorMessage}'",
											rateLine.TL_UnitFactorInfo,
											testCase.ExpectedErrorMessage
										);
									}
								}
							}
						}
					}
				}
			}
		}

		static readonly UnitFactorTestCase[] UnitFactorTestCases = new[]
		{
			// PacksWeight
			new UnitFactorTestCase(enableRegister: true, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, UnitFactorList.Codes.PacksWeight, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: false, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, UnitFactorList.Codes.PacksWeight, expectedErrorMessage: "Enter a valid Unit Factor."),
			new UnitFactorTestCase(enableRegister: true, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, string.Empty, UnitFactorList.Codes.PacksWeight, expectedErrorMessage: "Packs weight only supports CMB calculator."),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, string.Empty, CombinedCalculator.Code, UnitFactorList.Codes.PacksWeight, expectedErrorMessage: "Enter a valid Unit Factor."),
			new UnitFactorTestCase(enableRegister: null, string.Empty, string.Empty, string.Empty, UnitFactorList.Codes.PacksWeight, expectedErrorMessage: "Enter a valid Unit Factor."),

			// ProductLine
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, UnitCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CartageCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, ExcludeCompanyTariffsCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, FirstPlusAdditionalCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, FlatPlusPerUnitCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, NoteCalculator.Code, UnitFactorList.Codes.ProductLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, string.Empty, UnitFactorList.Codes.ProductLine, expectedErrorMessage: "Calculator is not supported for product line."),
			new UnitFactorTestCase(enableRegister: null, string.Empty, string.Empty, string.Empty, UnitFactorList.Codes.ProductLine, expectedErrorMessage: "Enter a valid Unit Factor."),

			// PackageLine
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, FlatCalculator.Code, UnitFactorList.Codes.PackageLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, UnitCalculator.Code, UnitFactorList.Codes.PackageLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, UnitFactorList.Codes.PackageLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CartageCalculator.Code, UnitFactorList.Codes.PackageLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, UnitFactorList.Codes.PackageLine, expectedErrorMessage: string.Empty),
			new UnitFactorTestCase(enableRegister: null, string.Empty, RatingConstants.RateCategory.WHS, string.Empty, UnitFactorList.Codes.PackageLine, expectedErrorMessage:  "Package line only supports FLT, UNT, CMB, CTG and CTZ calculators."),
			new UnitFactorTestCase(enableRegister: null, string.Empty, string.Empty, string.Empty, UnitFactorList.Codes.PackageLine, expectedErrorMessage:  "Enter a valid Unit Factor."),
		};

		class UnitFactorTestCase
		{
			bool? EnableRegister { get; }

			string RateType { get; }

			string RateCategory { get; }

			string Calculator { get; }

			string UnitFactor { get; }

			public string ExpectedErrorMessage { get; }

			public UnitFactorTestCase(bool? enableRegister, string rateType, string rateCategory, string calculator, string unitFactor, string expectedErrorMessage)
			{
				EnableRegister = enableRegister;
				RateType = rateType;
				RateCategory = rateCategory;
				UnitFactor = unitFactor;
				Calculator = calculator;
				ExpectedErrorMessage = expectedErrorMessage;
			}

			int Rank => (EnableRegister != null ? 1 : 0)
				+ (!string.IsNullOrEmpty(RateType) ? 10 : 0)
				+ (!string.IsNullOrEmpty(RateCategory) ? 100 : 0)
				+ (!string.IsNullOrEmpty(Calculator) ? 1000 : 0)
				+ (!string.IsNullOrEmpty(UnitFactor) ? 10000 : 0);

			public static UnitFactorTestCase Find(IEnumerable<UnitFactorTestCase> testCases, bool enableRegister, string rateType, string rateCategory, string calculator, string unitFactor)
			{
				var results = testCases
					.Where(testCase =>
						(testCase.EnableRegister == null || testCase.EnableRegister == enableRegister)
						&& (string.IsNullOrEmpty(testCase.RateType) || testCase.RateType == rateType)
						&& (string.IsNullOrEmpty(testCase.RateCategory) || testCase.RateCategory == rateCategory)
						&& (string.IsNullOrEmpty(testCase.Calculator) || testCase.Calculator == calculator)
						&& (string.IsNullOrEmpty(testCase.UnitFactor) || testCase.UnitFactor == unitFactor)
					)
					.OrderByDescending(testCase => testCase.Rank);

				return results.FirstOrDefault();
			}
		}

		public void TestUnitFactor_Simple()
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
				TestWHSLine.TL_RateCalculator = CombinedCalculator.Code;
				TestWHSLine.TL_ActualPercentage = 100;
				TestWHSLine.TL_WeightVolume = QuantityUnit.KG;
				TestWHSLine.Calculator.IsAccumulated = false;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertNoErrors("PacksWeight", TestWHSLine.TL_UnitFactorInfo);

				TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
				TestWHSLine.TL_RateCalculator = UnitCalculator.Code;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertNoErrors("ProductLine", TestWHSLine.TL_UnitFactorInfo);

				TestWHSLine.TL_UnitFactor = "???";
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertHasError("Invalid UnitFactor", TestWHSLine.TL_UnitFactorInfo, "Enter a valid Unit Factor.");
			}
		}

		public void TestUnitFactor_PacksWeight_WeightVolume()
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
				TestWHSLine.TL_RateCalculator = CombinedCalculator.Code;
				TestWHSLine.TL_ActualPercentage = 100;
				TestWHSLine.TL_WeightVolume = string.Empty;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertHasError(TestWHSLine.TL_UnitFactorInfo, "Packs weight requires weight unit.");

				TestWHSLine.TL_WeightVolume = QuantityUnit.KG;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertNoErrors(TestWHSLine.TL_UnitFactorInfo);

				TestWHSLine.TL_WeightVolume = Weight.Pounds;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertNoErrors(TestWHSLine.TL_UnitFactorInfo);

				TestWHSLine.TL_WeightVolume = QuantityUnit.PL;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertHasError(TestWHSLine.TL_UnitFactorInfo, "Packs weight requires weight unit.");
			}
		}

		public void TestUnitFactor_PacksWeight_IsAccumulated()
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
				TestWHSLine.TL_RateCalculator = CombinedCalculator.Code;
				TestWHSLine.TL_ActualPercentage = 100;
				TestWHSLine.TL_WeightVolume = QuantityUnit.KG;
				TestWHSLine.Calculator.IsAccumulated = false;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertNoErrors("Non Cumulative break is supported", TestWHSLine.TL_UnitFactorInfo);

				TestWHSLine.Calculator.IsAccumulated = true;
				TestWHSLine.Validation.ValidateTL_UnitFactor();
				AssertHasError("Cumulative break is NOT supported", TestWHSLine.TL_UnitFactorInfo, "Packs weight only supports non cumulative break.");
			}
		}

		public void TestUnitFactor_Warehouse_LoadedPackagesOnly()
		{
			TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;
			TestWHSLine.TL_RateCalculator = WarehousePackCalculator.Code;
			TestWHSLine.TL_ActualPercentage = 100;
			TestWHSLine.TL_WeightVolume = QuantityUnit.PK;
			TestWHSLine.Calculator.IsAccumulated = false;
			TestWHSLine.Validation.ValidateTL_UnitFactor();
			AssertNoErrors("LoadedPackagesOnly", TestWHSLine.TL_UnitFactorInfo);

			TestWHSLine.TL_WeightVolume = QuantityUnit.VE;
			TestWHSLine.Validation.ValidateTL_UnitFactor();
			AssertHasError("Invalid UnitFactor", TestWHSLine.TL_UnitFactorInfo, "Loaded Packages Only Unit Factor only supports WPK Calculator and PK Units.");

			TestWHSLine.TL_RateCalculator = CombinedCalculator.Code;
			TestWHSLine.TL_WeightVolume = QuantityUnit.PK;
			TestWHSLine.Validation.ValidateTL_UnitFactor();
			AssertEquals("Unit Factor is empty after Calculator changed from WPK", string.Empty, TestWHSLine.TL_UnitFactor);
			AssertNoErrors(TestWHSLine.TL_UnitFactorInfo);

			TestWHSLine.TL_RateCalculator = WarehousePackCalculator.Code;
			TestWHSLine.TL_WeightVolume = QuantityUnit.PK;
			TestWHSLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;
			TestWHSLine.Validation.ValidateTL_UnitFactor();
			AssertNoErrors("LoadedPackagesOnly", TestWHSLine.TL_UnitFactorInfo);
		}

		#region Implementation

		RateLine TestWHSLine
		{
			get
			{
				if (testWHSLine == null)
				{
					var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
					var testEntry = testRate.AddRateEntry(RatingConstants.RateCategory.WHS);
					testWHSLine = testEntry.RateLines.AddNew();
				}

				return testWHSLine;
			}
		}

		RateLine testWHSLine;

		#endregion
	}
}
