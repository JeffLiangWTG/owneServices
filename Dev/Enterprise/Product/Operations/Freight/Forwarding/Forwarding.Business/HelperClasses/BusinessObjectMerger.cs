using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class BusinessObjectMerger<T> where T : BusinessObject
	{
		protected BusinessObjectMerger(IEnumerable<T> sourceList)
		{
			Argument.NotNull(sourceList, "SourceList cannot be null");

			SourceList = sourceList;
		}

		public readonly IEnumerable<T> SourceList;

		#region Merge

		public abstract string CheckMerger();

		public abstract void DoMerge();

		#endregion

		#region Columns

		public IEnumerable<SchemaColumn> ComparableColumns
		{
			get { return comparableColumns ?? (comparableColumns = GetComparableColumnsCore()); }
		}
		IEnumerable<SchemaColumn> comparableColumns;

		public IEnumerable<SchemaColumn> IgnoredColumns
		{
			get { return ignoredColumns ?? (ignoredColumns = GetIgnoredColumnsCore()); }
		}
		IEnumerable<SchemaColumn> ignoredColumns;

		protected abstract IEnumerable<SchemaColumn> GetComparableColumnsCore();
		protected abstract IEnumerable<SchemaColumn> GetIgnoredColumnsCore();

		#endregion

		protected string GetDifferentColumnValuesError()
		{
			var columns = ColumnsWithDifferentValues().ToArray();
			if (columns.Any())
			{
				var obj = SourceList.FirstOrDefault(c => c != null);
				GetHumanReadableNames(obj);

				var names = columns
					.Select(c => humanReadableNameDic.ContainsKey(c) ? humanReadableNameDic[c] : c.Name)
					.ToArray();

				return Res.GetString("b00cbe3a-7207-401a-ae0e-af6aa676c8c0",
						"The following fields are different:{0}{0}{1}",
						System.Environment.NewLine,
						string.Join(", ", names));
			}

			return string.Empty;
		}

		protected bool HaveSameValuesInComparableColumns(T source, T target)
		{
			return source.PK == target.PK || ComparableColumns.All(column => Equals(source[column], target[column]));
		}

		IEnumerable<SchemaColumn> ColumnsWithDifferentValues()
		{
			return from column in ComparableColumns let values = SourceList.Select(c => c[column]).Distinct() where values.Count() > 1 select column;
		}

		void GetHumanReadableNames(BusinessObject obj)
		{
			if (humanReadableNameDic == null && obj != null)
			{
				humanReadableNameDic = ComparableColumns.ToDictionary(x => x, x => obj.ZPropertyInfoHash[x.Name].HumanReadableName.ToString());
			}
		}
		Dictionary<SchemaColumn, string> humanReadableNameDic;
	}
}
