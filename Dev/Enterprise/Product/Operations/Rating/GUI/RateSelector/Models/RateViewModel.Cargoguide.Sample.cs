#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class CargoguideRateViewModelSample : CargoguideRateViewModel
	{
		public CargoguideRateViewModelSample(bool includeSubjectCharges = true)
		{
			DisplayPrice = true;
			CarrierCode = "WW_QNTAS";
			CarrierName = "Qantas Looooooooooooooooooooooooong Name";
			CarrierServiceLevel = "STD";
			CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
			ContractNumber = "ABCD123456";
			Commodities = "Commodities very long list of commodities";
			CarrierCommodities = "VITAL - CLINICAL TRAILS / CELL & GENE THERAPY / HUMAN TISSUE & BIOLOGICAL SAMPLES / ORGANS";
			CommodityGroupErrorLevel = ErrorLevel.Warning;
			Deck = "Upper";
			PaymentTerms = "Prepaid";
			Origin = "AUSYD";
			Destination = "UAIEV";
			StartDate = new DateTime(2020, 01, 01);
			ExpiryDate = new DateTime(2020, 06, 01);
			IssueDate = new DateTime(2019, 12, 01);
			Remarks = "McLaren Looooooooooooooooooooonggggggggggggg Remarks";
			Ratio = "1:6";
			Reference = "Reference XYZ";
			Currency = "USD";
			TotalPriceString = "1,666.00 AUD";
			IsExpanded = true;
			CommodityGroups = new List<string>() { "Personal effects" };
			ContainerPayloadWeight = "600 KG";
			ContainerPayloadVolume = "4.3 M3";
			ContainerPivotWeight = "200.66 KG";

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

			if (includeSubjectCharges)
			{
				SubjectToCharges.Add(new ChargeViewModelSample
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

				SubjectToCharges.Add(new ChargeViewModelSample
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

			OptionalCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "WAR",
				Amount = 100,
				Currency = "EUR",
				LocalAmount = 150,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "War",
				IsOptional = true,
				IsSelected = false,
				DisplayPrice = DisplayPrice
			});

			OptionalCharges.Add(new ChargeViewModelSample
			{
				ChargeCode = "YIP",
				Amount = 200,
				Currency = "EUR",
				LocalAmount = 245,
				LocalCurrency = "AUD",
				ChargeCodeDescription = "Yip",
				IsOptional = true,
				IsSelected = false,
				DisplayPrice = DisplayPrice
			});

			RawRateJson = "raw rate";
			CargoguideRawRateJson = "raw rate";
		}

		public new bool DisplayPrice { get; set; }
		public new string TotalPriceString { get; set; }
		public new string RawRateJson { get; set; }
	}
}

#endregion
