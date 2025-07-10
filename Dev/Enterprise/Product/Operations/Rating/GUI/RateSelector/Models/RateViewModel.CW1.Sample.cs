#region SuppressResourceStringsCheckRegion
using System;
using System.Collections.Generic;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class CW1RateViewModelSample : CW1RateViewModel
	{
		public CW1RateViewModelSample()
		{
			ServiceProviderCode = "WW_EMIRS";
			ServiceProviderName = "Emirates";
			ServiceLevel = "EXP";
			Consignee = "KKKLLL";
			Consignor = "BBBCCC";
			TransitTime = "SMD";
			CarrierCode = "WW_QNTAS";
			CarrierName = "Qantas";
			CarrierServiceLevel = "STD";
			ContractNumber = "ABCD123456";
			Commodities = "General";
			CommodityGroups = new List<string>() { "GENL " };
			PaymentTerms = "Prepaid";
			Origin = "AUSYD";
			Via = "SGSIN";
			Destination = "UAIEV";
			StartDate = new DateTime(2020, 01, 01);
			ExpiryDate = new DateTime(2020, 06, 01);
			TotalPriceString = "1,666.00 AUD";
			DisplayPrice = true;
			IsExpanded = true;
			Lines = new List<IRateLine>();

			FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "FRT",
				Amount = 666,
				Currency = "USD",
				LocalAmount = 1000,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Freight",
				IsSelected = true,
				DisplayPrice = DisplayPrice
			});

			FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "XXX",
				ChargeCodeDescription = "XXX Description Looooooooonggg Description",
				IsSelected = true,
				IsIncluded = true,
				DisplayPrice = DisplayPrice
			});

			FreightCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "YYY",
				ChargeCodeDescription = "YYY Description",
				IsSelected = true,
				IsIncluded = true,
				DisplayPrice = DisplayPrice
			});

			OtherCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "BAF",
				Amount = 500,
				Currency = "AUD",
				LocalAmount = 500,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Bunker",
				IsSelected = true,
				DisplayPrice = DisplayPrice
			});

			OtherCharges.Add(new ChargeViewModelSample
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
				IsSelected = true,
				DisplayPrice = DisplayPrice
			});
		}

		public new bool DisplayPrice { get; set; }
		public new string TotalPriceString { get; set; }
	}
}
#endregion
