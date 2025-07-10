using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public interface IParamValue
{
	ZString Name { get; }

	bool? ShouldSkipEmptyValue { get; }

	bool IsEmpty { get; }
}

public interface IParamStringValue : IParamValue
{
	ZString StringValue { get; }
}
