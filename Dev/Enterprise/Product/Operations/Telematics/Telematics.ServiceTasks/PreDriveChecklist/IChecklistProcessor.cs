using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.PreDriveChecklist
{
	interface IChecklistProcessor
	{
		int ProcessChecklists(BusinessObjectFactory factory, ICollection<TelPreDriveChecklistHeader> checklists, CancellationToken cancellationToken);
	}
}
