using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class AttributeComparer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is a separator for Hash Code generation.")]
		public const string OptionalValueSeparator = "°";

		public static string GetHashCodeForConsolidation(ILineAttributes line)
		{
			var sep = OptionalValueSeparator;
			var stringBuilder = new ZStringBuilder();
			stringBuilder.Append(line.ExpiryDate.ToString());
			stringBuilder.Append(line.PackingDate.ToString());
			stringBuilder.Append(line.PartAttrib1.ToLower());
			stringBuilder.Append(line.PartAttrib2.ToLower());
			stringBuilder.Append(line.PartAttrib3.ToLower());
			if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				stringBuilder.Append(line.SerialNumber.ToLower());
			}
			stringBuilder.Append(line.AllocationKey.ToLower());

			return stringBuilder.ToStringWithDelimiterBetweenAppends(sep);
		}

		public static bool CompareWithoutBondedEntryKey(ILineAttributes source, ILineAttributes with)
		{
			return source.ExpiryDate.Equals(with.ExpiryDate) &&
				source.PackingDate.Equals(with.PackingDate) &&
				source.PartAttrib1.EqualsIgnoringCase(with.PartAttrib1) &&
				source.PartAttrib2.EqualsIgnoringCase(with.PartAttrib2) &&
				source.PartAttrib3.EqualsIgnoringCase(with.PartAttrib3) &&
				source.SerialNumber.EqualsIgnoringCase(with.SerialNumber) &&
				source.AllocationKey.EqualsIgnoringCase(with.AllocationKey);
		}

		public static bool Compare(ILineAttributes source, ILineAttributes with)
			=> CompareWithoutBondedEntryKey(source, with) && source.BondedEntryKey.EqualsIgnoringCase(with.BondedEntryKey);

		public static bool CompareWithIsEmptyCheck(ILineAttributes src, ILineAttributes with)
		{
			return (src.BondedEntryKey.IsEmpty || src.BondedEntryKey.EqualsIgnoringCase(with.BondedEntryKey)) &&
				(src.AllocationKey.IsEmpty || src.AllocationKey.EqualsIgnoringCase(with.AllocationKey)) &&
				CompareWithIsEmptyCheck((IPartAttributes)src, with);
		}

		public static bool CompareWithIsEmptyCheck(IPartAttributes src, IPartAttributes with)
		{
			return (src.SerialNumber.IsEmpty || src.SerialNumber.EqualsIgnoringCase(with.SerialNumber)) &&
				(src.ExpiryDate.IsEmpty || src.ExpiryDate.Equals(with.ExpiryDate)) &&
				(src.PackingDate.IsEmpty || src.PackingDate.Equals(with.PackingDate)) &&
				(src.PartAttrib1.IsEmpty || src.PartAttrib1.EqualsIgnoringCase(with.PartAttrib1)) &&
				(src.PartAttrib2.IsEmpty || src.PartAttrib2.EqualsIgnoringCase(with.PartAttrib2)) &&
				(src.PartAttrib3.IsEmpty || src.PartAttrib3.EqualsIgnoringCase(with.PartAttrib3));
		}
	}

	public class CustomAttributeComparer
	{
		public string GetHashCodeForConsolidation(ILineCustomAttributes src)
		{
			var builder = new System.Text.StringBuilder();
			builder.Append(AttributeComparer.OptionalValueSeparator);
			Append(src.CustomAttrib1);
			Append(src.CustomAttrib2);
			Append(src.CustomAttrib3);
			Append(src.CustomAttrib4);
			Append(src.CustomAttrib5);
			Append(src.CustomAttrib6);
			Append(src.CustomDecimal1.ToStringTrimZeros());
			Append(src.CustomDecimal2.ToStringTrimZeros());
			Append(src.CustomDecimal3.ToStringTrimZeros());
			Append(src.CustomDecimal4.ToStringTrimZeros());
			Append(src.CustomDecimal5.ToStringTrimZeros());
			Append(src.CustomDate1.ToString());
			Append(src.CustomDate2.ToString());
			Append(src.CustomDate3.ToString());
			Append(src.CustomDate4.ToString());
			Append(src.CustomDate5.ToString());
			Append(src.CustomFlag1.ToString());
			Append(src.CustomFlag2.ToString());
			Append(src.CustomFlag3.ToString());
			Append(src.CustomFlag4.ToString());
			Append(src.CustomFlag5.ToString());
			Append(src.CustomTextBlob1);

			return builder.ToString();

			void Append(string value)
			{
				builder.Append(value);
				builder.Append(AttributeComparer.OptionalValueSeparator);
			}
		}

		public bool Compare(ILineCustomAttributes src, ILineCustomAttributes with)
		{
			return src.CustomAttrib1.Equals(with.CustomAttrib1) &&
				src.CustomAttrib2.Equals(with.CustomAttrib2) &&
				src.CustomAttrib3.Equals(with.CustomAttrib3) &&
				src.CustomAttrib4.Equals(with.CustomAttrib4) &&
				src.CustomAttrib5.Equals(with.CustomAttrib5) &&
				src.CustomAttrib6.Equals(with.CustomAttrib6) &&
				src.CustomDecimal1.Equals(with.CustomDecimal1) &&
				src.CustomDecimal2.Equals(with.CustomDecimal2) &&
				src.CustomDecimal3.Equals(with.CustomDecimal3) &&
				src.CustomDecimal4.Equals(with.CustomDecimal4) &&
				src.CustomDecimal5.Equals(with.CustomDecimal5) &&
				src.CustomDate1.Equals(with.CustomDate1) &&
				src.CustomDate2.Equals(with.CustomDate2) &&
				src.CustomDate3.Equals(with.CustomDate3) &&
				src.CustomDate4.Equals(with.CustomDate4) &&
				src.CustomDate5.Equals(with.CustomDate5) &&
				src.CustomFlag1.Equals(with.CustomFlag1) &&
				src.CustomFlag2.Equals(with.CustomFlag2) &&
				src.CustomFlag3.Equals(with.CustomFlag3) &&
				src.CustomFlag4.Equals(with.CustomFlag4) &&
				src.CustomFlag5.Equals(with.CustomFlag5) &&
				src.CustomTextBlob1.Equals(with.CustomTextBlob1);
		}
	}
}
