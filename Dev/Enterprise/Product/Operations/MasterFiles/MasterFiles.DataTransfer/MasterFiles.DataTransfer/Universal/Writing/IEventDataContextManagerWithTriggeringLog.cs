using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public interface IEventDataContextManagerWithTriggeringLog : IEventDataContextManager
	{
		BaseStmALog TriggeringLogForUseInPopulatingEventContext { get; set; }
	}
}
