using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks.PreDriveChecklist
{
	class TelematicsChecklistAccessor : IChecklistAccessor
	{
		public ICollection<TelPreDriveChecklistHeader> GetChecklists(BusinessObjectFactory factory, CancellationToken cancellationToken)
		{
			return factory.Load<TelPreDriveChecklistHeader>(
				new ZQuery(TelPreDriveChecklistHeaderSchema.TPH_IsProcessed, false));
		}
	}
}
