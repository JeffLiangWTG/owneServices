using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class PaymentBasis : DocDataObject, IPaymentBasis
	{
		#region CalculationBasis

		public ZString CalculationBasis => calculationBasis ?? (calculationBasis = GetFormattedRate()).Value;

		ZString? calculationBasis;

		#endregion

		#region Quantity

		public ZString Quantity
		{
			get => quantity;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value))
				{
				}
			}
		}

		ZString quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion

		#region RateUnit

		public ICodeDescription RateUnit
		{
			get => rateUnit;
			set => rateUnit = SetChild(rateUnit, value);
		}

		ICodeDescription rateUnit;

		#endregion

		#region RateValue

		public ZDecimal RateValue
		{
			get => rateValue;
			set
			{
				if (SetNonPersistentPropertyValue(RateValueInfo, ref rateValue, value))
				{
				}
			}
		}

		ZDecimal rateValue;

		public ZPropertyInfo RateValueInfo => GetZPropertyInfo(nameof(RateValue));

		#endregion

		#region Currency

		public ICodeDescription Currency
		{
			get => currency;
			set => currency = SetChild(currency, value);
		}

		ICodeDescription currency;

		#endregion

		#region RateType

		public ICodeDescription RateType
		{
			get => rateType;
			set => rateType = SetChild(rateType, value);
		}

		ICodeDescription rateType;

		#endregion

		#region SuppressResourceStringsCheckRegion

		string GetFormattedRate()
		{
			if (IsBaseRate)
			{
				return FormattableString.Invariant($"Base Rate {Utilities.Round(RateValue, 2)} {Currency?.Code}");
			}

			if (IsPercentage)
			{
				return FormattableString.Invariant($"{Currency?.Code} {Utilities.Round(RateValue, 0)}%");
			}

			return FormattableString.Invariant($"{Utilities.Round(RateValue, 2)} {Currency?.Code}/{RateUnit?.Code}");
		}

		bool IsBaseRate => RateType != null && RateType.Code == "FLT";

		bool IsPercentage => RateType != null && RateType.Code == "PER"
			|| RateUnit != null && RateUnit.Code == "%";

		#endregion
	}
}
