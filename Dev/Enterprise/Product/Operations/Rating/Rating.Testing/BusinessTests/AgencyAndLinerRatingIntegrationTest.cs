using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AgencyAndLinerRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Contract Numbers

		public void TestContractNumbers_CLC_ShouldNotBeUsedInAutoRating()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var billOfLading = CreateBillOfLading();
			var clientContractNumber = billOfLading.Numbers.AddNew();
			clientContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC;
			clientContractNumber.CE_EntryNum = "CLC1";

			var carrierContractNumber = billOfLading.Numbers.AddNew();
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "CON1";

			var actualClientContractNumbers = billOfLading.RatingAdapter.ClientContractNumbers
				.Select(x => x.ToString())
				.ToArray();
			var expectedClientContractNumbers = new[] { "CON1" };

			AssertContainsExactElementsInAnyOrder(
				"BOL ClientContractNumber should not consider CLC",
				expectedClientContractNumbers,
				actualClientContractNumbers
			);

			var ratingHeader = Helper.NewClientRate(Consignee);
			var contractAAA = new AmountAndContractNumber(100, "CLC1");
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-1);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(10);

			Factory.Save();

			AutorateAndAssert(Array.Empty<AssertionCharge>(), billOfLading, Consignee, autorateCosts: false);
		}

		public void TestContractNumbers_Revenue_PopulateNo_ReplaceNo()
		{
			// Given:
			// - Client rate with entries differentiated by contract numbers
			// - Liner & Agency Carrier Contract Number registry items are both set to NO
			// When:
			// - Auto Revenue bill of lading
			// Then:
			// - Do not change job contract numbers
			// - Create charges for only rates matching job contract numbers

			var ratingHeader = Helper.NewClientRate(Consignee);
			var contractAAA = new AmountAndContractNumber(100, "aaa");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBBB = new AmountAndContractNumber(200, "bbb");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBBB.ChargeCode, contractBBB.Amount, contractNumber: contractBBB.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			var contractDoNotMatch = new AmountAndContractNumber(0, "???");

			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var shipment = CreateBillOfLading();

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA" });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { ZString.Empty });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: null,
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: null);

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { contractDoNotMatch.ContractNumber },
				expectedAmounts: Enumerable.Empty<AmountAndContractNumber>(),
				expectedJobContractNumbers: new[] { contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", "BBB" },
				expectedAmounts: new[] { contractAAA, contractBBB },
				expectedJobContractNumbers: new ZString[] { "AAA", "BBB" });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", ZString.Empty },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA", contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty, contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { ZString.Empty, contractDoNotMatch.ContractNumber });
		}

		public void TestContractNumbers_Revenue_PopulateYes_ReplaceNo()
		{
			// Given:
			// - Client rate with entries differentiated by contract numbers
			// - Liner & Agency Carrier Contract Number registry item PopulateContractNumbersFromRevenueRates set to Yes
			// - Liner & Agency Carrier Contract Number registry item ReplaceExistingContractNumbersWithNewNumbers set to No
			// When:
			// - Auto Revenue bill of lading
			// Then:
			// - Populate all Contract Numbers of the applicable rates to be loaded
			// - If Booking / BOL has a record of CON with blank value, the registry â€˜Populate contract numbers from revenue ratesâ€™ is NOT applicable.

			var ratingHeader = Helper.NewClientRate(Consignee);
			var contractAAA = new AmountAndContractNumber(100, "aaa");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBBB = new AmountAndContractNumber(200, "bbb");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBBB.ChargeCode, contractBBB.Amount, contractNumber: contractBBB.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			var contractDoNotMatch = new AmountAndContractNumber(0, "???");

			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var shipment = CreateBillOfLading();

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: null,
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { contractDoNotMatch.ContractNumber },
				expectedAmounts: Enumerable.Empty<AmountAndContractNumber>(),
				expectedJobContractNumbers: new[] { contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", "BBB" },
				expectedAmounts: new[] { contractAAA, contractBBB },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", contractBlank.ContractNumber },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { ZString.Empty, contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { contractBlank.ContractNumber, contractDoNotMatch.ContractNumber });
		}

		public void TestContractNumbers_Revenue_ReplaceYes()
		{
			// Given:
			// - Client rate with entries differentiated by contract numbers
			// - Liner & Agency Carrier Contract Number registry item ReplaceExistingContractNumbersWithNewNumbers set to Yes
			// When:
			// - Auto Revenue bill of lading
			// Then:
			// - Carrier Contract Number is NOT checked/matched against Rates
			// - Populate Contract Numbers of matching rates - Does not matter of the value of PopulateContractNumbersFromRevenueRates

			var ratingHeader = Helper.NewClientRate(Consignee);
			var contractAAA = new AmountAndContractNumber(100, "aaa");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBBB = new AmountAndContractNumber(200, "bbb");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBBB.ChargeCode, contractBBB.Amount, contractNumber: contractBBB.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			var contractDoNotMatch = new AmountAndContractNumber(0, "???");

			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var shipment = CreateBillOfLading();

			// isPopulateContractNumbersFromRevenueRates: false,
			// isReplaceExistingContractNumbersWithNewNumbers: true,
			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: null,
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			// isPopulateContractNumbersFromRevenueRates is set to true but the result should be the same as above
			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: null,
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: false,
				shipment,
				presetJobContractNumbers: new[] { contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBBB.ContractNumber, contractBlank.ContractNumber });
		}

		public void TestContractNumbers_Cost()
		{
			// Given:
			// - Costings with entries differentiated by contract numbers
			// When:
			// - Auto Cost bill of lading
			// Then:
			// - Create charges for only rates matching job contract numbers
			// - Rates with blank contract number are always accepted but if the same charge exists with contract number then it takes priority

			var ratingHeader = Helper.NewCosting(null);
			var contractAAA = new AmountAndContractNumber(100, "aaa");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBBB = new AmountAndContractNumber(200, "bbb");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBBB.ChargeCode, contractBBB.Amount, contractNumber: contractBBB.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			var contractDoNotMatch = new AmountAndContractNumber(0, "???");

			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			var shipment = CreateBillOfLading();

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA" });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { ZString.Empty });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: null,
				expectedAmounts: new[] { contractAAA, contractBBB, contractBlank },
				expectedJobContractNumbers: null);

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new[] { contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", "BBB" },
				expectedAmounts: new[] { contractAAA, contractBBB },
				expectedJobContractNumbers: new ZString[] { "AAA", "BBB" });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", ZString.Empty },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new[] { contractAAA.ContractNumber, contractBlank.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA", contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA", contractDoNotMatch.ContractNumber });

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: true,
				isReplaceExistingContractNumbersWithNewNumbers: true,
				isCost: true,
				shipment,
				presetJobContractNumbers: new[] { ZString.Empty, contractDoNotMatch.ContractNumber },
				expectedAmounts: new[] { contractBlank },
				expectedJobContractNumbers: new[] { ZString.Empty, contractDoNotMatch.ContractNumber });
		}

		struct AmountAndContractNumber
		{
			public AmountAndContractNumber(ZDecimal amount, string contractNumber, string chargeCode = "FRT")
			{
				Amount = amount;
				ContractNumber = contractNumber;
				ChargeCode = chargeCode;
			}

			public ZDecimal Amount { get; }
			public ZString ContractNumber { get; }
			public ZString ChargeCode { get; }
		}

		void AutorateAndAssertContractNumbers(
			bool isPopulateContractNumbersFromRevenueRates,
			bool isReplaceExistingContractNumbersWithNewNumbers,
			bool isCost,
			BillOfLading shipment,
			IEnumerable<ZString> presetJobContractNumbers,
			IEnumerable<AmountAndContractNumber> expectedAmounts,
			IEnumerable<ZString> expectedJobContractNumbers)
		{
			using (AgencyRegistry.Instance.PopulateContractNumbersFromRevenueRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isPopulateContractNumbersFromRevenueRates))
			using (AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isReplaceExistingContractNumbersWithNewNumbers))
			{
				shipment.Numbers.RemoveAndDeleteAll();
				if (presetJobContractNumbers != null)
				{
					foreach (var jobContractNumber in presetJobContractNumbers)
					{
						var number = shipment.Numbers.AddNew();
						number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
						number.CE_EntryNum = jobContractNumber;
					}
				}

				var expectedCharges = expectedAmounts.Select(amount => new AssertionCharge
				{
					ChargeCode = amount.ChargeCode,
					JR_OSSellAmt = amount.Amount
				});

				AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: isCost, autorateRevenue: !isCost);

				var actualJobContractNumbers = shipment.Numbers
					.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
					.Select(x => x.ToUpperInvariant());

				if (expectedJobContractNumbers == null)
				{
					AssertEquals(0, actualJobContractNumbers.Count());
				}
				else
				{
					var expectedContractNumbers = expectedJobContractNumbers.Select(x => x.ToUpperInvariant());
					AssertContainsExactElementsInAnyOrder(expectedContractNumbers, actualJobContractNumbers);
				}
			}
		}

		public void TestAutorateBillOfLading_NoRate_ContractNumbersShouldNotBeCleared()
		{
			var shipment = CreateBillOfLading();

			using (AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.Numbers.RemoveAndDeleteAll();

				var number1 = shipment.Numbers.AddNew();
				number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				number1.CE_EntryNum = "CON1";
				var number2 = shipment.Numbers.AddNew();
				number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				number2.CE_EntryNum = "CON2";

				AutorateAndAssert(Enumerable.Empty<AssertionCharge>(), shipment, Consignee);

				AssertContainsExactElementsInAnyOrder(
					"Contract numbers should remain unchanged after autorating with no rate.",
					new[] { number1, number2 },
					shipment.Numbers
				);
			}
		}

		public void TestAutorateBillOfLading_TwoChargeCodesWithDifferentContractNumbers_ShouldFilterBlank_ClientRate()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);
			var contractAAA = new AmountAndContractNumber(100, "aaa");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "", "BAF");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			var shipment = CreateBillOfLading();

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA" });
		}

		public void TestAutorateBillOfLading_TwoChargeCodesWithDifferentContractNumbers_ShouldFilterBlank_CompanyTariff()
		{
			var globalCharge1 = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT1");
			var globalCharge2 = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT2");
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Factory.Save();

			var companyTariff = Helper.NewGlobalTariff();
			var contractAAA = new AmountAndContractNumber(100, "aaa", globalCharge1.AC_Code);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractAAA.ChargeCode, contractAAA.Amount, contractNumber: contractAAA.ContractNumber);

			var contractBlank = new AmountAndContractNumber(300, "", globalCharge2.AC_Code);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, "AU", "", contractBlank.ChargeCode, contractBlank.Amount, contractNumber: contractBlank.ContractNumber);

			companyTariff.Factory.Save();

			var shipment = CreateBillOfLading();

			AutorateAndAssertContractNumbers(
				isPopulateContractNumbersFromRevenueRates: false,
				isReplaceExistingContractNumbersWithNewNumbers: false,
				isCost: false,
				shipment,
				presetJobContractNumbers: new ZString[] { "AAA" },
				expectedAmounts: new[] { contractAAA },
				expectedJobContractNumbers: new ZString[] { "AAA" });
		}

		#endregion

		#region Implementations

		BillOfLading CreateBillOfLading()
		{
			var shipment = Factory.NewWithValidTestData<BillOfLading>();
			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_ActualWeight = 500;
			shipment.JS_ActualVolume = 1;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Consignor.MainAddress.PK;
			shipment.ConsigneePK = Consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;

			return shipment;
		}

		#endregion
	}
}
