using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class ColumnComparer : BaseRateLineComparer
	{
		public ColumnComparer(SchemaColumn column)
		{
			this.column = column;
		}

		protected readonly SchemaColumn column;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (line1.ParentRateEntry.IsSpotEntry != line2.ParentRateEntry.IsSpotEntry)
			{
				return 0;
			}

			if (column.TableName == RateEntrySchema.Constants.TableName)
			{
				return CompareValues(line1.ParentRateEntry.GetValue(column), line2.ParentRateEntry.GetValue(column));
			}

			if (column.TableName == RateLinesSchema.Constants.TableName)
			{
				return CompareBizO(RateLine.GetBO(line1.Line), RateLine.GetBO(line2.Line));
			}

			throw new NotSupportedException("Can only compare values from either RateLine or RateEntry tables");
		}

		public override bool Equals(BaseRateLineComparer other)
		{
			return base.Equals(other) && column.Name == ((ColumnComparer)other).column.Name;
		}

		protected override string GetName()
		{
			return column.Name;
		}

		int CompareBizO(BusinessObject obj1, BusinessObject obj2)
		{
			var x = (IZType)obj1[column.Name];
			var y = (IZType)obj2[column.Name];

			return CompareValues(x, y);
		}

		internal static int CompareValues(IZType x, IZType y)
		{
			if (!x.IsEmpty && y.IsEmpty)
			{
				return 1;
			}

			if (x.IsEmpty && !y.IsEmpty)
			{
				return -1;
			}

			return 0;
		}
	}
}
