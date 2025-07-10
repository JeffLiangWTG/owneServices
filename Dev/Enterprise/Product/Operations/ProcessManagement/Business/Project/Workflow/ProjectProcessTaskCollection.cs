using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectProcessTaskCollection : ProcessTaskCollection
	{
		public ProjectProcessTaskCollection(Project project)
			: base(project)
		{
		}

		public ProjectProcessTaskCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new ProjectProcessTask this[int index]
		{
			get { return (ProjectProcessTask)Elements[index]; }
		}

		public new ProjectProcessTask AddNew()
		{
			return (ProjectProcessTask)base.AddNew();
		}

		public new Project Parent
		{
			get { return (Project)base.Parent; }
		}
	}
}
