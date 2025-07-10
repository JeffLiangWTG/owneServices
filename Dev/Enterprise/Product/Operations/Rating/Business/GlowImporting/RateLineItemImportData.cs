using System;
using System.Collections.Generic;
using CargoWise.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Represents a RateLineItemImportData whose source is a pair of value and it is critical
	/// that the first one is set before the second one.
	/// </summary>
	class RateLineItemImportDataPair<TColumn> : IRateLineItemImportDataMany
		where TColumn : struct, Enum
	{
		readonly IRateLineItemImportDataSingle first;
		readonly IRateLineItemImportDataSingle second;

		internal RateLineItemImportDataPair(SchemaColumn firstBizoColumn, TColumn firstCalculatorColumn, SchemaColumn secondBizoColumn, TColumn secondCalculatorColumn)
		{
			first = new RateLineItemImportDataWithColumn<TColumn>(firstBizoColumn, firstCalculatorColumn);
			second = new RateLineItemImportDataWithColumn<TColumn>(secondBizoColumn, secondCalculatorColumn);
		}
		internal RateLineItemImportDataPair(SchemaColumn bizoForConstant, string constantValue, SchemaColumn bizoForColumn, TColumn calculatorColumn)
		{
			first = new RateLineItemImportDataWithConstant(bizoForConstant, constantValue);
			second = new RateLineItemImportDataWithColumn<TColumn>(bizoForColumn, calculatorColumn);
		}

		public IEnumerable<IRateLineItemImportDataSingle> ToSingles()
		{
			return new[] { first, second };
		}
	}

	/// <summary>
	/// Represents a RateLineItemImportData whose source is a constant value.
	/// 
	/// These constant values are ignorable in the eyes of RateLineItemImportData in the sense that
	/// a RateLineItem that is setting some constant and some non-constant values will not persist
	/// if the non-constant values are not set.
	/// </summary>
	class RateLineItemImportDataWithConstant : IRateLineItemImportDataSingle
	{
		readonly string value;
		public bool IsIgnorable => true;

		public string BizoColumnName { get; }

		internal RateLineItemImportDataWithConstant(SchemaColumn bizoColumn, string value)
		{
			BizoColumnName = bizoColumn.Name;
			this.value = value;
		}

		public bool TryGetValue<TColumn>(Dictionary<TColumn, string> values, out string columnValue)
			where TColumn : struct, Enum
		{
			columnValue = value;
			return true;
		}
	}

	/// <summary>
	/// Represents a RateLineItemImportData whose source represents a value that is provided
	/// by the importing process. 
	/// </summary>
	class RateLineItemImportDataWithColumn<TColumn> : IRateLineItemImportDataSingle
		where TColumn : struct, Enum
	{
		readonly TColumn calculatorColumn;

		public bool IsIgnorable => false;

		public string BizoColumnName { get; }

		internal RateLineItemImportDataWithColumn(SchemaColumn bizoColumn, TColumn calculatorColumn)
		{
			this.calculatorColumn = calculatorColumn;
			BizoColumnName = bizoColumn.Name;
		}

		public bool TryGetValue<TColumn2>(Dictionary<TColumn2, string> mapping, out string value)
			where TColumn2 : struct, Enum
		{
			var mappingTColumn = mapping as Dictionary<TColumn, string>;
			if (!mappingTColumn.TryGetValue(calculatorColumn, out value))
			{
				return false;
			}
			return true;
		}
	}

	interface IRateLineItemImportDataMany : IRateLineItemImportData
	{
		IEnumerable<IRateLineItemImportDataSingle> ToSingles();
	}

	interface IRateLineItemImportDataSingle : IRateLineItemImportData
	{
		/// <summary>
		/// The name of the business object column
		/// </summary>
		string BizoColumnName { get; }

		/// <summary>
		/// When the RateLineItem being imported, receives objects of IRateLineItemImportData
		/// that are all either without value or with this IsIgnorable = True,
		/// then the RateLineItem will be aborted and not created for the RateLine.
		/// </summary>
		bool IsIgnorable { get; }

		/// <summary>
		/// Tries to source value that this IRateLineItemImportData represents from the mapping.
		/// Returns false if there is no value that matches. Returns true otherwise.
		/// </summary>
		bool TryGetValue<TColumn>(Dictionary<TColumn, string> mapping, out string value)
			where TColumn : struct, Enum;
	}

	interface IRateLineItemImportData { }
}
