using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.PreDriveChecklist
{
	interface IChecklistAccessor
	{
		ICollection<TelPreDriveChecklistHeader> GetChecklists(BusinessObjectFactory factory, CancellationToken cancellationToken);
	}
}
