using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectProcessTask : ProcessTasks
	{
		public ProjectProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Project; }
		}

		protected override Type ParentType
		{
			get { return typeof(Project); }
		}

		public new Project Parent
		{
			get { return (Project)base.Parent; }
		}

		#endregion
	}
}
