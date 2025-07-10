using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobProcessHeaderUpdater
	{
		readonly BusinessObjectFactory factory;

		public JobProcessHeaderUpdater(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void ProcessChanges(IEnumerable<BusinessObject> objects)
		{
			foreach (var obj in objects.OfType<IWorkflowProvider>())
			{
				if (obj is BusinessObject bizo && !bizo.IsDeleted)
				{
					ProcessJobHeaderProvider.GetForParentWithoutCreation(obj, factory)?.UpdateJobProperties();
				}
			}
		}
	}
}
