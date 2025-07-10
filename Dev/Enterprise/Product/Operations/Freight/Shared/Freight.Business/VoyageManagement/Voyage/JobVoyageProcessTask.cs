using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	class JobVoyageProcessTask : ProcessTask, IJobVoyageProcessTask
	{
		public JobVoyageProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(JobVoyage); }
		}

		public new JobVoyage Parent
		{
			get { return (JobVoyage)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return (Parent as IControllerIDProvider)?.ControllerID; }
		}
	}
}
