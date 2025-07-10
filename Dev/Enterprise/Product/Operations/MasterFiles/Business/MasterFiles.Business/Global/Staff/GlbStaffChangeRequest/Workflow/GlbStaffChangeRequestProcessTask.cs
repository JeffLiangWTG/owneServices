using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestProcessTask : ProcessTasks
	{
		public GlbStaffChangeRequestProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected internal override Type ParentType => typeof(GlbStaffChangeRequest);

		public override ControllerID ParentControllerID => null;
	}
}
