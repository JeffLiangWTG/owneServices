using System;

namespace CargoWise.RefDbRepo.Common.Utils;

public class ColumnMetaData
{
	public string ColumnName { get; init; }
	public string DataType { get; init; }
	public int ColumnSize { get; init; }
	public int NumericPrecision { get; init; }
	public int NumericScale { get; init; }
	public bool IsNullable { get; init; }

	public override bool Equals(object obj)
	{
		if (obj is not ColumnMetaData other) return false;
		return ColumnName == other.ColumnName
				&& DataType == other.DataType
				&& ColumnSize == other.ColumnSize
				&& NumericPrecision == other.NumericPrecision
				&& NumericScale == other.NumericScale
				&& IsNullable == other.IsNullable;
	}

	public override int GetHashCode() =>
		HashCode.Combine(ColumnName, DataType, ColumnSize, NumericPrecision, NumericScale, IsNullable);

	public override string ToString()
	{
		return $"ColumnName: {ColumnName}, DataType: {DataType}, ColumnSize: {ColumnSize}, NumericPrecision: {NumericPrecision}, NumericScale: {NumericScale}, IsNullable: {IsNullable}";
	}
}
