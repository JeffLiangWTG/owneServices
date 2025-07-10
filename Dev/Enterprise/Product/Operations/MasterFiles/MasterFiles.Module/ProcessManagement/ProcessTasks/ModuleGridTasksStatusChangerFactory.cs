using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	sealed public class ModuleGridTasksStatusChangerFactory : IModuleGridTasksStatusChangerFactory
	{
		public IModuleFilterTaskStatusChanger Create(ZGrid grid) => new ModuleGridTaskStatusChanger(grid);
	}
}
