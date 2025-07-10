using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Business.Testing;

public readonly record struct LogExpectedOn (
	IStmALogProvider[] BusinessObjects = null,
	ISimpleLogResult ServiceLog = null)
{
	public LogExpectedOn (
		IStmALogProvider businessObject,
		ISimpleLogResult serviceLog = null)
		: this([businessObject], serviceLog)
	{
	}

	public LogExpectedOn (ISimpleLogResult serviceLog)
		: this((IStmALogProvider[])null, serviceLog)
	{
	}
}
