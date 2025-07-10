namespace Enterprise.Rating.Business
{
	using CargoWise.Schema;
	using CargoWise.Types;

	class GuidColumnComparer : ColumnComparer
	{
		public GuidColumnComparer(SchemaGuidColumn column, string columnName, ZGuid preferredValue, ZGuid fallbackValue)
			: base(column)
		{
			this.columnName = columnName;
			this.preferredValue = preferredValue;
			this.fallbackValue = fallbackValue;
		}

		readonly string columnName;
		readonly ZGuid preferredValue;
		readonly ZGuid fallbackValue;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1GuidValue = (ZGuid)line1.ParentRateEntry.GetValue(column);
			var entry2GuidValue = (ZGuid)line2.ParentRateEntry.GetValue(column);

			if (!entry1GuidValue.IsEmpty && !entry2GuidValue.IsEmpty && !entry1GuidValue.Equals(entry2GuidValue))
			{
				if (!preferredValue.IsEmpty)
				{
					if (entry1GuidValue == preferredValue)
					{
						return 2;
					}
					if (entry2GuidValue == preferredValue)
					{
						return -2;
					}
				}

				if (!fallbackValue.IsEmpty)
				{
					if (entry1GuidValue == fallbackValue)
					{
						return 1;
					}
					if (entry2GuidValue == fallbackValue)
					{
						return -1;
					}
				}
			}

			return base.Compare(line1, line2);
		}

		#region SuppressResourceStringsCheckRegion

		protected override string GetName()
		{
			return columnName;
		}

		#endregion
	}
}
