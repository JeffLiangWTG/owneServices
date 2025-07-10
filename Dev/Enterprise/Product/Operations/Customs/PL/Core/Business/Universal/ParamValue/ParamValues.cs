using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public sealed record ParamValues(ZString Name, ZString[] StringValues, bool? ShouldSkipEmptyValue = null) : IParamStringValue
{
	public ZString StringValue => lazyStringValue.Value;

	public bool IsEmpty => StringValues is null || StringValues.Length == 0 || StringValues.All(x => x.IsEmpty);

	public override string ToString() => $"{Name} : [{StringValue}]";

	public override int GetHashCode() => (Name, StringValues).GetHashCode();

	readonly Lazy<ZString> lazyStringValue = new(() => ZString.Join(" | ", StringValues));
}
