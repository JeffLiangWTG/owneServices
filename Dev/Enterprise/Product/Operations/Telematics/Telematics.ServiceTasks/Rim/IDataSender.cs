using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public interface IDataSender
	{
		void Send(BusinessObjectFactory factory, IEnumerable<IPortionedData> data);
	}
}
