using System;
using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CustomValues;

[WTG.StaticAnalysis.Annotation.Immutable]
public class UniversalNullObject : IZType, IZTypeInternals
{
	UniversalNullObject()
	{
	}

	public override string ToString()
	{
		return string.Empty;
	}

	public static readonly UniversalNullObject Instance = new UniversalNullObject();

	public override bool Equals(object obj)
	{
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public int CompareTo(object obj)
	{
		if (obj == null || obj is UniversalNullObject)
		{
			return 0;
		}

		if (obj is string svalue)
		{
			return string.Compare(ToString(), svalue);
		}

		if (obj is ZString zvalue)
		{
			return zvalue.CompareTo(ToString());
		}

		return -1;
	}

	ZDataType IZType.DataType
	{
		get { return ZDataType.NonNumeric; }
	}

	Type IZType.BaseDataType
	{
		get { return typeof(object); }
	}

	bool IZType.IsEmpty
	{
		get { return true; }
	}

	bool IZType.IsValid
	{
		get { return true; }
	}

	bool IZType.IsDefault
	{
		get { return true; }
	}

	IZType IZType.Default
	{
		get { return Instance; }
	}

	[DebuggerStepThrough]
	object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
	{
		return DBNull.Value;
	}
}
