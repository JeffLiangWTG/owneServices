using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationProcessTask : ProcessTask
	{
		public HRJobApplicationProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.HRJobApplication; }
		}

		protected override Type ParentType
		{
			get { return typeof(HRJobApplication); }
		}

		#endregion
	}
}
