using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class RatesServiceChargeViewModelTest : ChargeViewModelTest
	{
		public void TestAmount_PopulateFromCalculations()
		{
			AutoRateInfo.Bases.Clear();
			AutoRateInfo.Currency = "UAH";
			AutoRateInfo.AddFlatPaymentBasis(66, "12345", "UAH");

			var viewModel = CreateViewModel();

			AssertEquals("The currency should match the expected value", "UAH", viewModel.Currency);
			AssertEquals("The amount should match the expected value", 66m, viewModel.Amount);
		}

		public void TestIsOptional()
		{
			Charge.ChargeType = ChargeType.Optional;
			var viewModel = CreateViewModel();
			AssertEquals("IsOptional should be true when ChargeType is Optional", true, viewModel.IsOptional);

			Charge.ChargeType = ChargeType.None;
			viewModel = CreateViewModel();
			AssertEquals("IsOptional should be false when ChargeType is None", false, viewModel.IsOptional);
		}

		public void TestIsIncluded()
		{
			Charge.ChargeType = ChargeType.Included;
			var viewModel = CreateViewModel();
			AssertEquals("Expected IsIncluded to be true for ChargeType.Included", true, viewModel.IsIncluded);

			Charge.ChargeType = ChargeType.SubjectTo;
			viewModel = CreateViewModel();
			AssertEquals("Expected IsIncluded to be false for ChargeType.SubjectTo", false, viewModel.IsIncluded);
		}

		public void TestChargeCodeError()
		{
			var viewModel = CreateViewModel();

			AssertEquals("The error level should be None initially.", ErrorLevel.None, viewModel.ChargeCodeErrorLevel);
			AssertNullOrEmpty("The charge code error should initially be null or empty.", viewModel.ChargeCodeError);

			// In response to a case where multiple empty descriptions were
			// joined with \r\n, making a non-empty string

			AutoRateInfo.Bases.Add(new PaymentBasis(
				new Quantity(777, "KG"),
				RateInfo.CreateUNT(200, "KG", "USD"),
				AdapterType.Consolidation,
				"12345"));

			viewModel = CreateViewModel();

			AssertEquals("After adding a payment basis, the error level should remain None.", ErrorLevel.None, viewModel.ChargeCodeErrorLevel);
			AssertNullOrEmpty("After adding a payment basis, the charge code error should remain null or empty.", viewModel.ChargeCodeError);
		}

		public void TestChargeCode_ChargeHasNoUniversalCode_PopulateCarrierChargeCode()
		{
			Charge.ChargeCode = null;
			Charge.CarrierChargeCodeInfo = new CarrierSpecificChargeCode
			{
				Code = "CGFRT",
				Description = "CG Freight"
			};

			var viewModel = CreateViewModel();

			AssertEquals("The carrier charge code should match the expected code.", "CGFRT", viewModel.ChargeCode);
			AssertEquals("The charge code description should match the expected description.", "CG Freight", viewModel.ChargeCodeDescription);
			AssertEquals("The error level should be set to Error.", ErrorLevel.Error, viewModel.ChargeCodeErrorLevel);
			AssertEquals("The error message should match the expected text.", "The charge code has NO mapping with any Universal Charge Code.", viewModel.ChargeCodeError);
			AssertNull("AccChargeCode should be null.", viewModel.AccChargeCode);
			AssertEquals("IsValid should be false.", false, viewModel.IsValid);
		}

		public void TestChargeCode_ChargeCodeNotMapped_PopulateUniversalChargeCode()
		{
			Charge.ChargeCode = "UFRT";
			Charge.CarrierChargeCodeInfo = new CarrierSpecificChargeCode
			{
				Code = "CGFRT",
				Description = "CG Freight"
			};

			RawResponse.ChargeCodes = new[]
			{
				new RefChargeCode
				{
					Code = "UFRT",
					Description = "Universal freight",
				}
			};

			var viewModel = CreateViewModel();

			AssertEquals("ChargeCode should match the expected value.", "UFRT", viewModel.ChargeCode);
			AssertEquals("ChargeCodeDescription should match the expected value.", "Universal freight", viewModel.ChargeCodeDescription);
			AssertEquals("ChargeCodeErrorLevel should be a warning.", ErrorLevel.Warning, viewModel.ChargeCodeErrorLevel);
			AssertEquals("ChargeCodeError message should match the expected value.",
				"No Charge Code is assigned with or has the same Code as Universal 'UFRT'.", viewModel.ChargeCodeError);
			AssertNull("AccChargeCode should be null.", viewModel.AccChargeCode);
			Assert("The user will be asked to setup mapping on fly.", viewModel.IsValid);
		}

		public void TestChargeCode_ChargeCodeHasMapping_PopulateMappedCode()
		{
			Charge.ChargeCode = "UFRT";
			Charge.CarrierChargeCodeInfo = new CarrierSpecificChargeCode
			{
				Code = "CGFRT",
				Description = "CG Freight"
			};

			InsertChargeCode(Factory, "LOCFRT", "Local Freight", CombinedCalculator.Code, "FRT", "UFRT");
			Factory.Save();

			RawResponse.ChargeCodes = new[]
			{
				new RefChargeCode
				{
					Code = "UFRT",
					Description = "Universal freight",
				}
			};

			var viewModel = CreateViewModel();

			AssertEquals("Expected mapped charge code", "LOCFRT", viewModel.ChargeCode);
			AssertEquals("Expected mapped charge code description", "Local Freight", viewModel.ChargeCodeDescription);
			AssertEquals("Expected no error level", ErrorLevel.None, viewModel.ChargeCodeErrorLevel);
			AssertNullOrEmpty("Expected no error message", viewModel.ChargeCodeError);
			AssertNotNull("Expected Account Charge Code to be non-null", viewModel.AccChargeCode.AC_Code);
			AssertEquals("Expected mapped account charge code", "LOCFRT", viewModel.AccChargeCode.AC_Code);
			AssertEquals("Expected the view model to be valid", true, viewModel.IsValid);
		}

		public void TestGetAutoRateInfo()
		{
			var viewModel = CreateViewModel();
			AssertEquals("AutoRateInfo comparison failed", AutoRateInfo, viewModel.AutoRateInfo);
		}

		public void TestChargeableHasDescription_DisplayItAsChargeWarning()
		{
			var chargeFRT = new Charge
			{
				ChargeCode = "FRT",
				IsHigherBreakLowerRate = true,
				PerUnitRate = 100,
				Unit = "KG",
				Currency = "USD",
			};

			var paymentBases = new[]
			{
				new PaymentBasis(
					new Quantity(100, "KG", description: "HBLR is applied"),
					RateInfo.CreateUNT(10, "KG", "UAH"),
					AdapterType.Consolidation,
					"McLaren")
			};

			var calcOutput = new CalculatorOutput(paymentBases);
			var calcResult = new CalculationResult(Line, calcOutput);
			var autoRateInfo = new AutoRateInfo(calcResult, new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria()), Factory);

			var viewModelWithHBLR = CreateViewModel(chargeFRT, autoRateInfo);
			AssertEquals(
				"Expected the ChargeCodeErrorLevel to be Warning for charges with HBLR",
				ErrorLevel.Warning,
				viewModelWithHBLR.ChargeCodeErrorLevel
			);
			AssertEquals(
				"Expected the ChargeCodeError to mention 'HBLR is applied'",
				"HBLR is applied",
				viewModelWithHBLR.ChargeCodeError
			);
		}

		protected override ChargeViewModel GetInstance(string chargeCode, decimal amount, string currency, ICurrencyConverter currencyConverter)
		{
			AutoRateInfo = new AutoRateInfo(Factory);
			AutoRateInfo.Currency = "USD";
			AutoRateInfo.Bases.Add(
				new PaymentBasis(
					new Quantity(666, "KG"),
					RateInfo.CreateFLT(amount, currency),
					AdapterType.Consolidation,
					"12345"));

			Charge.Currency = currency;
			Charge.ChargeCode = chargeCode;

			RawResponse.ChargeCodes = new[]
			{
				new RefChargeCode { Code = chargeCode }
			};

			var context = new RateSelectorContext
			{
				Factory = Factory,
				Filters = GetValidFilters(),
				Logger = new MemoryLogger(),
				RatesServiceResponse = RawResponse,
				CurrencyConverter = currencyConverter
			};

			return new RatesServiceChargeViewModel(Charge, AutoRateInfo, context, null);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Charge = new Charge
			{
				ChargeCode = "FRT",
				PerUnitRate = 100,
				Unit = "KG",
				Currency = "USD",
			};

			AutoRateInfo = new AutoRateInfo(Factory);
			AutoRateInfo.Currency = "USD";
			AutoRateInfo.Bases.Add(
				new PaymentBasis(
					new Quantity(666, "KG"),
					RateInfo.CreateUNT(100, "KG", "USD"),
					AdapterType.Consolidation,
					"12345"));

			Rate = new Rate
			{
				Origin = "AUSYD",
				Destination = "UAIEV",
				Provider = "CGGD",
				ContractNumber = "666",
				Carrier = "EMIRTS",
				Charges = new List<Charge>(new[] { Charge })
			};

			RawResponse = new RatesSearchResponse
			{
				Rates = new Rate[] { Rate },
				Carriers = new[]
				{
					new RefCarrier
					{
						Code = "EMIRTS",
						Name = "Emirates",
						IATACode = "EK"
					}
				},
				ChargeCodes = new[]
				{
					new RefChargeCode
					{
						Code = "FRT",
						Group = "FRT",
						Description = "Freight"
					}
				}
			};

			Filters = GetValidFilters();

			var header = new WiseHeader(new BusinessObjectFactory());
			header.TH_OH = TransportProvider1.PK;

			var entry = new WiseEntry(Rate, Factory);
			entry.ParentRatingHeader = header;

			var frtCharges = InsertChargeCode(Factory, "TestFRT", "Freight", "FLT", "FRT", "FRT");

			Line = new WiseLine(Factory, Charge);
			Line.ParentRateEntry = entry;
			Line.TL_AC = frtCharges.PK;
		}

		RatesServiceChargeViewModel CreateViewModel() => CreateViewModel(Charge);
		RatesServiceChargeViewModel CreateViewModel(Charge charge, AutoRateInfo autoRateInfoOverride = null)
		{
			var context = new RateSelectorContext
			{
				Factory = Factory,
				Filters = Filters,
				Logger = new MemoryLogger(),
				RatesServiceResponse = RawResponse,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			return new RatesServiceChargeViewModel(charge, autoRateInfoOverride ?? AutoRateInfo, context, null);
		}

		Rate Rate { get; set; }
		Charge Charge { get; set; }
		AutoRateInfo AutoRateInfo { get; set; }
		RatesSearchResponse RawResponse { get; set; }
		RateSelectorFilterStripBusinessObject Filters { get; set; }
		WiseLine Line { get; set; }
	}
}
