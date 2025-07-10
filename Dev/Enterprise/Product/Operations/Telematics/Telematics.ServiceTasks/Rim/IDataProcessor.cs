using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public interface IDataProcessor
	{
		IEnumerable<IPortionedData> Process(BusinessObjectFactory factory, IEnumerable<IDeviceData> data, int maximumRecordsInBatch, CancellationToken cancellationToken);
	}
}
