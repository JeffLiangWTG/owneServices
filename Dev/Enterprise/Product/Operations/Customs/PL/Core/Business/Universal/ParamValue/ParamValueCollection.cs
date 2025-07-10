using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public sealed class ParamValueCollection : List<IParamValue>
{
	public ParamValueCollection()
	{ }

	public ParamValueCollection(IEnumerable<IParamValue> collection) : base(collection)
	{ }

	public ParamValueCollection(int capacity, IEnumerable<IParamValue> collection = null) : base(capacity)
	{
		if (collection != null)
		{
			AddRange(collection);
		}
	}

	public void Add(string name, string value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, value, shouldSkipEmptyValue));
	}

	public void Add(string name, int value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, value.ToString(), shouldSkipEmptyValue));
	}

	public void Add(string name, int? value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, value?.ToString() ?? ZString.Empty, shouldSkipEmptyValue));
	}

	public void Add(string name, decimal value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, ((ZDecimal)value).ToString(), shouldSkipEmptyValue));
	}

	public void Add(string name, DateTime value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, ((ZDateTime)value).ToString(), shouldSkipEmptyValue));
	}

	public void Add(string name, DateTime? value, bool? shouldSkipEmptyValue = null)
	{
		Add(new ParamValue(name, value != null ? ((ZDateTime)value.Value).ToString() : ZString.Empty, shouldSkipEmptyValue));
	}

	public void Add(string name, string[] values, bool? shouldSkipEmptyValue = null)
		=> Add(new ParamValues(name, values.Select(x => (ZString)x).ToArray(), shouldSkipEmptyValue));
}
