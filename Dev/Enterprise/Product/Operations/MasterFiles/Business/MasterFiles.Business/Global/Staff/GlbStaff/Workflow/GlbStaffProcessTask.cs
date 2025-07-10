using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffProcessTask : ProcessTasks
	{
		public GlbStaffProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.GlbStaff; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(GlbStaff); }
		}

		#endregion

	}
}