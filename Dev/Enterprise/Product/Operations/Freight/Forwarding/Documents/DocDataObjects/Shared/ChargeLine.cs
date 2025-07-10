using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ChargeLine : DocDataObject, IChargeLine
	{
		#region Ctor

		public ChargeLine(object id)
			: base(id)
		{
		}

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region IsPrepaid

		public ZBool IsPrepaid
		{
			get => isPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsPrepaidInfo, ref isPrepaid, value))
				{
				}
			}
		}

		ZBool isPrepaid;

		public ZPropertyInfo IsPrepaidInfo => GetZPropertyInfo(nameof(IsPrepaid));

		#endregion

		#region PaymentBasis

		public IPaymentBasisCollection PaymentBases
		{
			get => paymentBases;
			set => paymentBases = SetChild(paymentBases, value);
		}

		IPaymentBasisCollection paymentBases;

		#endregion

		#region Cost

		public IMoney Cost { get; set; }

		#endregion

		#region Sell

		public IMoney Sell { get; set; }

		#endregion

		#region LocalCost

		public IMoney LocalCost { get; set; }

		#endregion

		#region LocalSell

		public IMoney LocalSell { get; set; }

		#endregion

		#region ChargeCode

		public ICodeDescription ChargeCode
		{
			get => chargeCode;
			set => chargeCode = SetChild(chargeCode, value);
		}

		ICodeDescription chargeCode;

		#endregion

		#region SellExchangeRate

		public ZDecimal SellExchangeRate
		{
			get => sellExchangeRate;
			set
			{
				if (SetNonPersistentPropertyValue(SellExchangeRateInfo, ref sellExchangeRate, value))
				{
				}
			}
		}

		ZDecimal sellExchangeRate;

		public ZPropertyInfo SellExchangeRateInfo => GetZPropertyInfo(nameof(SellExchangeRate));

		#endregion

		#region CostExchangeRate

		public ZDecimal CostExchangeRate
		{
			get => costExchangeRate;
			set
			{
				if (SetNonPersistentPropertyValue(CostExchangeRateInfo, ref costExchangeRate, value))
				{
				}
			}
		}

		ZDecimal costExchangeRate;

		public ZPropertyInfo CostExchangeRateInfo => GetZPropertyInfo(nameof(CostExchangeRate));

		#endregion

		#region CFX

		public ZDecimal CFX
		{
			get => cfx;
			set
			{
				if (SetNonPersistentPropertyValue(CFXInfo, ref cfx, value))
				{
				}
			}
		}

		ZDecimal cfx;

		public ZPropertyInfo CFXInfo => GetZPropertyInfo(nameof(CFX));

		#endregion

		#region ChargeLineAttributes

		public IReadOnlyCollection<ChargeLineAttribute> ChargeLineAttributes
		{
			get => chargeLineAttributes;
			set => chargeLineAttributes = SetChildCollection(chargeLineAttributes, value);
		}

		IReadOnlyCollection<ChargeLineAttribute> chargeLineAttributes;

		IReadOnlyCollection<IChargeLineAttribute> IChargeLine.ChargeLineAttributes => ChargeLineAttributes;

		#endregion

	}
}
