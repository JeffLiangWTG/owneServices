using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonProcessTask : ProcessTasks
	{
		public GlbPersonProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		protected internal override Type ParentType
		{
			get { return typeof(GlbPerson); }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.GlbPerson; }
		}

		#endregion

	}
}
