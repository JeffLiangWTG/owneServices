using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class ChargesSummaryItem : IComparable
	{
		public ChargesSummaryItem(ZString type, ZDecimal @break, ZDecimal value, ZDecimal flatAmount)
		{
			this.Type = type;
			this.Break = @break;
			fValue = value;
			fFlatAmount = flatAmount;
		}

		public ChargesSummaryItem(ZString type, ZDecimal value)
			: this(type, 0m, value, 0m)
		{
		}

		public ChargesSummaryItem(ChargesSummaryItem item)
			: this(item.Type, item.Break, item.Value, item.FlatAmount)
		{
		}

		#region Properties

		public readonly ZString Type;
		public readonly ZDecimal Break;

		public ZDecimal Value
		{
			get { return fValue; }
		}

		ZDecimal fValue;

		public ZDecimal FlatAmount
		{
			get { return fFlatAmount; }
		}

		ZDecimal fFlatAmount;

		#endregion

		public ZString ColumnCaption
		{
			get
			{
				switch (Type)
				{
					case Calculator.Items.Operator.MIN:
						return Res.GetString("4c9c0a98-8648-47fc-9e5c-73bc4b2ce58a", "Min");

					case Calculator.Items.Operator.BAS:
						return RatingDataRegistry.Instance.BaseRateText.Value;

					case Calculator.Items.Operator.UNT:
						return Res.GetString("74ccc787-bf7d-405d-b35f-97b4ffbc9b6e", "Per Unit");

					default:
						return Type + Break.ToString("f0");
				}
			}
		}

		public ZString GetStringValue(RefCurrency currencyToFormat)
		{
			var formatMask = currencyToFormat != null
									? string.Format((NoResString)"f{0}", currencyToFormat.Decimals) // string format mask
									: "f2";

			if (!Value.IsEmpty && !FlatAmount.IsEmpty)
			{
				return String.Format("{0}/{1}", Value.ToString(formatMask), FlatAmount.ToString(formatMask));
			}

			if (!Value.IsEmpty)
			{
				return Value.ToString(formatMask);
			}

			if (!FlatAmount.IsEmpty)
			{
				return FlatAmount.ToString(formatMask);
			}

			return ZString.Empty;
		}

		public ZDecimal GetDecimalValue(RefCurrency currencyToFormat)
		{
			if (!Value.IsEmpty)
			{
				return Value;
			}

			if (!FlatAmount.IsEmpty)
			{
				return FlatAmount;
			}

			return ZDecimal.Zero;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ Break.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return CompareTo(obj) == 0;
		}

		public static ChargesSummaryItem operator +(ChargesSummaryItem item1, ChargesSummaryItem item2)
		{
			return new ChargesSummaryItem(item1.Type, item1.Break, item1.Value + item2.Value, item1.FlatAmount + item2.FlatAmount);
		}

		public bool Convert(CurrencyConverter currencyConverter, RefCurrency from, RefCurrency to)
		{
			var newValue = Value;
			if (!Value.IsEmpty)
			{
				newValue = Convert(currencyConverter, from, to, Value);
				if (newValue.IsEmpty)
				{
					return false;
				}
			}

			var newFlatAmount = FlatAmount;
			if (!FlatAmount.IsEmpty)
			{
				newFlatAmount = Convert(currencyConverter, from, to, FlatAmount);
				if (newFlatAmount.IsEmpty)
				{
					return false;
				}
			}

			fValue = newValue;
			fFlatAmount = newFlatAmount;

			return true;
		}

		ZDecimal Convert(CurrencyConverter currencyConverter, RefCurrency from, RefCurrency to, ZDecimal amount)
		{
			return currencyConverter.ConvertExact(new Money(amount, from), to).Amount;
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			var item2 = obj as ChargesSummaryItem;

			var types = new List<string>(new string[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus });
			var typeIndex1 = types.IndexOf(Type);
			var typeIndex2 = types.IndexOf(item2.Type);
			var result = typeIndex1.CompareTo(typeIndex2);
			if (result == 0)
			{
				result = Break.CompareTo(item2.Break);
			}

			return result;
		}

		#endregion
	}
}

