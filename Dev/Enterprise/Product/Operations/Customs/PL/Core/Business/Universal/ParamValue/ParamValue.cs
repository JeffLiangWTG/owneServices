using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public sealed record ParamValue(ZString Name, ZString StringValue, bool? ShouldSkipEmptyValue = null) : IParamStringValue
{
	public bool IsEmpty => string.IsNullOrEmpty(StringValue);

	public override string ToString() => $"{Name} : {StringValue}";

	public override int GetHashCode() => (Name, Value: StringValue).GetHashCode();
}
