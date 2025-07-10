using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class ChargesCollection : IChargesCollection
	{
		#region Constructor

		public ChargesCollection(AgencyShipment shipment, AgencyHouseBillLookups lookups, bool isOriginal, bool isHBL = true)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.lookups = Argument.NotNull(lookups, nameof(lookups));

			this.isOriginal = isOriginal;
			this.isHBL = isHBL;

			charges = new Lazy<ChargeLine[]>(CreateCharges);
			jobCharges = new Lazy<JobCharge[]>(GetJobCharges);
			chargesToExcludeByDefault = new Lazy<HashSet<ZGuid>>(GetExcludedCharges);
			houseBillCharges = new AgencyHouseBillCharges(shipment);
		}

		readonly AgencyShipment shipment;
		readonly bool isOriginal;
		readonly bool isHBL;
		readonly AgencyHouseBillLookups lookups;
		readonly Lazy<ChargeLine[]> charges;
		readonly Lazy<JobCharge[]> jobCharges;
		readonly Lazy<HashSet<ZGuid>> chargesToExcludeByDefault;
		readonly AgencyHouseBillCharges houseBillCharges;

		#endregion

		#region LumpSum

		public ZBool ShowAsLumpSum => showAsLumpSum ?? (showAsLumpSum = ShouldShowChargesAsLumpSum()).Value;
		ZBool? showAsLumpSum;

		bool ShouldShowChargesAsLumpSum()
		{
			if (shipment.JS_RL_NKDestination.IsEmpty)
			{
				return false;
			}

			var query = new ZQuery(RefCountrySchema.RN_Code, shipment.JS_RL_NKDestination.SubstringSafe(0, 2));
			var country = shipment.Factory.LoadTop1<RefCountry>(query);

			return country != null
				&& DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.Value.Contains(country.PK.ToGuid());
		}

		public IMoney LumpSum => lumpSum ?? (lumpSum = CalculateLumpSum());
		IMoney lumpSum;

		IMoney CalculateLumpSum()
		{
			var currency = new CodeDescription(lookups.Currencies)
			{
				Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
			};

			var result = new Money
			{
				Currency = currency
			};

			if (FilteredCharges.Length == 0)
			{
				return new LumpSum(result);
			}

			var currencyCode = FilteredCharges
				.First()
				.Sell.Currency.Code;

			var allChargesInSameCurrency = FilteredCharges
				.Skip(1)
				.All(ch => ch.Sell.Currency.Code == currencyCode);

			if (!allChargesInSameCurrency)
			{
				currency.Code = Core.Constants.CurrencyCodes.UnitedStates;
				var usd = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency.Code);

				var converter = GetCurrencyConverter();

				result.Amount = FilteredCharges
					.Sum(ch =>
					{
						var refCurrency = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ch.Sell.Currency.Code);
						var money = new MasterFiles.Business.Money(ch.Sell.Amount, refCurrency);
						return converter.ConvertRounded(money, usd).Amount;
					});
			}
			else
			{
				currency.Code = currencyCode;
				result.Amount = FilteredCharges.Sum(ch => ch.Sell.Amount);
			}

			return new LumpSum(result);
		}

		CurrencyConverter GetCurrencyConverter()
		{
			return shipment?.ShipmentJobHeader?.CurrencyConverter
				?? CurrencyConverter.New(shipment.Factory, ZDateTime.Now, ExchangeRateType.Sell, 1);
		}

		#endregion

		#region Hide

		public ZBool Hide => shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;

		#endregion

		#region ShowAsAgreed

		public ZBool ShowAsAgreed => showAsAgreed ?? (showAsAgreed = GetShowAsAgreed()).Value;
		bool? showAsAgreed;

		bool GetShowAsAgreed()
		{
			return shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed
				|| isOriginal && (shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges
					|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges
					|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges);
		}

		#endregion

		#region ShowCollect

		public ZBool ShowCollect => showCollect ?? (showCollect = GetShowCollectCharges()).Value;
		bool? showCollect;

		bool GetShowCollectCharges()
		{
			return shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges
				|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges
				|| !isOriginal && (shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges
					|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges);
		}

		#endregion

		#region ShowPrepaid

		public ZBool ShowPrepaid => showPrepaid ?? (showPrepaid = GetPrepaidCharges()).Value;
		bool? showPrepaid;

		bool GetPrepaidCharges()
		{
			return shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges
				|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges
				|| !isOriginal && (shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges
					|| shipment.JS_HBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges);
		}

		#endregion

		#region Create charges data objects

		HashSet<ZGuid> GetExcludedCharges()
		{
			var allCharges = jobCharges.Value.OfType<JobCharge>();
			var appliedReversals = new HashSet<ZGuid>();

			var excludedCharges = allCharges
				.Where(charge =>
					charge.JR_OH_SellAccount == ZGuid.Empty ||
					charge.JR_OSSellAmt <= 0 ||
					HasReversal(charge, allCharges, ref appliedReversals)
				)
				.Select(charge => charge.PK)
				.ToHashSet();

			return excludedCharges;
		}

		bool HasReversal(JobCharge targetCharge, IEnumerable<JobCharge> charges, ref HashSet<ZGuid> appliedReversals)
		{
			foreach (var charge in charges)
			{
				if (charge.ChargeCode == targetCharge.ChargeCode
					&& charge.CostCurrency == targetCharge.CostCurrency
					&& charge.JR_OSSellAmt + targetCharge.JR_OSSellAmt == 0
					&& charge.JR_GC == targetCharge.JR_GC
					&& !appliedReversals.Contains(charge.PK))
				{
					appliedReversals.Add(charge.PK);
					return true;
				}
			}

			return false;
		}

		JobCharge[] GetJobCharges()
		{
			var jobCharges = new List<JobCharge>();
			jobCharges.AddRange(houseBillCharges.GetChargesAtOrigin());

			if (houseBillCharges.PrintChargesBilledToLocalClientAtDestAsCollect)
			{
				jobCharges.AddRange(houseBillCharges.GetChargesAtDestination(false));
			}

			return jobCharges.ToArray();
		}

		ChargeLine[] CreateCharges()
		{
			var chargeLines = new List<ChargeLine>();
			var revenueCharges = houseBillCharges.GetCharges(isHBL);

			foreach (var charge in revenueCharges)
			{
				var isPrepaid = houseBillCharges.JobHeaderAtOrigin != null && charge.JR_OH_SellAccount == houseBillCharges.JobHeaderAtOrigin.LocalChargesPK;
				chargeLines.Add(CreateChargeLine(charge, isPrepaid));
			}

			return chargeLines.ToArray();
		}

		ChargeLine CreateChargeLine(JobCharge jobCharge, bool isPrepaid)
		{
			var localCurrency = (jobCharge.Company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency;

			return new ChargeLine(jobCharge.PK)
			{
				Description = jobCharge.JR_Desc,
				IsPrepaid = isPrepaid,
				Cost = new Money
				{
					Amount = jobCharge.JR_OSCostAmt,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = jobCharge.JR_RX_NKCostCurrency
					}
				},
				CostExchangeRate = jobCharge.JR_OSCostExRate,
				Sell = new Money
				{
					Amount = jobCharge.JR_OSSellAmt,
					Currency = new CodeDescription(jobCharge.Lookups.SellCurrencies)
					{
						Code = jobCharge.JR_RX_NKSellCurrency
					}
				},
				SellExchangeRate = jobCharge.JR_OSSellExRate,
				LocalCost = new Money
				{
					Amount = jobCharge.JR_LocalCostAmt,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = localCurrency
					}
				},
				LocalSell = new Money
				{
					Amount = jobCharge.JR_LocalSellAmt,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = localCurrency
					}
				},
				CFX = jobCharge.JR_LineCFX,
				PaymentBases = new PaymentBasisCollection(jobCharge),
				ChargeCode = new CodeDescription(jobCharge.Lookups.ChargeCodes)
				{
					Code = jobCharge.ChargeCode?.AC_Code ?? ZString.Empty,
				},
				ChargeLineAttributes = jobCharge.JobChargeAttributes.Select(jobChargeAttribute => CreateChargeLineAttribute(jobChargeAttribute)).ToArray()
			};
		}

		ChargeLineAttribute CreateChargeLineAttribute(JobChargeAttrib jobChargeAttrib)
		{
			return new ChargeLineAttribute()
			{
				Name = jobChargeAttrib.EC_Name,
				Value = jobChargeAttrib.EC_Value,
				Amount = jobChargeAttrib.EC_Amount
			};
		}

		#endregion

		#region FilteredCharges

		ChargeLine[] FilteredCharges => filteredCharges ?? (filteredCharges = GetFilteredCharges());
		ChargeLine[] filteredCharges;

		ChargeLine[] GetFilteredCharges()
		{
			if (Hide)
			{
				return Array.Empty<ChargeLine>();
			}

			return charges
				.Value
				.Where(ch =>
					!chargesToExcludeByDefault.Value.Contains(new ZGuid(ch.Identifier))
					&& (ShowPrepaid && ch.IsPrepaid || ShowCollect && !ch.IsPrepaid))
				.ToArray();
		}

		#endregion

		#region IReadOnlyCollection<IChargeLine> members

		public int Count => FilteredCharges.Length;

		#endregion

		#region IEnumerable members

		public IEnumerator<IChargeLine> GetEnumerator() => FilteredCharges.Cast<IChargeLine>().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region All

		public IReadOnlyCollection<IChargeLine> All => charges.Value;

		#endregion
	}
}
