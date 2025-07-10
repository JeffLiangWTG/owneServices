using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration;

public interface IScheduleTaskModuleService : IService
{
	ZString ScheduleTaskModuleName { set; get; }
}
