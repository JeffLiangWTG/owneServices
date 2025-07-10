using System;
using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[CodeAlive("Used in xaml designer")]
	public class NonContainerizedRatesViewModelSample : NonContainerizedRatesViewModel
	{
		public NonContainerizedRatesViewModelSample()
		{
			#region SuppressResourceStringsCheckRegion

			var displayPrice = true;
			var rates = (IList<RateViewModel>)Rates;

			rates.Add(new CW1RateViewModelSample()
			{
				ServiceProviderCode = "WW_LUFTHS",
				ServiceProviderName = "Lufthansa",
				ServiceLevel = "STD",
				Consignee = "QQQWWW",
				Consignor = "BBBCCC",
				TransitTime = "OWN",
				CarrierCode = "WW_QNTAS",
				CarrierName = "Qantas",
				CarrierServiceLevel = "STD",
				ContractNumber = "ABCD123456",
				Commodities = "General",
				PaymentTerms = "Prepaid",
				Origin = "AUSYD",
				Destination = "UAIEV",
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2020, 06, 01),
				Currency = "AUD",
				DisplayPrice = displayPrice,
				IsExpanded = false
			});

			rates.Add(new CW1RateViewModelSample()
			{
				ServiceProviderCode = "WW_EMIRS",
				ServiceProviderName = "Emirates",
				ServiceLevel = "EXP",
				Consignee = "KKKLLL",
				Consignor = "BBBCCC",
				TransitTime = "SMD",
				CarrierCode = "WW_QNTAS",
				CarrierName = "Qantas",
				CarrierServiceLevel = "STD",
				ContractNumber = "ABCD123456",
				Commodities = "General",
				PaymentTerms = "Prepaid",
				Origin = "AUSYD",
				Via = "SGSIN",
				Destination = "UAIEV",
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2020, 06, 01),
				DisplayPrice = displayPrice,
				IsExpanded = false
			});

			var rateWithCharges = new CargoguideRateViewModelSample
			{
				CarrierCode = "WW_QNTAS",
				CarrierName = "Qantas Looooooooooooooooooooooooong Name",
				CarrierServiceLevel = "STD",
				ContractNumber = "ABCD123456",
				Commodities = "General",
				Deck = "Upper",
				PaymentTerms = "Prepaid",
				Origin = "AUSYD",
				Destination = "UAIEV",
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2020, 06, 01),
				IssueDate = new DateTime(2019, 12, 01),
				Remarks = "McLaren Looooooooooooooooooooonggggggggggggg Remarks",
				Ratio = "1:6",
				Currency = "USD",
				DisplayPrice = displayPrice,
				TotalPriceString = "AUD 1,666.55",
				IsExpanded = false,
				CommodityGroups = new List<string>() { "Personal effects" },
			};

			rateWithCharges.FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "FRT",
				Amount = 666,
				Currency = "USD",
				LocalAmount = 1000,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Freight",
				DisplayPrice = displayPrice
			});

			rateWithCharges.FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "INC1",
				IsIncluded = true,
				DisplayPrice = displayPrice
			});

			rateWithCharges.FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "INC2",
				IsIncluded = true,
				DisplayPrice = displayPrice
			});

			rateWithCharges.FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "INC3",
				IsIncluded = true,
				DisplayPrice = displayPrice
			});

			rateWithCharges.FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "INC4",
				IsIncluded = true,
				DisplayPrice = displayPrice
			});

			rateWithCharges.SubjectToCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "BAF",
				Amount = 500,
				Currency = "AUD",
				LocalAmount = 500,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Bunker",
				DisplayPrice = displayPrice
			});

			rateWithCharges.SubjectToCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "ABCD",
				ChargeCodeError = "No mapping to universal code",
				ChargeCodeErrorLevel = ErrorLevel.Error,
				Amount = 100,
				Currency = "BTC",
				LocalAmount = 0,
				LocalCurrency = "AUD",
				LocalAmountError = "No exchange rate",
				ChargeCodeDescription = "Some carrier charge code",
				DisplayPrice = displayPrice
			});

			rateWithCharges.OptionalCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "BAF",
				Amount = 500,
				Currency = "AUD",
				LocalAmount = 500,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Bunker",
				DisplayPrice = displayPrice,
				IsSelected = false
			});

			rateWithCharges.OptionalCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "ABCD",
				ChargeCodeError = "No mapping to universal code",
				ChargeCodeErrorLevel = ErrorLevel.Error,
				Amount = 100,
				Currency = "BTC",
				LocalAmount = 0,
				LocalCurrency = "AUD",
				LocalAmountError = "No exchange rate",
				ChargeCodeDescription = "Some carrier charge code",
				DisplayPrice = displayPrice
			});

			rates.Add(rateWithCharges);

			rates.Add(new CargoguideRateViewModelSample
			{
				CarrierCode = "WW_SINAIR",
				CarrierName = "Singapore Airlines",
				CarrierServiceLevel = "EXP",
				ContractNumber = "XXXXX666666",
				Commodities = "Automotive",
				Deck = "Lower",
				PaymentTerms = "Collect",
				Origin = "AUSYD",
				Destination = "UAIEV",
				StartDate = new DateTime(2020, 02, 01),
				ExpiryDate = new DateTime(2020, 08, 01),
				IssueDate = new DateTime(2019, 10, 01),
				Remarks = "MU",
				Ratio = "1:6",
				DisplayPrice = displayPrice,
				TotalPriceString = "AUD 2,450.99",
				IsExpanded = false,
				CommodityGroups = new List<string>() { "Hazardous Goods (Non-Explosives)" },
			});

			rates.Add(new CargoguideRateViewModelSample
			{
				CarrierCode = "WW_EMIRS",
				CarrierName = "Emirates",
				CarrierServiceLevel = "STD",
				ContractNumber = "666666999999",
				Commodities = "Humans",
				Deck = "",
				PaymentTerms = "Both",
				Origin = "AUSYD",
				Destination = "UAIEV",
				StartDate = new DateTime(2020, 03, 01),
				ExpiryDate = new DateTime(2020, 05, 01),
				IssueDate = new DateTime(2019, 12, 31),
				Remarks = "Coronavirus",
				Ratio = "",
				DisplayPrice = displayPrice,
				TotalPriceString = "AUD 3,099.00",
				IsExpanded = false,
				CommodityGroups = new List<string>() { "General Cargo" },
			});

			StatusText = "Loading rates...";
			ViewMode = ViewMode.CardView;

			#endregion
		}
	}
}
