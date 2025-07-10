using System.Collections.Generic;
using System.Dynamic;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class DynamicCsvRecord : DynamicObject
	{
		public DynamicCsvRecord()
		{
			property = new Dictionary<string, object>();
		}

		public DynamicCsvRecord(params string[] prams) : this()
		{
			AddProperties(prams);
		}

		readonly Dictionary<string, object> property;

		const string ColumnNamePrefix = "Column";

		int index;

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var name = binder.Name;
			return property.TryGetValue(name, out result);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return property.Keys;
		}

		public int PropertiesCount => property.Count;

		internal void AddProperties(params string[] prams)
		{
			foreach (var pram in prams)
			{
				property.Add(ColumnNamePrefix + index++, pram);
			}
		}

		internal void Append(DynamicCsvRecord dynamicCsvRecord)
		{
			foreach (var pair in dynamicCsvRecord.property)
			{
				property.Add(ColumnNamePrefix + index++, pair.Value);
			}
		}

		internal static DynamicCsvRecord Combine(params DynamicCsvRecord[] records)
		{
			var record = new DynamicCsvRecord();
			for (int i = 0; i < records.Length; i++)
			{
				record.Append(records[i]);
			}
			return record;
		}
	}
}
