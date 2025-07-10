using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectConvertedFromJiraProject : Project
	{
		public ProjectConvertedFromJiraProject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void LoadRelatedItems(WorkTaskRelatedItemGenPivotCollection collection)
		{
			// loading results in unnecessary db hits, since we only need to add new related items, not actually access existing ones.
		}
	}
}
