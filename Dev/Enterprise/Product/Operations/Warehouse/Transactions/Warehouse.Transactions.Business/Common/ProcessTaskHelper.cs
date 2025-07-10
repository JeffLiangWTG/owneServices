using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ProcessTaskHelper
	{
		public static void DeleteProcessTasksAndRelatedProcessHeader(BusinessObjectFactory factory, IProcessTask[] processTasksToDelete)
		{
			var processHeaders = factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, processTasksToDelete.Select(task => task.P9_FH_ProcessHeader).Where(p => p.IsValid).Distinct()));
			processTasksToDelete.ForEach(t => ((IBusiness)t).Delete());

			foreach (var header in processHeaders)
			{
				if (!header.Tasks.Any())
				{
					header.Delete();
				}
			}
		}
	}
}
