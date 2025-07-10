using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public interface IDataAccessor
	{
		IEnumerable<IDeviceData> GetData(IFactory factory);
	}
}
