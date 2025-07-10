using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business
{
	sealed class SGBillApportionmentCalculator
	{
		public SGBillApportionmentCalculator(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			GatherApportionmentData();
		}

		public void Calculate()
		{
			if (bill.ApportionmentDirty && packsData != null && packsData.Count > 0)
			{
				var totalPackLinePriceInSGD = packsData?.Values.Sum(x => x) ?? 0m;
				var shouldBeZeroRatio = totalPackLinePriceInSGD == ZDecimal.Zero;

				var totalCustomValue = ZDecimal.Zero;
				var totalDutyAmount = ZDecimal.Zero;
				var totalTaxAmount = ZDecimal.Zero;
				var totalAdditionalValue = ZDecimal.Zero;
				if (isImport)
				{
					totalAdditionalValue = GetValueInSGD(bill.OtherChargesValue, bill.RefOtherChargesValueCurrency) + GetValueInSGD(bill.ABL_TransportValue, bill.TransportValueCurrency) + GetValueInSGD(bill.ABL_InsuranceValue, bill.InsuranceValueCurrency) - GetValueInSGD(bill.DiscountValue, bill.RefDiscountValueCurrency);
				}
				else if (isExport)
				{
					totalAdditionalValue = GetValueInSGD(bill.OtherChargesValue, bill.RefOtherChargesValueCurrency) - GetValueInSGD(bill.DiscountValue, bill.RefDiscountValueCurrency);
				}
				var shouldUpdateCustomsValue = isImport || isExport;
				var listWithLinePrice = new List<(AsycudaPack pack, ZDecimal linePrice)>();
				foreach (var packData in packsData)
				{
					if (shouldBeZeroRatio || packData.Value.IsEmpty)
					{
						(var customValue, var dutyAmount, var taxAmount) = ApportionmentOnPack(packData.Key, 0m, shouldUpdateCustomsValue);
						totalCustomValue += customValue;
						totalDutyAmount += dutyAmount;
						totalTaxAmount += taxAmount;
					}
					else
					{
						listWithLinePrice.Add((packData.Key, packData.Value));
					}
				}
				var total = listWithLinePrice.Count;
				if (total > 0)
				{
					var totalValue = ZDecimal.Zero;
					var i = 0;
					while (i < total)
					{
						(var pack, var linePrice) = listWithLinePrice[i++];
						var additionalValue = i == total ? totalAdditionalValue - totalValue : Utilities.Round(totalAdditionalValue * linePrice / totalPackLinePriceInSGD, 2);
						totalValue += additionalValue;
						(var customValue, var dutyAmount, var taxAmount) = ApportionmentOnPack(pack, linePrice + additionalValue, shouldUpdateCustomsValue);
						totalCustomValue += customValue;
						totalDutyAmount += dutyAmount;
						totalTaxAmount += taxAmount;
					}
				}
				StatisticsValuesFromPacks(totalCustomValue, totalDutyAmount, totalTaxAmount);
			}

			bill.ApportionmentDirty = false;
		}

		ZDecimal GetValueInSGD(ZDecimal amount, RefCurrency currency)
		{
			return amount.IsEmpty || currency == null ? ZDecimal.Zero : currency.ConvertUsingCustomsRate(dateForRate, amount, sgd);
		}

		#region Apportionment

		void GatherApportionmentData()
		{
			var header = bill.Header;
			if (header != null)
			{
				isImport = header.IsImport;
				isExport = header.IsExport;

				dateForRate = header.ValuationDate;
				sgd = RefCurrency.LoadFromCurrencyCode(bill.Factory, Core.Constants.CurrencyCodes.Singapore);

				packsData = new Dictionary<AsycudaPack, ZDecimal>();

				foreach (var pack in bill.Packs.OfType<AsycudaPack>())
				{
					var linePriceCurrency = pack.RefLinePriceCurrency;
					var linePrice = pack.LinePrice;

					if (linePriceCurrency == null)
					{
						linePrice = ZDecimal.Zero;
					}
					else if (linePriceCurrency.RX_Code != Core.Constants.CurrencyCodes.Singapore)
					{
						var cC = CurrencyConverter.New(bill.Factory, dateForRate, ZArchitecture.Core.ExchangeRateType.Customs, 0);
						linePrice = cC.ConvertExact(new Money(linePrice, linePriceCurrency), sgd, false).Amount;
					}

					packsData.Add(pack, linePrice);
				}
			}
		}

		readonly AsycudaBill bill;
		bool isImport;
		bool isExport;
		ZDateTime dateForRate;
		RefCurrency sgd;
		Dictionary<AsycudaPack, ZDecimal> packsData;

		(ZDecimal customValue, ZDecimal dutyAmount, ZDecimal taxAmount) ApportionmentOnPack(AsycudaPack pack, ZDecimal newCustomsValue, bool shouldUpdateCustomsValue)
		{
			var customValue = ZDecimal.Zero;
			var dutyAmount = ZDecimal.Zero;
			var taxAmount = ZDecimal.Zero;

			var packedItem = pack.PackedItem;
			if (packedItem != null)
			{
				if (shouldUpdateCustomsValue)
				{
					packedItem.SetCustomsValue(Math.Max(ZDecimal.Zero, newCustomsValue));
				}

				customValue = packedItem.API_CustomsValue;
				dutyAmount = packedItem.API_DutyAmount;
				taxAmount = packedItem.API_TaxAmount;
			}
			return (customValue, dutyAmount, taxAmount);
		}

		#endregion

		#region Statistics

		void StatisticsValuesFromPacks(ZDecimal totalCustomValue, ZDecimal totalDutyAmount, ZDecimal totalTaxAmount)
		{
			bill.ABL_CustomsValue = totalCustomValue.Round(2);
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Singapore;

			if (isImport)
			{
				bill.DutyAmount = totalDutyAmount.Round(2);
			}
			bill.TaxAmount = totalTaxAmount.Round(2);
		}

		#endregion
	}
}
