using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public abstract class NPOISearchTable : IEnumerable<Dictionary<string, string>>
	{
		protected NPOISearchTable(ISheet sheet, ILogger logger, HashSet<string> tableHeaders, HashSet<string> searchHeaders, Func<string, string> getCellFormat = null)
		{
			TableHeaders = Argument.NotNull(tableHeaders, nameof(tableHeaders));
			SearchHeaders = Argument.NotNull(searchHeaders, nameof(searchHeaders));
			Argument.IsTrue(searchHeaders.All(tableHeaders.Contains), nameof(searchHeaders));
			Logger = Argument.NotNull(logger, nameof(logger));
			Sheet = Argument.NotNull(sheet, nameof(sheet));
			SheetName = Sheet.SheetName;

			DataRows = new Lazy<Dictionary<int, Dictionary<string, string>>>(PopulateIndexesAndGetDataRows(sheet, getCellFormat));
		}

		public string SheetName { get; }

		#region Heading & Ending

		protected HashSet<string> TableHeaders { get; }
		protected HashSet<string> SearchHeaders { get; }

		protected bool IsValid(IRow row) => IsValidRowCore(row);
		protected virtual bool IsValidRowCore(IRow row) => true;

		protected Dictionary<string, (int ColumnIndex, Dictionary<string, HashSet<int>> SheetRows)> SearchDictionary
		{
			get;
			private set;
		}
		protected Dictionary<string, int> HeadersDictionary
		{
			get;
			private set;
		}
		ISheet Sheet { get; }

		Lazy<Dictionary<int, Dictionary<string, string>>> DataRows { get; }

		protected ILogger Logger { get; }

		#endregion

		#region Searching & Enumerating

		public IEnumerator<Dictionary<string, string>> GetEnumerator()
		{
			return DataRows.Value.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Length => DataRows.Value.Count;

		public Dictionary<string, string> this[int rowIndex] => DataRows.Value[rowIndex];

		public string[] Find(string header, params (string Header, string Value)[] searchKeys)
			=> Find(new[] { header }, searchKeys)
				.Select(c => c[0])
				.ToArray<string>();

		public string[][] Find(string[] headers, params (string Header, string Value)[] filters)
		{
			var missingHeaders = headers.Select(StringExtensions.ToSearchKey).Except(HeadersDictionary.Keys).ToList();
			if (missingHeaders.Count > 0)
			{
				Logger.LogError("Unrecognized Headers: {}", missingHeaders);
				return Array.Empty<string[]>();
			}

			var missingSearchHeaders = filters.Select(f => f.Header).Select(StringExtensions.ToSearchKey).Except(SearchDictionary.Keys).ToList();
			if (missingSearchHeaders.Count > 0)
			{
				Logger.LogError("Unrecognized Search Headers: {}", missingSearchHeaders);
				return Array.Empty<string[]>();
			}

			IEnumerable<int> filteredRowIndexes;
			if (filters.Length > 0)
			{
				var rowFilters = filters.Select(searchKey => SearchDictionary[searchKey.Header.ToSearchKey()].SheetRows.GetOrDefault(searchKey.Value.ToSearchKey()))
					.Select(filteredRowIndex => filteredRowIndex ?? new HashSet<int>())
					.ToArray<HashSet<int>>();

				filteredRowIndexes = rowFilters.Aggregate((current, next) => current.Intersect(next).ToHashSet());

				if (!filteredRowIndexes.Any())
				{
					Logger.LogError("Unable to find result, sheet:{0} filters: {1}", Sheet.SheetName, filters);
					return Array.Empty<string[]>();
				}
			}
			else
			{
				filteredRowIndexes = DataRows.Value.Keys;
			}

			var results = new List<string[]>();

			foreach (var filteredRowIndex in filteredRowIndexes)
			{
				var rowData = DataRows.Value[filteredRowIndex];
				var dataList = new List<string>();
				foreach (var header in headers)
				{
					dataList.Add(rowData[header.ToSearchKey()]);
				}
				results.Add(dataList.ToArray());
			}

			return results.ToArray();
		}

		#endregion

		#region Implementation

		Dictionary<int, Dictionary<string, string>> PopulateIndexesAndGetDataRows(ISheet sheet, Func<string, string> getCellFormat = null)
		{
			var rowCount = sheet.PhysicalNumberOfRows;
			var dataRowsDict = new Dictionary<int, Dictionary<string, string>>();

			for (var rowIdx = 0; rowIdx <= rowCount; rowIdx++)
			{
				var excelRow = sheet.GetRow(rowIdx);
				if (excelRow == null)
				{
					continue;
				}

				if (PopulateIndexDictionaries(excelRow).HasValue)
				{
					continue;
				}

				if (excelRow.Count() < TableHeaders.Count || !IsValid(excelRow))
				{
					continue;
				}

				foreach (var item in SearchDictionary)
				{
					var cellValue = excelRow.GetCell(item.Value.ColumnIndex).ToSearchKey();
					item.Value.SheetRows.GetOrAddNew(cellValue).Add(rowIdx);
				}

				var dataRow = HeadersDictionary.ToDictionary(item => item.Key,
					item => excelRow.GetCell(item.Value)
						.ValueToString(getCellFormat?.Invoke(item.Key)));

				dataRowsDict[rowIdx] = dataRow;
			}

			return dataRowsDict;
		}

		bool? PopulateIndexDictionaries(IRow excelRow)
		{
			if (SearchDictionary != null && HeadersDictionary != null)
			{
				return null;
			}

			var searchDictInitialized = false;
			var headerDictInitialized = false;
			if (SearchDictionary == null)
			{
				var searchCells = excelRow.Where(cell => SearchHeaders.Contains(cell.GetTextOrMerged()));
				if (searchCells.Count() == SearchHeaders.Count)
				{
					SearchDictionary = searchCells.ToDictionary(
						cell => cell.GetTextOrMerged().ToSearchKey(),
						cell => (cell.ColumnIndex, new Dictionary<string, HashSet<int>>())
					);
					searchDictInitialized = true;
				}
			}

			if (HeadersDictionary == null)
			{
				var headerCells = excelRow.Where(cell => TableHeaders.Contains(cell.GetTextOrMerged()));
				if (headerCells.Count() == TableHeaders.Count)
				{
					HeadersDictionary = headerCells.ToDictionary(
						cell => cell.GetTextOrMerged().ToSearchKey(),
						cell => cell.ColumnIndex
					);
					headerDictInitialized = true;
				}
			}

			return searchDictInitialized && headerDictInitialized;
		}

		#endregion
	}
}
