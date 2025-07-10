using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class PostcodeLengthComparer : ColumnComparer
	{
		public PostcodeLengthComparer(SchemaColumn column) : base(column)
		{
			Validate(column);
		}

		public override int Compare(FastLine line1, FastLine line2)
		{
			var rate1 = line1.ParentRateEntry;
			var rate2 = line2.ParentRateEntry;

			return ComparePostcodes(
				(ZString)rate1.GetValue(column),
				(ZString)rate2.GetValue(column));
		}

		static int ComparePostcodes(ZString postcode1, ZString postcode2)
		{
			var result = CompareValues(postcode1, postcode2);

			if (result == 0 && !postcode1.IsEmpty && !postcode2.IsEmpty)
			{
				if (postcode1.Length > postcode2.Length)
				{
					result = 1;
				}
				if (postcode1.Length < postcode2.Length)
				{
					result = -1;
				}
			}

			return result;
		}

		static void Validate(SchemaColumn column)
		{
			if (column.TableName != RateEntrySchema.Constants.TableName)
			{
				throw new ArgumentException("Column must belong to the RateEntry table", nameof(column));
			}
		}
	}
}
